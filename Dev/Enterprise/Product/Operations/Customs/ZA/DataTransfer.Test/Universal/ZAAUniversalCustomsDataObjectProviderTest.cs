using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class ZAAUniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var reader = provider.GetNewJobDeclarationDataObjectReader(new UniversalDataBuss.DataObjects.Universal.Shipment(), new TestErrorLogger(), Factory, null);
			AssertEquals(typeof(JobDeclarationDataObjectReaderForWOT), reader.GetType());
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			var reader = provider.GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>())));
			AssertEquals(typeof(DeclarationDataObjectWriter), reader.GetType());
		}

		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			var helper = provider.GetNewUniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(typeof(UniversalDataObjectReaderHelper), helper.GetType());
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(provider.TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(provider.TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new ZAAUniversalCustomsDataObjectProvider();
		}

		IUniversalCustomsDataObjectProvider provider;
	}
}
