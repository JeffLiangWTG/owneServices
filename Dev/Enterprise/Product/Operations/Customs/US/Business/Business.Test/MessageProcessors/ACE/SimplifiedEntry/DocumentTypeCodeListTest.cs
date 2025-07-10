using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DocumentTypeCodeListTest : TestCaseWithFactory
	{
		public void TestGetDescriptionFromDocumentType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "DIS Form List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "AMS01", "AMS_FOREIGN_GOVT_EXPORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertEquals(ZString.Empty, DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, "A"));
			AssertEquals(DocumentTypeCodeList.Descriptions._01, DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, DocumentTypeCodeList.Codes._01));
			AssertEquals("AMS_FOREIGN_GOVT_EXPORT", DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, "AMS01"));
		}
	}
}
