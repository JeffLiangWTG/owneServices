using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	class EV1ParserTest
	{
		[Test]
		public void TestReadEV1PDF()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Tariffs\TestFiles\Input\APND_U_MAY_8_2023_Used_Vehicle_Numbers_0307023_508c.pdf");
			var list = EV1Parser.ReadEV1PDF(inputPath);
			Assert.AreEqual(303, list.Count);
			Assert.AreEqual(31, list.Where(x => x.IsMandatory).Count());
		}
	}
}
