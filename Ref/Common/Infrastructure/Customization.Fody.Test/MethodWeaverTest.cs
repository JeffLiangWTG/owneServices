using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Customization.Fody.Test
{
	[TestFixture]
	class WeaverTest
	{
		[TestCaseSource(nameof(CallVirtMethodWeaveTestCases))]
		public void CallVirtMethodWeave(MethodInfo callMethod, MethodInfo callvirtMethod, string textWriterOperand)
		{
			var processor = _method.Body.GetILProcessor();
			_method.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
			var current = _method.Body.Instructions.First();
			foreach (var instruction in GetCallVirtInstructions(callMethod, callvirtMethod))
			{
				processor.InsertAfter(current, instruction);
				current = instruction;
			}

			Assert.AreEqual(1, _method.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == textWriterOperand).Count());

			new ModuleWeaver().Process(_method);

			Assert.AreEqual(0, _method.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == textWriterOperand).Count());
		}
		IEnumerable<Instruction> GetCallVirtInstructions(MethodInfo callMethod, MethodInfo callvirtMethod)
		{
			yield return Instruction.Create(OpCodes.Nop);

			if (callMethod != null)
			{
				yield return Instruction.Create(OpCodes.Call, _module.ImportReference(callMethod));
			}
			yield return Instruction.Create(OpCodes.Ldstr, "TestWeaving");
			yield return Instruction.Create(OpCodes.Callvirt, _module.ImportReference(callvirtMethod));

			yield return Instruction.Create(OpCodes.Nop);
			yield return Instruction.Create(OpCodes.Ret);
		}

		static IEnumerable CallVirtMethodWeaveTestCases
		{
			get
			{
				var getErrorMethod = typeof(System.Console)
				.GetMethods()
				.Where(x => x.Name == "get_Error").First();

				var getOutMethod = typeof(System.Console)
						.GetMethods()
						.Where(x => x.Name == "get_Out").First();

				var textWriterWriteLineMethod = typeof(System.IO.TextWriter)
				.GetMethods()
				.Where(x => x.Name == nameof(System.IO.TextWriter.WriteLine))
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
						parameters[0].ParameterType == typeof(string);
				});

				var textWriterWriteMethod = typeof(System.IO.TextWriter)
				.GetMethods()
				.Where(x => x.Name == nameof(System.IO.TextWriter.Write))
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
						parameters[0].ParameterType == typeof(string);
				});

				var textWriterWriteLineObjectMethod = typeof(System.IO.TextWriter)
				.GetMethods()
				.Where(x => x.Name == nameof(System.IO.TextWriter.WriteLine))
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
						parameters[0].ParameterType == typeof(object);
				});

				var textWriterWriteObjectMethod = typeof(System.IO.TextWriter)
				.GetMethods()
				.Where(x => x.Name == nameof(System.IO.TextWriter.Write))
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
						parameters[0].ParameterType == typeof(object);
				});

				var getAppSettingsMethod = typeof(System.Configuration.ConfigurationManager).GetMethods()
				.Where(x => x.Name == "get_AppSettings").First();

				var nameValueGetMethod = typeof(System.Collections.Specialized.NameValueCollection).GetMethods()
					.Where(x => x.Name == "get_Item")
					.Single(x =>
					{
						var parameters = x.GetParameters();
						return parameters.Length == 1 &&
							parameters[0].ParameterType == typeof(string);
					});

				yield return new TestCaseData(getErrorMethod, textWriterWriteLineMethod, TextWriterWriteLineOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_ErrorWriteLine"
				};

				yield return new TestCaseData(getErrorMethod, textWriterWriteMethod, TextWriterWriteOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_ErrorWrite"
				};

				yield return new TestCaseData(getErrorMethod, textWriterWriteLineObjectMethod, TextWriterWriteLineObjectOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_ErrorWriteLine_Object"
				};

				yield return new TestCaseData(getErrorMethod, textWriterWriteObjectMethod, TextWriterWriteObjectOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_ErrorWrite_Object"
				};

				yield return new TestCaseData(getOutMethod, textWriterWriteLineMethod, TextWriterWriteLineOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_OutWriteLine"
				};

				yield return new TestCaseData(getOutMethod, textWriterWriteMethod, TextWriterWriteOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_OutWrite"
				};

				yield return new TestCaseData(getAppSettingsMethod, nameValueGetMethod, NameValueCollectionGetOperand)
				{
					TestName = "CallVirtMethodWeaveTestCases_NameValueCollection_Get"
				};
			}
		}

		[TestCaseSource(nameof(CallMethodWeaveTestCases))]
		public void CallMethodWeave(MethodInfo callMethod, string textWriterOperand)
		{
			var processor = _method.Body.GetILProcessor();
			_method.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
			var current = _method.Body.Instructions.First();
			foreach (var instruction in GetCallInstructions(callMethod))
			{
				processor.InsertAfter(current, instruction);
				current = instruction;
			}

			Assert.AreEqual(1, _method.Body.Instructions.Where(x => x.OpCode == OpCodes.Call && x.Operand.ToString() == textWriterOperand).Count());

			new ModuleWeaver().Process(_method);

			Assert.AreEqual(0, _method.Body.Instructions.Where(x => x.OpCode == OpCodes.Call && x.Operand.ToString() == textWriterOperand).Count());
		}
		IEnumerable<Instruction> GetCallInstructions(MethodInfo callMethod)
		{
			yield return Instruction.Create(OpCodes.Nop);

			yield return Instruction.Create(OpCodes.Ldstr, "TestWeaving");
			yield return Instruction.Create(OpCodes.Call, _module.ImportReference(callMethod));

			yield return Instruction.Create(OpCodes.Nop);
			yield return Instruction.Create(OpCodes.Ret);
		}

		[Test]
		public void ConfigurationBuilderWeave()
		{
			var ctorOperand = "System.Void Microsoft.Extensions.Configuration.ConfigurationBuilder::.ctor()";
			var newCtorOperand = "System.Void CargoWise.RefDbRepo.Common.Customization.Fody.RepoConfigurationBuilder::.ctor()";
			var buildOperand = "Microsoft.Extensions.Configuration.IConfigurationRoot Microsoft.Extensions.Configuration.ConfigurationBuilder::Build()";
			var newBuildOperand = "Microsoft.Extensions.Configuration.IConfigurationRoot Microsoft.Extensions.Configuration.IConfigurationBuilder::Build()";
			var processor = _method.Body.GetILProcessor();
			InjectVariableInMethod(_method, typeof(ConfigurationBuilder));
			_method.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
			var current = _method.Body.Instructions.First();
			foreach (var instruction in GetConfigurationBuilderInstructions())
			{
				processor.InsertAfter(current, instruction);
				current = instruction;
			}

			Assert.True(_method.Body.Variables.Any(x => x.VariableType.Name == nameof(ConfigurationBuilder)));
			Assert.True(_method.Body.Instructions.Any(x => x.OpCode == OpCodes.Newobj && x.Operand.ToString() == ctorOperand));
			Assert.True(_method.Body.Instructions.Any(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == buildOperand));
			new ModuleWeaver().Process(_method);
			Assert.False(_method.Body.Variables.Any(x => x.VariableType.Name == nameof(ConfigurationBuilder)));
			Assert.False(_method.Body.Instructions.Any(x => x.OpCode == OpCodes.Newobj && x.Operand.ToString() == ctorOperand));
			Assert.True(_method.Body.Instructions.Any(x => x.OpCode == OpCodes.Newobj && x.Operand.ToString() == newCtorOperand));
			Assert.False(_method.Body.Instructions.Any(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == buildOperand));
			Assert.True(_method.Body.Instructions.Any(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == newBuildOperand));
		}
		void InjectVariableInMethod(MethodDefinition method, Type newVariableType)
		{
			var newVariable = new VariableDefinition(method.Module.ImportReference(newVariableType));
			method.Body.Variables.Add(newVariable);
		}

		IEnumerable<Instruction> GetConfigurationBuilderInstructions()
		{
			var ctorMethod = typeof(ConfigurationBuilder).GetConstructors().FirstOrDefault();
			yield return Instruction.Create(OpCodes.Nop);
			yield return Instruction.Create(OpCodes.Newobj, _module.ImportReference(ctorMethod));
			yield return Instruction.Create(OpCodes.Stloc_0);
			yield return Instruction.Create(OpCodes.Nop);
			yield return Instruction.Create(OpCodes.Callvirt, _module.ImportReference(typeof(ConfigurationBuilder).GetMethod("Build")));
			yield return Instruction.Create(OpCodes.Stloc_1);
		}

		static IEnumerable CallMethodWeaveTestCases
		{
			get
			{
				var consoleWriteLineMethod = typeof(System.Console)
				.GetMethods()
				.Where(x => x.Name == nameof(System.Console.WriteLine))
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
						parameters[0].ParameterType == typeof(string);
				});

				var consoleWriteMethod = typeof(System.Console)
				.GetMethods()
				.Where(x => x.Name == nameof(System.Console.Write))
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
						parameters[0].ParameterType == typeof(string);
				});

				yield return new TestCaseData(consoleWriteLineMethod, ConsoleWriteLineOperand)
				{
					TestName = "CallMethodWeaveTestCases_ConsoleWriteLine"
				};

				yield return new TestCaseData(consoleWriteMethod, ConsoleWriteOperand)
				{
					TestName = "CallMethodWeaveTestCases_ConsoleWrite"
				};
			}
		}

		[Test]
		public void TestRelatedAssembliesAreReplaced()
		{
			var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..");
			var baseStagingPath = Path.Combine(basePath, @"Staging\net8.0");
			var baseProducerPath = Path.Combine(basePath, @"UniversalXMLProducers\net8.0");

			CheckAssemblies(GetRelatedAssemblyNames(baseProducerPath, "CargoWise.RefDbRepo.UniversalXMLProducers.").ToArray(), baseProducerPath);
			CheckAssemblies(GetRelatedAssemblyNames(baseProducerPath, "ReferenceData.").ToArray(), baseProducerPath);
			CheckAssemblies(uxmlProcessorAssemblyNames, baseStagingPath);
		}

		static List<string> GetRelatedAssemblyNames(string rootPath, string containedName)
		{
			List<string> names = new List<string>();
			DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
			FileInfo[] files = directoryInfo.GetFiles();

			foreach (var file in files)
			{
				if (!file.Name.Contains("Test") && file.Name.Contains(containedName) && file.Extension.ToUpperInvariant() == ".DLL")
				{
					names.Add(file.Name);
				}
			}
			return names;
		}

		void CheckAssemblies(string[] assemblyNames, string path)
		{
			foreach (var name in assemblyNames)
			{
				var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(path, name));
				foreach (var type in assembly.MainModule.Types)
				{
					foreach (var method in type.Methods)
					{
						var body = method.Body;
						if (body != null)
						{
							foreach (var instruction in body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt && (x.Operand.ToString() == TextWriterWriteOperand || x.Operand.ToString() == TextWriterWriteLineOperand)))
							{
								var notReplaced = false;
								var prevInstruction = instruction.Previous;
								while (prevInstruction != null && !OpCodeBoundaryList.Contains(prevInstruction.OpCode))
								{
									var operand = prevInstruction.Operand?.ToString();
									if (prevInstruction.OpCode == OpCodes.Call && (operand == TextWriterGetErrorOperand || operand == TextWriterGetOutOperand))
									{
										notReplaced = true;
										break;
									}
									prevInstruction = prevInstruction.Previous;
								}
								Assert.IsTrue(!notReplaced);
							}
						}
					}
				}
			}
		}

		[TestCaseSource(nameof(ReplaceMethodsTestCases))]
		public void TestConsoleWriteMethodsAreReplaced(string textWriterOperand)
		{
			var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..");
			var baseStagingPath = Path.Combine(basePath, @"Staging\net8.0");
			var baseProducerPath = Path.Combine(basePath, @"UniversalXMLProducers\net8.0");

			foreach (var uxmlProducer in GetRelatedAssemblyNames(baseProducerPath, "CargoWise.RefDbRepo.UniversalXMLProducers."))
			{
				CheckConsoleWriteMethodsAreReplaced(Path.Combine(baseProducerPath, uxmlProducer), textWriterOperand);
			}

			foreach (var referenceDataProducer in GetRelatedAssemblyNames(baseProducerPath, "ReferenceData."))
			{
				CheckConsoleWriteMethodsAreReplaced(Path.Combine(baseProducerPath, referenceDataProducer), textWriterOperand);
			}

			foreach (var uxmlProcessor in uxmlProcessorAssemblyNames)
			{
				CheckConsoleWriteMethodsAreReplaced(Path.Combine(baseStagingPath, uxmlProcessor), textWriterOperand);
			}
		}

		[Test]
		public void TestConfigurationBuilderIsReplaced()
		{
			var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..");
			var baseStagingPath = Path.Combine(basePath, @"Staging\net8.0");
			var baseProducerPath = Path.Combine(basePath, @"UniversalXMLProducers\net8.0");
			var assemblyNames = GetXmlProducerEntryAssembyNames(baseProducerPath);
			Assert.Multiple(() =>
			{
				foreach (var name in assemblyNames)
				{
					var assembly=AssemblyDefinition.ReadAssembly(name);
					foreach (var module in assembly.Modules)
					{
						foreach (var type in module.Types)
						{
							foreach (var method in type.Methods)
							{
								var body = method.Body;
								if (body != null)
								{
									Assert.IsFalse(body.Instructions.Any(x => x.OpCode == OpCodes.Newobj && x.Operand.ToString() == ConfigurationBuilderCtorOperand && !methodWeavedIgnoreList.Contains(method.ToString()))
										, $@"ConfigurationBuilder should be replaced by RepoConfigurationBuilder in {method.FullName}.");
								}
							}
						}
					}
				}
			});
		}

		[Test]
		public void TestMainMethodsAreWeaved()
		{
			var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..");
			var baseStagingPath = Path.Combine(basePath, @"Staging\net8.0");
			var baseProducerPath = Path.Combine(basePath, @"UniversalXMLProducers\net8.0");
			AssertMethodIsInserted(GetXmlProducerEntryAssembyNames(baseProducerPath), HandleExceptionOperand);
			AssertMethodIsInserted(uxmlProcessorAssemblyNames.Select(x => Path.Combine(baseStagingPath, x)).ToArray(), HandleExceptionOperand);
		}

		void AssertMethodIsInserted(string[] assemblyNames, string methodOperand)
		{
			Assert.Multiple(() =>
			{
				foreach (var name in assemblyNames)
				{
					var assembly = AssemblyDefinition.ReadAssembly(name);
					foreach (var module in assembly.Modules)
					{
						foreach (var type in module.Types.Where(x => x.Name == "Program"))
						{
							foreach (var method in type.Methods.Where(x => x.Name == "Main"))
							{
								Assert.AreEqual(1, method.Body.Instructions.Count(x => x.OpCode == OpCodes.Call && x.Operand.ToString() == methodOperand),
									$"InsertMethodProvider.HandleException() should be inserted in {method.FullName}");
							}
						}
					}
				}
			});
		}

		static string[] GetXmlProducerEntryAssembyNames(string rootPath)
		{
			var files = Directory.GetFiles(rootPath, "*.exe")
				.Where(x => !x.EndsWith("chromedriver.exe") && !x.EndsWith("testhost.exe"));
			return files.Select(x => Path.ChangeExtension(x, ".dll")).ToArray();
		}

		void CheckConsoleWriteMethodsAreReplaced(string assemblyPath, string textWriterOperand)
		{
			var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
			Assert.Multiple(() =>
			{
				foreach (var module in assembly.Modules)
				{
					foreach (var type in module.Types)
					{
						foreach (var method in type.Methods)
						{
							var body = method.Body;
							if (body != null)
							{
								Assert.IsFalse(body.Instructions.Any(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == textWriterOperand && !methodWeavedIgnoreList.Contains(method.ToString()))
									, $@"{method} is not weaved, please fix it by using the following format to print message.

var errorMessage = $""This is a message with certain variable {{variable}} to print"";	please build the message string in a separate statement.
Console.Error.Write/Console.Error.WriteLine/Console.Write/Console.WriteLine(errorMessage);

Console.Error.WriteLine(exception);	//please print exception in a separate statement, instead of combine the exception with other message.
");
							}
						}
						foreach (var nestedType in type.NestedTypes)
						{
							foreach (var method in nestedType.Methods)
							{
								var body = method.Body;
								if (body != null)
								{
									Assert.IsFalse(body.Instructions.Any(x => x.OpCode == OpCodes.Callvirt && x.Operand.ToString() == textWriterOperand && !methodWeavedIgnoreList.Contains(method.ToString()))
										, $@"{method} is not weaved, please fix it by using the following format to print message.

var errorMessage = $""This is a message with certain variable {{variable}} to print"";	please build the message string in a separate statement.
Console.Error.Write/Console.Error.WriteLine/Console.Write/Console.WriteLine(errorMessage);

Console.Error.WriteLine(exception);	//please print exception in a separate statement, instead of combine the exception with other message.
");
								}
							}
						}
					}
				}
			});
		}

		readonly IEnumerable<string> methodWeavedIgnoreList = new[]
		{
			"System.String CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.ExportTariffsProducer::DownloadFile(System.String,System.String,System.String)",
			"System.Xml.Linq.XDocument CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.ManyToOneCodeListsParserXML`2::LoadAsXDocument(CargoWise.RefDbRepo.DEReferenceData.Services.DownloadResult)",
			"System.String CargoWise.RefDbRepo.FRReferenceData.Services.RITADataDownloader::GetWebPage(System.String,System.String,System.String)",
			"System.Void NDesk.Options.OptionSet::WriteOptionDescriptions(System.IO.TextWriter)",
			"System.Void NDesk.Options.OptionSet::Write(System.IO.TextWriter,System.Int32&,System.String)",
			"System.Void CargoWise.RefDbRepo.UniversalXmlFileWatcher.Utilities.FileProcessor::DeleteOverdueFiles()",
			"System.Void CargoWise.RefDbRepo.UniversalXmlParser.Utilities.FileTrace/<>c__DisplayClass4_0::<SaveLastSuccessLineNumberAsync>b__0()"
		};

		static IEnumerable ReplaceMethodsTestCases
		{
			get
			{
				yield return new TestCaseData(TextWriterWriteLineOperand);
				yield return new TestCaseData(TextWriterWriteOperand);
				yield return new TestCaseData(TextWriterWriteLineObjectOperand);
				yield return new TestCaseData(TextWriterWriteObjectOperand);
				yield return new TestCaseData(ConsoleWriteLineOperand);
				yield return new TestCaseData(ConsoleWriteOperand);
			}
		}

		[SetUp]
		public void Setup()
		{
			_module = ModuleDefinition.CreateModule("test", ModuleKind.Dll);
			_method = new MethodDefinition("TestMethod", Mono.Cecil.MethodAttributes.Static, _module.TypeSystem.Void);
			_module.Types.First().Methods.Add(_method);
		}

		ModuleDefinition _module;
		MethodDefinition _method;

		readonly OpCode[] OpCodeBoundaryList = { OpCodes.Nop, OpCodes.Beq_S, OpCodes.Bge_S, OpCodes.Bge_Un_S, OpCodes.Bgt_S, OpCodes.Bgt_Un_S, OpCodes.Ble_S, OpCodes.Ble_Un_S, OpCodes.Blt_S, OpCodes.Blt_Un_S, OpCodes.Brfalse_S, OpCodes.Brtrue_S, OpCodes.Br_S, OpCodes.Leave_S,
				OpCodes.Beq, OpCodes.Bge, OpCodes.Bge_Un, OpCodes.Bgt, OpCodes.Bgt_Un, OpCodes.Ble, OpCodes.Ble_Un, OpCodes.Blt, OpCodes.Blt_Un, OpCodes.Brfalse, OpCodes.Brtrue, OpCodes.Br, OpCodes.Leave };

		const string TextWriterWriteLineOperand = "System.Void System.IO.TextWriter::WriteLine(System.String)";
		const string TextWriterWriteOperand = "System.Void System.IO.TextWriter::Write(System.String)";
		const string TextWriterWriteLineObjectOperand = "System.Void System.IO.TextWriter::WriteLine(System.Object)";
		const string TextWriterWriteObjectOperand = "System.Void System.IO.TextWriter::Write(System.Object)";
		const string ConsoleWriteLineOperand = "System.Void System.Console::WriteLine(System.String)";
		const string ConsoleWriteOperand = "System.Void System.Console::Write(System.String)";

		const string TextWriterGetErrorOperand = "System.IO.TextWriter System.Console::get_Error()";
		const string TextWriterGetOutOperand = "System.IO.TextWriter System.Console::get_Out()";

		const string NameValueCollectionGetOperand = "System.String System.Collections.Specialized.NameValueCollection::get_Item(System.String)";
		const string ConfigurationBuilderCtorOperand = "System.Void Microsoft.Extensions.Configuration.ConfigurationBuilder::.ctor()";

		const string HandleExceptionOperand = "System.Void CargoWise.RefDbRepo.Common.Customization.Fody.InsertMethodProvider::HandleException()";

		readonly string[] uxmlProcessorAssemblyNames = {
			"CargoWise.RefDbRepo.Staging.eHubMessageDownloader.dll",
			"CargoWise.RefDbRepo.Staging.DataPublishingProcessor.dll",
			"CargoWise.RefDbRepo.Staging.DataPurgingProcessor.dll",
			"CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.dll",
			"CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.dll",
			"CargoWise.RefDbRepo.SourceDataBinaryScanner.dll",
			"CargoWise.RefDbRepo.UniversalXmlFileWatcher.dll",
			"CargoWise.RefDbRepo.UniversalXmlProcessor.dll",
			"CargoWise.RefDbRepo.UniversalXmlParser.dll"
		};
	}
}
