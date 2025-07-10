using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI.Testing
{
	sealed class LandedCostingHistoryViewPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestName()
		{
			AssertEquals("Name", LandedCostingHistoryViewPlugIn.PlugInName, plugIn.Name);
		}

		public void TestLicence()
		{
			AssertEquals("Licence", Env.Licence.Forwarder, plugIn.LicenceCheckPoint);
		}

		public void TestCollectionLoaded()
		{
			dummyMaster.FKSchemaColumnInLandedCostHistoryExposed = LandedCostHistorySchema.LH_OP;
			var collection = new GenericLandedCostHistoryCollection(dummyMaster);
			var lCHistory = Factory.New<LandedCostHistory>();
			lCHistory.LH_OP = dummyMaster.PK;
			collection.Add(lCHistory);
			AssertEquals("Collection is loaded", 1, ((GenericLandedCostHistoryCollection)plugIn.BusinessEntity).Count);
		}

		public void TestBusinessObject()
		{
			dummyMaster.FKSchemaColumnInLandedCostHistoryExposed = LandedCostHistorySchema.LH_OP;
			AssertEquals("Type", typeof(GenericLandedCostHistoryCollection), plugIn.BusinessEntity.GetType());
			AssertEquals("ReadOnly", true, ((GenericLandedCostHistoryCollection)plugIn.BusinessEntity).ReadOnly);
		}

		public void TestUserControl()
		{
			AssertEquals("User Control Type", typeof(LandCostHistoryUserControl), plugIn.UserControl.GetType());
			AssertEquals("LCHistory Master is set", dummyMaster, ((LandCostHistoryUserControl)plugIn.UserControl).LCHistoryMaster);
		}

		DummyLandedCostHistoryMaster dummyMaster;
		LandedCostingHistoryViewPlugInTestProxy plugIn;
		protected override void SetUp()
		{
			base.SetUp();
			dummyMaster = Factory.New<DummyLandedCostHistoryMaster>();
			plugIn = new LandedCostingHistoryViewPlugInTestProxy(dummyMaster);
		}

		protected override void TearDown()
		{
			base.TearDown();
			plugIn?.Dispose();
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var product = (BusinessObject)Factory.New<MasterFiles.Integration.IOrgSupplierPart>();
			var lCHeader = Factory.New<LandedCostHeader>();
			var lCHistory = lCHeader.Histories.AddNew();
			lCHistory.LH_OP = product.PK;
			return new LandedCostingHistoryViewPlugIn((ILandedCostHistoryMaster)product);
		}

		sealed class LandedCostingHistoryViewPlugInTestProxy : LandedCostingHistoryViewPlugIn
		{
			public LandedCostingHistoryViewPlugInTestProxy(ILandedCostHistoryMaster lCHistoryHost) : base(lCHistoryHost) { }

			public new LicenceCheckpoint LicenceCheckPoint => base.LicenceCheckPoint;
		}
	}
}
