using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	class CustomsEntryInstructionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReader_OverrideCustomsOffice()
		{
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var input = new EntryInstruction
			{
				AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(),
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = "CustomsOfficeOverride",
				Value = "CNT"
			});

			var output = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			Factory.SaveForTesting();

			AssertEquals("CNT", output.CEI_CustomsOfficeOverride);

			input.AddInfoCollection.Clear();
			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo { Key = "CustomsOfficeOverride", Value = "" });

			output = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			Factory.SaveForTesting();

			AssertEquals(ZString.Empty, output.CEI_CustomsOfficeOverride);
		}
	}
}
