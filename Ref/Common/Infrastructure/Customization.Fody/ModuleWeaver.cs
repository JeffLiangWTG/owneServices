using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CargoWise.RefDbRepo.Common.Customization.Fody
{
	public partial class ModuleWeaver : BaseModuleWeaver
	{
		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield return "netstandard";
			yield return "mscorlib";
			yield return "System";
		}

		public override void Execute()
		{
			if (ModuleDefinition != null)
			{
				RelaceMethods(ModuleDefinition);
				InsertMethods(ModuleDefinition);
			}
		}

		void RelaceMethods(ModuleDefinition module)
		{
			foreach (var type in module.Types)
			{
				if (type != null)
				{
					foreach (var method in type.Methods)
					{
						if (method != null)
						{
							Process(method);
						}
					}
					if (type.HasNestedTypes)
					{
						foreach (var nestedType in type.NestedTypes)
						{
							foreach (var method in nestedType.Methods)
							{
								if (method != null)
								{
									Process(method);
								}
							}
						}
					}
				}
			}
		}

		void InsertMethods(ModuleDefinition module)
		{
			var programType = module.Types.FirstOrDefault(x => x.FullName.EndsWith(".Program"));
			if (programType != null)
			{
				var mainMethod = programType.Methods.FirstOrDefault(x => x.Name == "Main");
				var methodBodyFirstInstruction = mainMethod.Body.Instructions.FirstOrDefault();
				var processor = mainMethod.Body.GetILProcessor();
				processor.InsertBefore(methodBodyFirstInstruction, Instruction.Create(OpCodes.Call, mainMethod.Module.ImportReference(typeof(InsertMethodProvider).GetMethod(nameof(InsertMethodProvider.HandleException)))));
			}
		}
	}
}
