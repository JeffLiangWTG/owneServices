using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.ForwardAir.Transforms.X12_990_2_UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardAir.Tests
{
	[TestClass]
	public class X12_990_2_UniversalEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestX12_990_2_UniversalEvent()
		{
			InitialiseTestingContexts();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "X12_990_2_UniversalEvent.TestFiles.Test1_input.xml";
			string expectedFile = "X12_990_2_UniversalEvent.TestFiles.Test1_output.xml";
			mapTester.Execute<X12_990_2_UniversalEvent>(sourceFile, expectedFile);

			sourceFile = "X12_990_2_UniversalEvent.TestFiles.Test2_input.xml";
			expectedFile = "X12_990_2_UniversalEvent.TestFiles.Test2_output.xml";
			mapTester.Execute<X12_990_2_UniversalEvent>(sourceFile, expectedFile);
		}

		private static void InitialiseTestingContexts()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "SelectSubscribedReference",
				OutputParm = "@reference",
				InputParms = new List<string> { 
					"@senderId", "FORAIRCMH", 
					"@recipientId", "", 
					"@ST_ID", "FORAWB", 
					"@value", "17804647"
				},
				Result = "CPUC00676800"
			});

			new TransformAccessor().SetCodeMapsTestingContext(ctx);
		}
	}
}
