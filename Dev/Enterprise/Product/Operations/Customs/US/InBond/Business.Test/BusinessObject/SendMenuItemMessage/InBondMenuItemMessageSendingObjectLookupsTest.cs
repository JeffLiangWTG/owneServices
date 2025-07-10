using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondMenuItemMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImporterList()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			ConsigneeCollection collection = inBondMenuItemMessageSendingObject.Lookups.ImporterList;
			AssertNotNull(collection);
		}

		public void TestShippingProviders()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			ShippingProviderCollection collection = inBondMenuItemMessageSendingObject.Lookups.ShippingProviders;
			AssertNotNull(collection);
		}

		public void TestCarrierCollection()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			USCarrierCombinedCollection collection = inBondMenuItemMessageSendingObject.Lookups.CarrierCollection;
			AssertNotNull(collection);
		}

		public void TestRegionDistrictPorts()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			var collection = inBondMenuItemMessageSendingObject.Lookups.RegionDistrictPorts;
			AssertNotNull(collection);
		}

		public void TestForeignPorts()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			ZZRefCusCodeListCombinedCollection collection = inBondMenuItemMessageSendingObject.Lookups.ForeignPorts;
			AssertNotNull(collection);
		}

		public void TestMessageStatusList()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			AssertEquals(Factory.GetCachedValue<ImportMessageStatusList>(), inBondMenuItemMessageSendingObject.Lookups.MessageStatusList);
		}

		public void TestEntryTypeList()
		{
			var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			var list = inBondMenuItemMessageSendingObject.Lookups.EntryTypeList;
			AssertEquals(2, list.Count);
			AssertEquals("Transport and Export", InbondCommonTypeList.Descriptions._2TransportandExport, list.GetDescriptionFromCode("62"));
			AssertEquals("Immediate Export", InbondCommonTypeList.Descriptions._3ImmediateExport, list.GetDescriptionFromCode("63"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
		}

		InBondMenuItemMessageData inBondMenuItemMessageData;
	}
}
