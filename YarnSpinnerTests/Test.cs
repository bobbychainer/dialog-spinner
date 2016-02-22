﻿using NUnit.Framework;
using System;

namespace YarnSpinner.Tests
{


	[TestFixture ()]
	public class Test
	{

		string nextExpectedLine = null;
		int nextExpectedOptionCount = -1;
		int nextOptionToSelect = -1;
		string nextExpectedCommand = null;


		Yarn.MemoryVariableStore storage = new Yarn.MemoryVariableStore();
		Yarn.Dialogue dialogue;

		[SetUp()]
		public void Init()
		{

			if (System.IO.Path.GetFileName(Environment.CurrentDirectory) != "Tests") {
				if (TestContext.CurrentContext.TestDirectory == Environment.CurrentDirectory) {
					// Hop up to the folder that contains the Tests folder
					var topLevelPath = System.IO.Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
					Environment.CurrentDirectory = topLevelPath;
				}

				var newWorkingDir = 
					System.IO.Path.Combine (Environment.CurrentDirectory, "Tests");
				Environment.CurrentDirectory = newWorkingDir;
			}


			dialogue = new Yarn.Dialogue (storage);

			dialogue.LogDebugMessage = delegate(string message) {
				
				Console.WriteLine (message);

			};

			dialogue.LogErrorMessage = delegate(string message) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine (message);
				Console.ResetColor ();
			};

			dialogue.library.RegisterFunction ("assert", 1, delegate(Yarn.Value[] parameters) {
				if (parameters[0].AsBool == false)
					Assert.Fail ("Assertion failed");
			});

			dialogue.library.RegisterFunction ("prepare_for_options", 2, delegate(Yarn.Value[] parameters) {
				nextExpectedOptionCount = (int)parameters[0].AsNumber;
				nextOptionToSelect = (int)parameters[1].AsNumber;
			});

			dialogue.library.RegisterFunction ("expect_line", -1, delegate(Yarn.Value[] parameters) {
				nextExpectedLine = parameters[0].AsString;
			});

			dialogue.library.RegisterFunction ("expect_command", -1, delegate(Yarn.Value[] parameters) {
				nextExpectedCommand = parameters[0].AsString;
			});
		}

		[Test ()]
		public void TestNodeExists ()
		{
		

			dialogue.LoadFile ("../Unity/Assets/Yarn Spinner/Examples/Demo Assets/Space.json");

			dialogue.Compile ();

			Assert.True (dialogue.NodeExists ("Sally"));

			// Test clearing everything
			dialogue.UnloadAll ();

			// Load an empty node
			dialogue.LoadString("// Test, this is empty");
			dialogue.Compile ();

			Assert.False (dialogue.NodeExists ("Sally"));


		}

		[Test()]
		public void TestParsingSmileys()
		{
			var path = System.IO.Path.Combine ("TestCases", "Smileys.node");
			dialogue.LoadFile (path);
			dialogue.Compile ();
		}

		[Test()]
		public void TestCommands()
		{
			var path = System.IO.Path.Combine ("TestCases", "Commands.node");
			dialogue.LoadFile (path);
			dialogue.Compile ();

			foreach (var result in dialogue.Run()) {
				HandleResult (result);
			}
		}

		[Test()]
		public void TestMissingNode() 
		{
			var path = System.IO.Path.Combine ("TestCases", "Smileys.node");
			dialogue.LoadFile (path);
			dialogue.Compile ();

			foreach (var result in dialogue.Run("THIS NODE DOES NOT EXIST")) {
				
			}
		}

		[Test()]
		public void TestGettingCurrentNodeName()  {
			dialogue.LoadFile ("../Unity/Assets/Yarn Spinner/Examples/Demo Assets/Space.json");

			// dialogue should not be running yet
			Assert.IsNull (dialogue.currentNode);

			foreach (var result in dialogue.Run("Sally")) {
				// Should now be in the node we requested
				Assert.AreEqual (dialogue.currentNode, "Sally");
				// Stop immediately
				dialogue.Stop ();
			}

			// Current node should now be null
			Assert.IsNull (dialogue.currentNode);
		}

		[Test()]
		public void TestGettingRawSource() {
			dialogue.LoadFile ("Example.json");

			dialogue.Compile ();

			var source = dialogue.GetTextForNode ("LearnMore");

			Assert.IsNotNull (source);

			Assert.AreEqual (source, "A: HAHAHA");
		}

		private void HandleResult(Yarn.Dialogue.RunnerResult result) {

			Console.WriteLine (result);

			if (result is Yarn.Dialogue.LineResult) {
				var text = (result as Yarn.Dialogue.LineResult).line.text;

				if (nextExpectedLine != null) {
					Assert.AreEqual (text, nextExpectedLine);
				}
			} else if (result is Yarn.Dialogue.OptionSetResult) {
				var optionCount = (result as Yarn.Dialogue.OptionSetResult).options.options.Count;
				var resultDelegate = (result as Yarn.Dialogue.OptionSetResult).setSelectedOptionDelegate;

				if (nextExpectedOptionCount != -1) {
					Assert.AreEqual (nextExpectedOptionCount, optionCount);
				}

				if (nextOptionToSelect != -1) {
					resultDelegate (nextOptionToSelect);
				}
			} else if (result is Yarn.Dialogue.CommandResult) {
				var commandText = (result as Yarn.Dialogue.CommandResult).command.text;

				if (nextExpectedCommand != null) {
					Assert.AreEqual (nextExpectedCommand, commandText);
				}
			}

			Console.WriteLine (result.ToString ());

			nextExpectedLine = null;
			nextExpectedCommand = null;
			nextExpectedOptionCount = -1;
			nextOptionToSelect = -1;

		}
	}

}

