using System;
using System.IO;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using NUnit.Framework;

namespace CargoWise.Blazor.Common.Test
{
	[SetUpFixture]
	[AssemblySetup]
	public class SetUpTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
		}
	}
}
