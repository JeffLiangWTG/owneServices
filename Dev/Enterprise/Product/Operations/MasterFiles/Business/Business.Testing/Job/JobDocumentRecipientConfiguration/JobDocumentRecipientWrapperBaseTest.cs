using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobDocumentRecipientWrapperBaseTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestOrganisation();

		public abstract void TestContacts();

		public abstract void TestPropertiesReadonliness();

		public abstract void TestWrappedProperties();

		public abstract void TestCanDelete();

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			DummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			Configuration = new JobDocumentRecipientConfiguration(Factory, DummyDocumentSupportable);
		}

		protected IDocumentSupportable DummyDocumentSupportable;
		protected JobDocumentRecipientConfiguration Configuration;

		#endregion
	}
}
