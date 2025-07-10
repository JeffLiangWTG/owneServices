using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.CmdLine
{
	[TestFixture]
	public class RunnerTests
	{
		[TestCase(Constants.ProgramFunctions.Tariffs, "TariffRunner")]
		[TestCase(Constants.ProgramFunctions.Carriers, "CarrierRunner")]
		[TestCase(Constants.ProgramFunctions.ExchangeRates, "ExchangeRateRunner")]
		public void CreateRunner(string function, string typeName)
		{
			var runner = ZAReferenceData.CmdLine.Runner.Create(function);
			Assert.That(runner, Is.Not.Null);
			Assert.That(runner.GetType().FullName, Is.EqualTo($"CargoWise.RefDbRepo.ZAReferenceData.CmdLine.{typeName}"));
		}

		[Test]
		public void InvalidFunction()
		{
			var ex = Assert.Throws<ArgumentException>(() => ZAReferenceData.CmdLine.Runner.Create("NotSupportedFunc"));
			Assert.That(ex.Message, Contains.Substring("Function not supported: NotSupportedFunc"));
		}
	}
}
