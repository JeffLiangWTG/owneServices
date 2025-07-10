using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobDocsAndCartageFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadChildEditableObjectsCore()
		{
			var query = new ZQuery(JobDocsAndCartageSchema.PK, CreateDocsAndCartageInNewFactory());

			Factory.ResetDatabaseLoadCount();

			var docsAndCartageObjects = Factory.Load<JobDocsAndCartage>(query);
			foreach (var docsAndCartage in docsAndCartageObjects)
			{
				var orderItems = docsAndCartage.OrderItems;
				var services = docsAndCartage.Services;
				var docs = docsAndCartage.RequiredDocuments;
			}

			//JobDocsAndCartage: 1
			//JobOrderItem: 1
			//JobRequiredDocument: 1
			//JobService: 1

			//Hits: 4/1

			AssertMaxDbHits(4, Factory);
		}

		List<ZGuid> CreateDocsAndCartageInNewFactory()
		{
			var results = new List<ZGuid>();
			var createrFactory = new BusinessObjectFactory();

			var shipment1 = createrFactory.New<CommonShipment>();
			var shipment2 = createrFactory.New<CommonShipment>();
			results.Add(JobDocsAndCartage.New(shipment1).PK);
			results.Add(JobDocsAndCartage.New(shipment2).PK);

			var declaration = createrFactory.New<IBaseJobDeclaration>();
			results.Add(JobDocsAndCartage.New((IDocsAndCartageParent)declaration).PK);

			createrFactory.Save();

			return results;
		}
	}
}
