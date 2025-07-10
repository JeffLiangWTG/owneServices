using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class JobDeclarationZControllerDeciderTest : TestCaseWithFactory
	{
		public void TestGetZController()
		{
			var expectResults = new Dictionary<JobDeclaration, ControllerID>();
			var shipmentExportDec = Factory.New<JobDeclaration>();
			shipmentExportDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var shipmentExport = Factory.New<ForwardingShipment>();
			shipmentExportDec.JE_JS = shipmentExport.PK;
			expectResults.Add(shipmentExportDec, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			var shipmentImportDec = Factory.New<JobDeclaration>();
			shipmentImportDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var shipmentImport = Factory.New<ForwardingShipment>();
			shipmentImportDec.JE_JS = shipmentImport.PK;
			expectResults.Add(shipmentImportDec, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			var shipmentImportByExternalBrokerDec = Factory.New<JobDeclaration>();
			shipmentImportByExternalBrokerDec.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			var shipmentImportByExternalBroker = Factory.New<ForwardingShipment>();
			shipmentImportByExternalBrokerDec.JE_JS = shipmentImportByExternalBroker.PK;
			expectResults.Add(shipmentImportByExternalBrokerDec, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			var shipmentMiscellaneousDec = Factory.New<JobDeclaration>();
			shipmentMiscellaneousDec.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			var shipmentMiscellaneous = Factory.New<ForwardingShipment>();
			shipmentMiscellaneousDec.JE_JS = shipmentMiscellaneous.PK;
			expectResults.Add(shipmentMiscellaneousDec, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			var exportDec = Factory.New<JobDeclaration>();
			exportDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			expectResults.Add(exportDec, ControllerIDs.Customs.JobDeclaration);
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			expectResults.Add(importDec, ControllerIDs.Customs.JobDeclaration);
			var importByExternalBrokerDec = Factory.New<JobDeclaration>();
			importByExternalBrokerDec.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			expectResults.Add(importByExternalBrokerDec, ControllerIDs.Customs.JobDeclaration);
			var miscellaneousDec = Factory.New<JobDeclaration>();
			miscellaneousDec.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			expectResults.Add(miscellaneousDec, ControllerIDs.Customs.JobDeclaration);
			var protestDec = Factory.New<JobDeclaration>();
			protestDec.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			expectResults.Add(protestDec, ControllerIDs.Customs.US.Protest);
			var reconDec = Factory.New<JobDeclaration>();
			reconDec.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconEntry = reconDec.CustomsEntryHeaders.AddNew();
			reconEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			expectResults.Add(reconDec, ControllerIDs.Customs.US.Recon);
			var drawbackDec = Factory.New<JobDeclaration>();
			drawbackDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			expectResults.Add(drawbackDec, ControllerIDs.Customs.US.Drawback);
			var fTZDec = Factory.New<JobDeclaration>();
			fTZDec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			expectResults.Add(fTZDec, ControllerIDs.Customs.JobDeclaration);
			Factory.Save();
			AssertNull("Null result", JobDeclarationZControllerDecider.GetZController(null));
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "A!@";
			AssertNull("A!@", JobDeclarationZControllerDecider.GetZController(dec));
			foreach (var pair in expectResults)
			{
				dec = pair.Key;
				AssertEquals(dec.JE_MessageType, pair.Value, JobDeclarationZControllerDecider.GetZController(dec).ID);
			}
		}
	}
}
