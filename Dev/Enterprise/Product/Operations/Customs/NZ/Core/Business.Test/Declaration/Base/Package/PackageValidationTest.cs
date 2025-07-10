using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class PackageValidationTest : Customs.Business.Testing.CusDecHouseContainerPackValidationTest
	{
		public void TestCannotLinkPackingLinesAgainstMasterBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MYBILL";

			var packingLine = declaration.Packages.AddNew();
			packingLine.CW_HouseBill = "MB:MYBILL";
			AssertHasError(packingLine.CW_HouseBillInfo, PackageValidation.MessageErrorPackLinesMustBeLinkedToHouseBill);

			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertNoError(packingLine.CW_HouseBillInfo, PackageValidation.MessageErrorPackLinesMustBeLinkedToHouseBill);
		}

		public void TestCanLinkPackingLinesToMasterBillOnTSWDec()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "OB00398478";

			var packingLine = declaration.Packages.AddNew();
			packingLine.CW_HouseBill = "MB:OB00398478";
			AssertNoError("TSW allows for direct masters - package must therefore link to MB", packingLine.CW_HouseBillInfo, PackageValidation.MessageErrorPackLinesMustBeLinkedToHouseBill);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			packingLine.CW_HouseBill = "MB:OB00398478";
			AssertHasError("Legacy job must link to HB", packingLine.CW_HouseBillInfo, PackageValidation.MessageErrorPackLinesMustBeLinkedToHouseBill);
		}

		public void TestTSWLinkPackingLinesToBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = "08104345351";
			declaration.JE_HouseBill = "G7-59298";

			var packingLine = declaration.Packages.AddNew();
			packingLine.CW_HouseBill = "MB:08104345351";
			AssertHasError(packingLine.CW_HouseBillInfo, PackageValidation.MessageErrorLinkPackLinesToHouseBill);

			packingLine.CW_HouseBill = "HB:G7-59298 (MB:08104345351)";
			AssertNoError(packingLine.CW_HouseBillInfo, PackageValidation.MessageErrorLinkPackLinesToHouseBill);
		}

		public void TestCheckCW_PackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "OB00398478";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "YKKU4385741";
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;

			var packingLine = declaration.Packages.AddNew();
			packingLine.CW_HouseBill = "MB:OB00398478";
			packingLine.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			packingLine.CW_PackType = "";
			AssertNoMessageErrors("TSW consignment for empty containers does not report package type", packingLine.CW_PackTypeInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			packingLine.Validation.ValidateCW_PackType();
			AssertHasMessageErrors("Legacy job validated for package type", packingLine.CW_PackTypeInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			packingLine.Validation.ValidateCW_PackType();
			AssertHasMessageErrors("TSW consignment for non-empty containers validates package type as normal", packingLine.CW_PackTypeInfo);
		}

		public void TestValidatePackType()
		{
			package.CW_PackType = "";
			AssertEquals("Invalid", true, package.CW_PackTypeInfo.HasMessageErrors());

			package.CW_PackType = "XX";
			AssertEquals("Invalid", true, package.CW_PackTypeInfo.HasMessageErrors());

			package.CW_PackType = "PK";
			AssertEquals("Valid", false, package.CW_PackTypeInfo.HasMessageErrors());
		}

		public void TestValidatePackQty()
		{
			package.CW_PackQty = 0;
			AssertHasMessageErrors(package.CW_PackQtyInfo);
			package.CW_PackQty = 1;
			AssertNoMessageErrors(package.CW_PackQtyInfo);

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;

			package.CW_PackQty = 0;
			AssertHasMessageErrors(package.CW_PackQtyInfo);
			packingGroup.CR_CO_Container = container.PK;
			AssertNoMessageErrors(package.CW_PackQtyInfo);
		}

		public void TestValidateBulkType()
		{
			package = packingGroup.Packages.AddNew();

			package.CW_PackQty = 2;
			package.CW_PackType = "VG";
			AssertEquals("Pack type is a bulk type", true, package.CW_PackQtyInfo.HasMessageErrors());
		}

		#region Implementation

		JobDeclaration declaration;
		Bill houseBill;
		PackingGroup packingGroup;
		Package package;
		protected override void SetUp()
		{
			base.SetUp();
			new UniversalReferenceTestDataHelper(Factory).CreateCusCodeListWithAttribute("UNE", "UNPKG", "VG", "Bulk, gas (1031 mbar and 15C)", UniversalReferenceConstants.UNPackTypeStartDate, UniversalReferenceConstants.UNPackTypeStartDate, "BULK", "BULK");
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			houseBill = declaration.Bills.AddNew();
			packingGroup = houseBill.PackingGroups.AddNew();
			package = packingGroup.Packages.AddNew();
		}

		#endregion

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
