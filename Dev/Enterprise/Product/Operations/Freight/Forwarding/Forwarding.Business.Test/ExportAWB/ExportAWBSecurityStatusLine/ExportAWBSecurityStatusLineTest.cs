using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBSecurityStatusLineTest : TestCaseWithFactory
	{
		public void TestEAS_ApprovalExpiryDate()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();
			Assert("Blank line = expiry is read-only", line.EAS_ApprovalExpiryDateInfo.ReadOnly);

			line.EAS_RN_NKCountryCode = "AU";
			Assert("Approval category is blank = expiry is read-only", line.EAS_ApprovalExpiryDateInfo.ReadOnly);

			line.EAS_ApprovalCategory = "KC";
			Assert("KC supports Expiry Date = expiry is not read-only", !line.EAS_ApprovalExpiryDateInfo.ReadOnly);

			line.EAS_ApprovalExpiryDate = ZDate.Today.AddDays(1);
			line.EAS_ApprovalCategory = "RA";
			Assert("RA supports Expiry Date = expiry is not read-only", !line.EAS_ApprovalExpiryDateInfo.ReadOnly);
			AssertEquals("Expiry Date has not been blanked", ZDate.Today.AddDays(1), line.EAS_ApprovalExpiryDate);

			line.EAS_ApprovalCategory = "RC";
			Assert("RC doesn't support Expiry Date = expiry is read-only", line.EAS_ApprovalExpiryDateInfo.ReadOnly);
			Assert("Expiry date has been blanked", line.EAS_ApprovalExpiryDate.IsEmpty);
		}

		public void TestIsSavedByFactory()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			AssertEquals("Object w/o header behaves normally", true, line.IsSavedByFactory);
			line.Delete();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			line = awbHeader.ExportAWBSecurityStatusLines.AddNew();

			AssertEquals("Not overridden new AWB is never saved", false, line.IsSavedByFactory);
			Factory.Save();

			AssertEquals(false, line.IsInDatabase);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			line.EAS_ExemptionGround = ExemptionCodes.Codes.NuclearMaterial;

			awbHeader.Consol.JK_OverrideWaybillDefaults = true;
			awbHeader.Consol.JK_OverrideWaybillDefaults = false;
			awbHeader.ExportAWBSecurityStatusLines.RemoveAndDelete(line);
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed and repopulated", false, line.IsSavedByFactory);
			AssertEquals(true, line.IsDeleted);

			line = awbHeader.ExportAWBSecurityStatusLines.AddNew();
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, line.IsSavedByFactory);

			awbHeader.ForceSavingByFactory = true;
			Assert(line.IsEmpty);
			AssertEquals("Forced empty rate line is not saved", false, line.IsSavedByFactory);

			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			AssertEquals("Forced filled new rate line is saved", true, line.IsSavedByFactory);

			Factory.Save();
			AssertEquals(true, line.IsInDatabase);

			awbHeader.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, line.IsSavedByFactory);
		}

		public void TestShipments_Uniqueness()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();

			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			line.Shipments.Add(shipment1);
			line.Shipments.Add(shipment2);
			line.Shipments.Add(shipment1);

			AssertContainsExactElementsInAnyOrder("shipments",
				new[] { shipment1, shipment2 },
				line.Shipments);
		}

		public void TestApprovalCategoryList()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_RN_NKCountryCode = string.Empty;
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "AA", "AC", "AH", "CA", "CH", "KC", "RA", "RC", "RE" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "AA", "KC", "RA", "RC", "RE" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "KC", "RA" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "AC", "AH", "CH", "KC", "RA" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));
		}

		public void TestApprovalCategoryList_ListShouldBeForCountryPopulated_NotLoginCompany()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "KC", "RA" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));

			line.EAS_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "AC", "AH", "CH", "KC", "RA" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));
		}
	}
}
