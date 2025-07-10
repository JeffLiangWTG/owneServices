using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyShipmentFilterStripTest : BaseAgencyTest
	{
		public void TestReferenceNumberFilter()
		{
			NewReferenceNumber(Shipment1, "AU", "COC", "MUNDANE");
			NewReferenceNumber(Shipment2, "US", "COC", "MAGIC");
			NewReferenceNumber(Shipment3, "AU", "COC", "MAGIC");
			NewReferenceNumber(Shipment4, "AU", "COC", "INSANE");
			NewReferenceNumber(Shipment1, "AU", "ASL", "MAGIC");
			NewReferenceNumber(Shipment2, "AU", "ASL", "");
			AssertNotNull("lazy load", Shipment5);
			Factory.Save();
			Asserter.AddFieldOfInterest("AU:COC", (s) => GetValue(s, "AU", "COC"));
			Asserter.AddFieldOfInterest("US:COC", (s) => GetValue(s, "US", "COC"));
			Asserter.AddFieldOfInterest("AU:ASL", (s) => GetValue(s, "AU", "ASL"));
			ReferenceNumberFilter filter = (ReferenceNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.AdditionalReferenceNumbers];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("empty", filter, Shipment1, Shipment2, Shipment3, Shipment4, Shipment5);
			SetFilter(filter, "AU", "COC", "MAGIC");
			Asserter.AssertMatches("AU:COC:MAGIC*", filter, Shipment3);
			SetFilter(filter, "", "COC", "MAGIC");
			Asserter.AssertMatches("COC:MAGIC*", filter, Shipment2, Shipment3);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			SetFilter(filter, "", "ASL", "");
			Asserter.AssertMatches("Without ASL", filter, Shipment2, Shipment3, Shipment4, Shipment5);
			SetFilter(filter, "", "ASL", "");
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("With ASL", filter, Shipment1);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			SetFilter(filter, "", "COC", "MAGIC");
			Asserter.AssertMatches("Without COC:MAGIC", filter, Shipment1, Shipment4, Shipment5);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			SetFilter(filter, "", "COC", "ANE");
			Asserter.AssertMatches("Without COC:*ANE", filter, Shipment2, Shipment3, Shipment5);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			SetFilter(filter, "", "COC", "M");
			Asserter.AssertMatches("Without COC:M*", filter, Shipment4, Shipment5);
		}

		public void TestChargeDescription()
		{
			JobHeader header1 = new JobHeader.Loader(Shipment1).TryLoadOrCreate();
			header1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobHeader header2 = new JobHeader.Loader(Shipment2).TryLoadOrCreate();
			header2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header2, "FRT", 100).JR_Desc = "A2";
			JobHeader header3 = new JobHeader.Loader(Shipment3).TryLoadOrCreate();
			header3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header3, "FRT", 100).JR_Desc = "A3";
			JobHeader header4 = new JobHeader.Loader(Shipment4).TryLoadOrCreate();
			header4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header4, "FRT", 100).JR_Desc = "B4";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ChargeDescription];
			Asserter.AssertMatches("empty", filter, Shipment1, Shipment2, Shipment3, Shipment4);
			filter.Property = "A";
			Asserter.AssertMatches("A", filter, Shipment2, Shipment3);
			filter.Property = "A3";
			Asserter.AssertMatches("A3", filter, Shipment3);
		}

		public void TestChargeDescription_NotFilters()
		{
			var header1 = new JobHeader.Loader(Shipment1).TryLoadOrCreate();
			header1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var header2 = new JobHeader.Loader(Shipment2).TryLoadOrCreate();
			header2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header2, "FRT", 100).JR_Desc = "A2";
			var header3 = new JobHeader.Loader(Shipment3).TryLoadOrCreate();
			header3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header3, "FRT", 100).JR_Desc = "A3";
			var header4 = new JobHeader.Loader(Shipment4).TryLoadOrCreate();
			header4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header4, "FRT", 100).JR_Desc = "B4";
			Factory.Save();
			var filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ChargeDescription];
			filter.IsActive = true;
			filter.Property = "A";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("All shipments with charge codes not equal to 'A'", filter, Shipment2, Shipment3, Shipment4);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			Asserter.AssertMatches("All shipments with charge codes not containing 'A'", filter, Shipment4);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			Asserter.AssertMatches("All shipments with charge codes not starting with 'A'", filter, Shipment4);
		}

		public void TestChargeDescription_NoBlankFilters()
		{
			var filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ChargeDescription];
			Assert(!filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert(!filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
		}

		public void TestLocalSellAmount()
		{
			// Shipment1: No header.
			// Shipment2: No Charges.
			// Shipment3: Zero Freight Charge.
			// Shipment4: Non Zero Freight Charge.
			// Shipment5: Non Zero Origin Charge.
			var evilOrphan = Factory.NewJobForTesting<JobHeader>();
			evilOrphan.JH_ParentID = ZGuid.Empty;
			evilOrphan.JH_JobNum = "BOB";
			evilOrphan.JH_GB = GlbBranch.CurrentBranch.PK;
			evilOrphan.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(evilOrphan, "FRT", 500);
			if (Shipment1.Job != null)
			{
				Shipment1.Job.Delete();
			}

			JobHeader header2 = new JobHeader.Loader(Shipment2).TryLoadOrCreate();
			header2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobHeader header3 = new JobHeader.Loader(Shipment3).TryLoadOrCreate();
			header3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header3, "FRT", 0);
			JobHeader header4 = new JobHeader.Loader(Shipment4).TryLoadOrCreate();
			header4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header4, "FRT", 500m);
			AddCharge(header4, "DPCH", 100m);
			JobHeader header5 = new JobHeader.Loader(Shipment5).TryLoadOrCreate();
			header5.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AddCharge(header5, "OPCH", 500m);
			Factory.Save();
			ChargeModuleFilter filter = (ChargeModuleFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LocalSellAmount];
			Asserter.AssertMatches("empty", filter, Shipment1, Shipment2, Shipment3, Shipment4, Shipment5);
			filter.UseLowerBound = true;
			filter.UseUpperBound = true;
			filter.LowerBound = 500;
			filter.UpperBound = 600;
			Asserter.AssertMatches("500 <= x <= 601", filter, Shipment4, Shipment5);
			filter.LowerBound = 501;
			Asserter.AssertMatches("501 <= x <= 600", filter, Shipment4);
			filter.UpperBound = 599;
			Asserter.AssertMatches("501 <= x <= 599", filter);
			filter.UseLowerBound = false;
			Asserter.AssertMatches("x <= 599", filter, Shipment1, Shipment2, Shipment3, shipment5);
			filter.UseLowerBound = true;
			filter.UseUpperBound = false;
			filter.LowerBound = 1;
			Asserter.AssertMatches("1 <= x", filter, Shipment4, Shipment5);
			filter.UseUpperBound = true;
			filter.LowerBound = 0;
			filter.UpperBound = 0;
			Asserter.AssertMatches("0 <= x <= 0", filter, Shipment1, Shipment2, Shipment3);
			filter.LowerBound = 500;
			filter.UpperBound = 500;
			Asserter.AssertMatches("500 <= x <= 500", filter, Shipment5);
			filter.ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Asserter.AssertMatches("500 <= x(FRT) <= 500", filter, Shipment4);
			filter.ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Asserter.AssertMatches("500 <= x(ORG) <= 500", filter, Shipment5);
			filter.ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Asserter.AssertMatches("500 <= x(DST) <= 500", filter);
			filter.LowerBound = 100;
			Asserter.AssertMatches("100 <= x(DST) <= 500", filter, Shipment4);
		}

		public void TestTradeLaneFilter()
		{
			OrgHeader principal1 = Factory.New<OrgHeader>();
			principal1.OH_Code = "Principal1";
			principal1.OH_IsShippingProvider = true;
			principal1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			OrgHeader principal2 = Factory.New<OrgHeader>();
			principal2.OH_Code = "Principal2";
			principal2.OH_IsShippingProvider = true;
			principal2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BANOWATITEST";
			JobSailing sailing1 = CreateSailing(vessel, "111", "NLAMS", "AUBNE", ZDateTime.Empty);
			JobSailing sailing2 = CreateSailing(vessel, "222", "NLAMS", "AUMSE", ZDateTime.Empty);
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			tradeLane1.EJ_Code = "STL";
			tradeLane1.EJ_OH_RelatedOrg = principal1.PK;
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();
			tradeLane2.EJ_Code = "ATL";
			tradeLane2.EJ_OH_RelatedOrg = principal2.PK;
			JobTradeLaneVoyage tradeLaneVoyage1 = sailing1.Voyage.TradeLanes.AddNew();
			tradeLaneVoyage1.NB_EJ = tradeLane1.PK;
			tradeLaneVoyage1.NB_OH = principal1.PK;
			JobTradeLaneVoyage tradeLaneVoyage2 = sailing2.Voyage.TradeLanes.AddNew();
			tradeLaneVoyage2.NB_EJ = tradeLane2.PK;
			tradeLaneVoyage2.NB_OH = principal2.PK;
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.TradeLane];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = tradeLane1.PK;
			Asserter.AssertMatches("Trade Lane1", filter, Shipment1);
		}

		public void TestTradeLaneFilterIsNotPublishedOnWeb()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.TradeLane];
			AssertNotNull("TradeLane filter", filter);
			AssertEquals("IsPublishedOnWeb", false, filter.IsPublishedOnWeb);
		}

		public void TestBranchRelatedPortsFilter()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "C1";
			GlbBranch branch1 = company.Branches.AddNew();
			branch1.GB_Code = "B1";
			branch1.GB_RL_NKHomePort = "AUSYD";
			branch1.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUBNE";
			GlbBranch branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B2";
			branch2.GB_RL_NKHomePort = "AUPER";
			branch2.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUFRE";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "MAJAPAHITTEST";
			JobSailing sailing1 = CreateSailing(vessel, "1234", "AUCNS", "AUBNE", ZDateTime.Empty);
			JobSailing sailing2 = CreateSailing(vessel, "1234", "AUPER", "AUMEL", ZDateTime.Empty);
			JobSailing sailing3 = CreateSailing(vessel, "1234", "AUFRE", "AUSYD", ZDateTime.Empty);
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
			filter.Property = branch1.PK;
			Asserter.AssertMatches("Branch1", filter, Shipment1, Shipment3);
			filter.Property = branch2.PK;
			Asserter.AssertMatches("Branch2", filter, Shipment2, Shipment3);
		}

		public void TestBranchRelatedPortsFilterProperties()
		{
			ModuleGuidFilter branchRelatedPortsFilter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals("IsPublishedOnWeb", false, branchRelatedPortsFilter.IsPublishedOnWeb);
			AssertEquals("Visibility", FilterVisibility.Visible, branchRelatedPortsFilter.Visibility);
			AssertEquals("DefaultProperty", ZGuid.Empty, branchRelatedPortsFilter.DefaultProperty);
			AgencyShipmentFilterStripForTesting filterStrip1 = new AgencyShipmentFilterStripForTesting();
			filterStrip1.SetAllowSearchOfUnlocoOutsideLoginBranchForTest(false);
			ModuleGuidFilter filter1 = (ModuleGuidFilter)filterStrip1[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);
			AssertEquals("DefaultProperty", GlbBranch.CurrentBranch.PK, filter1.DefaultProperty);
			AgencyShipmentFilterStripForTesting filterStrip2 = new AgencyShipmentFilterStripForTesting();
			filterStrip2.SetAllowSearchOfUnlocoOutsideLoginBranchForTest(false);
			Globals.IsWeb = true;
			try
			{
				ModuleGuidFilter filter2 = (ModuleGuidFilter)filterStrip2[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
				AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				AssertEquals("DefaultProperty", ZGuid.Empty, filter2.DefaultProperty);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestValidatePrincipalFilter()
		{
			string expectedError = "Please select a principal to filter by";
			ModuleGuidFilter filter;
			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Principal];
			filter.Property = ZGuid.Empty;
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, expectedError);
			filterStrip = null;
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Principal];
			filter.Validation.ValidateProperty();
			AssertHasError(filter.PropertyInfo, expectedError);
			filter.Property = ZGuid.NewZGuid();
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, expectedError);
		}

		public void TestBaseFilter()
		{
			Shipment1.JS_IsShipping = true;
			Shipment1.JS_IsCFSRegistered = false;
			Shipment1.JS_IsForwardRegistered = false;
			Shipment1.JS_IsBooking = false;
			Shipment2.JS_IsShipping = false;
			Shipment2.JS_IsCFSRegistered = true;
			Shipment2.JS_IsForwardRegistered = false;
			Shipment2.JS_IsBooking = false;
			Shipment3.JS_IsShipping = false;
			Shipment3.JS_IsCFSRegistered = false;
			Shipment3.JS_IsForwardRegistered = true;
			Shipment3.JS_IsBooking = false;
			Shipment4.JS_IsShipping = false;
			Shipment4.JS_IsCFSRegistered = false;
			Shipment4.JS_IsForwardRegistered = false;
			Shipment4.JS_IsBooking = true;
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_IsShipping);
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_IsCFSRegistered);
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_IsForwardRegistered);
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_IsBooking);
			AgencyShipment[] shipments = Factory.Load<AgencyShipment>(FilterStrip.Filter);
			Asserter.AssertMatches("Show Only Agency Shipments", filterStrip.Filter, Shipment1);
		}

		public void TestBookingRefReleaseFilter()
		{
			Shipment1.JS_CFSReference = "Booking Ref 1";
			Shipment2.JS_CFSReference = "Booking Ref 2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_CFSReference);
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BookingRef];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = Shipment1.JS_CFSReference;
			Asserter.AssertMatches("Shipment1", filter, Shipment1);
		}

		public void TestContainerNumberFilter()
		{
			AgencyShipmentContainer container1a = Shipment1.RealContainers.AddNew();
			container1a.JC_ContainerNum = "FAKE4100011";
			AgencyShipmentContainer container1b = Shipment1.RealContainers.AddNew();
			container1b.JC_ContainerNum = "FAKE4100027";
			AgencyShipmentContainer container2a = Shipment2.RealContainers.AddNew();
			container2a.JC_ContainerNum = "FAKE4100011";
			AssertNotNull(Shipment3);
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ContainerNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
			filter.Property = "FAKE4100011";
			Asserter.AssertMatches("FAKE4100011", filter, Shipment1, Shipment2);
			filter.Property = "FAKE4100027";
			Asserter.AssertMatches("FAKE4100027", filter, Shipment1);
		}

		public void TestContainerNumberFilter_BlankAndCLShipmentContainersAreFound()
		{
			AgencyShipmentContainer container1a = Shipment1.RealContainers.AddNew();
			container1a.JC_ContainerNum = "HELLO";
			container1a.JC_ContainerMode = Constants.ContainerModes.FCL;
			Shipment2.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			AgencyShipmentContainer container2a = Shipment2.RealContainers.AddNew();
			container2a.JC_ContainerNum = "HELLO";
			container2a.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AgencyShipmentContainer container3a = Shipment3.RealContainers.AddNew();
			container3a.JC_ContainerNum = "HELLO";
			container3a.JC_ContainerMode = ZString.Empty;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ContainerNumber];
			filter.Property = "HELLO";
			Asserter.AssertMatches("Non FCL Shipment Containers should be excluded from search", filter, Shipment1, Shipment3);
		}

		public void TestGoodsItemIDFilter()
		{
			Shipment1.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var container1a = Shipment1.RealContainers.AddNew();
			container1a.JC_ContainerNum = "FAKE4100011";
			var container1b = Shipment1.RealContainers.AddNew();
			container1b.JC_ContainerNum = "FAKE4100027";
			var container2a = Shipment2.RealContainers.AddNew();
			container2a.JC_ContainerNum = "FAKE4100011";
			Shipment3.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.GoodsItemID];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
			filter.Property = "FAKE4100011";
			Asserter.AssertMatches("FAKE4100011", filter, Shipment1);
			filter.Property = "FAKE4100027";
			Asserter.AssertMatches("FAKE4100027", filter, Shipment1);
			container1b.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			Factory.Save();
			Asserter.AssertMatches("FAKE4100027", filter, Shipment1);
			container1b.JC_ContainerMode = Constants.ContainerModes.FCL;
			Factory.Save();
			Asserter.AssertMatches("FAKE4100027", filter);
		}

		public void TestOceanBillOfLadingFilter()
		{
			Shipment1.JS_HouseBill = "Shipment1";
			Shipment2.JS_HouseBill = "Shipment2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_HouseBill);
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.OceanBillOfLading];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = "Shipment1";
			Asserter.AssertMatches("Shipment1", filter, Shipment1);
		}

		public void TestShipmentNumberFilter()
		{
			Shipment1.JS_UniqueConsignRef = "V00000100";
			Shipment2.JS_UniqueConsignRef = "V00000200";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ShipmentNumber];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = "V00000100";
			Asserter.AssertMatches("Shipment1", filter, Shipment1);
		}

		public void TestShippersRefFilter()
		{
			Shipment1.JS_BookingReference = "Shipment1";
			Shipment2.JS_BookingReference = "Shipment2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_BookingReference);
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ShippersRef];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = Shipment1.JS_BookingReference;
			Asserter.AssertMatches("BookingRef", filter, Shipment1);
		}

		public void TestBookingPartyFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Shipment1.BookingPartyDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Shipment2.BookingPartyDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("BookingParty.OH_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BookingParty];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = org1.PK;
			Asserter.AssertMatches("Org1", filter, Shipment1);
		}

		public void TestBookingPartyFilterIsNotPublishedOnWeb()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BookingParty];
			AssertNotNull("BookingParty filter", filter);
			AssertEquals("IsPublishedOnWeb", false, filter.IsPublishedOnWeb);
		}

		public void TestCarrierFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Shipment1.JS_OA_BookedShippingLineAddress = org1.MainAddress.PK;
			Shipment2.JS_OA_BookedShippingLineAddress = org2.MainAddress.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("BookedShippingLine.OH_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Carrier];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = org1.PK;
			Asserter.AssertMatches("Org1", filter, Shipment1);
		}

		public void TestCarrierFilterIsNotPublishedOnWeb()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Carrier];
			AssertNotNull("Carrier filter", filter);
			AssertEquals("IsPublishedOnWeb", false, filter.IsPublishedOnWeb);
		}

		public void TestConsignorConsigneeFilter()
		{
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			Shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.MainAddress.PK;
			Shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee1.MainAddress.PK;
			DirtyAddresses(Shipment1);
			Shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor2.MainAddress.PK;
			Shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = consignee1.MainAddress.PK;
			DirtyAddresses(Shipment2);
			Shipment3.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.MainAddress.PK;
			Shipment3.ConsigneeDocumentaryAddress.E2_OA_Address = consignee2.MainAddress.PK;
			DirtyAddresses(Shipment3);
			Shipment4.ConsignorDocumentaryAddress.E2_OA_Address = consignor2.MainAddress.PK;
			Shipment4.ConsigneeDocumentaryAddress.E2_OA_Address = consignee2.MainAddress.PK;
			DirtyAddresses(Shipment4);
			Factory.Save();
			Asserter.AddFieldOfInterest("Consignor.OH_Code");
			Asserter.AddFieldOfInterest("Consignee.OH_Code");
			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ConsignorConsignee];
			filter.Property1 = consignor1.PK;
			Asserter.AssertMatches("Consignor1", filter, Shipment1, Shipment3);
			filter.Property2 = consignee1.PK;
			Asserter.AssertMatches("Consignor & Consignee", filter, Shipment1);
			filter.Property1 = ZGuid.Empty;
			Asserter.AssertMatches("Consignee", filter, Shipment1, Shipment2);
		}

		public void TestConsignorConsigneeFilterIsNotPublishedOnWeb()
		{
			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ConsignorConsignee];
			AssertNotNull("ConsignorConsignee filter", filter);
			AssertEquals("IsPublishedOnWeb", false, filter.IsPublishedOnWeb);
		}

		public void TestNotifyPartyFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Shipment1.NotifyPartyDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			DirtyAddresses(Shipment1);
			Shipment2.NotifyPartyDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			DirtyAddresses(Shipment2);
			Factory.Save();
			Asserter.AddFieldOfInterest("Principal.OH_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.NotifyParty];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = org1.PK;
			Asserter.AssertMatches("Principal", filter, Shipment1);
		}

		public void TestNotifyPartyFilterIsNotPublishedOnWeb()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.NotifyParty];
			AssertNotNull("NotifyParty filter", filter);
			AssertEquals("IsPublishedOnWeb", false, filter.IsPublishedOnWeb);
		}

		public void TestPrincipalFilter()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			Factory.Save();
			Shipment1.JS_OH_DeliveryAgent = principal1.PK;
			Shipment2.JS_OH_DeliveryAgent = principal2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Principal];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = principal1.PK;
			Asserter.AssertMatches("Principal1", filter, Shipment1);
		}

		public void TestPrincipalFilterVisibility()
		{
			AssertEquals("Precondition: AgencyPrincipalAccess", true, Env.Security.AgencyPrincipalAccess.IsAllowed);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Principal];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			try
			{
				AgencyShipmentFilterStripForTesting filterStrip1 = new AgencyShipmentFilterStripForTesting();
				ModuleGuidFilter filter1 = (ModuleGuidFilter)filterStrip1[AgencyShipmentFilterStrip.Descriptions.Principal];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);
				Globals.IsWeb = true;
				try
				{
					AgencyShipmentFilterStripForTesting filterStrip2 = new AgencyShipmentFilterStripForTesting();
					ModuleGuidFilter filter2 = (ModuleGuidFilter)filterStrip2[AgencyShipmentFilterStrip.Descriptions.Principal];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			}
		}

		public void TestPrincipalFilterIsNotPublishedOnWeb()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Principal];
			AssertNotNull("Principal filter", filter);
			AssertEquals("IsPublishedOnWeb", false, filter.IsPublishedOnWeb);
		}

		public void TestCargoTypeFilter()
		{
			Shipment1.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment2.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_PackingMode);
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.CargoType];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("FCL", filter, Shipment1);
		}

		public void TestCommodityFilter()
		{
			PackLine packLine1 = Shipment1.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = "GEN";
			PackLine packLine2 = Shipment2.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = "REF";
			CommonContainer container = Shipment3.BookedContainers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "APLB";
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.Commodity];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
			filter.Property = "PER";
			Asserter.AssertMatches("PER", filter);
			filter.Property = "REF";
			Asserter.AssertMatches("REF", filter, Shipment2);
			filter.Property = "APLB";
			Asserter.AssertMatches("APLB", filter, Shipment3);
		}

		public void TestServiceLevelFilterList()
		{
			Shipment1.JS_RS_NKServiceLevel = "STD";
			Shipment2.JS_RS_NKServiceLevel = "STD";
			Shipment3.JS_RS_NKServiceLevel = "URG";
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.ServiceLevel];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
			filter.Property = "STD";
			Asserter.AssertMatches("STD", filter, Shipment1, Shipment2);
			filter.Property = "URG";
			Asserter.AssertMatches("URG", filter, Shipment3);
		}

		public void TestVoyageVesselFilter()
		{
			JobSailing sailing1 = CreateSailing(TestVessel1, "111", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing2 = CreateSailing(TestVessel1, "222", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing3 = CreateSailing(TestVessel1, "1111", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing4 = CreateSailing(TestVessel2, "111", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing5 = CreateSailing(TestVessel2, "222", HomePort, OverseasPort, ZDateTime.Today);
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Shipment4.JS_JX = ZGuid.Empty;
			Shipment5.JS_JX = ZGuid.Empty;
			Transport transport4 = Shipment4.Transports.AddNew();
			transport4.JW_Vessel = TestVessel2.RV_FK;
			transport4.JW_VoyageFlight = "111";
			Transport transport5 = Shipment5.Transports.AddNew();
			transport5.JW_IsLinked = true;
			transport5.JW_JX = sailing5.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Sailing.JX_JV_VoyageFlight");
			Asserter.AddFieldOfInterest("Sailing.JX_JV_NKVessel");
			Asserter.AddFieldOfInterest("Transports", (s) => s.Transports.Count.ToString());
			Asserter.AddFieldOfInterest("First Voyage", (s) => s.Transports[0].JW_VoyageFlight);
			Asserter.AddFieldOfInterest("First Vessel", (s) => s.Transports[0].JW_Vessel);
			var filter = (VoyageVesselModuleFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.VoyageVessel];
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			Asserter.AssertMatches("Empty Filter", filter, Shipment1, Shipment2, Shipment3, Shipment4, Shipment5);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = "111";
			Asserter.AssertMatches("Voyage Equals", filter, Shipment1, Shipment4);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("Voyage Starts With", filter, Shipment1, Shipment3, Shipment4);
			filter.Vessel = TestVessel1.RV_Name;
			Asserter.AssertMatches("Vessel / Voyage", filter, Shipment1, Shipment3);
			filter.VoyageFlightNo = "";
			Asserter.AssertMatches("Vessel", filter, Shipment1, Shipment2, Shipment3);
			filter.VoyageFlightNo = "222";
			filter.Vessel = TestVessel2.RV_Name;
			Asserter.AssertMatches("Vessel / Voyage", filter, Shipment5);
		}

		public void TestLoadFilter()
		{
			JobSailing sailing1 = CreateSailing(TestVessel1, "111", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing2 = CreateSailing(TestVessel1, "111", AlternateHomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing3 = CreateSailing(TestVessel1, "111", OverseasPort2, OverseasPort, ZDateTime.Today);
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Sailing.JX_JA_RL_NKPortOfLoading");
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LoadDischarge];
			filter.Property1 = HomePort;
			Asserter.AssertMatches("Port", filter, Shipment1);
			filter.Property1 = HomePort.SubstringSafe(0, 2);
			Asserter.AssertMatches("Country", filter, Shipment1, Shipment2);
			filter.Property1 = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
		}

		public void TestDischargeFilter()
		{
			JobSailing sailing1 = CreateSailing(TestVessel1, "111", OverseasPort, HomePort, ZDateTime.Today);
			JobSailing sailing2 = CreateSailing(TestVessel1, "111", OverseasPort, AlternateHomePort, ZDateTime.Today);
			JobSailing sailing3 = CreateSailing(TestVessel1, "111", OverseasPort, OverseasPort2, ZDateTime.Today);
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Sailing.JX_JB_RL_NKPortOfDischarge");
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LoadDischarge];
			filter.Property2 = HomePort;
			Asserter.AssertMatches("Port", filter, Shipment1);
			filter.Property2 = HomePort.SubstringSafe(0, 2);
			Asserter.AssertMatches("Country", filter, Shipment1, Shipment2);
			filter.Property2 = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
		}

		public void TestLoadDischargeFilter()
		{
			JobSailing sailing1 = CreateSailing(TestVessel1, "000", "AUSYD", "USLAX", ZDateTime.Today);
			JobSailing sailing2 = CreateSailing(TestVessel1, "111", "CNLYG", "GRALS", ZDateTime.Today);
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			var transport1 = Shipment1.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "ACFUN";
			transport1.JW_RL_NKDiscPort = "USABE";
			var transport2 = Shipment2.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "KIFNI";
			transport2.JW_RL_NKDiscPort = "MZBRA";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LoadDischarge];
			filter.Property1 = "AUSYD";
			Asserter.AssertMatches("Load Value Filter", filter, Shipment1);
			filter.Property1 = "ACFUN";
			Asserter.AssertMatches("Load Value Filter", filter, Shipment1);
			filter.Property1 = "CNLYG";
			Asserter.AssertMatches("Load Value Filter", filter, Shipment2);
			filter.Property1 = "KIFNI";
			Asserter.AssertMatches("Load Value Filter", filter, Shipment2);
			filter.Property1 = null;
			filter.Property2 = "USLAX";
			Asserter.AssertMatches("Discharge Value Filter", filter, Shipment1);
			filter.Property2 = "USABE";
			Asserter.AssertMatches("Discharge Value Filter", filter, Shipment1);
			filter.Property2 = "GRALS";
			Asserter.AssertMatches("Discharge Value Filter", filter, Shipment2);
			filter.Property2 = "MZBRA";
			Asserter.AssertMatches("Discharge Value Filter", filter, Shipment2);
		}

		public void TestVoyageVesselFilterWithIsBlankOperator()
		{
			JobSailing sailing1 = CreateSailing(TestVessel1, "111", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing2 = CreateSailing(TestVessel1, "222", HomePort, OverseasPort, ZDateTime.Today);
			JobSailing sailing3 = CreateSailing(TestVessel2, "1111", HomePort, OverseasPort, ZDateTime.Today);
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Shipment4.JS_JX = ZGuid.Empty;
			Shipment5.JS_JX = ZGuid.Empty;
			Transport transport1 = Shipment4.Transports.AddNew();
			transport1.JW_Vessel = TestVessel2.RV_FK;
			transport1.JW_VoyageFlight = "";
			transport1.JW_IsLinked = false;
			Transport transport2 = Shipment5.Transports.AddNew();
			transport2.JW_IsLinked = false;
			Factory.Save();
			Asserter.AddFieldOfInterest("Sailing.JX_JV_VoyageFlight");
			Asserter.AddFieldOfInterest("Sailing.JX_JV_NKVessel");
			Asserter.AddFieldOfInterest("Transports", (s) => s.Transports.Count.ToString());
			Asserter.AddFieldOfInterest("First Voyage", (s) => s.Transports[0].JW_VoyageFlight);
			Asserter.AddFieldOfInterest("First Vessel", (s) => s.Transports[0].JW_Vessel);
			var filter = (VoyageVesselModuleFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.VoyageVessel];
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			Asserter.AssertMatches("Empty Filter", filter, Shipment5);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("Empty Filter", filter, Shipment1, Shipment2, Shipment3, Shipment4);
		}

		public void TestOriginFilter()
		{
			JobSailing sailing = CreateSailing(TestVessel1, "111", AlternateHomePort2, OverseasPort3, ZDateTime.Today);
			Shipment1.JS_RL_NKOrigin = HomePort;
			Shipment1.JS_RL_NKDestination = OverseasPort;
			Shipment1.JS_JX = sailing.PK;
			Shipment2.JS_RL_NKOrigin = AlternateHomePort;
			Shipment2.JS_RL_NKDestination = OverseasPort;
			Shipment2.JS_JX = sailing.PK;
			Shipment3.JS_RL_NKOrigin = OverseasPort2;
			Shipment3.JS_RL_NKDestination = OverseasPort;
			Shipment3.JS_JX = sailing.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_RL_NKOrigin);
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.OriginDestination];
			filter.Property1 = HomePort;
			Asserter.AssertMatches("Port", filter, Shipment1);
			filter.Property1 = HomePort.SubstringSafe(0, 2);
			Asserter.AssertMatches("Country", filter, Shipment1, Shipment2);
			filter.Property1 = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
		}

		public void TestDestinationFilter()
		{
			JobSailing sailing = CreateSailing(TestVessel1, "111", AlternateHomePort2, OverseasPort3, ZDateTime.Today);
			Shipment1.JS_RL_NKOrigin = OverseasPort;
			Shipment1.JS_RL_NKDestination = HomePort;
			Shipment1.JS_JX = sailing.PK;
			Shipment2.JS_RL_NKOrigin = OverseasPort;
			Shipment2.JS_RL_NKDestination = AlternateHomePort;
			Shipment2.JS_JX = sailing.PK;
			Shipment3.JS_RL_NKOrigin = OverseasPort;
			Shipment3.JS_RL_NKDestination = OverseasPort2;
			Shipment3.JS_JX = sailing.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_RL_NKDestination);
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.OriginDestination];
			filter.Property2 = HomePort;
			Asserter.AssertMatches("Port", filter, Shipment1);
			filter.Property2 = HomePort.SubstringSafe(0, 2);
			Asserter.AssertMatches("Country", filter, Shipment1, Shipment2);
			filter.Property2 = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2, Shipment3);
		}

		public void TestATA()
		{
			ZDateTime now = ZDateTime.Now;
			ExportSailing.Destination.JB_A_ARV = now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ExportSailing.PK;
			Factory.Save();
			AssertDateRangeFilter(AgencyShipmentFilterStrip.Descriptions.ATA, now, shipment);
		}

		public void TestATD()
		{
			ZDateTime now = ZDateTime.Now;
			ExportSailing.Origin.JA_A_DEP = now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ExportSailing.PK;
			Factory.Save();
			AssertDateRangeFilter(AgencyShipmentFilterStrip.Descriptions.ATD, now, shipment);
		}

		public void TestBookedDate()
		{
			ZDateTime now = ZDateTime.Now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_A_BKD = now;
			Factory.Save();
			AssertDateRangeFilter(AgencyShipmentFilterStrip.Descriptions.BookedDate, now, shipment);
		}

		public void TestETA()
		{
			ZDateTime now = ZDateTime.Now;
			ExportSailing.Destination.JB_E_ARV = now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ExportSailing.PK;
			Factory.Save();
			AssertDateRangeFilter(AgencyShipmentFilterStrip.Descriptions.ETA, now, shipment);
		}

		public void TestETD()
		{
			ZDateTime now = ZDateTime.Now;
			ExportSailing.Origin.JA_E_DEP = now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ExportSailing.PK;
			Factory.Save();
			AssertDateRangeFilter(AgencyShipmentFilterStrip.Descriptions.ETD, now, shipment);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestNoChargesFilter()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, currentBranch.GB_GC));
			AccChargeCode code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));
			JobHeader headerWithoutCharges = Factory.NewJobForTesting<JobHeader>();
			headerWithoutCharges.JH_ParentID = Shipment1.PK;
			headerWithoutCharges.JH_GB = currentBranch.PK;
			headerWithoutCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerWithoutCharges.JH_JobNum = "Job1";
			JobHeader headerWithCharges = Factory.NewJobForTesting<JobHeader>();
			headerWithCharges.JH_ParentID = Shipment2.PK;
			headerWithCharges.JH_GB = currentBranch.PK;
			headerWithCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerWithCharges.JH_JobNum = "Job2";
			JobHeader otherHeaderWithCharges = Factory.NewJobForTesting<JobHeader>();
			otherHeaderWithCharges.JH_ParentID = Shipment3.PK;
			otherHeaderWithCharges.JH_GC = otherBranch.GB_GC;
			otherHeaderWithCharges.JH_GB = otherBranch.PK;
			otherHeaderWithCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			otherHeaderWithCharges.JH_JobNum = "Job3";
			JobHeader randomDetatchedHeader = Factory.NewJobForTesting<JobHeader>();
			randomDetatchedHeader.JH_ParentID = ZGuid.Empty;
			randomDetatchedHeader.JH_GB = currentBranch.PK;
			randomDetatchedHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			randomDetatchedHeader.JH_JobNum = "detatched";
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_JH = headerWithCharges.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 10m;
			JobCharge otherCharge = Factory.New<JobCharge>();
			otherCharge.JR_JH = otherHeaderWithCharges.PK;
			otherCharge.JR_AC = code.PK;
			ExceptionReporterTestListener.Instance.Clear(); // Clear Developer Exception reported because JobCharge and Job belong to different companies
			otherCharge.JR_GB = currentBranch.PK;
			otherCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			otherCharge.JR_LocalSellAmt = 0m;
			JobCharge randomCharge = Factory.New<JobCharge>();
			randomCharge.JR_JH = randomDetatchedHeader.PK;
			randomCharge.JR_AC = code.PK;
			randomCharge.JR_GB = otherBranch.PK;
			randomCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStrip["Invoiced / Charges"];
			filter["No Charges"] = false;
			Asserter.AssertMatches("Un-Ticked", filter, Shipment1, Shipment2, Shipment3);
			filter["No Charges"] = true;
			Asserter.AssertMatches("Ticked", filter, Shipment1);
		}

		public void TestSailingStatusFilter()
		{
			ZDateTime now = ZDateTime.Now;
			JobSailing currentSailing = CreateSailing(TestVessel1, "111", HomePort, OverseasPort, now.AddDays(1));
			JobSailing departedSailing = CreateSailing(TestVessel1, "111", HomePort, OverseasPort, now.AddDays(-1));
			Shipment1.JS_JX = currentSailing.PK;
			Shipment2.JS_JX = departedSailing.PK;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.SailingStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Shipment1, Shipment2);
			filter.Property = AgencyShipmentFilterStrip.SailingStatus.Current;
			Asserter.AssertMatches("Current", filter, Shipment1);
			filter.Property = AgencyShipmentFilterStrip.SailingStatus.Departed;
			Asserter.AssertMatches("Departed", filter, Shipment2);
		}

		public void TestShipmentStatusFilter()
		{
			Shipment1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			Shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			Shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment5.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_ShipmentStatus);
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
			filter.Property = "ALL";
			Asserter.AssertMatches("All", filter, Shipment1, Shipment2, Shipment3, Shipment4, Shipment5);
			filter.Property = "UCF";
			Asserter.AssertMatches("Unconfirmed", filter, Shipment1, Shipment2, Shipment3);
			filter.Property = "BOL-All";
			Asserter.AssertMatches("Bills of Lading", filter, Shipment4, Shipment5);
			filter.Property = ShipmentStatusList.Codes.Booked;
			Asserter.AssertMatches("Booked", filter, Shipment1);
		}

		public void TestEntryTypeAndNumber()
		{
			AgencyShipmentContainer container1A = Shipment1.RealContainers.AddNew();
			container1A.CustomsEntryNumberType = "CAN";
			container1A.CustomsEntryNumber = "123";
			AgencyShipmentContainer container1B = Shipment1.RealContainers.AddNew();
			container1B.CustomsEntryNumberType = "CCN";
			container1B.CustomsEntryNumber = "456";
			CusEntryNumber shipmentNumber = Shipment2.CusEntryNumbers.AddNew();
			shipmentNumber.CE_ParentID = Shipment2.PK;
			shipmentNumber.CE_ParentTable = Shipment2.TableName;
			shipmentNumber.CE_EntryType = "CAN";
			shipmentNumber.CE_EntryNum = "456";
			Shipment3.CustomsEntryNumberType = "EXDC";
			AgencyShipmentContainer container4A = Shipment4.RealContainers.AddNew();
			container4A.CustomsEntryNumberType = "EXDC";
			Factory.Save();
			EntryNumberModuleFilter filter = (EntryNumberModuleFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EntryTypeAndNumber];
			Asserter.AssertMatches("Empty Filter", filter, Shipment1, Shipment2, Shipment3, Shipment4);
			filter.EntryType = "CAN";
			Asserter.AssertMatches("CAN", filter, Shipment1, Shipment2);
			filter.EntryType = "CCN";
			Asserter.AssertMatches("CCN", filter, Shipment1);
			filter.EntryType = "";
			filter.Property = "456";
			Asserter.AssertMatches("Non-Empty Filter", filter, Shipment1, Shipment2);
			filter.EntryType = "EXDC";
			Asserter.AssertMatches("EXDC", filter, Shipment3, Shipment4);
		}

		public void TestInvoiceStatusFilter()
		{
			JobHeader job1 = new JobHeader.Loader(Shipment1).TryLoadOrCreate();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_Status = "WRK";
			JobHeader job2 = new JobHeader.Loader(Shipment2).TryLoadOrCreate();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_Status = "INV";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip["Invoice Status"];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Shipment1, Shipment2);
			filter.Property = "INV";
			Asserter.AssertMatches("INV", filter, Shipment2);
		}

		public void TestConsignorCompanyNameFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithConsignor("000", "");
			AgencyShipment shipment2 = GetShipmentWithConsignor("M00", "");
			AgencyShipment shipment3 = GetShipmentWithConsignor("0M0", "");
			AgencyShipment shipment4 = GetShipmentWithConsignor("00M", "");
			AgencyShipment shipment5 = GetShipmentWithConsignor("M", "");
			AgencyShipment shipment6 = GetShipmentWithConsignor("111", "222");
			AgencyShipment shipment7 = GetShipmentWithConsignor("111", "M22");
			AgencyShipment shipment8 = GetShipmentWithConsignor("111", "2M2");
			AgencyShipment shipment9 = GetShipmentWithConsignor("111", "22M");
			AgencyShipment shipment0 = GetShipmentWithConsignor("111", "M");
			AgencyShipment emptyShipment = GetShipment("Empty");
			Factory.Save();
			Asserter.AddFieldOfInterest("ConsignorDocumentaryAddress.E2_CompanyName");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ConsignorCompanyName];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7, shipment8, shipment9, shipment0, emptyShipment);
			filter.Property = "M";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("Contains 'M'", filter, shipment2, shipment3, shipment4, shipment5, shipment7, shipment8, shipment9, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Asserter.AssertMatches("DoesNotStartWith 'M'", filter, shipment1, shipment3, shipment4, shipment6, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("Equal 'M'", filter, shipment5, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Asserter.AssertMatches("NotContains 'M'", filter, shipment1, shipment6, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("NotEqual 'M'", filter, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("StartsWith 'M'", filter, shipment2, shipment5, shipment7, shipment0);
		}

		public void TestConsigneeCompanyNameFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithConsignee("000", "");
			AgencyShipment shipment2 = GetShipmentWithConsignee("M00", "");
			AgencyShipment shipment3 = GetShipmentWithConsignee("0M0", "");
			AgencyShipment shipment4 = GetShipmentWithConsignee("00M", "");
			AgencyShipment shipment5 = GetShipmentWithConsignee("M", "");
			AgencyShipment shipment6 = GetShipmentWithConsignee("111", "222");
			AgencyShipment shipment7 = GetShipmentWithConsignee("111", "M22");
			AgencyShipment shipment8 = GetShipmentWithConsignee("111", "2M2");
			AgencyShipment shipment9 = GetShipmentWithConsignee("111", "22M");
			AgencyShipment shipment0 = GetShipmentWithConsignee("111", "M");
			AgencyShipment emptyShipment = GetShipment("Empty");
			Factory.Save();
			Asserter.AddFieldOfInterest("ConsigneeDocumentaryAddress.E2_CompanyName");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ConsigneeCompanyName];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7, shipment8, shipment9, shipment0, emptyShipment);
			filter.Property = "M";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("Contains 'M'", filter, shipment2, shipment3, shipment4, shipment5, shipment7, shipment8, shipment9, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Asserter.AssertMatches("DoesNotStartWith 'M'", filter, shipment1, shipment3, shipment4, shipment6, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("Equal 'M'", filter, shipment5, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Asserter.AssertMatches("NotContains 'M'", filter, shipment1, shipment6, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("NotEqual 'M'", filter, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("StartsWith 'M'", filter, shipment2, shipment5, shipment7, shipment0);
		}

		public void TestLocalClientCompanyNameFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithLocalClient("000");
			AgencyShipment shipment2 = GetShipmentWithLocalClient("M00");
			AgencyShipment shipment3 = GetShipmentWithLocalClient("0M0");
			AgencyShipment shipment4 = GetShipmentWithLocalClient("00M");
			AgencyShipment shipment5 = GetShipmentWithLocalClient("M");
			AgencyShipment emptyShipment = GetShipment("Empty");
			Factory.Save();
			Asserter.AddFieldOfInterest("Job.LocalCharges.OH_FullName");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LocalClientCompanyName];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, shipment3, shipment4, shipment5, emptyShipment);
			filter.Property = "M";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("Contains 'M'", filter, shipment2, shipment3, shipment4, shipment5);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Asserter.AssertMatches("DoesNotStartWith 'M'", filter, shipment1, shipment3, shipment4, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("Equal 'M'", filter, shipment5);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Asserter.AssertMatches("NotContains 'M'", filter, shipment1, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("NotEqual 'M'", filter, shipment1, shipment2, shipment3, shipment4, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("StartsWith 'M'", filter, shipment2, shipment5);
		}

		public void TestLocalClientFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithLocalClient("111");
			AgencyShipment shipment2 = GetShipmentWithLocalClient("222");
			OrgHeader localClient = shipment1.Job.LocalCharges;
			Factory.Save();
			Asserter.AddFieldOfInterest("Job.LocalCharges.OH_FullName");
			Asserter.AddFieldOfInterest("Job.LocalCharges.PK");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LocalClient];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2);
			filter.Property = localClient.PK;
			Asserter.AssertMatches("Local Client '111'", filter, shipment1);
		}

		public void TestBookingPartyCompanyNameFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithBookingParty("000", "");
			AgencyShipment shipment2 = GetShipmentWithBookingParty("M00", "");
			AgencyShipment shipment3 = GetShipmentWithBookingParty("0M0", "");
			AgencyShipment shipment4 = GetShipmentWithBookingParty("00M", "");
			AgencyShipment shipment5 = GetShipmentWithBookingParty("M", "");
			AgencyShipment shipment6 = GetShipmentWithBookingParty("111", "222");
			AgencyShipment shipment7 = GetShipmentWithBookingParty("111", "M22");
			AgencyShipment shipment8 = GetShipmentWithBookingParty("111", "2M2");
			AgencyShipment shipment9 = GetShipmentWithBookingParty("111", "22M");
			AgencyShipment shipment0 = GetShipmentWithBookingParty("111", "M");
			AgencyShipment emptyShipment = GetShipment("Empty");
			Factory.Save();
			Asserter.AddFieldOfInterest("BookingPartyDocumentaryAddress.E2_CompanyName");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BookingPartyCompanyName];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7, shipment8, shipment9, shipment0, emptyShipment);
			filter.Property = "M";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("Contains 'M'", filter, shipment2, shipment3, shipment4, shipment5, shipment7, shipment8, shipment9, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Asserter.AssertMatches("DoesNotStartWith 'M'", filter, shipment1, shipment3, shipment4, shipment6, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("Equal 'M'", filter, shipment5, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Asserter.AssertMatches("NotContains 'M'", filter, shipment1, shipment6, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("NotEqual 'M'", filter, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("StartsWith 'M'", filter, shipment2, shipment5, shipment7, shipment0);
		}

		public void TestNotifyPartyCompanyNameFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithNotifyParty("000", "");
			AgencyShipment shipment2 = GetShipmentWithNotifyParty("M00", "");
			AgencyShipment shipment3 = GetShipmentWithNotifyParty("0M0", "");
			AgencyShipment shipment4 = GetShipmentWithNotifyParty("00M", "");
			AgencyShipment shipment5 = GetShipmentWithNotifyParty("M", "");
			AgencyShipment shipment6 = GetShipmentWithNotifyParty("111", "222");
			AgencyShipment shipment7 = GetShipmentWithNotifyParty("111", "M22");
			AgencyShipment shipment8 = GetShipmentWithNotifyParty("111", "2M2");
			AgencyShipment shipment9 = GetShipmentWithNotifyParty("111", "22M");
			AgencyShipment shipment0 = GetShipmentWithNotifyParty("111", "M");
			AgencyShipment emptyShipment = GetShipment("Empty");
			Factory.Save();
			Asserter.AddFieldOfInterest("NotifyPartyDocumentaryAddress.E2_CompanyName");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.NotifyPartyCompanyName];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7, shipment8, shipment9, shipment0, emptyShipment);
			filter.Property = "M";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("Contains 'M'", filter, shipment2, shipment3, shipment4, shipment5, shipment7, shipment8, shipment9, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Asserter.AssertMatches("DoesNotStartWith 'M'", filter, shipment1, shipment3, shipment4, shipment6, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("Equal 'M'", filter, shipment5, shipment0);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Asserter.AssertMatches("NotContains 'M'", filter, shipment1, shipment6, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("NotEqual 'M'", filter, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7, shipment8, shipment9, emptyShipment);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("StartsWith 'M'", filter, shipment2, shipment5, shipment7, shipment0);
		}

		public void TestConsignorRelatedPartiesFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithConsignor("con1", "");
			AgencyShipment shipment2 = GetShipmentWithConsignor("con2", "");
			AgencyShipment emptyShipment = GetShipment("Empty");
			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");
			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(shipment1.Consignor, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(shipment1.Consignor, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(shipment2.Consignor, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(shipment2.Consignor, party3);
			AgencyShipment newShipment1 = GetShipmentWithConsignor("consignor1", "");
			AgencyShipment newShipment2 = GetShipmentWithConsignor("consignor2", "");
			AgencyShipment newShipment3 = GetShipmentWithConsignor("consignor3", "");
			AgencyShipment newShipment4 = GetShipmentWithConsignor("consignor4", "");
			AgencyShipment newShipment5 = GetShipmentWithConsignor("consignor5", "");
			AgencyShipment newShipment6 = GetShipmentWithConsignor("consignor6", "");
			OrgHeader newParty = GetOrgHeader("newParty");
			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newShipment1.Consignor, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newShipment2.Consignor, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newShipment3.Consignor, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newShipment4.Consignor, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newShipment5.Consignor, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newShipment6.Consignor, newParty);
			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;
			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;
			Factory.Save();
			Asserter.AddFieldOfInterest("JS_UniqueConsignRef");
			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ConsignorRelatedParties];
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, emptyShipment, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, shipment1);
			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, shipment1, shipment2);
			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, shipment2);
			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);
			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newShipment1);
			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newShipment3);
			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newShipment4, newShipment5, newShipment6);
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("Mode = FCL", filter, newShipment5);
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("Mode = LCL", filter, newShipment6);
		}

		public void TestConsigneeRelatedPartiesFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithConsignee("con1", "");
			AgencyShipment shipment2 = GetShipmentWithConsignee("con2", "");
			AgencyShipment emptyShipment = GetShipment("Empty");
			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");
			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(shipment1.Consignee, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(shipment1.Consignee, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(shipment2.Consignee, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(shipment2.Consignee, party3);
			AgencyShipment newShipment1 = GetShipmentWithConsignee("consignee1", "");
			AgencyShipment newShipment2 = GetShipmentWithConsignee("consignee2", "");
			AgencyShipment newShipment3 = GetShipmentWithConsignee("consignee3", "");
			AgencyShipment newShipment4 = GetShipmentWithConsignee("consignee4", "");
			AgencyShipment newShipment5 = GetShipmentWithConsignee("consignee5", "");
			AgencyShipment newShipment6 = GetShipmentWithConsignee("consignee6", "");
			OrgHeader newParty = GetOrgHeader("newParty");
			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newShipment1.Consignee, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newShipment2.Consignee, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newShipment3.Consignee, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newShipment4.Consignee, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newShipment5.Consignee, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newShipment6.Consignee, newParty);
			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;
			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;
			Factory.Save();
			Asserter.AddFieldOfInterest("JS_UniqueConsignRef");
			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ConsigneeRelatedParties];
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, emptyShipment, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, shipment1);
			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, shipment1, shipment2);
			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, shipment2);
			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);
			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newShipment1);
			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newShipment3);
			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newShipment4, newShipment5, newShipment6);
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("Mode = FCL", filter, newShipment5);
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("Mode = LCL", filter, newShipment6);
		}

		public void TestLocalClientRelatedPartiesFilter()
		{
			AgencyShipment shipment1 = GetShipmentWithLocalClient("client1");
			AgencyShipment shipment2 = GetShipmentWithLocalClient("client2");
			AgencyShipment emptyShipment = GetShipment("Empty");
			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");
			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(shipment1.Job.LocalCharges, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(shipment1.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(shipment2.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(shipment2.Job.LocalCharges, party3);
			AgencyShipment newShipment1 = GetShipmentWithLocalClient("newClient1");
			AgencyShipment newShipment2 = GetShipmentWithLocalClient("newClient2");
			AgencyShipment newShipment3 = GetShipmentWithLocalClient("newClient3");
			AgencyShipment newShipment4 = GetShipmentWithLocalClient("newClient4");
			AgencyShipment newShipment5 = GetShipmentWithLocalClient("newClient5");
			AgencyShipment newShipment6 = GetShipmentWithLocalClient("newClient6");
			OrgHeader newParty = GetOrgHeader("newParty");
			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newShipment1.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newShipment2.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newShipment3.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newShipment4.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newShipment5.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newShipment6.Job.LocalCharges, newParty);
			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;
			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;
			Factory.Save();
			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.LocalClientRelatedParties];
			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, emptyShipment, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, shipment1);
			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, shipment1, shipment2);
			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, shipment2);
			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);
			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newShipment1);
			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);
			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newShipment3);
			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newShipment4, newShipment5, newShipment6);
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("Mode = FCL", filter, newShipment5);
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("Mode = LCL", filter, newShipment6);
		}

		#region Billing Filters
		public void TestAPInvoiceNumberFilter()
		{
			JobHeader job = new JobHeader.Loader(Shipment1).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = "00001001";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Shipment2.JS_UniqueConsignRef = "EmptyShipment";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.APInvoiceNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Shipment1, Shipment2);
			filter.Property = "00001001";
			Asserter.AssertMatches("00001001", filter, Shipment1);
			filter.Property = "00001002";
			Asserter.AssertMatches("00001002", filter);
		}

		public void TestARTransactionFilter()
		{
			JobHeader job = new JobHeader.Loader(Shipment1).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = "00000101";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Shipment2.JS_UniqueConsignRef = "EmptyShipment";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.ARTransactionNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Shipment1, Shipment2);
			filter.Property = "00000101";
			Asserter.AssertMatches("00000101", filter, Shipment1);
			filter.Property = "00000102";
			Asserter.AssertMatches("00000102", filter);
		}

		#endregion
		#region Implementation
		static string GetValue(AgencyShipment shipment, string country, string type)
		{
			foreach (CusEntryNumber number in shipment.Numbers)
			{
				if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
				{
					return number.CE_EntryNum;
				}
			}

			return null;
		}

		static void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		static CusEntryNumber NewReferenceNumber(AgencyShipment shipment, string countryCode, string type, string number)
		{
			CusEntryNumber result = shipment.Numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		JobCharge AddCharge(JobHeader header, string chargeCode, decimal localSellAmt)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, header.JH_GC);
			JobCharge charge = (JobCharge)((IBusinessObjectCollection)header["Charges"]).AddNew();
			charge.JR_AC = header.Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_LocalSellAmt = localSellAmt;
			return charge;
		}

		void DirtyAddresses(IDocAddresses addresses)
		{
			foreach (JobDocAddress address in addresses.DocAddresses)
			{
				address.HasChanges = true;
			}
		}

		void AssertDateRangeFilter(string filterName, ZDateTime date, AgencyShipment shipment)
		{
			AgencyShipment[] shipments;
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[filterName];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = date.AddDays(-1);
			shipments = Factory.Load<AgencyShipment>(filter.Query);
			AssertCollectionContains(shipment, shipments);
			filter.Property2 = date.AddDays(-1);
			shipments = Factory.Load<AgencyShipment>(filter.Query);
			AssertCollectionNotContains(shipment, shipments);
			filter.Property2 = date.AddDays(1);
			shipments = Factory.Load<AgencyShipment>(filter.Query);
			AssertCollectionContains(shipment, shipments);
			filter.Property1 = date.AddDays(1);
			shipments = Factory.Load<AgencyShipment>(filter.Query);
			AssertCollectionNotContains(shipment, shipments);
			filter.Property1 = ZDateTime.Empty;
			shipments = Factory.Load<AgencyShipment>(filter.Query);
			AssertCollectionContains(shipment, shipments);
		}

		JobSailing CreateSailing(RefVessel vessel, ZString voyage, ZString load, ZString discharge, ZDateTime eTD)
		{
			JobVoyage resultVoyage = Factory.New<JobVoyage>();
			resultVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			resultVoyage.JV_RV_NKVessel = vessel.RV_FK;
			resultVoyage.JV_VoyageFlight = voyage;
			VoyageOrigin origin = resultVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = eTD;
			VoyageDestination destination = resultVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			resultVoyage.GenerateSailings();
			return resultVoyage.Sailings[0];
		}

		OrgHeader GetOrgHeader(ZString fullName)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = fullName;
			return result;
		}

		OrgRelatedParty GetOrgRelatedParty(OrgHeader parent, OrgHeader relatedParty)
		{
			OrgRelatedParty result = Factory.NewWithValidTestData<OrgRelatedParty>();
			result.PR_OH_Parent = parent.PK;
			result.PR_OH_RelatedParty = relatedParty.PK;
			result.PR_PartyType = ZString.Empty;
			result.PR_FreightDirection = ZString.Empty;
			result.PR_FreightTransportMode = ZString.Empty;
			result.PR_FreightContainerMode = ZString.Empty;
			return result;
		}

		AgencyShipment GetShipmentWithConsignor(ZString companyName, ZString overridenName)
		{
			AgencyShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.ConsignorDocumentaryAddress;
			SetOrgHeaderAndOverriden(result, docAddress, companyName, overridenName);
			return result;
		}

		AgencyShipment GetShipmentWithConsignee(ZString companyName, ZString overridenName)
		{
			AgencyShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.ConsigneeDocumentaryAddress;
			SetOrgHeaderAndOverriden(result, docAddress, companyName, overridenName);
			return result;
		}

		AgencyShipment GetShipmentWithLocalClient(ZString companyName)
		{
			AgencyShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobHeader job = new JobHeader.Loader(result).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_LocalChargesAddr = GetOrgHeader(companyName).MainAddress.PK;
			return result;
		}

		AgencyShipment GetShipmentWithBookingParty(ZString companyName, ZString overridenName)
		{
			AgencyShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.BookingPartyDocumentaryAddress;
			SetOrgHeaderAndOverriden(result, docAddress, companyName, overridenName);
			return result;
		}

		AgencyShipment GetShipmentWithNotifyParty(ZString companyName, ZString overridenName)
		{
			AgencyShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.NotifyPartyDocumentaryAddress;
			SetOrgHeaderAndOverriden(result, docAddress, companyName, overridenName);
			return result;
		}

		void SetOrgHeaderAndOverriden(AgencyShipment shipment, JobDocAddress docAddress, ZString companyName, ZString overridenName)
		{
			docAddress.E2_OA_Address = GetOrgHeader(companyName).MainAddress.PK;
			SetDocAddressOverriden(docAddress, overridenName);
		}

		void SetDocAddressOverriden(JobDocAddress docAddress, ZString companyName)
		{
			if (!companyName.IsEmpty)
			{
				docAddress.E2_AddressOverride = ZBool.True;
				docAddress.E2_CompanyName = companyName;
			}
		}

		AgencyShipment GetShipment(ZString numberSuffix)
		{
			AgencyShipment result = Factory.NewWithValidTestData<AgencyShipment>();
			result.JS_UniqueConsignRef = "Shipment" + numberSuffix;
			Asserter.AddToScope(result);
			return result;
		}

		int shipmentNumberIndex;
		FilterStripAsserter<AgencyShipment> Asserter
		{
			get
			{
				if (asserter == null)
				{
					asserter = new FilterStripAsserter<AgencyShipment>(Factory, (s) => s.JS_UniqueConsignRef);
				}

				return asserter;
			}
		}

		FilterStripAsserter<AgencyShipment> asserter;
		AgencyShipment Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = Factory.New<AgencyShipment>();
					shipment1.JS_UniqueConsignRef = "Shipment1";
					Asserter.AddToScope(shipment1);
				}

				return shipment1;
			}
		}

		AgencyShipment shipment1;
		AgencyShipment Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = Factory.New<AgencyShipment>();
					shipment2.JS_UniqueConsignRef = "Shipment2";
					Asserter.AddToScope(shipment2);
				}

				return shipment2;
			}
		}

		AgencyShipment shipment2;
		AgencyShipment Shipment3
		{
			get
			{
				if (shipment3 == null)
				{
					shipment3 = Factory.New<AgencyShipment>();
					shipment3.JS_UniqueConsignRef = "Shipment3";
					Asserter.AddToScope(shipment3);
				}

				return shipment3;
			}
		}

		AgencyShipment shipment3;
		AgencyShipment Shipment4
		{
			get
			{
				if (shipment4 == null)
				{
					shipment4 = Factory.New<AgencyShipment>();
					shipment4.JS_UniqueConsignRef = "Shipment4";
					Asserter.AddToScope(shipment4);
				}

				return shipment4;
			}
		}

		AgencyShipment shipment4;
		AgencyShipment Shipment5
		{
			get
			{
				if (shipment5 == null)
				{
					shipment5 = Factory.New<AgencyShipment>();
					shipment5.JS_UniqueConsignRef = "Shipment5";
					Asserter.AddToScope(shipment5);
				}

				return shipment5;
			}
		}

		AgencyShipment shipment5;
		#region FilterStrip
		AgencyShipmentFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new AgencyShipmentFilterStripForTesting();
				}

				return filterStrip;
			}
		}

		AgencyShipmentFilterStrip filterStrip;
		class AgencyShipmentFilterStripForTesting : AgencyShipmentFilterStrip
		{
			protected override CodeDescriptionPairList NewShipmentStatusList()
			{
				return new AgencyShipmentStatusList(false);
			}

			protected override ZString DefaultShipmentStatusFilter
			{
				get
				{
					return "ALL";
				}
			}

			protected override ZBool AllowSearchOfUnlocoOutsideLoginBranch
			{
				get
				{
					return allowSearchOfUnlocoOutsideLoginBranch;
				}
			}

			bool allowSearchOfUnlocoOutsideLoginBranch = true;
			public void SetAllowSearchOfUnlocoOutsideLoginBranchForTest(bool allow)
			{
				allowSearchOfUnlocoOutsideLoginBranch = allow;
			}

			protected override SecurityCheckpoint JobInvoicingSecurity
			{
				get
				{
					return Env.Security.None;
				}
			}
		}
		#endregion
		#endregion
	}
}
