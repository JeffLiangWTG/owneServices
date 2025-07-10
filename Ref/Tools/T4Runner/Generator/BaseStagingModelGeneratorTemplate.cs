using System;
using System.Reflection;

namespace CargoWise.RefDbRepo.T4Runner.Generator
{
	public abstract class BaseStagingModelGeneratorTemplate : BaseModelGeneratorTemplate
	{
		Assembly StagingAssembly { get; }
		
		protected BaseStagingModelGeneratorTemplate(string targetPath, string[] entities) : base(targetPath, entities)
		{
			StagingAssembly = Assembly.Load("CargoWise.RefDbRepo.Staging.Schema_New");
		}

		protected override Type GetEntityType(string entity)
		{
			return StagingAssembly.GetType($"CargoWise.RefDbRepo.Staging.Schema_New.{entity}");
		}

	}
}