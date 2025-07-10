using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OrgCustomLabelsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOT_FieldName()
		{
			var mustBeEnteredMessage = "Please enter a Field.";
			var invalidCodeMessage = "Enter a valid Field.";
			var mustHaveGoodsDescriptionMessage = "Goods Description is mandatory for printing Customs Declaration.";
			var targetInfo = customLabel.OT_FieldNameInfo;
			customLabel.OT_FieldName = ZString.Empty;
			AssertHasError(targetInfo, mustBeEnteredMessage);
			AssertNoError(targetInfo, invalidCodeMessage);
			AssertHasError(targetInfo, mustHaveGoodsDescriptionMessage);

			customLabel.OT_FieldName = ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber;
			AssertNoError(targetInfo, mustBeEnteredMessage);
			AssertNoError(targetInfo, invalidCodeMessage);
			AssertHasError(targetInfo, mustHaveGoodsDescriptionMessage);

			customLabel.OT_FieldName = "AA";
			AssertNoError(targetInfo, mustBeEnteredMessage);
			AssertHasError(targetInfo, invalidCodeMessage);
			AssertHasError(targetInfo, mustHaveGoodsDescriptionMessage);

			customLabelCollection.AddNew().OT_FieldName = ExportDeclarationDocumentFieldList.Codes.GoodsDescription;
			customLabel.Validation.ValidateOT_FieldName();
			AssertNoError(targetInfo, mustBeEnteredMessage);
			AssertHasError(targetInfo, invalidCodeMessage);
			AssertNoError(targetInfo, mustHaveGoodsDescriptionMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.New<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(org);
			customLabelCollection = orgWrapper.ExportCustomDocumentLabels;
			customLabel = customLabelCollection.AddNew();
		}

		CustomDocumentsCollection customLabelCollection;
		OrgCustomLabels customLabel;
	}
}
