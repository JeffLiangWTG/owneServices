using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocUSContacts))]
	sealed class DocUSContactsTest : USOrganisationWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocUSContacts.New(USOrganisation, Factory) };
		}

		public void TestNew()
		{
			AssertNull("New", DocUSContacts.New(null, Factory));
			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("New", DocUSContacts.New(USOrganisation, Factory));
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			AssertNotNull("New", DocUSContacts.New(USOrganisation, Factory));
		}

		#region Implementation
		DocUSOrganisation orgWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			orgWrapper = DocUSOrganisation.New(USOrganisation, Factory);
			AssertNotNull("OrgWrapper created not null", orgWrapper);
		}

		#endregion
	}
}
