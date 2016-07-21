﻿/*

The MIT License (MIT)

Copyright (c) 2015 Secret Lab Pty. Ltd. and Yarn Spinner contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using CsvHelper;
using CommandLine;

namespace Yarn
{

	class BaseOptions {
		[Option('d', "debug", HelpText = "Show debugging information.")]
		public bool showDebuggingInfo { get; set; }

	}

	class ConsoleOptions : BaseOptions {
		[Option('t', "show-tokens", HelpText="Show the list of parsed tokens and exit.")]
		public bool showTokensAndExit { get; set; }

		[Option('p', "show-parse-tree", HelpText="Show the parse tree and exit.")]
		public bool showParseTreeAndExit { get; set; }

		[Option('w', "wait-for-input", HelpText="After showing each line, wait for the user to press a key.")]
		public bool waitForInput { get; set; }

		[Option('s', "start-node", Default = Dialogue.DEFAULT_START, HelpText="Start at the given node.")]
		public string startNode { get; set; }

		[Option('v', "variables", HelpText="Set default variable.")]
		public IList<string> variables { get; set; }

		[Option('V', "verify-script", HelpText="Verifies the provided script")]
		public bool verifyAndExit { get; set; }

		[Option('o', "only", HelpText="Only consider this node.")]
		public string onlyConsiderNode { get; set; }

		[Option('r', "run-times", HelpText="Run the script this many times.", Default=1)]
		public int runTimes { get; set; }

		[Option('c', "dump-bytecode", HelpText="Show program bytecode and exit.")]
		public bool compileAndExit { get; set; }

		[Option('a', "analyse", HelpText="Show analysis of the program and exit.")]
		public bool analyseAndExit { get; set; }

		[Option('1', "select-first-choice", HelpText="Automatically select the the first option when presented with options.")]
		public bool automaticallySelectFirstOption { get; set; }

		[Option('T', "string-table", HelpText = "The string table to use.")]
		public string stringTable { get; set; }

		[Value(0, HelpText = "The files to parse.")]
		public IList<string> files { get; set; }


	}

	class YarnSpinnerConsole
	{

		public static void Main (string[] args)
		{

			var results = CommandLine.Parser.Default.ParseArguments<ConsoleOptions>(args);

			var returnCode = results.MapResult(
				(ConsoleOptions options) => { Execute(options); return 0; },
				errors => { Console.WriteLine(errors); return 1; });

			Environment.Exit(returnCode);


		}

		static void Execute(ConsoleOptions options)
		{
			

			// Verify that all the files are valid
			string[] allowedExtensions = { ".node", ".json" };

			var inputFiles = new List<string>();

			foreach (var input in options.files) {
				var extension = System.IO.Path.GetExtension(input);

				var allowed = true;
				foreach (var ext in allowedExtensions) {
					if (extension == ext) {
						allowed = true;
						break;
					}
				}
				if (!allowed) {
					Console.WriteLine(string.Format("File {0} is not a valid Yarn file", input));
					return;
				}

				if (System.IO.File.Exists(input) == false) {
					Console.WriteLine(string.Format("File {0} is not a valid Yarn file", input));
					return;
				}
				inputFiles.Add(input);
			}

			// Create the object that handles callbacks
			var impl = new ConsoleRunnerImplementation(waitForLines: options.waitForInput);

			// load the default variables we got on the command line
			foreach (var variable in options.variables)
			{
				var entry = variable.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

				float value;
				// If there aren't two parts to this or the second part isn't a float, fail
				if (entry.Length != 2 || float.TryParse(entry[1], out value) == false) {
					Console.WriteLine(string.Format("Skipping invalid variable {0}", variable));
					continue;
				}
				var name = entry[0];

				impl.SetNumber(name, value);
			}

			// Load nodes
			var dialogue = new Dialogue(impl);

			// Add some methods for testing
			dialogue.library.RegisterFunction("add_three_operands", 3, delegate (Value[] parameters)
			{
				return parameters[0] + parameters[1] + parameters[2];
			});

			dialogue.library.RegisterFunction("last_value", -1, delegate (Value[] parameters)
			{
				// return the last value
				return parameters[parameters.Length - 1];
			});

			dialogue.library.RegisterFunction("is_even", 1, delegate (Value[] parameters)
			{
				return (int)parameters[0].AsNumber % 2 == 0;
			});

			// Register the "assert" function, which stops execution if its parameter evaluates to false
			dialogue.library.RegisterFunction("assert", -1, delegate (Value[] parameters)
			{
				if (parameters[0].AsBool == false)
				{

					// TODO: Include file, node and line number
					if (parameters.Length > 1 && parameters[1].AsBool)
					{
						dialogue.LogErrorMessage("ASSERTION FAILED: " + parameters[1].AsString);
					}
					else {
						dialogue.LogErrorMessage("ASSERTION FAILED");
					}
					Environment.Exit(1);
				}
			});


			// Register a function to let test scripts register how many
			// options they expect to send
			dialogue.library.RegisterFunction("prepare_for_options", 2, delegate (Value[] parameters)
			{
				impl.numberOfExpectedOptions = (int)parameters[0].AsNumber;
				impl.autoSelectOptionNumber = (int)parameters[1].AsNumber;
			});

			dialogue.library.RegisterFunction("expect_line", 1, delegate (Value[] parameters)
			{
				impl.expectedNextLine = parameters[0].AsString;
			});

			dialogue.library.RegisterFunction("expect_command", 1, delegate (Value[] parameters)
			{
				impl.expectedNextCommand = parameters[0].AsString;
			});

			if (options.automaticallySelectFirstOption == true)
			{
				impl.autoSelectFirstOption = true;
			}

			// If debugging is enabled, log debug messages; otherwise, ignore them
			if (options.showDebuggingInfo)
			{
				dialogue.LogDebugMessage = delegate (string message)
				{
					Console.WriteLine("Debug: " + message);
				};
			}
			else {
				dialogue.LogDebugMessage = delegate (string message) { };
			}

			dialogue.LogErrorMessage = delegate (string message)
			{
				Console.WriteLine("ERROR: " + message);
			};

			if (options.verifyAndExit)
			{
				try
				{
					dialogue.LoadFile(options.files[0], false, false, options.onlyConsiderNode);
				}
				catch (Exception e)
				{
					Console.WriteLine("Error: " + e.Message);
				}
				return;
			}

			dialogue.LoadFile(options.files[0], options.showTokensAndExit, options.showParseTreeAndExit, options.onlyConsiderNode);

			// Load string table
			if (options.stringTable != null)
			{

				var parsedTable = new Dictionary<string, string>();

				using (var reader = new System.IO.StreamReader(options.stringTable))
				{
					using (var csvReader = new CsvReader(reader))
					{
						if (csvReader.ReadHeader() == false)
						{
							Console.WriteLine(string.Format("{0} is not a valid string table", options.stringTable));
							Environment.Exit(1);
						}

						foreach (var row in csvReader.GetRecords<LocalisedLine>())
						{
							parsedTable[row.LineCode] = row.LineText;
						}
					}
				}

				dialogue.AddStringTable(parsedTable);
			}

			if (options.compileAndExit)
			{
				var result = dialogue.GetByteCode();
				Console.WriteLine(result);
				return;
			}

			if (options.analyseAndExit)
			{

				var context = new Yarn.Analysis.Context();

				dialogue.Analyse(context);

				foreach (var diagnosis in context.FinishAnalysis())
				{
					Console.WriteLine(diagnosis.ToString(showSeverity: true));
				}
				return;
			}

			// Only run the program when we're not emitting debug output of some kind
			var runProgram =
				options.showTokensAndExit == false &&
				options.showParseTreeAndExit == false &&
				options.compileAndExit == false &&
				options.analyseAndExit == false;

			if (runProgram)
			{
				// Run the conversation

				for (int run = 0; run < options.runTimes; run++)
				{
					foreach (var step in dialogue.Run(options.startNode))
					{

						// It can be one of three types: a line to show, options
						// to present to the user, or an internal command to run

						if (step is Dialogue.LineResult)
						{
							var lineResult = step as Dialogue.LineResult;
							impl.RunLine(lineResult.line);
						}
						else if (step is Dialogue.OptionSetResult)
						{
							var optionsResult = step as Dialogue.OptionSetResult;
							impl.RunOptions(optionsResult.options, optionsResult.setSelectedOptionDelegate);
						}
						else if (step is Dialogue.CommandResult)
						{
							var commandResult = step as Dialogue.CommandResult;
							impl.RunCommand(commandResult.command.text);
						}
					}
					impl.DialogueComplete();
				}


			}
		}

		// A simple Implementation for the command line.
		private class ConsoleRunnerImplementation : Yarn.VariableStorage {

			private bool waitForLines = false;

			Yarn.MemoryVariableStore variableStore;

			// The number of options we expect to see when we next
			// receive options. -1 means "don't care"
			public int numberOfExpectedOptions = -1;

			// The index of the option to automatically select, starting from 0.
			// -1 means "do not automatically select an option".
			public int autoSelectOptionNumber = -1;

			public string expectedNextLine = null;

			public string expectedNextCommand = null;

			public bool autoSelectFirstOption = false;

			public ConsoleRunnerImplementation(bool waitForLines = false) {
				this.variableStore = new MemoryVariableStore();
				this.waitForLines = waitForLines;
			}

			public void RunLine (Yarn.Line lineText)
			{

				if (expectedNextLine != null && expectedNextLine != lineText.text) {
					// TODO: Output diagnostic info here
					Console.WriteLine(string.Format("Unexpected line.\nExpected: {0}\nReceived: {1}",
						expectedNextLine, lineText.text));
					Environment.Exit (1);
				}

				expectedNextLine = null;

				Console.WriteLine (lineText.text);
				if (waitForLines == true) {
					Console.Read();
				}
			}

			public void RunOptions (Options optionsGroup, OptionChooser optionChooser)
			{

				Console.WriteLine("Options:");
				for (int i = 0; i < optionsGroup.options.Count; i++) {
					var optionDisplay = string.Format ("{0}. {1}", i + 1, optionsGroup.options [i]);
					Console.WriteLine (optionDisplay);
				}


				// Check to see if the number of expected options
				// is what we're expecting to see
				if (numberOfExpectedOptions != -1 &&
					optionsGroup.options.Count != numberOfExpectedOptions) {
					// TODO: Output diagnostic info here
					Console.WriteLine (string.Format("[ERROR: Expected {0} options, but received {1}]", numberOfExpectedOptions, optionsGroup.options.Count));
					Environment.Exit (1);
				}

				// If we were told to automatically select an option, do so
				if (autoSelectOptionNumber != -1) {
					Console.WriteLine ("[Received {0} options, choosing option {1}]", optionsGroup.options.Count, autoSelectOptionNumber);

					optionChooser (autoSelectOptionNumber);

					autoSelectOptionNumber = -1;

					return;

				}

				// Reset the expected options counter
				numberOfExpectedOptions = -1;



				if (autoSelectFirstOption == true) {
					Console.WriteLine ("[automatically choosing option 1]");
					optionChooser (0);
					return;
				}

				do {
					Console.Write ("? ");
					try {
						var selectedKey = Console.ReadKey ().KeyChar.ToString();
						int selection;

						if (int.TryParse(selectedKey, out selection) == true)
						{
							Console.WriteLine();

							// we present the list as 1,2,3, but the API expects
							// answers as 0,1,2
							selection -= 1; 

							if (selection > optionsGroup.options.Count)
							{
								Console.WriteLine("Invalid option.");
							}
							else {
								optionChooser(selection);
								break;
							}
						}

					} catch (FormatException) {}

				} while (true);
			}

			public void RunCommand (string command)
			{

				if (expectedNextCommand != null && expectedNextCommand != command) {
					// TODO: Output diagnostic info here
					Console.WriteLine(string.Format("Unexpected command.\n\tExpected: {0}\n\tReceived: {1}",
						expectedNextCommand, command));
					Environment.Exit (1);
				}

				expectedNextCommand = null;

				Console.WriteLine("Command: <<"+command+">>");
			}

			public void DialogueComplete ()
			{
				// All done
			}

			public void HandleErrorMessage (string error)
			{
				Console.WriteLine("Error: " + error);
			}

			public void HandleDebugMessage (string message)
			{
				Console.WriteLine("Debug: " + message);
			}

			public virtual void SetNumber (string variableName, float number)
			{
				variableStore.SetNumber(variableName, number);
			}

			public virtual float GetNumber (string variableName)
			{
				return variableStore.GetNumber(variableName);
			}

			public virtual void SetValue (string variableName, Value value) {
				variableStore.SetValue(variableName, value);
			}

			public virtual Value GetValue (string variableName) {
				return variableStore.GetValue(variableName);
			}

			public void Clear()
			{
				variableStore.Clear();
			}
		}


	}
}

