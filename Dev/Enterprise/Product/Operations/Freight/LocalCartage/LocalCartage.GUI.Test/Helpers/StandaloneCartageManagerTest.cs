using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class StandaloneCartageManagerTest : TestCaseWithFactory
	{
		public void TestParentNotes()
		{
			AssertCollectionContains("Should have note", note, CartageExporter.ParentNotes.GetAllNotes());
		}

		public void TestParentLogs()
		{
			AssertCollectionContains("Should have log", log, CartageExporter.ParentLogs.GetAllLogs());
		}

		public void TestFactory()
		{
			AssertEquals("Should have same factory", cartage.Factory, CartageExporter.ParentFactory);
		}

		public void TestDescription()
		{
			AssertEquals("Should have description", "", CartageExporter.Description);
		}

		public void TestGetCartageForExport()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			AssertEquals("Should have cartage", cartage, CartageExporter.GetCartageForExport(buffer));
			Assert("Should have no errors", !buffer.HasErrors);
		}

		public void TestJobNumber()
		{
			AssertEquals("Should have Local Transport JobNumber", "T01", CartageExporter.ParentJobNumber);
		}

		public void TestSendTo()
		{
			AssertNotNull("Should have Local Client", CartageExporter.SendTo);
			AssertEquals("Should have Local Client", cartage.LocalClient, CartageExporter.SendTo);
		}

		public void TestSendToDescription()
		{
			AssertEquals("Should have Local Transport Provider", "Local Client", CartageExporter.SendToDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cartage = Factory.New<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex().Dispose();
			cartage.LocalClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			cartage.JJ_ConsignmentID = "T01";
			note = cartage.Notes.AddNew();
			log = cartage.Logs.AddNew();
			manager = new StandaloneCartageManager(cartage);
		}

		CommonCartage cartage;
		StandaloneCartageManager manager;
		StmNote note;
		StmALog log;

		ICartageExporter CartageExporter
		{
			get
			{
				return manager;
			}
		}
	}
}
