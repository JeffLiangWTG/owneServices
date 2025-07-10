using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassShipment))]
	public class GatePassShipmentTest : CFSBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		#region Test Save Performance

		public void TestSavePerformance()
		{
			GatePassShipment motherShip = Factory.New<GatePassShipment>();
			CFSLoadListConsol mainConsol = motherShip.Consols.AddNew();
			GatePassShipment lastChild = AddShipmentsToConsol(mainConsol);

			for (int i = 0; i <= 5; i++)
			{
				lastChild = AddShipmentsToConsol(lastChild.Consols.AddNew());
			}

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			motherShip = newFactory.Load<GatePassShipment>(motherShip.PK);

			motherShip.JS_HouseBill = "dd";
			newFactory.Save();

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			GatePassShipment[] shipmentsLoadedIntoFactoryWhenSavingOneShipment = newFactory.Load<GatePassShipment>(query);
			AssertEquals("Saving one shipment shouldn't load all related shipments", 1, shipmentsLoadedIntoFactoryWhenSavingOneShipment.Length);
		}

		GatePassShipment AddShipmentsToConsol(CFSLoadListConsol mainConsol)
		{
			GatePassShipment result = null;
			for (int i = 0; i <= 15; i++)
			{
				result = (GatePassShipment)mainConsol.Shipments.AddNew();
				result.DeliveryConfirms.AddNew();
				result.DeliveryConfirms.AddNew();
			}
			return result;
		}

		#endregion
		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Now;
			return shipment;
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestSavedShipmentCannotBeDeleted()
		{
			BusinessObject shipment = GetNewBusinessObject();
			Factory.Save();
			shipment.Delete();
		}

		[ExpectNoExceptions]
		public void TestUnSavedShipmentCanBeDeleted()
		{
			BusinessObject shipment = GetNewBusinessObject();
			shipment.Delete();
		}
	}
}
