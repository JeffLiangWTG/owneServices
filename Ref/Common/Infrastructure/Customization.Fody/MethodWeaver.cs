using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CargoWise.RefDbRepo.Common.Customization.Fody
{
	public partial class ModuleWeaver
	{
		void Initialize()
		{
			_replaceMethodProvider = new ReplaceMethodProvider();

			_opCodeBoundaryList = new OpCode[] { OpCodes.Nop, OpCodes.Beq_S, OpCodes.Bge_S, OpCodes.Bge_Un_S, OpCodes.Bgt_S, OpCodes.Bgt_Un_S, OpCodes.Ble_S, OpCodes.Ble_Un_S, OpCodes.Blt_S, OpCodes.Blt_Un_S, OpCodes.Brfalse_S, OpCodes.Brtrue_S, OpCodes.Br_S, OpCodes.Leave_S,
				OpCodes.Beq, OpCodes.Bge, OpCodes.Bge_Un, OpCodes.Bgt, OpCodes.Bgt_Un, OpCodes.Ble, OpCodes.Ble_Un, OpCodes.Blt, OpCodes.Blt_Un, OpCodes.Brfalse, OpCodes.Brtrue, OpCodes.Br, OpCodes.Leave };
		}

		public void Process(MethodDefinition method)
		{
			Initialize();

			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineOperand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteLineMethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArg0Operand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteLineArg0MethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArg0Arg1Operand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteLineArg0Arg1MethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArg0Arg1Arg2Operand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteLineArg0Arg1Arg2MethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArgOperand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteLineArgsMethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineObjectOperand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteLineObjectMethodReplaceAddIn"), TextWriterGetErrorOperand);

			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteOperand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteMethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArg0Operand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteArg0MethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArg0Arg1Operand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteArg0Arg1MethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArg0Arg1Arg2Operand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteArg0Arg1Arg2MethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArgOperand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteArgsMethodReplaceAddIn"), TextWriterGetErrorOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteObjectOperand, _replaceMethodProvider.GetMethodInfo("TextWriterErrorWriteObjectMethodReplaceAddIn"), TextWriterGetErrorOperand);

			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineOperand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteLineMethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArg0Operand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteLineArg0MethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArg0Arg1Operand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteLineArg0Arg1MethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArg0Arg1Arg2Operand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteLineArg0Arg1Arg2MethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteLineArgOperand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteLineArgsMethodReplaceAddIn"), TextWriterGetOutOperand);

			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteOperand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteMethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArg0Operand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteArg0MethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArg0Arg1Operand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteArg0Arg1MethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArg0Arg1Arg2Operand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteArg0Arg1Arg2MethodReplaceAddIn"), TextWriterGetOutOperand);
			ReplaceInstructions(method, OpCodes.Callvirt, TextWriterWriteArgOperand, _replaceMethodProvider.GetMethodInfo("TextWriterOutWriteArgsMethodReplaceAddIn"), TextWriterGetOutOperand);

			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteLineOperand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteLineMethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteLineArg0Operand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteLineArg0MethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteLineArg0Arg1Operand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteLineArg0Arg1MethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteLineArg0Arg1Arg2Operand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteLineArg0Arg1Arg2MethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteLineArgOperand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteLineArgsMethodReplaceAddIn"));

			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteOperand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteMethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteArg0Operand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteArg0MethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteArg0Arg1Operand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteArg0Arg1MethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteArg0Arg1Arg2Operand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteArg0Arg1Arg2MethodReplaceAddIn"));
			ReplaceInstructions(method, OpCodes.Call, ConsoleWriteArgOperand, _replaceMethodProvider.GetMethodInfo("ConsoleWriteArgsMethodReplaceAddIn"));

			ReplaceInstructions(method, OpCodes.Callvirt, NameValueCollectionGetOperand, _replaceMethodProvider.GetMethodInfo("NameValueCollectionGetMethodReplaceAddIn"), ConfigurationManagerOperand);

			ReplaceConfigurationBuilder(method);
		}

		void ReplaceInstructions(MethodDefinition method, OpCode opCode, string operandString, MethodBase replacingMethod, string callGetMethodName = null)
		{
			var processor = method.Body?.GetILProcessor();
			if (processor == null)
			{
				return;
			}

			var originalInstructions = method.Body?.Instructions?.Where(x => x.OpCode == opCode && x.Operand.ToString() == operandString);
			if (originalInstructions != null)
			{
				foreach (var instruction in originalInstructions.ToArray())
				{
					if (instruction != null)
					{
						if (string.IsNullOrEmpty(callGetMethodName))
						{
							processor.Replace(instruction, Instruction.Create(opCode, method.Module.ImportReference(replacingMethod)));
						}
						else
						{
							var hasGetter = false;
							var prevInstruction = instruction.Previous;
							while (prevInstruction != null && !_opCodeBoundaryList.Contains(prevInstruction.OpCode))
							{
								if (prevInstruction?.OpCode == OpCodes.Call && prevInstruction?.Operand?.ToString() == callGetMethodName)
								{
									hasGetter = true;
									break;
								}
								prevInstruction = prevInstruction.Previous;
							}
							if (hasGetter)
							{
								processor.Replace(instruction, Instruction.Create(OpCodes.Callvirt, method.Module.ImportReference(replacingMethod)));
							}
						}
					}
				}
			}
		}

		void ReplaceConfigurationBuilder(MethodDefinition method)
		{
			ReplaceVariableType(method, typeof(ConfigurationBuilder), typeof(IConfigurationBuilder));
			ReplaceInstructions(method, OpCodes.Newobj, ConfigurationBuilderCtorOperand, _replaceMethodProvider.RepoConfigurationBuilderCtor);
			ReplaceInstructions(method, OpCodes.Callvirt, ConfigurationBuilderBuildOperand, _replaceMethodProvider.IConfigurationBuilderBuildMethod);
		}

		void ReplaceVariableType(MethodDefinition method, Type variableTypeToReplace, Type newVariableType)
		{
			var variableDefinitions = method.Body?.Variables?.Where(v => v.VariableType.FullName == variableTypeToReplace.FullName);
			if (variableDefinitions != null)
			{
				foreach (var definition in variableDefinitions)
				{
					definition.VariableType = method.Module.ImportReference(newVariableType);
				}
			}
		}

		ReplaceMethodProvider _replaceMethodProvider;

		static OpCode[] _opCodeBoundaryList;
		const string TextWriterWriteOperand = "System.Void System.IO.TextWriter::Write(System.String)";
		const string TextWriterWriteArg0Operand = "System.Void System.IO.TextWriter::Write(System.String,System.Object)";
		const string TextWriterWriteArg0Arg1Operand = "System.Void System.IO.TextWriter::Write(System.String,System.Object,System.Object)";
		const string TextWriterWriteArg0Arg1Arg2Operand = "System.Void System.IO.TextWriter::Write(System.String,System.Object,System.Object,System.Object)";
		const string TextWriterWriteArgOperand = "System.Void System.IO.TextWriter::Write(System.String,System.Object[])";
		const string TextWriterWriteObjectOperand = "System.Void System.IO.TextWriter::Write(System.Object)";

		const string TextWriterWriteLineOperand = "System.Void System.IO.TextWriter::WriteLine(System.String)";
		const string TextWriterWriteLineArg0Operand = "System.Void System.IO.TextWriter::WriteLine(System.String,System.Object)";
		const string TextWriterWriteLineArg0Arg1Operand = "System.Void System.IO.TextWriter::WriteLine(System.String,System.Object,System.Object)";
		const string TextWriterWriteLineArg0Arg1Arg2Operand = "System.Void System.IO.TextWriter::WriteLine(System.String,System.Object,System.Object,System.Object)";
		const string TextWriterWriteLineArgOperand = "System.Void System.IO.TextWriter::WriteLine(System.String,System.Object[])";
		const string TextWriterWriteLineObjectOperand = "System.Void System.IO.TextWriter::WriteLine(System.Object)";

		const string ConsoleWriteOperand = "System.Void System.Console::Write(System.String)";
		const string ConsoleWriteArg0Operand = "System.Void System.Console::Write(System.String,System.Object)";
		const string ConsoleWriteArg0Arg1Operand = "System.Void System.Console::Write(System.String,System.Object,System.Object)";
		const string ConsoleWriteArg0Arg1Arg2Operand = "System.Void System.Console::Write(System.String,System.Object,System.Object,System.Object)";
		const string ConsoleWriteArgOperand = "System.Void System.Console::Write(System.String,System.Object[])";

		const string ConsoleWriteLineOperand = "System.Void System.Console::WriteLine(System.String)";
		const string ConsoleWriteLineArg0Operand = "System.Void System.Console::WriteLine(System.String,System.Object)";
		const string ConsoleWriteLineArg0Arg1Operand = "System.Void System.Console::WriteLine(System.String,System.Object,System.Object)";
		const string ConsoleWriteLineArg0Arg1Arg2Operand = "System.Void System.Console::WriteLine(System.String,System.Object,System.Object,System.Object)";
		const string ConsoleWriteLineArgOperand = "System.Void System.Console::WriteLine(System.String,System.Object[])";

		const string TextWriterGetErrorOperand = "System.IO.TextWriter System.Console::get_Error()";
		const string TextWriterGetOutOperand = "System.IO.TextWriter System.Console::get_Out()";

		const string NameValueCollectionGetOperand = "System.String System.Collections.Specialized.NameValueCollection::get_Item(System.String)";
		const string ConfigurationManagerOperand = "System.Collections.Specialized.NameValueCollection System.Configuration.ConfigurationManager::get_AppSettings()";
		const string ConfigurationBuilderCtorOperand = "System.Void Microsoft.Extensions.Configuration.ConfigurationBuilder::.ctor()";
		const string ConfigurationBuilderBuildOperand = "Microsoft.Extensions.Configuration.IConfigurationRoot Microsoft.Extensions.Configuration.ConfigurationBuilder::Build()";
	}
}
