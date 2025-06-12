using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.CRO.Transforms.UniversalTransaction_2_Interfactura;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.CRO.Tests.UniversalTransaction_2_InterfacturaTest
{
	[TestClass]
	public class UniversalTransaction_2_InterfacturaTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalTransaction_2_Interfactura()
		{
			InitialiseCodeMapsTestingContext();

			var ctx = new TestingMessageContext();
			ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "InterFAC_S00001084_201401231629099870.txt");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalTransaction_2_Interfactura.TestFiles.Input1.xml";
			string expectedFile = "UniversalTransaction_2_Interfactura.TestFiles.Output1.xml";
			mapTester.Execute<UniversalTransaction_2_Interfactura>(sourceFile, expectedFile);
			Assert.AreEqual("InterFAC_S00001166J_201401231629099870.txt", ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			sourceFile = "UniversalTransaction_2_Interfactura.TestFiles.Tax_Input.xml";
			expectedFile = "UniversalTransaction_2_Interfactura.TestFiles.Tax_Output.xml";
			mapTester.Execute<UniversalTransaction_2_Interfactura>(sourceFile, expectedFile);
			Assert.AreEqual("InterFAC_SMEL0001375_201401231629099870.txt", ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			sourceFile = "UniversalTransaction_2_Interfactura.TestFiles.Currency_Input.xml";
			expectedFile = "UniversalTransaction_2_Interfactura.TestFiles.Currency_Output.xml";
			mapTester.Execute<UniversalTransaction_2_Interfactura>(sourceFile, expectedFile);
			Assert.AreEqual("InterFAC_INV00001370_201401231629099870.txt", ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			sourceFile = "UniversalTransaction_2_Interfactura.TestFiles.CRD_Input.xml";
			expectedFile = "UniversalTransaction_2_Interfactura.TestFiles.CRD_Output.xml";
			mapTester.Execute<UniversalTransaction_2_Interfactura>(sourceFile, expectedFile);
			Assert.AreEqual("InterFAC_S00001090B_201401231629099870.txt", ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "CROMTYMTY" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "CROMTYMTY_INT" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Interfactura xml-file - Export A/R Invoices", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Payment Method" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NO IDENTIFICADO" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Tax Regime" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Regimen General de Ley de Personas Morales" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Account Number" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NO IDENTIFICADO" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Payment Condition" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PAGO EN UNA SOLA EXHIBICION" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Document Processing Site" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "RÍO MANZANARES 308-A OTE COL. FUENTES DEL VALLE NUEVO LEÓN MONTERREY 66220 MEXICO" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Payment Terms" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Payment Terms:" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Unit of Measurement" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KGS" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "VAT%" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "16" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "No VAT%" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Invoice Terms", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Text" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "COD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Cash on Delivery" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "INV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Days" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Transaction Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "TipoDocumento" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "INV" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Factura" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "CRD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Credito" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> { 
			        "@code", "DE"
			    },
				Result = ""
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> { 
			        "@code", "AU"
			    },
				Result = "Australia"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> { 
			        "@code", "JP"
			    },
				Result = "Japan"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> { 
			        "@code", "MX"
			    },
				Result = "Mexico"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> { 
			        "@code", ""
			    },
				Result = ""
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
