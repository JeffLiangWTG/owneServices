using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusRelatedEventsProviderTest : TestCaseWithFactory
	{
		[TestDate(2014, 07, 30)]
		public void TestGetDateOfFirstArrivalQuery()
		{
			Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var currentCountry = Env.CurrentCompany.Country.Code;
			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			leg1.JW_RL_NKLoadPort = currentCountry + "AAA";
			leg1.JW_RL_NKDiscPort = "USLAX";
			leg1.JW_ETA = ZDateTime.Today;
			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			leg2.JW_RL_NKLoadPort = "USCHI";
			leg2.JW_RL_NKDiscPort = currentCountry + "BBB";
			leg2.JW_ETA = ZDateTime.Today.AddMonths(2);

			var mawb1 = Factory.NewWithValidTestData<CusMAWB>();
			mawb1.CM_ArrivalDate = ZDateTime.Empty;
			var mawb2 = Factory.NewWithValidTestData<CusMAWB>();
			mawb2.CM_ArrivalDate = ZDateTime.Today.AddMonths(1);
			var mawb3 = Factory.NewWithValidTestData<CusMAWB>();
			mawb3.CM_ArrivalDate = ZDateTime.Today;
			var mawb4 = Factory.NewWithValidTestData<CusMAWB>();
			mawb4.CM_ArrivalDate = ZDateTime.Today.AddMonths(3);
			var mawb5 = Factory.NewWithValidTestData<CusMAWB>();
			mawb5.CM_ArrivalDate = ZDateTime.Today.AddMonths(4);

			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(CusMAWB));
			query.AddToFilter(provider.GetDateOfFirstArrivalQueryForTesting(consol));
			var mawbs = Factory.Load<CusMAWB>(query);
			Assert(mawbs.Contains(mawb1));
			Assert(mawbs.Contains(mawb2));
			Assert(!mawbs.Contains(mawb3));
			Assert(mawbs.Contains(mawb4));
			Assert(!mawbs.Contains(mawb5));
		}

		public void TestPivotHouseBills()
		{
			var cusOceanBill = Factory.New<TestCusSCAOceanBill>();
			cusOceanBill.CB_OceanBill = "MB1234567";
			cusOceanBill.CB_MasterHouseBill = "HB235689";

			var cusHouse = Factory.New<TestCusSCAHouse>();
			cusHouse.CA_CB = cusOceanBill.PK;
			cusHouse.CA_HouseBill = "J000001";

			var cusContainer = Factory.New<TestCusSCAContainer>();
			cusContainer.CN_CB = cusOceanBill.PK;
			cusContainer.CN_ContainerNumber = "CONT123456";

			var cusPivot = Factory.New<TestCusSCAPivot>();
			cusPivot.CV_CN = cusContainer.PK;
			cusPivot.CV_CA = cusHouse.PK;

			consol.JK_MasterBillNum = "MB1234567";
			shipment.JS_HouseBill = "HB235689";

			var container0 = consol.Containers.AddNew();
			container0.JC_ContainerNum = "CONT123456";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT24566";

			var packLine0 = shipment.OuterPackLines.AddNew();
			packLine0.JL_JC = container0.PK;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;

			Factory.Save();

			var relatedHouseBills = provider.CusRelatedBusinessObjects(shipment, "J000001");
			AssertNotNull(relatedHouseBills);
			AssertCollectionContains(cusHouse, relatedHouseBills);
			AssertCollectionContains(cusPivot, relatedHouseBills);
		}

		public void TestMasterBills()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = Factory.NewWithValidTestData<CusHAWB>();
			mawb.CM_MAWB = "12312341234";
			mawb.CM_MasterHouseBill = "HB111000";
			mawb.CM_ApplicationCode = "TST";

			hawb.CS_CM = mawb.PK;
			hawb.CS_MasterHouseBill = "HB111000";
			Factory.Save();

			AssertCollectionNotContains(mawb, provider.CusRelatedParentBusinessObjects(shipment));

			shipment.JS_HouseBill = "HB111000";
			consol.JK_MasterBillNum = mawb.CM_MAWB;
			hawb.CS_JS = shipment.PK;
			mawb.CM_JK = consol.PK;
			Factory.Save();

			AssertCollectionContains("The master bill should be part of the shipment's BusinessObjectsWithRelatedEvents", mawb, provider.CusRelatedParentBusinessObjects(shipment));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			provider = new CusRelatedEventsProviderForTest();
			provider.SetupShipment(shipment);

			Factory.Save();
		}
		ForwardingConsol consol;
		ForwardingShipment shipment;
		CusRelatedEventsProviderForTest provider;

		#endregion
	}
}
