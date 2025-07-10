using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	class CustomsEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestAddInfosIncludeOverrideCustomsOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CustomsOfficeOverride = "CNT";
			Factory.Save();

			var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.SouthAfrica));
			var result = writer.GetDataObject(instruction);

			AssertEquals("CustomsOfficeOverride", "CNT", result.AddInfoCollection.GetZStringValue("CustomsOfficeOverride"));
		}
	}
}
