using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.eTail.ServiceTasks.Testing
{
	[TestedType(typeof(HVLVDeclarationCSHLogSubscriber))]
	class HVLVDeclarationCSHLogSubscriberTest : LogSubscriberTest<HVLVDeclarationCSHLogSubscriber>
	{
		public void TestUSDeclarationReleaseStatusChanges_FeedbackToHVCImportCustomsClearanceStatus()
		{
			HVLVConsignment consignment;
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "CCC", "CSTA Code for setting HVC_ExportCustomsClearanceStatus", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USSYD";
				shipment.Consols.AddNew();

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
				consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;
				consignment.Items.AddNew();
				Factory.Save();

				var response = new ConvertToStandAloneDeclarationService().ConvertToStandAloneDeclaration(consignment);
				Assert("Pre-condotion: convertion succeed", response.ConversionSucceeded);

				Factory.Save();

				var declaration = consignment.ImportDeclaration as Customs.US.Business.JobDeclaration;
				AssertNotNull("Pre-condition: declaration is created", declaration);

				declaration.ReleaseStatus = "REL";
				Factory.Save();

				RunLogWalkerCycleForTest();

				consignment.Reload();
				AssertEquals("Import HVC_ImportCustomsClearanceStatus should be updated", "REL", consignment.HVC_ImportCustomsClearanceStatus);

				shipment.JS_RL_NKOrigin = "USSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.NewZealand;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.ReleaseStatus = "CCC";
				Factory.Save();

				RunLogWalkerCycleForTest();

				consignment.Reload();
				AssertEquals("Export HVC_ExportCustomsClearanceStatus should be updated", "CCC", consignment.HVC_ExportCustomsClearanceStatus);
			}
		}

		public void TestAUDeclarationCustomsStatusChanges_PropagateToHVCImportCustomsClearanceStatus()
		{
			HVLVConsignment consignment;
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AAA", "CSTA Code for setting HVC_ImportCustomsClearanceStatus", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "BBB", "CSTEX Code for setting HVC_ExportCustomsClearanceStatus", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.ExportCustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.Consols.AddNew();

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;
				consignment.Items.AddNew();
				Factory.Save();

				var response = new ConvertToStandAloneDeclarationService().ConvertToStandAloneDeclaration(consignment);
				Assert("Pre-condition: conversion succeed for import declaration", response.ConversionSucceeded);

				Factory.Save();

				var declaration = consignment.ImportDeclaration as Customs.AU.Declaration.Business.JobDeclaration;
				AssertNotNull("Pre-condition: import declaration is created", declaration);

				declaration.JE_ConsolidatedCargoStatus = "AAA";
				Factory.Save();

				RunLogWalkerCycleForTest();

				consignment.Reload();
				AssertEquals("Import Customs Clearance Status is updated", "AAA", consignment.HVC_ImportCustomsClearanceStatus);

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.NewZealand;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ConsolidatedCargoStatus = "BBB";
				Factory.Save();

				RunLogWalkerCycleForTest();

				consignment.Reload();
				AssertEquals("Export Customs Clearance Status is updated", "BBB", consignment.HVC_ExportCustomsClearanceStatus);
			}
		}
	}
}
