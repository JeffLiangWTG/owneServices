using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocumentRecipientConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOrganisationPKFilter()
		{
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());

			configuration.OrganisationPKFilter = ZGuid.Empty;
			AssertNoErrors(configuration.OrganisationPKFilterInfo);

			configuration.OrganisationPKFilter = ZGuid.Invalid;
			AssertHasError(configuration.OrganisationPKFilterInfo, "Enter a valid selection.");

			configuration.OrganisationPKFilter = ZGuid.NewZGuid();
			AssertHasError(configuration.OrganisationPKFilterInfo, "Enter a valid selection.");

			configuration.OrganisationPKFilter = orgHeader.PK;
			AssertNoErrors(configuration.OrganisationPKFilterInfo);
		}

		public void TestDocumentGroupFilter()
		{
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";
			Factory.Save();

			configuration.DocumentGroupFilter = ZString.Empty;
			AssertNoErrors(configuration.DocumentGroupFilterInfo);

			configuration.DocumentGroupFilter = "XXX";
			AssertHasError(configuration.DocumentGroupFilterInfo, "This filter must be used with the Organization filter.");

			configuration.DocumentGroupFilter = ContactType.Consignee.Code;
			AssertHasError(configuration.DocumentGroupFilterInfo, "This filter must be used with the Organization filter.");

			configuration.OrganisationPKFilter = orgHeader.PK;
			AssertNoErrors(configuration.DocumentGroupFilterInfo);

			configuration.OrganisationPKFilter = ZGuid.Invalid;
			AssertHasError(configuration.DocumentGroupFilterInfo, "This filter must be used with the Organization filter.");

			configuration.OrganisationPKFilter = ZGuid.Empty;
			AssertHasError(configuration.DocumentGroupFilterInfo, "This filter must be used with the Organization filter.");

			configuration.DocumentPKFilter = document.PK;
			AssertHasError(configuration.DocumentGroupFilterInfo, "This filter must be used with the Organization filter.");
		}

		public void TestDocumentPKFilter()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";
			Factory.Save();

			configuration.DocumentPKFilter = ZGuid.Empty;
			AssertNoErrors(configuration.DocumentPKFilterInfo);

			configuration.DocumentPKFilter = ZGuid.Invalid;
			AssertHasError(configuration.DocumentPKFilterInfo, "Enter a valid selection.");

			configuration.DocumentPKFilter = ZGuid.NewZGuid();
			AssertHasError(configuration.DocumentPKFilterInfo, "Enter a valid selection.");

			configuration.DocumentPKFilter = document.PK;
			AssertNoErrors(configuration.DocumentPKFilterInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			dummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
		}

		IDocumentSupportable dummyDocumentSupportable;
		JobDocumentRecipientConfiguration configuration;

		#endregion
	}
}
