using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace WinzorFramework.Test;

public class DependenciesTest
{
	[Test]
	public void TestProjectReferences()
	{
		var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WinzorFramework.dll");
		var assembly = Assembly.LoadFrom(assemblyPath);
		var references = assembly.GetReferencedAssemblies();
		foreach (var notAllowedReference in NotAllowedReferences)
		{
			Assert.That(references.All(r => !r.Name.Equals(notAllowedReference)));
		}
	}

	static readonly string[] NotAllowedReferences = { "WTG.Blazor.Controls" };
}

