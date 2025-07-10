using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckExportDeclarationType()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bills = movementHeader.Header.Bills.AddNew();
			var goodsitems = bills.GoodsItems.AddNew();

			goodsitems.Validation.ValidateExportDeclarationType();
			AssertNoMessageErrorContaining(goodsitems.ExportDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);

			goodsitems.ExportDeclarationNumber = "1010-1231231332";
			goodsitems.IsDeclarationPartial = new ZBool(false);
			goodsitems.Validation.ValidateExportDeclarationType();
			AssertHasMessageErrorContaining(goodsitems.ExportDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);

			goodsitems.ExportDeclarationType = "EXP";
			goodsitems.Validation.ValidateExportDeclarationType();
			AssertNoMessageErrorContaining(goodsitems.ExportDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
