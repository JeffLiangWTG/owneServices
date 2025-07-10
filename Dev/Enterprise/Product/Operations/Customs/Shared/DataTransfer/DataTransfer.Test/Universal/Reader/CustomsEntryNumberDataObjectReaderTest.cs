using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestBasicCusEntryNumberFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryNumberDataObject = SetupEntryNumber();
			var entryNumberBO = new CustomsEntryNumberDataObjectReader<BaseJobDeclaration>(entryNumberDataObject, logger, CurrentCompanyHelper, declaration).ReadIntoBusinessObject();
			AssertCusEntryNumberContents(entryNumberBO, declaration.TableName, declaration.PK, CurrentCompanyHelper.TargetCountryCode);

			AssertNotEquals("PreCondition", "ABB324", entryNumberBO.CE_EntryNum);
			entryNumberBO.CE_EntryNum = "ABB324";
			entryNumberBO = new CustomsEntryNumberDataObjectReader<BaseJobDeclaration>(entryNumberDataObject, logger, CurrentCompanyHelper, declaration).ReadIntoBusinessObject();
			AssertNotEquals("Should have been updated", "ABB324", entryNumberBO.CE_EntryNum);
			AssertCusEntryNumberContents(entryNumberBO, declaration.TableName, declaration.PK, CurrentCompanyHelper.TargetCountryCode);

			AssertNotEquals("PreCondition", "ABB324", entryNumberBO.CE_EntryNum);
			entryNumberBO.CE_EntryNum = "ABB324";
			entryNumberBO.CE_RN_NKCountryCode = "ZZ";
			var newEntryNumberBO = new CustomsEntryNumberDataObjectReader<BaseJobDeclaration>(entryNumberDataObject, logger, CurrentCompanyHelper, declaration).ReadIntoBusinessObject();
			AssertEquals("Should not be updated", "ABB324", entryNumberBO.CE_EntryNum);
			AssertCusEntryNumberContents(newEntryNumberBO, declaration.TableName, declaration.PK, CurrentCompanyHelper.TargetCountryCode);
		}
	}
}
