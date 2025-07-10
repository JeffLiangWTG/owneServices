using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingHeaserProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadWhenIDInvalid()
		{
			var strategy = new RatingHeaderProcessTaskLoadStrategy();
			AssertEquals(null, strategy.GetTypeForLoad(JobShipmentSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertEquals(null, strategy.GetTypeForLoad(JobShipmentSchema.Constants.Prefix, ZGuid.Invalid, Factory));
		}

		public void TestGetTypeForLoad()
		{
			var ratingHeader = Factory.New<Quote>() as RatingHeader;
			var strategy = new RatingHeaderProcessTaskLoadStrategy();
			AssertEquals(typeof(QuotationProcessTask), strategy.GetTypeForLoad(ratingHeader.TablePrefix, ratingHeader.PK, Factory));

			ratingHeader = Factory.New<ClientRate>();
			AssertEquals(typeof(ClientRateProcessTask), strategy.GetTypeForLoad(ratingHeader.TablePrefix, ratingHeader.PK, Factory));

			ratingHeader = Factory.New<CompanyTariff>();
			AssertEquals(typeof(CompanyTariffProcessTask), strategy.GetTypeForLoad(ratingHeader.TablePrefix, ratingHeader.PK, Factory));
		}

		public void TestGetTypeForLoadWhenParentNotInCache()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.New<Quote>();
			quote.TH_OH = orgHeader.PK;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = orgHeader.PK;

			var companyTariff = Factory.New<CompanyTariff>();

			Factory.Save();

			var strategy = new RatingHeaderProcessTaskLoadStrategy();

			var factory2 = new BusinessObjectFactory();
			AssertEquals(typeof(QuotationProcessTask), strategy.GetTypeForLoad(quote.TablePrefix, quote.PK, factory2));
			AssertEquals(typeof(ClientRateProcessTask), strategy.GetTypeForLoad(clientRate.TablePrefix, clientRate.PK, factory2));
			AssertEquals(typeof(CompanyTariffProcessTask), strategy.GetTypeForLoad(companyTariff.TablePrefix, companyTariff.PK, factory2));
		}

		public void TestAddAdditionalParentFilters()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Factory.New<Quote>();
			quote.TH_OH = orgHeader.PK;
			quote.FillWithValidTestData();
			var quotationProcessTask = ((IWorkflowProvider)quote).WorkflowItems.AddNew();

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = orgHeader.PK;
			clientRate.FillWithValidTestData();
			var clientRateProcessTask = ((IWorkflowProvider)clientRate).WorkflowItems.AddNew();

			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.FillWithValidTestData();
			var companyTariffProcessTask = ((IWorkflowProvider)companyTariff).WorkflowItems.AddNew();

			Factory.Save();

			var tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.QuotationWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(quotationProcessTask, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.ClientRateWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(clientRateProcessTask, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.CompanyTariffsWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(companyTariffProcessTask, tasks);
		}

		ZDBOnlyQuery GetNewQueryForTestAddAdditionalParentFilters(string workflowTypeCode)
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowTypeCode);

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(RatingHeader), ProcessTasksSchema.P9_ParentID);
			var strategy = new RatingHeaderProcessTaskLoadStrategy();
			strategy.AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}
	}
}
