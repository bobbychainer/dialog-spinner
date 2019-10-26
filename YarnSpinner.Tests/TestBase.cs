﻿using Xunit;
using System;
using System.Collections;
using System.Collections.Generic;
using Yarn;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Globalization;

namespace YarnSpinner.Tests
{

    public class TestBase
    {

        string nextExpectedLine = null;
        int nextExpectedOptionCount = -1;
        int nextOptionToSelect = -1;
        string nextExpectedCommand = null;


        protected VariableStorage storage = new MemoryVariableStore();
        protected Dialogue dialogue;

        protected bool errorsCauseFailures = true;

        // Returns the path that contains the test case files.

        public static string ProjectRootPath {
            get {
                var path = Assembly.GetCallingAssembly().Location.Split(Path.DirectorySeparatorChar).ToList();

                var index = path.FindIndex(x => x == "YarnSpinner.Tests");

                if (index == -1)
                {
                    throw new System.IO.DirectoryNotFoundException("Cannot find test data directory");                    
                }

                var testDataDirectory = path.Take(index).ToList();

                var pathToTestData = string.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), testDataDirectory.ToArray());

                return pathToTestData;
            }
        }


        public static string TestDataPath
        {
            get
            {
                return Path.Combine(ProjectRootPath, "Tests");
            }
        }

        public static string UnityDemoScriptsPath
        {
            get
            {
                return Path.Combine(ProjectRootPath, "Unity/Assets/YarnSpinner/Examples/DemoAssets/Space");
            }
        }


        public TestBase()
        {

            dialogue = new Dialogue (storage);

            dialogue.LogDebugMessage = delegate(string message) {

                Console.WriteLine (message);

            };

            dialogue.LogErrorMessage = delegate(string message) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine ("ERROR: " + message);
                Console.ResetColor ();

                if (errorsCauseFailures == true) {
                    Assert.NotNull(message);
                }
                    
            };

            dialogue.library.RegisterFunction ("assert", -1, delegate(Yarn.Value[] parameters) {
                if (parameters[0].AsBool == false) {
                    if( parameters.Length > 1 && parameters[1].AsBool ) {
                        Assert.NotNull ("Assertion failed: " + parameters[1].AsString);
                    } else {
                        Assert.NotNull ("Assertion failed");
                    }
                }
            });

            dialogue.library.RegisterFunction ("prepare_for_options", 2, delegate(Value[] parameters) {
                nextExpectedOptionCount = (int)parameters[0].AsNumber;
                nextOptionToSelect = (int)parameters[1].AsNumber;
            });

            dialogue.library.RegisterFunction ("expect_line", -1, delegate(Value[] parameters) {
                nextExpectedLine = parameters[0].AsString;
            });

            dialogue.library.RegisterFunction ("expect_command", -1, delegate(Value[] parameters) {
                nextExpectedCommand = parameters[0].AsString;
            });

            dialogue.lineHandler = delegate (Line line) {
                var text = line.Text;

                Console.WriteLine("Line: " + text);

                if (IsExpectingLine) {
                    Assert.Equal (text, nextExpectedLine);
                }

                nextExpectedLine = null;
            
                return false;
            };

            dialogue.optionsHandler = delegate (OptionSet optionSet) {
                var optionCount = optionSet.Options.Length;

                Console.WriteLine("Options:");
                foreach (var option in optionSet.Options) {
                    Console.WriteLine(" - " + option.Line);
                }

                if (nextExpectedOptionCount != -1) {
                    Assert.Equal (nextExpectedOptionCount, optionCount);
                }

                if (nextOptionToSelect != -1) {
                    dialogue.SetSelectedOption(nextOptionToSelect);                    
                } else {
                    dialogue.SetSelectedOption(0);                    
                }

                nextExpectedOptionCount = -1;
                nextOptionToSelect = -1;
            };

            dialogue.commandHandler = delegate (Command command) {
                Console.WriteLine("Command: " + command.Text);

                if (nextExpectedCommand != null) {
                    Assert.Equal (nextExpectedCommand, command.Text);
                }

                return false;
            };

            dialogue.nodeCompleteHandler = (string nodeName) => false;

            dialogue.dialogueCompleteHandler = () => {};

        }


        protected void RunStandardTestcase(string nodeName = "Start") {

            dialogue.SetNode(nodeName);

            do {
                dialogue.Continue();
            } while (dialogue.IsActive);
            
        }

        protected void ExpectLine(string line) {
            nextExpectedLine = line;
        }

        protected bool IsExpectingLine {
            get {
                return nextExpectedLine != null;
            }
        }

        protected string CreateTestNode(string source) {
            return $"title: Start\n---\n{source}\n===";
            
        }
    }
}

