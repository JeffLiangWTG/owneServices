using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	internal class AssemblyTest
	{
		static public IEnumerable<Assembly> AssembliesToTest
		{
			get
			{
				yield return Assembly.Load("CargoWise.RefDbRepo.SEReferenceData.Business");
				yield return Assembly.Load("CargoWise.RefDbRepo.SEReferenceData.CmdLine");
				yield return Assembly.Load("CargoWise.RefDbRepo.SEReferenceData.Services");
				yield return Assembly.Load("CargoWise.RefDbRepo.SEReferenceData.Tests");
			}
		}

		[Test]
		[TestCaseSource(nameof(AssembliesToTest))]
		public void TestDotNetFrameworkTarget(Assembly assembly)
		{
			var target = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
			Assert.That(target.FrameworkName, Is.EqualTo(".NETCoreApp,Version=v8.0"));
		}
	}
}
