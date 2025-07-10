using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BuildTools;
using CargoWise.Common;
using Mono.Cecil;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	public abstract class AssemblyReferenceTest : TestCase
	{
		protected AssemblyReferenceTest(string solutionToCheck)
		{
			SolutionToCheck = solutionToCheck;
		}

		public void TestGivenETailAssembly_ThenItHasNoProjectReferencesInSpecifiedSolution()
		{
			var solution = BuildXml.Instance.GetAllSolutionFileNames().FirstOrDefault(s => s.Contains(SolutionToCheck));
			var eTailAssembly = GetETailAssembly();
			var eTailAssemblyName = eTailAssembly.GetName().Name;

			AssertNotNullOrEmpty($"Expected to find {SolutionToCheck} in BuildXml", solution);

			foreach (var assemblyName in BuildXml.Instance.GetAllAssembliesInSolution(solution))
			{
				var assemblyPath = GetAssemblyPath(assemblyName);

				using (var assembly = AssemblyDefinition.ReadAssembly(assemblyPath))
				{
					var references = assembly.MainModule.AssemblyReferences;

					CombineAssertions(() =>
					{
						foreach (var reference in references)
						{
							if (reference.Name == eTailAssemblyName)
							{
								Assert(string.Format("The assembly {0} references {1} assembly dll. Expected {1} not to be referenced specified dlls", assemblyName, eTailAssemblyName), false);
							}
						}
					});
				}
			}

			Assert(true);
		}

		static Assembly GetETailAssembly() => AssemblyContainingType(typeof(HVLVConsignment));

		readonly string SolutionToCheck;

		static Assembly AssemblyContainingType(Type type) => Assembly.GetAssembly(type);

		#region Implementation

		static string GetAssemblyPath(string assemblyName)
		{
			var assemblyPath = Path.Combine(AssemblyLoader.GetBinPath(), assemblyName);

			if (File.Exists(assemblyPath + ".dll"))
			{
				assemblyPath += ".dll";
			}

			return assemblyPath;
		}

		#endregion
	}
}
