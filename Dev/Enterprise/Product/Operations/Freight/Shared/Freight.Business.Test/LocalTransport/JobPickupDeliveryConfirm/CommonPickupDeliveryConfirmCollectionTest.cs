using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;
using static Enterprise.Freight.Business.CommonPickupDeliveryConfirmCollection;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonPickupDeliveryConfirmCollection))]
	sealed class CommonPickupDeliveryConfirmCollectionTest : ActiveBusinessObjectCollectionTestCase<CommonPickupDeliveryConfirmCollection>
	{
		protected override CommonPickupDeliveryConfirmCollection GetCollectionToTest()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			return new CommonPickupDeliveryConfirmCollection(Factory, shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
		}

		public void TestSortingByShipment()
		{
			var shipment1 = Factory.New<CommonShipment>();
			shipment1.OuterPackLines.AddNew();
			var confirm1 = shipment1.DeliveryConfirms.AddNew();
			confirm1.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.OuterPackLines.AddNew();
			var confirm2 = shipment2.DeliveryConfirms.AddNew();
			confirm2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var confirms = new CommonPickupDeliveryConfirmCollection(newFactory);
			confirms.ApplySort("FullGatePass", System.ComponentModel.ListSortDirection.Ascending);
			confirms.AdditionalFilter = new ZQuery();
			AssertEquals(2, confirms.Count);
		}

		public void TestNoMissindDivotWhenPacklineAndConfirmAreAddedInDiffrentFactories()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 3;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 30;
			packline.JL_ActualVolume = 20;

			var consol = shipment.Consols.AddNew();
			consol.Shipments.Add(shipment);

			Factory.Save();

			var shipmentFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = shipmentFactory.Load<CommonShipment>(shipment.PK);
			var confirm = shipmentInNewFactory.PickupConfirms.AddNew();
			confirm.FillWithValidTestData();

			var consolFactory = new BusinessObjectFactory();
			var consolInNewFactory = consolFactory.Load<CommonConsol>(consol.PK);

			var firstShipment = consolInNewFactory.Shipments.Cast<CommonShipment>().First();
			var packline2 = firstShipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 5;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 60;
			packline2.JL_ActualVolume = 70;

			var thisIsAHack = firstShipment.PickupConfirms.Count;  //force ActiveBusinessObjectCollection index to load, data refresh depends on it.

			shipmentFactory.Save();
			consolFactory.Save();

			var resultFactory = new BusinessObjectFactory();
			var resultConfirm = resultFactory.Load<CommonPickupDeliveryConfirm>(confirm.PK);
			AssertEquals(2, resultConfirm.Divots.Count);
		}

		public void TestNoMissindDivotWhenPacklineAndConfirmAreAddedInDiffrentFactories_MultipleConfirmsAndPacklinesAdded()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 3;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 30;
			packline.JL_ActualVolume = 20;

			var consol = shipment.Consols.AddNew();
			consol.Shipments.Add(shipment);

			Factory.Save();

			var shipmentFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = shipmentFactory.Load<CommonShipment>(shipment.PK);
			var confirm1 = shipmentInNewFactory.PickupConfirms.AddNew();
			confirm1.FillWithValidTestData();
			confirm1.EU_DropMode = "ANY";
			confirm1.Divots.First().J8_PackagesDelivered = 1;

			var confirm2 = shipmentInNewFactory.PickupConfirms.AddNew();
			confirm2.FillWithValidTestData();
			confirm2.EU_DropMode = "ANY";
			confirm2.Divots.First().J8_PackagesDelivered = 1;

			var confirm3 = shipmentInNewFactory.PickupConfirms.AddNew();
			confirm3.FillWithValidTestData();
			confirm3.EU_DropMode = "ANY";
			confirm3.Divots.First().J8_PackagesDelivered = 1;

			var consolFactory = new BusinessObjectFactory();
			var consolInNewFactory = consolFactory.Load<CommonConsol>(consol.PK);

			var firstShipment = consolInNewFactory.Shipments.Cast<CommonShipment>().First();
			var packline2 = firstShipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 5;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 60;
			packline2.JL_ActualVolume = 70;

			var packline3 = firstShipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 11;
			packline3.JL_ActualVolume = 32;

			var packline4 = firstShipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 6;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 44;
			packline4.JL_ActualVolume = 66;

			var thisIsAHack = firstShipment.PickupConfirms.Count;  //force ActiveBusinessObjectCollection index to load, data refresh depends on it.

			shipmentFactory.Save();
			consolFactory.Save();

			var resultFactory = new BusinessObjectFactory();
			foreach (var confirm in new[] { confirm1, confirm2, confirm3 })
			{
				var confirmInResultFacotry = resultFactory.Load<CommonPickupDeliveryConfirm>(confirm.PK);
				AssertEquals(4, confirmInResultFacotry.Divots.Count);
			}
		}

		public void TestDivotsAreNotCreatedForSubShipmentConfirmsOnMasterShipmentConfirmCollection()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subShipment = masterShipment.CoLoadShipments.AddNew();
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline = subShipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 4;

			var confirm1 = Factory.New<CommonPickupDeliveryConfirm>();
			confirm1.EU_PickupDeliveryType = "PCU";
			confirm1.EU_JS = subShipment.PK;
			subShipment.PickupConfirms.Add(confirm1);

			var confirm2 = Factory.New<CommonPickupDeliveryConfirm>();
			confirm2.EU_PickupDeliveryType = "PCU";
			confirm2.EU_JS = subShipment.PK;
			masterShipment.PickupConfirms.Add(confirm2);

			AssertEquals("Divot should be created when shipment matches.", 1, confirm1.Divots.Count);
			AssertEquals("Divot should not be created when sub-shipment confirm is added to master shipment collection.", 0, confirm2.Divots.Count);
		}

		[TestDate(2021, 1, 1)]
		public void TestGetDivotInformation()
		{
			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var confirm = shipment.PickupConfirms.AddNew();
			var divot = packline.ConfirmDivots.First();

			var expected = FormattableString.Invariant($@"Duplicate divots (1):
Divot PK: '{divot.PK}',
J8_JL: '{packline.PK}',
J8_EU_PickupDeliverConfirm: '{confirm.PK}',
J8_PackagesDelivered: '0',
J8_DeliveryWeight: '0',
J8_DeliveryVolume: '0',
IsInDatabase: 'False',
J8_SystemCreateTimeUtc: '',
J8_SystemCreateUser: ''
PackLine: IsInDatabase: 'False', JL_SystemCreateTimeUtc: '', JL_SystemCreateUser: '', JL_SystemLastEditTimeUtc: '', JL_SystemLastEditUser: ''

Confirms of duplicate divots (1):
IsInDatabase: 'False', EU_PickupDeliveryType: 'PCU', EU_JS: '{shipment.PK}', EU_D1: '{Guid.Empty}', EU_JC: '{Guid.Empty}'
EU_SystemCreateTimeUtc: '', EU_SystemCreateUser: '', EU_SystemLastEditTimeUtc: '', EU_SystemLastEditUser: ''
");

			var debugInfo = CommonPickupDeliveryConfirmRelationship.GetDivotDebugInformation(divot, new List<CommonConfirmDivot>() { divot });
			AssertEquals(expected, debugInfo);
		}

		[TestDate(2021, 1, 1)]
		public void TestGetDivotInformationWithDuplicates()
		{
			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var confirm = shipment.PickupConfirms.AddNew();
			var divot = packline.ConfirmDivots.First();

			var divot2 = packline.ConfirmDivots.AddNew();
			divot2.J8_JL = divot.J8_JL;
			divot2.J8_EU_PickupDeliverConfirm = divot.J8_EU_PickupDeliverConfirm;

			var expected = FormattableString.Invariant($@"Duplicate divots (2):
Divot PK: '{divot.PK}',
J8_JL: '{packline.PK}',
J8_EU_PickupDeliverConfirm: '{confirm.PK}',
J8_PackagesDelivered: '0',
J8_DeliveryWeight: '0',
J8_DeliveryVolume: '0',
IsInDatabase: 'False',
J8_SystemCreateTimeUtc: '',
J8_SystemCreateUser: ''
PackLine: IsInDatabase: 'False', JL_SystemCreateTimeUtc: '', JL_SystemCreateUser: '', JL_SystemLastEditTimeUtc: '', JL_SystemLastEditUser: ''
Divot PK: '{divot2.PK}',
J8_JL: '{packline.PK}',
J8_EU_PickupDeliverConfirm: '{confirm.PK}',
J8_PackagesDelivered: '0',
J8_DeliveryWeight: '0',
J8_DeliveryVolume: '0',
IsInDatabase: 'False',
J8_SystemCreateTimeUtc: '',
J8_SystemCreateUser: ''
PackLine: IsInDatabase: 'False', JL_SystemCreateTimeUtc: '', JL_SystemCreateUser: '', JL_SystemLastEditTimeUtc: '', JL_SystemLastEditUser: ''

Confirms of duplicate divots (1):
IsInDatabase: 'False', EU_PickupDeliveryType: 'PCU', EU_JS: '{shipment.PK}', EU_D1: '{Guid.Empty}', EU_JC: '{Guid.Empty}'
EU_SystemCreateTimeUtc: '', EU_SystemCreateUser: '', EU_SystemLastEditTimeUtc: '', EU_SystemLastEditUser: ''
");

			AssertEquals(expected, CommonPickupDeliveryConfirmRelationship.GetDivotDebugInformation(divot, new List<CommonConfirmDivot>() { divot, divot2 }));
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Does not currently support remove, only delete", true);
		}

		public void TestJ8_JLCanBeEmpty()
		{
			var commonPickupDeliveryConfirm = Factory.New<CommonPickupDeliveryConfirm>();

			CommonConfirmDivot divot = Factory.New<CommonConfirmDivot>();
			divot.J8_JL = ZGuid.Empty;
			commonPickupDeliveryConfirm.Divots.Add(divot);

			var testConfirmCollection = GetCollectionToTest();
			AssertNoExceptionThrown(() => { testConfirmCollection.Add(commonPickupDeliveryConfirm); });
		}
	}
}
