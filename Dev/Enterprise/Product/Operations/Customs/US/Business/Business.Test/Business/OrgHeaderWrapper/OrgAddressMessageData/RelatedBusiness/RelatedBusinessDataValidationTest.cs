using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class RelatedBusinessDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_RelatedBusiness()
		{
			RelatedBusinessData.US_RelatedBusiness = ZString.Empty;
			AssertHasMessageErrorContaining(RelatedBusinessData.US_RelatedBusinessInfo, MandatoryValidation.YouHaveNotEntered);
			RelatedBusinessData.US_RelatedBusiness = "~";
			AssertNoMessageErrorContaining(RelatedBusinessData.US_RelatedBusinessInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(RelatedBusinessData.US_RelatedBusinessInfo, ListValidation.InvalidCodeMessageError);
			RelatedBusinessData.US_RelatedBusiness = ImporterRelatedBusinessTypeList.Codes._01;
			AssertNoMessageErrorContaining(RelatedBusinessData.US_RelatedBusinessInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_NameOfEntity()
		{
			RelatedBusinessData.US_NameOfEntity = ZString.Empty;
			AssertHasMessageErrorContaining(RelatedBusinessData.US_NameOfEntityInfo, MandatoryValidation.YouHaveNotEntered);
			RelatedBusinessData.US_NameOfEntity = "A";
			AssertNoMessageErrorContaining(RelatedBusinessData.US_NameOfEntityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Number()
		{
			RelatedBusinessData.US_Number = ZString.Empty;
			AssertHasMessageErrorContaining(RelatedBusinessData.US_NumberInfo, MandatoryValidation.YouHaveNotEntered);
			RelatedBusinessData.US_Number = "A";
			AssertNoMessageErrorContaining(RelatedBusinessData.US_NumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(RelatedBusinessData.US_NumberInfo, RelatedBusinessDataValidation.NumberNotInRightFormat);
			RelatedBusinessData.US_Number = "12-1234567AB";
			AssertNoMessageError(RelatedBusinessData.US_NumberInfo, RelatedBusinessDataValidation.NumberNotInRightFormat);
			RelatedBusinessData.US_Number = "123-45-6789";
			AssertNoMessageError(RelatedBusinessData.US_NumberInfo, RelatedBusinessDataValidation.NumberNotInRightFormat);
			RelatedBusinessData.US_Number = "181101-12345";
			AssertNoMessageError(RelatedBusinessData.US_NumberInfo, RelatedBusinessDataValidation.NumberNotInRightFormat);
		}

		#region Implementation

		RelatedBusinessData RelatedBusinessData
		{
			get
			{
				if (relatedBusinessData == null)
				{
					var organization = Factory.New<OrgHeader>();
					var wrapper = OrgHeaderWrapper.New(organization);
					var messageData = new OrgAddressMessageData(wrapper);
					relatedBusinessData = messageData.RelatedBusinessItems.AddNew();
				}

				return relatedBusinessData;
			}
		}
		RelatedBusinessData relatedBusinessData;

		#endregion
	}
}
