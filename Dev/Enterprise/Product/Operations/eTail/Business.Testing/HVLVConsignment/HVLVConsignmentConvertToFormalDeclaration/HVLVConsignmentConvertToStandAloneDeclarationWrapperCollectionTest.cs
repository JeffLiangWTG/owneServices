using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection))]
	public class HVLVConsignmentConvertToStandAloneDeclarationWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection>
	{
		public void TestHVLVConsignmentConvertToStandAloneDeclarationCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				var consignments = new HVLVShipmentConsignmentCollection(shipment);

				var consignment1 = consignments.AddNew();
				consignment1.HVC_RN_NKShipperCountryCode = "US";
				consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

				var consignment2 = consignments.AddNew();
				consignment2.HVC_RN_NKShipperCountryCode = "AU";
				consignment2.HVC_RN_NKConsigneeCountryCode = "US";
				consignment2.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;

				var consignmentsToStandAloneDeclarationCollection = new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(consignments);
				AssertContainsExactElementsInAnyOrder(new List<HVLVConsignment> { consignment1, consignment2 }, consignmentsToStandAloneDeclarationCollection.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().Select(t => t.Consignment));
			}
		}

		public void TestHVLVConsignmentForStandAloneDeclarationConversionWrapperCollection_ShouldNotIncludeConsignmentsWithJobDeclarations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var consignments = new HVLVShipmentConsignmentCollection(shipment);
				var consignment1 = consignments.AddNew();
				consignment1.HVC_RN_NKShipperCountryCode = "US";
				consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
				var consignment2 = consignments.AddNew();
				consignment2.HVC_JE_ExportDeclaration = jobDeclaration.PK;
				var consignment3 = consignments.AddNew();
				consignment3.HVC_JE_ExportDeclaration = jobDeclaration.PK;

				var consignmentsConvertToStandAloneDeclaration = new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(consignments);
				AssertContainsExactElementsInAnyOrder(new List<HVLVConsignment> { consignment1 }, consignmentsConvertToStandAloneDeclaration.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().Select(t => t.Consignment));
			}
		}

		public void TestHVLVConsignmentConvertToStandAloneDeclarationCollection_ShouldNotContainConsignmentsWithClearedReleaseStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				var consignments = new HVLVShipmentConsignmentCollection(shipment);

				var consignment1 = consignments.AddNew();
				consignment1.HVC_RN_NKShipperCountryCode = "US";
				consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;

				var consignment2 = consignments.AddNew();
				consignment1.HVC_RN_NKShipperCountryCode = "US";
				consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment2.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

				var consignmentsToStandAloneDeclarationCollection = new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(consignments);

				AssertEquals("Expected one consignment in ConsignmentsToStandAloneDeclarationCollection", 1, consignmentsToStandAloneDeclarationCollection.Count);

				var consignmentToStandAloneDeclaration = consignmentsToStandAloneDeclarationCollection.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().First();
				AssertEquals("Expected consignment2 to be in ConsignmentsToStandAloneDeclarationCollection", consignment2.PK, consignmentToStandAloneDeclaration.Consignment.PK);
				AssertEquals("Expected consignment2 import release status to be HELD", HVLVReleaseStatus.Held, consignmentToStandAloneDeclaration.Consignment.HVC_ImportReleaseStatus);
			}
		}

		public void TestHVLVConsignmentConvertToStandAloneDeclarationCollection_ShouldShowCustomsClearanceStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "Ian is cool", "HLD");
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AAA", "Ian is cool (but export)", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignments = new HVLVShipmentConsignmentCollection(shipment);

				var exportConsignment = consignments.AddNew();
				AssertPreconditions("precondition", exportConsignment);
				exportConsignment.HVC_RN_NKShipperCountryCode = Core.Constants.CountryCodes.Australia;
				exportConsignment.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.NewZealand;
				exportConsignment.HVC_ExportCustomsClearanceStatus = "AAA";

				var importConsignment = consignments.AddNew();
				AssertPreconditions("precondition", importConsignment);
				importConsignment.HVC_RN_NKShipperCountryCode = Core.Constants.CountryCodes.NewZealand;
				importConsignment.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Australia;
				importConsignment.HVC_ImportCustomsClearanceStatus = "&&&";

				var exportConsignmentWrapper = new HVLVConsignmentForStandAloneDeclarationConversionWrapper(exportConsignment);
				var importConsignmentWrapper = new HVLVConsignmentForStandAloneDeclarationConversionWrapper(importConsignment);

				CombineAssertions(() =>
				{
					AssertEquals("Expected export consignment to have correct export customs clearance status", "Ian is cool (but export)", exportConsignmentWrapper.ExportCustomsClearanceStatusDescription);
					AssertEquals("Expected export consignment to have no import customs clearance status", string.Empty, exportConsignmentWrapper.ImportCustomsClearanceStatusDescription);

					AssertEquals("Expected import consignment to have correct import customs clearance status", "Ian is cool", importConsignmentWrapper.ImportCustomsClearanceStatusDescription);
					AssertEquals("Expected import consignment to have no export customs clearance status", string.Empty, importConsignmentWrapper.ExportCustomsClearanceStatusDescription);
				});
			}

			void AssertPreconditions(string message, HVLVConsignment consignment)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("precondition - Export release status is expected to be NON by default", HVLVReleaseStatus.None, consignment.HVC_ExportReleaseStatus);
					AssertEquals("precondition - Import release status is expected to be NON by default", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);
					AssertEquals("precondition - Release Status Desciption is expected to be None by default", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
					AssertEquals("precondition - Export Customs Status Description is expected to be empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);
					AssertEquals("precondition - Import Customs Status Description is expected to be empty by default", string.Empty, consignment.ImportCustomsClearanceStatusDescription);
				});
			}
		}

		protected override HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection GetCollectionToTest()
		{
			var consignments = new HVLVShipmentConsignmentCollection(Factory.New<ForwardingShipment>());
			return new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(consignments);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HVLVConsignmentForStandAloneDeclarationConversionWrapper();
		}
	}
}
