using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckExportDeclarationType()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var detail = movementHeader.GoodsItems.AddNew();
			movementHeader.GoodsItems.Add(detail);
			detail.Validation.ValidateExportDeclarationType();
			AssertNoMessageErrorContaining(detail.ExportDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);

			detail.ExportDeclarationNumber = "1010-1231231332";
			detail.IsDeclarationPartial = new ZBool(false);
			detail.Validation.ValidateExportDeclarationType();
			AssertHasMessageErrorContaining(detail.ExportDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);

			detail.ExportDeclarationType = "EXP";
			detail.Validation.ValidateExportDeclarationType();
			AssertNoMessageErrorContaining(detail.ExportDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
