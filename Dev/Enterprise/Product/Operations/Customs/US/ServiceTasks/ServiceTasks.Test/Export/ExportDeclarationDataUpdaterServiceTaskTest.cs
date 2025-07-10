using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(ExportDeclarationDataUpdaterServiceTask))]
	sealed class ExportDeclarationDataUpdaterServiceTaskTest : ServiceTaskTestCase<ExportDeclarationDataUpdaterServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestRunTaskCore()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipping = Factory.New<OrgHeader>();
				shipping.OH_Code = "SHI";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "B00001001";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_OverrideFreightDefaults = true;
				declaration.JE_IsCancelled = false;
				declaration.JE_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
				declaration.JE_RL_NKPortOfLoading = "USLAX";
				declaration.JE_MasterBill = "M000001";
				declaration.JE_OH_ShippingLine = shipping.PK;
				declaration.JE_VoyageFlightNo = "93";
				declaration.US_DateOfExport = ZDateTime.Today.AddDays(-1);
				declaration.US_RL_NKPortOfExport = "USLAX";
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
				entry.CH_EntryStatus = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
				entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
				var entryNum = Factory.New<CusEntryNumber>();
				entryNum.CE_EntryType = "ZZZ";
				entryNum.CE_EntryNum = "CE000001";
				entryNum.CE_ParentID = entry.PK;
				entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.ITN;
				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_UniqueConsignRef = "C000001";
				consol.JK_RL_NKLoadPort = "USPHL";
				consol.JK_RL_NKDischargePort = "CAMTL";
				consol.JK_MasterBillNum = "M000002";
				shipment.Consols.Add(consol);
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_Code = "CRR";
				var transport = consol.MostInterestingTransportForBinding[0];
				transport.JW_VoyageFlight = "94";
				transport.JW_Vessel = "BOSTON EXPRESS";
				transport.JW_ETD = ZDateTime.Today.AddDays(1);
				transport.JW_ETA = ZDateTime.Today.AddDays(10);
				transport.CarrierPK = carrier.PK;
				var exportTransport = consol.Transports.ExportTransport;
				exportTransport.JW_ETD = ZDateTime.Today.AddDays(11);
				exportTransport.JW_ETA = ZDateTime.Today.AddDays(15);
				Factory.Save();
				var serviceTask = new ExportDeclarationDataUpdaterServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
				AssertEquals("JE_RL_NKPortOfLoading should be updated to JK_RL_NKLoadPort", consol.JK_RL_NKLoadPort, declarationLoaded.JE_RL_NKPortOfLoading);
				AssertEquals("JE_MasterBill should be updated to JK_MasterBillNum", consol.JK_MasterBillNum, declarationLoaded.JE_MasterBill);
				AssertEquals("JE_OH_ShippingLine should be updated to carrier.PK", carrier.PK, declarationLoaded.JE_OH_ShippingLine);
				AssertEquals("JE_VoyageFlightNo should be updated to JW_VoyageFlight", transport.JW_VoyageFlight, declarationLoaded.JE_VoyageFlightNo);
				AssertEquals("US_RL_NKPortOfExport should be updated to JK_RL_NKLoadForExportTransport", consol.JK_RL_NKLoadForExportTransport, declarationLoaded.US_RL_NKPortOfExport);
				AssertEquals("US_DateOfExport should be updated to JW_ETD", exportTransport.JW_ETD, declarationLoaded.US_DateOfExport);
				AssertEquals("JE_EntryStatus should be updated to CH_EntryStatus", entry.CH_EntryStatus, declaration.JE_EntryStatus);
				AssertEquals("JE_MessageStatus should be updated to CH_Status", entry.CH_Status, declaration.JE_MessageStatus);
				AssertEquals("CH_Status should be updated to RSR", AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired, declarationLoaded.ActiveEntryHeaders[0].CH_Status);
				AssertEquals("US_ShouldBeReportToCustoms should be updated to true", true, declarationLoaded.ActiveEntryHeaders[0].US_ShouldBeReportToCustoms);
			}
		}

		public void TestCheckEDUServiceTaskRegistryEnabled()
		{
			var methodInfo = typeof(ExportDeclarationDataUpdaterServiceTask).GetMethod(nameof(ExportDeclarationDataUpdaterServiceTask.CheckEDUServiceTaskRegistryEnabled));
			Assert("ExportDeclarationDataUpdaterServiceTask is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCompanyBranchBecomeInactiveDuringProcessing() => new USServiceTaskTestCommon().AssertCompanyBranchBecomeInactiveDuringProcessing<ExportDeclarationDataUpdaterServiceTask>();

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
