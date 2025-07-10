using System;
using CargoWise.RefDbRepo.LLIReferenceData.Business;
using CargoWise.RefDbRepo.LLIReferenceData.CmdLine;
using CargoWise.RefDbRepo.XmlProducer.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.LLIReferenceData.Tests;

[TestFixture]
class CmdLineTests
{
	[Test]
	public void EmptyArgument()
	{
		var exception = Assert.Throws<ArgumentException>(() => RunProcess([]));
		Assert.That(exception.Message, Does.Contain("No arguments entered"));
	}

	[Test]
	public void InvalidFunction()
	{
		var exception = Assert.Throws<ArgumentException>(() => RunProcess(["nothing"]));
		Assert.That(exception.Message, Does.Contain("Invalid argument entered: NOTHING"));
	}

	[Test]
	public void RunNormal()
	{
		using (VesselListProgram.SetAction(_ => { }))
		{
			var result = RunProcess([Constants.ProgramFunctions.LLIVessel]);
			Assert.That(result, Is.EqualTo((int)ProducerStatus.Success));
		}
	}

	static int RunProcess(string[] parameters)
	{
		return Program.Main(parameters);
	}
}
