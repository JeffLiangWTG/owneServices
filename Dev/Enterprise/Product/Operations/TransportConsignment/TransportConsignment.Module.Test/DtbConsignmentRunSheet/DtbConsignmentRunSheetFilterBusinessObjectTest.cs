using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using FilterConstants = Enterprise.TransportConsignment.Module.DtbConsignmentRunSheetFilterBusinessObject.FilterConstants;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetFilterBusinessObject))]
	public class DtbConsignmentRunSheetFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Test Text Filters Max Length

		public void TestTextFiltersMaxLength()
		{
			CombineAssertions(() =>
			{
				var filterBizO = GetNewFilterStripBusinessObject();
				TextFilterNameAndMaxLengthDictionary.ForEach(pair => AssertEquals($"The max length of filter {pair.Key} should be set as {pair.Value}.", Math.Min(pair.Value, ModuleFilter.MaxMaximumLength), filterBizO[pair.Key].MaxLength));
			});
		}

		IDictionary<string, int> TextFilterNameAndMaxLengthDictionary => new Dictionary<string, int>
		{
			{ FilterConstants.DriverLicense, Math.Min(GenRegCertAccredMaintListSchema.XZ_RefNumber.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocDriversLicence.MaxLength) },
			{ FilterConstants.DriverName, Math.Min(GlbStaffSchema.GS_FullName.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocDriversName.MaxLength) },
			{ FilterConstants.TransportCompanyName, Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocTransportCoName.MaxLength) },
			{ FilterConstants.VehicleRegistration, Math.Min(RefEquipmentSchema.RQ_Registration.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocTruckRegistration.MaxLength) },
			{ FilterConstants.RunSheetNumber, ModuleNumberFilter.MultiplyMaxLength(DtbConsignmentRunSheetSchema.KG_RunSheetNumber.MaxLength) }
		};

		#endregion

		#region Test Numbers And References Filters

		#region TestRunSheetNumber

		public void TestRunSheetNumber()
		{
			var runSheetCRS1 = Helper.CreateRunSheet("CRS1");
			var runSheetCRS2 = Helper.CreateRunSheet("CRS2");
			var runSheetT1 = Helper.CreateRunSheet("T1");
			Asserter.AddToScope(runSheetCRS1);
			Asserter.AddToScope(runSheetCRS2);
			Asserter.AddToScope(runSheetT1);

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.RunSheetNumber];
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "CRS";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("It should return all runsheets with Runsheet number starting from C", filterBizO.Filter, runSheetCRS1, runSheetCRS2);

			filter.Property = "CRS1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all runsheet with Runsheet number CRS1", filterBizO.Filter, runSheetCRS1);

			filter.Property = "ABC";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("There are no runsheets for number ABC", filterBizO.Filter);
		}

		#endregion

		#endregion

		#region Test Dates Filters

		#region TestStartTime

		[TestDate(2013, 07, 25)]
		public void TestStartDate()
		{
			AssertStartAndEndDate(DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.StartTime, DtbConsignmentRunSheetSchema.KG_StartTime);
		}

		#endregion

		#region TestEndTime

		[TestDate(2013, 07, 25)]
		public void TestEndDate()
		{
			AssertStartAndEndDate(DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.EndTime, DtbConsignmentRunSheetSchema.KG_EndTime);
		}

		#endregion

		void AssertStartAndEndDate(string filterName, SchemaDateTimeOffsetColumn column)
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var runSheetWithoutDates = Helper.CreateRunSheet();
			var runSheet1 = Helper.CreateRunSheet();
			var runSheet2 = Helper.CreateRunSheet();
			var runSheet3 = Helper.CreateRunSheet();
			runSheet1[column] = now.AddDays(-4);
			runSheet2[column] = now.AddDays(-2);
			runSheet3[column] = now.AddDays(-1);

			Asserter.AddToScope(runSheetWithoutDates);
			Asserter.AddToScope(runSheet1);
			Asserter.AddToScope(runSheet2);
			Asserter.AddToScope(runSheet3);

			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleDateFilter)filterBizO[filterName]);
			AssertEquals(FilterCategories.Dates, filter.Category);

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-4);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("", filterBizO.Filter, runSheet1, runSheet2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-2);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("", filterBizO.Filter, runSheet2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(-5);
			Asserter.AssertMatches("", filterBizO.Filter);
		}

		#endregion

		#region Test Organisation Staff filters

		#region TestDriversLicense

		public void TestDriversLicense()
		{
			var driverWithoutLicense = Helper.CreateDriver("D1", "TestD1");
			var driverWithCertificateDG1 = Helper.CreateDriver("D2", "TestD2", CertificateTypePairList.Codes.DG1, "DRV");
			var driverWithLicenseH1 = Helper.CreateDriver("D3", "TestD3", CertificateTypePairList.Codes.CA1, "DRVH1");
			var driverWithLicenseH2 = Helper.CreateDriver("D4", "TestD4", CertificateTypePairList.Codes.CA1, "H2");

			var runSheetWithoutDriver = Helper.CreateRunSheet();
			var runSheetWithDriverWithoutLicense = Helper.CreateRunSheet();
			var runSheetWithDriverWithCertificateDG1 = Helper.CreateRunSheet();
			var runSheetWithDriverWithLicenseH1 = Helper.CreateRunSheet();
			var runSheetWithDriverWithLicenseH2 = Helper.CreateRunSheet();
			var runSheetWithDriverWithAdhocLicense1 = Helper.CreateRunSheet();
			var runSheetWithDriverWithAdhocLicense2 = Helper.CreateRunSheet();

			runSheetWithDriverWithoutLicense.KG_GS_NKTruckDriver = driverWithoutLicense.GS_Code;
			runSheetWithDriverWithCertificateDG1.KG_GS_NKTruckDriver = driverWithCertificateDG1.GS_Code;
			runSheetWithDriverWithLicenseH1.KG_GS_NKTruckDriver = driverWithLicenseH1.GS_Code;
			runSheetWithDriverWithLicenseH2.KG_GS_NKTruckDriver = driverWithLicenseH2.GS_Code;
			runSheetWithDriverWithAdhocLicense1.KG_AdHocDriversLicence = "DRVAdhoc";
			runSheetWithDriverWithAdhocLicense2.KG_AdHocDriversLicence = "H3";
			Asserter.AddToScope(runSheetWithoutDriver);
			Asserter.AddToScope(runSheetWithDriverWithoutLicense);
			Asserter.AddToScope(runSheetWithDriverWithCertificateDG1);
			Asserter.AddToScope(runSheetWithDriverWithLicenseH1);
			Asserter.AddToScope(runSheetWithDriverWithLicenseH2);
			Asserter.AddToScope(runSheetWithDriverWithAdhocLicense1);
			Asserter.AddToScope(runSheetWithDriverWithAdhocLicense2);

			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.DriverLicense]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = "DRV";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("It should return all run sheets with drivers' license starting from DRV", filterBizO.Filter, runSheetWithDriverWithLicenseH1, runSheetWithDriverWithAdhocLicense1);

			filter.Property = "H2";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all run sheets with drivers' license starting from H2", filterBizO.Filter, runSheetWithDriverWithLicenseH2);

			filter.Property = "H3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all run sheets with drivers' license starting from H3", filterBizO.Filter, runSheetWithDriverWithAdhocLicense2);

			filter.Property = "T";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("There are no run Sheets with with drivers' license T", filterBizO.Filter);
		}

		#endregion

		#region TestDriversName

		public void TestDriversName()
		{
			var driver = Helper.CreateDriver("Dr1", "TestD1");

			var runSheetWithoutDriver = Helper.CreateRunSheet();
			var runSheetWithDriver = Helper.CreateRunSheet();
			var runSheetWithAdhocDriver = Helper.CreateRunSheet();
			runSheetWithDriver.KG_GS_NKTruckDriver = driver.GS_Code;
			runSheetWithAdhocDriver.KG_AdHocDriversName = "D1";
			Asserter.AddToScope(runSheetWithoutDriver);
			Asserter.AddToScope(runSheetWithDriver);
			Asserter.AddToScope(runSheetWithAdhocDriver);
			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.DriverName]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = "D";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("It should return all run sheets with drivers' names starting from D", filterBizO.Filter, runSheetWithDriver, runSheetWithAdhocDriver);

			filter.Property = "Dr1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all run Sheet with driver name Dr1", filterBizO.Filter, runSheetWithDriver);

			filter.Property = "T";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("There are no run Sheets with driver name T", filterBizO.Filter);
		}

		#endregion

		#region TestDriversBranch

		public void TestDriversBranch()
		{
			var currentCompany = GlbCompany.CurrentCompany;

			var branch1 = SetupBranch(currentCompany, "TB1", "Test Branch1");
			var driver1 = SetupDriver(branch1, "DR1", "TestD1");
			var driversGroup1 = Helper.CreateDriverGroup("DG1", driver1);

			var branch2 = SetupBranch(currentCompany, "TB2", "Test Branch2");
			var driver2 = SetupDriver(branch2, "DR2", "TestD2");
			var driversGroup2 = Helper.CreateDriverGroup("DG2", driver2);

			var driver3 = Helper.CreateDriver("DR3", "TestD3");
			driversGroup1.Staff.Add(driver3);
			driversGroup2.Staff.Add(driver3);

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, driversGroup1.PK.ToGuid());
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, driversGroup2.PK.ToGuid());

			var runSheetWithDriver1 = CreateRunSheetWithDriver(driver1);
			var runSheetWithDriver2 = CreateRunSheetWithDriver(driver2);
			var runSheetWithDriver3 = CreateRunSheetWithDriver(driver3);
			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleGuidFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.DriverBranch]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = branch1.PK;
			Asserter.AssertMatches("It should return 2 run sheets with Branch1", filterBizO.Filter, runSheetWithDriver1, runSheetWithDriver3);

			filter.Property = branch2.PK;
			Asserter.AssertMatches("It should return 2 run Sheets with Branch2", filterBizO.Filter, runSheetWithDriver2, runSheetWithDriver3);
		}

		DtbConsignmentRunSheet CreateRunSheetWithDriver(GlbStaff driver)
		{
			var runSheet = Helper.CreateRunSheet();
			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			Asserter.AddToScope(runSheet);

			return runSheet;
		}

		GlbStaff SetupDriver(GlbBranch branch, string driverName, string loginName)
		{
			var driver = Helper.CreateDriver(driverName, loginName);
			driver.GS_GB_HomeBranch = branch.PK;

			return driver;
		}

		GlbBranch SetupBranch(GlbCompany curCompany, string code, string branchName)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_BranchName = branchName;
			branch.GB_GC = curCompany.PK;

			return branch;
		}

		#endregion

		#region TestStaffDriver

		public void TestStaffDriver()
		{
			var driver1 = Helper.CreateDriver("Dr1", "TestD1");
			var driver2 = Helper.CreateDriver("Dr2", "TestD2");
			var driver3 = Helper.CreateDriver("T", "TestT");

			var runSheetWithDriver1 = Helper.CreateRunSheet();
			var runSheetWithDriver2 = Helper.CreateRunSheet();
			runSheetWithDriver1.KG_GS_NKTruckDriver = driver1.GS_Code;
			runSheetWithDriver2.KG_GS_NKTruckDriver = driver2.GS_Code;
			Asserter.AddToScope(runSheetWithDriver1);
			Asserter.AddToScope(runSheetWithDriver2);
			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleGuidFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.StaffDriver]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = driver1.PK;
			Asserter.AssertMatches("It should return all run sheets with Driver1", filterBizO.Filter, runSheetWithDriver1);

			filter.Property = driver2.PK;
			Asserter.AssertMatches("It should return all run Sheet with Driver2", filterBizO.Filter, runSheetWithDriver2);

			filter.Property = driver3.PK;
			Asserter.AssertMatches("There are no run Sheets with driver Driver3", filterBizO.Filter);
		}

		#endregion

		#region TestTransportCompany

		public void TestTransportCompany()
		{
			var org1 = Helper.CreateOrganisation("Org1");
			var org2 = Helper.CreateOrganisation("Org2");
			var org3 = Helper.CreateOrganisation("Org3");

			var runSheetWithDriver1 = Helper.CreateRunSheet();
			var runSheetWithDriver2 = Helper.CreateRunSheet();
			runSheetWithDriver1.KG_OH_TransportCo = org1.PK;
			runSheetWithDriver2.KG_OH_TransportCo = org2.PK;
			Asserter.AddToScope(runSheetWithDriver1);
			Asserter.AddToScope(runSheetWithDriver2);
			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleGuidFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.TransportCompany]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = org1.PK;
			Asserter.AssertMatches("It should return all run sheets for Transport company Org1", filterBizO.Filter, runSheetWithDriver1);

			filter.Property = org2.PK;
			Asserter.AssertMatches("It should return all run sheets for Transport company Org2", filterBizO.Filter, runSheetWithDriver2);

			filter.Property = org3.PK;
			Asserter.AssertMatches("There are no run Sheets for Transport company Org3", filterBizO.Filter);
		}

		#endregion

		#region TestTransportCompanyName

		public void TestTransportCompanyName()
		{
			var org1 = Helper.CreateOrganisation("Org1");
			var org2 = Helper.CreateOrganisation("Org2");
			var org3 = Helper.CreateOrganisation("Org3");
			var orgT = Helper.CreateOrganisation("orgT");
			org1.OH_FullName = "Org1";
			org2.OH_FullName = "Org2";
			org3.OH_FullName = "Org3";
			orgT.OH_FullName = "orgT";

			var runSheetForOrg1 = Helper.CreateRunSheet();
			var runSheetForOrg2 = Helper.CreateRunSheet();
			var runSheetForOrg3 = Helper.CreateRunSheet();
			runSheetForOrg1.KG_OH_TransportCo = org1.PK;
			runSheetForOrg2.KG_OH_TransportCo = org2.PK;
			runSheetForOrg3.KG_AdHocTransportCoName = "Org3";
			Asserter.AddToScope(runSheetForOrg1);
			Asserter.AddToScope(runSheetForOrg2);
			Asserter.AddToScope(runSheetForOrg3);
			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.TransportCompanyName]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = "Org";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("It should return all run sheets which Transport company starts from Org", filterBizO.Filter,
				runSheetForOrg1, runSheetForOrg2, runSheetForOrg3);

			filter.Property = "Org1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all run sheets for Transport company Org1", filterBizO.Filter, runSheetForOrg1);

			filter.Property = "Org3";
			Asserter.AssertMatches("It should return all run sheets for Transport company Org3", filterBizO.Filter, runSheetForOrg3);

			filter.Property = "orgT";
			Asserter.AssertMatches("There are no run Sheets for Transport company orgT", filterBizO.Filter);
		}

		#endregion

		#region TestVehicle

		[TestDate(2013, 2, 4)]
		public void TestVehicle()
		{
			var truck1 = Helper.CreateVehicle("V1", "R1");
			var truck2 = Helper.CreateVehicle("V2");
			var truck3 = Helper.CreateVehicle("V3");
			var runSheetWithoutTruck = Helper.CreateRunSheet();
			var runSheetWithTruck1 = Helper.CreateRunSheet();
			var runSheetWithTruck21 = Helper.CreateRunSheet(null, ZDateTimeOffset.Today, ZDateTimeOffset.Today.AddHours(12));
			var runSheetWithTruck22 = Helper.CreateRunSheet(null, ZDateTimeOffset.Today.AddHours(12), ZDateTimeOffset.Today.AddDays(1));
			runSheetWithTruck1.KG_RQ_Truck = truck1.PK;
			runSheetWithTruck21.KG_RQ_Truck = truck2.PK;
			runSheetWithTruck22.KG_RQ_Truck = truck2.PK;
			Asserter.AddToScope(runSheetWithoutTruck);
			Asserter.AddToScope(runSheetWithTruck1);
			Asserter.AddToScope(runSheetWithTruck21);
			Asserter.AddToScope(runSheetWithTruck22);

			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleGuidFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.Vehicle]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = truck1.PK;
			Asserter.AssertMatches("It should return all run sheets for Vehicle V1", filterBizO.Filter, runSheetWithTruck1);

			filter.Property = truck2.PK;
			Asserter.AssertMatches("It should return all run sheets for Vehicle V2", filterBizO.Filter, runSheetWithTruck21, runSheetWithTruck22);

			filter.Property = truck3.PK;
			Asserter.AssertMatches("There are no runsheets for Vehicle V3", filterBizO.Filter);
		}

		#endregion

		#region TestVehicleRegistration

		public void TestVehicleRegistration()
		{
			var vehicleWithoutRegistration = Helper.CreateVehicle("V1");
			var vehicleWithRegistrationR1 = Helper.CreateVehicle("V2", "R1");
			var vehicleWithRegistrationR21 = Helper.CreateVehicle("V3", "R21");
			var vehicleWithRegistrationR22 = Helper.CreateVehicle("V4", "R22");

			var runSheetWithoutVehicle = Helper.CreateRunSheet();
			var runSheetWithVehicleWithoutRegistration = CreateRunsheetWithVehicle(vehicleWithoutRegistration);
			var runSheetWithVehicleWithRegistrationR1 = CreateRunsheetWithVehicle(vehicleWithRegistrationR1);
			var runSheetWithVehicleWithRegistrationR21 = CreateRunsheetWithVehicle(vehicleWithRegistrationR21);
			var runSheetWithVehicleWithRegistrationR22 = CreateRunsheetWithVehicle(vehicleWithRegistrationR22);
			var runSheetWithAdhocRegistrationR1 = Helper.CreateRunSheet();
			var runSheetWithAdhocRegistrationR2 = Helper.CreateRunSheet();

			runSheetWithAdhocRegistrationR1.KG_AdHocTruckRegistration = "R1Adhoc";
			runSheetWithAdhocRegistrationR2.KG_AdHocTruckRegistration = "R2Adhoc";

			Asserter.AddToScope(runSheetWithoutVehicle);
			Asserter.AddToScope(runSheetWithVehicleWithoutRegistration);
			Asserter.AddToScope(runSheetWithVehicleWithRegistrationR1);
			Asserter.AddToScope(runSheetWithVehicleWithRegistrationR21);
			Asserter.AddToScope(runSheetWithVehicleWithRegistrationR22);
			Asserter.AddToScope(runSheetWithAdhocRegistrationR1);
			Asserter.AddToScope(runSheetWithAdhocRegistrationR2);

			Factory.Save();

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.VehicleRegistration]);
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.IsActive = true;
			filter.Property = "R2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("It should return all run sheets with Truck registration starting from R2", filterBizO.Filter, runSheetWithVehicleWithRegistrationR21,
				runSheetWithVehicleWithRegistrationR22, runSheetWithAdhocRegistrationR2);

			filter.Property = "R21";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all run sheets with Truck registration starting from R21", filterBizO.Filter, runSheetWithVehicleWithRegistrationR21);

			filter.Property = "R2Adhoc";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("It should return all run sheets with Truck registration starting from R2Adhoc", filterBizO.Filter, runSheetWithAdhocRegistrationR2);

			filter.Property = "T";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("There are no run Sheets with with Truck registration T", filterBizO.Filter);
		}

		DtbConsignmentRunSheet CreateRunsheetWithVehicle(RefEquipment vehicle)
		{
			var runSheet = Helper.CreateRunSheet();
			runSheet.KG_RQ_Truck = vehicle.PK;
			return runSheet;
		}

		#endregion

		#endregion

		#region Test Status and Flags Filters

		#region TestRunSheetStatusFilter

		#endregion

		#region TestActiveStatus

		public void TestRunSheetStatus()
		{
			var runSheetWithPlanning = Helper.CreateRunSheet("WithPlanning");
			runSheetWithPlanning.KG_IsPlanning = true;
			var runSheetWithoutInstructions = Helper.CreateRunSheet("WithoutIns");
			runSheetWithoutInstructions.KG_IsPlanning = false;
			var runSheetWithAllInstructionsEmptyTimeInAndOut = Helper.CreateRunSheet("ALLInsEmp");
			runSheetWithAllInstructionsEmptyTimeInAndOut.KG_IsPlanning = false;
			var runSheetWithSomeInstructionsEmptyTimeIn = Helper.CreateRunSheet("SomeInsEmpIn");
			runSheetWithSomeInstructionsEmptyTimeIn.KG_IsPlanning = false;
			var runSheetWithAllInstructionsNonEmptyTimeIn = Helper.CreateRunSheet("ALLInsNonEmpIn");
			runSheetWithAllInstructionsNonEmptyTimeIn.KG_IsPlanning = false;
			var runSheetWithSomeInstructionsEmptyTimeOut = Helper.CreateRunSheet("SomeInsEmpOut");
			runSheetWithSomeInstructionsEmptyTimeOut.KG_IsPlanning = false;
			var runSheetWithAllInstructionsNonEmptyTimeOut = Helper.CreateRunSheet("ALLInsNonEmpOut");
			runSheetWithAllInstructionsNonEmptyTimeOut.KG_IsPlanning = false;
			var runSheetWithAllInstructionsNonEmptyTimeInAndOut = Helper.CreateRunSheet("InNnEmpInOut");
			runSheetWithAllInstructionsNonEmptyTimeInAndOut.KG_IsPlanning = false;
			var runSheetWithSomeInstructionsCompleted = Helper.CreateRunSheet("SomeInsCompl");
			runSheetWithSomeInstructionsCompleted.KG_IsPlanning = false;

			CreateRunSheetInstruction(runSheetWithAllInstructionsEmptyTimeInAndOut, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);
			CreateRunSheetInstruction(runSheetWithAllInstructionsEmptyTimeInAndOut, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);

			CreateRunSheetInstruction(runSheetWithSomeInstructionsEmptyTimeIn, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);
			CreateRunSheetInstruction(runSheetWithSomeInstructionsEmptyTimeIn, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);

			CreateRunSheetInstruction(runSheetWithAllInstructionsNonEmptyTimeIn, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);
			CreateRunSheetInstruction(runSheetWithAllInstructionsNonEmptyTimeIn, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);

			CreateRunSheetInstruction(runSheetWithSomeInstructionsEmptyTimeOut, ZDateTimeOffset.Empty, ZDateTimeOffset.Now);
			CreateRunSheetInstruction(runSheetWithSomeInstructionsEmptyTimeOut, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);

			CreateRunSheetInstruction(runSheetWithAllInstructionsNonEmptyTimeOut, ZDateTimeOffset.Empty, ZDateTimeOffset.Now);
			CreateRunSheetInstruction(runSheetWithAllInstructionsNonEmptyTimeOut, ZDateTimeOffset.Empty, ZDateTimeOffset.Now);

			CreateRunSheetInstruction(runSheetWithAllInstructionsNonEmptyTimeInAndOut, ZDateTimeOffset.Now, ZDateTimeOffset.Now);
			CreateRunSheetInstruction(runSheetWithAllInstructionsNonEmptyTimeInAndOut, ZDateTimeOffset.Now, ZDateTimeOffset.Now);

			CreateRunSheetInstruction(runSheetWithSomeInstructionsCompleted, ZDateTimeOffset.Now, ZDateTimeOffset.Now);
			CreateRunSheetInstruction(runSheetWithSomeInstructionsCompleted, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);

			Factory.Save();

			Asserter.AddToScope(runSheetWithPlanning);
			Asserter.AddToScope(runSheetWithoutInstructions);
			Asserter.AddToScope(runSheetWithAllInstructionsEmptyTimeInAndOut);

			Asserter.AddToScope(runSheetWithSomeInstructionsEmptyTimeIn);
			Asserter.AddToScope(runSheetWithAllInstructionsNonEmptyTimeIn);

			Asserter.AddToScope(runSheetWithSomeInstructionsEmptyTimeOut);
			Asserter.AddToScope(runSheetWithAllInstructionsNonEmptyTimeOut);

			Asserter.AddToScope(runSheetWithAllInstructionsNonEmptyTimeInAndOut);
			Asserter.AddToScope(runSheetWithSomeInstructionsCompleted);

			var filterBizO = new DtbConsignmentRunSheetFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterBizO[DtbConsignmentRunSheetFilterBusinessObject.FilterConstants.IsActiveStatus]);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			filter.IsActive = true;

			filter.Property = IsActiveStatuses.Codes.Planning;
			Asserter.AssertMatches("Planning runsheets should be returned.", filterBizO.Filter, runSheetWithPlanning);

			filter.Property = IsActiveStatuses.Codes.NotStarted;
			Asserter.AssertMatches("Not Started runsheets should be returned.", filterBizO.Filter, runSheetWithoutInstructions);

			filter.Property = IsActiveStatuses.Codes.InProgress;
			Asserter.AssertMatches("In Progress runsheets should be returned.", filterBizO.Filter, runSheetWithAllInstructionsEmptyTimeInAndOut,
				runSheetWithSomeInstructionsEmptyTimeIn, runSheetWithSomeInstructionsEmptyTimeOut, runSheetWithAllInstructionsNonEmptyTimeIn,
				runSheetWithAllInstructionsNonEmptyTimeOut, runSheetWithSomeInstructionsCompleted);

			filter.Property = IsActiveStatuses.Codes.Completed;
			Asserter.AssertMatches("Completed runsheets should be returned.", filterBizO.Filter, runSheetWithAllInstructionsNonEmptyTimeInAndOut);

			filter.Property = IsActiveStatuses.Codes.All;
			Asserter.AssertMatches("All runsheets should be returned.", filterBizO.Filter,
				runSheetWithPlanning, runSheetWithoutInstructions, runSheetWithAllInstructionsEmptyTimeInAndOut, runSheetWithSomeInstructionsEmptyTimeIn,
				runSheetWithSomeInstructionsEmptyTimeOut, runSheetWithAllInstructionsNonEmptyTimeIn, runSheetWithAllInstructionsNonEmptyTimeOut,
				runSheetWithAllInstructionsNonEmptyTimeInAndOut, runSheetWithSomeInstructionsCompleted);
		}

		void CreateRunSheetInstruction(DtbConsignmentRunSheet runSheet, ZDateTimeOffset timeIn, ZDateTimeOffset timeOut)
		{
			var runSheetInstructions = runSheet.RunSheetInstructions;
			var runSheetInstruction = runSheetInstructions.AddNew();
			runSheetInstruction.K1_Sequence = runSheetInstructions.Count;
			runSheetInstruction.K1_IsAcceptedByDriver = true;
			if (!timeOut.IsEmpty && (timeIn.IsEmpty || timeIn > timeOut))
			{
				timeIn = timeOut;
			}
			runSheetInstruction.K1_TimeIn = timeIn;
			runSheetInstruction.K1_TimeOut = timeOut;

			if (runSheet.KG_RunSheetNumber == "InNnEmpInOut")
			{
				foreach (var instruction in runSheet.RunSheetInstructions)
				{
					instruction.K1_IsAcceptedByDriver = true;
					instruction.K1_TimeIn = ZDateTimeOffset.Now;
					instruction.K1_TimeOut = ZDateTimeOffset.Now;
					instruction.K1_ReceivedBy = "TST";
				}
			}
		}

		#endregion
		#endregion
		#region AssertConsignmentTextFilter

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbConsignmentRunSheetFilterBusinessObject();
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		FilterStripAsserter<DtbConsignmentRunSheet> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<DtbConsignmentRunSheet>(Factory, consignmentRunsheet =>
					string.Format("Runsheet {0}", consignmentRunsheet.KG_RunSheetNumber)));
			}
		}

		TransportBookingConsignmentTestHelper helper;
		FilterStripAsserter<DtbConsignmentRunSheet> asserter;

		#endregion
	}
}
