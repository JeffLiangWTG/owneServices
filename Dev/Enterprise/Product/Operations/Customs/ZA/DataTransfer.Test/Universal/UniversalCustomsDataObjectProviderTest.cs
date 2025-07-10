using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var reader = new UniversalCustomsDataObjectProvider().GetNewJobDeclarationDataObjectReader(new UniversalDataBuss.DataObjects.Universal.Shipment(), new TestErrorLogger(), Factory, null);
			AssertEquals(typeof(JobDeclarationDataObjectReader), reader.GetType());
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			var reader = new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>())));
			AssertEquals(typeof(DeclarationDataObjectWriter), reader.GetType());
		}

		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			var helper = new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(typeof(UniversalDataObjectReaderHelper), helper.GetType());
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
