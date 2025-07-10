using System;
using System.Reflection;

namespace CargoWise.RefDbRepo.T4Runner.Generator
{
	public abstract class BaseSafeModelGeneratorTemplate : BaseModelGeneratorTemplate
	{ 
		Assembly SafeAssembly { get; }
		
		protected BaseSafeModelGeneratorTemplate(string targetPath, string[] entities) : base(targetPath, entities)
		{
			SafeAssembly = Assembly.Load("CargoWise.RefDbRepo.Service.Schema_0_9_New");
		}

		protected override Type GetEntityType(string entity)
		{
			return SafeAssembly.GetType($"CargoWise.RefDbRepo.Service.Schema_0_9_New.{entity}");
		}
	}
}