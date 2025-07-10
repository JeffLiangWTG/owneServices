using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeclarationFromShipmentPuller))]
	sealed class DeclarationFromShipmentPullerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Shipment", null, Puller.Shipment);
			Puller.ShipmentPK = shipment.PK;
			AssertEquals("Shipment", shipment, Puller.Shipment);
		}

		public void TestCreateDeclarationForShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertEquals("Result", null, Puller.CreateOrLoadDeclarationForShipment(secondFactory));
			Puller.ShipmentPK = shipment.PK;
			BaseJobDeclaration dec = Puller.CreateOrLoadDeclarationForShipment(secondFactory);
			AssertEquals("Shipment", shipment.PK, dec.Shipment.PK);
			AssertEquals("Decs Factory", secondFactory, dec.Factory);

			BaseJobDeclaration secondCallDec = Puller.CreateOrLoadDeclarationForShipment(secondFactory);
			AssertEquals("Declaration Received On Second Call", dec, secondCallDec);
		}

		public void TestValidateShipmentPK()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			Puller.ShipmentPK = new ZGuid("D967307A-42E5-4DD7-9105-C2A0AFAB6127");
			AssertHasError(Puller.ShipmentPKInfo, DeclarationFromShipmentPuller.InvalidShipment);

			Puller.ShipmentPK = shipment.PK;
			AssertNoError(Puller.ShipmentPKInfo, DeclarationFromShipmentPuller.InvalidShipment);
		}

		#region Implementation

		DeclarationFromShipmentPuller fPuller;
		DeclarationFromShipmentPuller Puller
		{
			get
			{
				if (fPuller == null)
				{
					fPuller = new DeclarationFromShipmentPuller(Factory);
				}
				return fPuller;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationFromShipmentPuller(Factory);
		}

		#endregion
	}
}
