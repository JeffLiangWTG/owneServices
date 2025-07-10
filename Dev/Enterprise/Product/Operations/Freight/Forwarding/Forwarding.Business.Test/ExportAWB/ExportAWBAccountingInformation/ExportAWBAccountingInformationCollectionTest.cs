using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBAccountingInformationCollection))]
	sealed class ExportAWBAccountingInformationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportAWBAccountingInformationCollection(AWBHeader, Factory);
		}

		ShipmentExportAWBHeader AWBHeader
		{
			get
			{
				if (awbHeader == null)
				{
					ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_OverrideWaybillDefaults = true;
					return (awbHeader = (ShipmentExportAWBHeader)shipment.AWBHeader);
				}

				return awbHeader;
			}
		}
		ShipmentExportAWBHeader awbHeader;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ExportAWBAccountingInformation>();
		}

		public void TestSequence()
		{
			ExportAWBAccountingInformationCollection collection = ((ExportAWBAccountingInformationCollection)Collection);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			AssertEquals((ZByte)0, collection[0].EA_Sequence);
			AssertEquals((ZByte)1, collection[1].EA_Sequence);
			AssertEquals((ZByte)2, collection[2].EA_Sequence);
			AssertEquals((ZByte)3, collection[3].EA_Sequence);

			collection[1].EA_Sequence = 5;
			collection.AddNew().EA_InformationID = "AAE";
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			ShipmentExportAWBHeader master2 = factory.Load<ShipmentExportAWBHeader>(AWBHeader.PK);
			ExportAWBAccountingInformationCollection collection2 = new ExportAWBAccountingInformationCollection(master2, factory);
			collection2.Load();

			AssertEquals((ZByte)0, collection2[0].EA_Sequence);
			AssertEquals((ZByte)2, collection2[1].EA_Sequence);
			AssertEquals((ZByte)3, collection2[2].EA_Sequence);
			AssertEquals((ZByte)5, collection2[3].EA_Sequence);
			AssertEquals((ZByte)6, collection2[4].EA_Sequence);
		}
	}
}
