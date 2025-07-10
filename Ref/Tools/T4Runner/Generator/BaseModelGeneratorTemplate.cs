using System;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.T4Runner.Generator
{
	public abstract class BaseModelGeneratorTemplate
	{
		protected string TargetPath  { get; }
		
		protected string[] Entities  { get; }
		
		protected BaseModelGeneratorTemplate(string targetPath, string[] entities)
		{
			TargetPath = targetPath;
			Entities = entities;
		}

		public void Generate()
		{
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string targetActualPath = Path.Combine(currentDirectory, TargetPath);
			foreach (var entity in Entities)
			{
				Type type = GetEntityType(entity);
				if (type != null)
				{
					using (var writer = new StreamWriter($@"{targetActualPath}\{GetFileName(entity)}"))
					{
						var content = GetContent(type);
						writer.Write(content);
					}   
				}   
			}
		}
		
		protected abstract Type GetEntityType(string entity);
		
		protected abstract string GetContent(Type type);

		protected virtual string GetFileName(string entity)
		{
			return $"{entity}.cs";
		}
	}
}