using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	public class DBKTariffProgramTest
	{
		[Test]
		public void TestDBKTariffProgram()
		{
			AssertDBKTariffLog(new string[] { "DBKTARIFF" }, "Please enter the instruction in the following format:\tDBKTARIFF \"PDF File Path\"");
			var wrongPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Input\wrong.pdf");
			AssertDBKTariffLog(new string[] { "DBKTARIFF", wrongPath }, "The file does not exist.");
			var txtPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Input\empty.txt");
			AssertDBKTariffLog(new string[] { "DBKTARIFF", txtPath }, "The file is not a PDF.");
		}

		void AssertDBKTariffLog(string[] args, string message)
		{
			using (var stringWriter = new StringWriter())
			{
				Console.SetOut(stringWriter);
				DBKTariffProgram.Run(args);
				var output = stringWriter.ToString();
				StringAssert.Contains(message, output);
			}
		}
	}
}
