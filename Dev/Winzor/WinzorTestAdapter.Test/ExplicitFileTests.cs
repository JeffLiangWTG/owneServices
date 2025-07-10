using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WinzorTestAdapter.Test;

public class ExplicitFileTests
{
	[Test]
	public void ExplicitFileShouldContainDefaultEntriesUnedited()
	{
		var assembly = Assembly.Load("WinzorTestAdapter");
		var resourceName = assembly.GetManifestResourceNames().Where(name => name.Equals("WinzorTestAdapter.Explicit.txt")).FirstOrDefault();
		Assert.That(resourceName, Is.Not.Null, "Explicit.txt resource not found in the assembly.");

		var explicitList = new HashSet<string>();
		using (var stream = assembly.GetManifestResourceStream(resourceName))
		using (var reader = new StreamReader(stream))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				explicitList.Add(line);
			}
		}

		Assert.That(explicitList, Does.Contain(TestClassesInfo.ExplicitTest), "ExplicitTest line not found in Explicit.txt.");
		Assert.That(explicitList, Does.Contain($"{TestClassesInfo.ExplicitWithWhiteSpaceTest}\t"), "ExplicitWithWhiteSpaceTest line with expected tab not found in Explicit.txt.");
	}
}
