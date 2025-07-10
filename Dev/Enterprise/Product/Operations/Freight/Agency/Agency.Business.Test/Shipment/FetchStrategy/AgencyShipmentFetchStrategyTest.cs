using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			CreateShipments();
			var factory = new BusinessObjectFactory();
			_ = factory.Load<AgencyShipment>(new ZQuery());
			AssertMaxDbHits(1, factory);
		}

		public void TestFetchForView_BookingPartyNameOrPK()
			=> AssertFetchForView("BookingPartyNameOrPK", new Dictionary<string, int> { { JobDocAddress.Schema.TableName, 1 } });

		public void TestFetchForView_ConsigneeNameOrPK()
			=> AssertFetchForView("ConsigneeNameOrPK", new Dictionary<string, int> { { JobDocAddress.Schema.TableName, 1 } });

		public void TestFetchForView_ConsignorContact()
			=> AssertFetchForView("ConsignorContact", new Dictionary<string, int> { { JobDocAddress.Schema.TableName, 1 } });

		public void TestFetchForView_ConsignorNameOrPK()
			=> AssertFetchForView("ConsignorNameOrPK", new Dictionary<string, int> { { JobDocAddress.Schema.TableName, 1 } });

		public void TestFetchForView_Job_JH_Status()
			=> AssertFetchForView("Job+JH_Status", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_Job_JH_HoldReason()
			=> AssertFetchForView("Job+JH_HoldReason", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_Job_JH_ProfitLossReasonCode()
			=> AssertFetchForView("Job+JH_ProfitLossReasonCode", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_Job_JH_TotalProfitRevenueMargin()
			=> AssertFetchForView("Job+JH_TotalProfitRevenueMargin", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_NumbersAsString()
			=> AssertFetchForView("NumbersAsString", new Dictionary<string, int> { { CusEntryNumber.Schema.TableName, 1 } });

		#region Implementation

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			CreateShipments();

			var newFactory = NewFactory();
			var agencyShipments = newFactory.Load<AgencyShipment>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var agencyShipment in agencyShipments)
			{
				agencyShipment.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var agencyShipment in agencyShipments)
			{
				_ = agencyShipment[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		void CreateShipments()
		{
			for (var i = 0; i < 12; i++)
			{
				CreateAgencyShipment(i);
			}

			Factory.Save();
		}

		void CreateAgencyShipment(int uniqueRef)
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_UniqueConsignRef = $"S000001{uniqueRef:00}";
		}

		#endregion
	}
}
