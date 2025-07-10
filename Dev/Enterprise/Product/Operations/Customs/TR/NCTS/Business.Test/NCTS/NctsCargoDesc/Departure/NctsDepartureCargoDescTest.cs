using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDesc))]
	class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		public override int CountSupportingInfoTypes => base.CountSupportingInfoTypes + 1;

		public void TestLookups()
		{
			AssertType<NctsDepartureCargoDescPhase4Lookups>(goodsItem.Lookups);
		}

		public void TestLookups_NCTS5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var phase5Lookups = new NctsDepartureCargoDescPhase5Lookups(goodsItem);

			AssertType<NctsDepartureCargoDescPhase5Lookups>(phase5Lookups);
		}

		public void TestValidation()
		{
			AssertType<NctsDepartureCargoDescPhase4Validation>(goodsItem.Validation);
		}

		public void TestValidation_NCTS5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var phase5Validation = new NctsDepartureCargoDescPhase5Validation(goodsItem);

			AssertType<NctsDepartureCargoDescPhase5Validation>(phase5Validation);
		}

		public void TestExportDeclarationNumber()
		{
			AssertEquals("1010-1231231332", goodsItem.ExportDeclaration.CE_EntryNum);
		}

		public void TestCE_EntryType()
		{
			AssertEquals("EXP", goodsItem.ExportDeclaration.CE_EntryType);
		}

		public void TestCE_Category()
		{
			AssertEquals("", goodsItem.ExportDeclaration.CE_Category);
			goodsItem.IsDeclarationPartial = new ZBool(true);
			AssertEquals("1", goodsItem.ExportDeclaration.CE_Category);
		}

		public void TestAdditionalInfos()
		{
			CombineAssertions(() =>
			{
				AssertType<EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>>("Type", goodsItem.AdditionalInfos);
				AssertEquals("IsRegisteredEditableChildObject", true, goodsItem.IsRegisteredEditableChildObject(goodsItem.AdditionalInfos));
			});
		}

		public void TestFees()
		{
			AssertType<Customs.Business.CusInBondFeeCollection<NctsCargoDescFee>>(goodsItem.Fees);
		}

		public void TestPackages()
		{
			AssertType<EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>>(goodsItem.Packages);
		}

		public void TestIsDeclarationPartialCaption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsDepartureCargoDesc), nameof(NctsDepartureCargoDesc.IsDeclarationPartial), false, x => x.Caption == "Partial?");
		}

		public void TestBY_Description_MaxLength()
		{
			AssertEquals("Goods Desciption Max Length", 280, goodsItem.BY_Description_MaxLength);
		}

		public void TestDecimalPlacesBY_MonetaryValue()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(goodsItem.GetType(), "BY_MonetaryValue", false, attr => attr.DecimalPlaces == 2);
		}

		public void TestDecimalPlacesBY_NetWeight()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(goodsItem.GetType(), "BY_NetWeight", false, attr => attr.DecimalPlaces == 3);
		}

		public void TestDecimalPlacesBY_GrossWeight()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(goodsItem.GetType(), "BY_GrossWeight", false, attr => attr.DecimalPlaces == 3);
		}

		public void TestDecimalPlacesBY_CustomsSecondQuantity()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(goodsItem.GetType(), "BY_CustomsSecondQuantity", false, attr => attr.DecimalPlaces == 3);
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.Turkey;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.MovementHeader.GoodsItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			goodsItem.ExportDeclarationNumber = "1010-1231231332";
			goodsItem.ExportDeclarationType = "EXP";
			goodsItem.IsDeclarationPartial = new ZBool(false);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
	}
}
