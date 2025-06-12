using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.AsycudaCustoms.Transform.AsySAD_2_GMI;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	[TestClass]
	public class AsySAD_2_GMITests
	{
		string path;
		MapTester mapTester;

		void doTest( string sourceFile, string expectedFile )
		{
			InitialiseTestingMessageContext();
			mapTester.ExecuteCompiled<AsySAD2GMI>(path + sourceFile, path + expectedFile);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAsySAD_2_GMI()
		{
			InitialiseCodeMapsTestingContext();
			mapTester = new MapTester(Assembly.GetExecutingAssembly());
			path = "AsySAD_2_GMI.TestFiles.";

			doTest("01_Tiny_Asycuda_Input.xml", "01_Tiny_Asycuda_Output.xml");
			doTest("02_Full_Asycuda_Input.xml", "02_Full_Asycuda_Output.xml");
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			var ctx = new CodeMapsTestingContext();
			string TS_Name = "Asycuda Ushipment2AsycudaSAD";

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCounterInterfaceValue",
				OutputParm = "@StartValue",
				InputParms = new List<string> { 
							"@TransformatonSetName", TS_Name,
							"@Name", "AsycudaGmiMsgNo",
							"@MaxValue", "999999999",
							"@IncrementValue", "1"
					},
				Result = "51"
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		private static void InitialiseTestingMessageContext()
		{
			var messageContext = new TestingMessageContext();
			messageContext.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "EDINACDAT");
			messageContext.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "ASYCUDADECLARATION");

			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(messageContext);
		}
	}
}