using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class FinaliseDocketActionMethodApplicatorTest<TDocket, TApplicator> : OperationalActionMethodApplicatorTest
		where TDocket : WhsDocket
		where TApplicator : FinalizeDocketsActionMethodApplicator<TDocket>
	{
		#region TestApplyApplicator

		public void TestApplyApplicator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket(Helper, data.Org1, data.Whs1);
			GetNewDocketLine(Helper, docket, data.Part1, 10m);
			PrepareDocketsForFinalisation(Helper, docket);

			Factory.Save();

			ApplyApplicator(new[] { docket }, $@"
INFO: {docket.Description} [HL W00000001] - was successfully finalized.".Trim());

			var docketInNewFactory = new BusinessObjectFactory().Load<WhsDocket>(docket.PK);
			Assert("Docket is finalized.", docketInNewFactory.IsFinalised);
		}

		public void TestApplyApplicator_CannotFinalise()
		{
			TestApplyApplicator_CannotFinaliseCore();
		}

		protected virtual void TestApplyApplicator_CannotFinaliseCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket(Helper, data.Org1, data.Whs1);
			PrepareDocketsForFinalisation(Helper, docket);

			Factory.Save();
			ApplyApplicator(new[] { docket }, $@"
WARNING: {docket.Description} [HL W00000001] - could not be auto-finalized. It must be manually finalized.".Trim());
			var docketInNewFactory = new BusinessObjectFactory().Load<WhsDocket>(docket.PK);
			Assert("Docket is not finalized.", !docketInNewFactory.IsFinalised);
		}

		public void TestApplyApplicator_AlreadyFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket(Helper, data.Org1, data.Whs1);
			GetNewDocketLine(Helper, docket, data.Part1, 10m);
			PrepareDocketsForFinalisation(Helper, docket);
			docket.FinaliseDocketWithoutUserConfirmation();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);
			Factory.Save();

			ApplyApplicator(new[] { docket }, $@"WARNING: {docket.Description} [HL W00000001] - is already finalized.".Trim());
		}

		public void TestApplyApplicator_MultipleDockets()
		{
			TestApplyApplicator_MultipleDocketsCore();
		}

		protected virtual void TestApplyApplicator_MultipleDocketsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket1 = GetNewDocket(Helper, data.Org1, data.Whs1, "R1");
			var type = docket1.Description;

			GetNewDocketLine(Helper, docket1, data.Part1, 10m);

			var docket2 = GetNewDocket(Helper, data.Org1, data.Whs1, "R2");

			var docket3 = GetNewDocket(Helper, data.Org1, data.Whs1, "R3");
			GetNewDocketLine(Helper, docket3, data.Part1, 10m);

			PrepareDocketsForFinalisation(Helper, docket1, docket2, docket3);

			Factory.Save();
			ApplyApplicator(new[] { docket1, docket2, docket3 }, $@"
INFO: {docket1.Description} [HL W00000001] - was successfully finalized.
WARNING: {docket2.Description} [HL W00000002] - could not be auto-finalized. It must be manually finalized.
INFO: {docket3.Description} [HL W00000003] - was successfully finalized.".Trim());
		}

		#endregion

		#region TestApplyApplicator_EachDocketIsFinalisedInSeparateFactory

		public void TestApplyApplicator_EachDocketIsFinalisedInSeparateFactory()
		{
			var otherFactory = new BusinessObjectFactory();
			var helperInOtherFactory = new WhsTestHelperFunctions(otherFactory);
			var data = new TestDataSimpleEnvironment(otherFactory);
			var docket1InOtherFactory = GetNewDocket(helperInOtherFactory, data.Org1, data.Whs1, "R1");
			GetNewDocketLine(helperInOtherFactory, docket1InOtherFactory, data.Part1, 10m);
			var docket2InOtherFactory = GetNewDocket(helperInOtherFactory, data.Org1, data.Whs1, "R2");
			GetNewDocketLine(helperInOtherFactory, docket2InOtherFactory, data.Part1, 10m);

			PrepareDocketsForFinalisation(helperInOtherFactory, docket1InOtherFactory, docket2InOtherFactory);
			otherFactory.Save();

			var docket1 = Factory.Load<WhsDocket>(docket1InOtherFactory.PK);
			var docket2 = Factory.Load<WhsDocket>(docket2InOtherFactory.PK);
			ApplyApplicator(new[] { docket1, docket2 }, $@"
INFO: {docket1.Description} [HL W00000001] - was successfully finalized.
INFO: {docket2.Description} [HL W00000002] - was successfully finalized.".Trim());
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket1); // this should show as finalized due to data refresh
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket2);

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			AssertEquals("No docket lines should be loaded in Operational Actions Factory. Separate factory should be used for finalisation.", 0, Factory.Load<WhsDocketLine>(query).Length);
		}

		#endregion

		#region TestApplicator_ConcurrencyError

		public void TestApplicator_ConcurrencyError()
		{
			if (ShouldRunTestApplicator_ConcurrencyError)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var docket = GetNewDocket(Helper, data.Org1, data.Whs1);
				GetNewDocketLine(Helper, docket, data.Part1, 10m);
				PrepareDocketsForFinalisation(Helper, docket);
				Factory.Save();

				var applicator = (TApplicator)GetNewBusinessObject();

				applicator.UserModifyDataInDBAction += (f) =>
				{
					var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var docketToModify = otherFactory.Load<WhsDocket>(docket.PK);
					docketToModify.Lines.DeleteAll();
					otherFactory.Save();
				};

				var log = new DummyOperationalActionSectionLog();
				AssertNoExceptionThrown("Should not throw exception", () => applicator.Apply(log, new[] { docket }));
				AssertEquals($@"WARNING: {docket.Description} [HL W00000001] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this form to try again.",
					log.MessagesString().Trim());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ShouldRunTestApplicator_ConcurrencyError => true;

		#endregion

		#region Implementation

		protected abstract TDocket GetNewDocket(WhsTestHelperFunctions helper, OrgHeader client, WhsWarehouse warehouse, string reference = "1");

		protected abstract WhsDocketLine GetNewDocketLine(WhsTestHelperFunctions helper, TDocket docket, OrgSupplierPart part, ZDecimal quantity);

		protected virtual void PrepareDocketsForFinalisation(WhsTestHelperFunctions helper, params TDocket[] dockets)
		{
		}

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
