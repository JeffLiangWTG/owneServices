using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(ManifestBillSynchroniser<>))]
	public abstract class ManifestBillSynchroniserTest : SynchroniserTestCase
	{
		public void TestBillNumberShouldBeInSyncAsHouseBillNumberChanges()
		{
			var manifestBill = GetManifestBill(Consol);
			var synchroniser = GetManifestBillSynchroniser(manifestBill, Shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			var oldHouseBill = GetMasterBillNumber(shipment);
			AssertEquals(oldHouseBill, manifestBill.MasterBillNumberInfo.Value);
			Shipment.JS_HouseBill = "BILL1";
			var newHouseBill = GetMasterBillNumber(shipment);
			AssertEquals(newHouseBill, manifestBill.MasterBillNumberInfo.Value);
		}

		public void TestManifestBillSynchroniser()
		{
			var manifestBill = GetManifestBill(Consol);
			var synchroniser = GetManifestBillSynchroniser(manifestBill, Shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertGeneralPropertiesSynchronisedResult(manifestBill, Shipment);
			AssertDocAddressesSynchronisedResult(manifestBill, Shipment);
			AssertSpecficPropertiesSynchronisedResult(manifestBill, Shipment);
		}

		public void TestSynchroniserWhenAddressValidationIsDisabled()
		{
			var rawEnableSetting = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = false;

				var manifestBill = GetManifestBill(Consol);
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = "House123";
				shipment.JS_ActualWeight = 100;
				shipment.JS_UnitOfWeight = "KG";
				shipment.JS_ActualVolume = 50;
				shipment.JS_UnitOfVolume = "M3";
				shipment.JS_OuterPacks = 10;
				shipment.JS_F3_NKPackType = "UT";
				Shipment.ConsignorDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				Shipment.ConsignorDocumentaryAddress.ValidationStatus = AddressValidationStatus.Invalid;
				Shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				Shipment.NotifyPartyDocumentaryAddress.ValidationStatus = AddressValidationStatus.Invalid;
				Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				Shipment.ConsigneeDocumentaryAddress.ValidationStatus = AddressValidationStatus.Invalid;
				var synchroniser = GetManifestBillSynchroniser(manifestBill, Shipment);
				synchroniser.SetEnabled(true, false);
				synchroniser.Synchronise();

				AssertDocAddressesSynchronisedResult(manifestBill, Shipment);
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawEnableSetting;
			}
		}

		protected void AssertGeneralPropertiesSynchronisedResult(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
		{
			AssertEquals("MasterBillNumberInfo", GetMasterBillNumber(shipment), bill.MasterBillNumberInfo.Value);
			AssertEquals("PortOfLadingInfo", GetPortOfLading(shipment), bill.PortOfLadingInfo.Value);
			AssertEquals("PlaceOfReceiptInfo", GetPlaceOfReceipt(shipment), bill.PlaceOfReceiptInfo.Value);
			AssertEquals("LastForeignPortInfo", GetLastForeignPort(shipment), bill.LastForeignPortInfo.Value);
			AssertEquals("WeightInfo", GetWeight(shipment), bill.WeightInfo.Value);
			AssertEquals("WeightUQInfo", GetWeightUQ(shipment), bill.WeightUQInfo.Value);
			AssertEquals("VolumeInfo", GetVolume(shipment), bill.VolumeInfo.Value);
			AssertEquals("VolumeUQInfo", GetVolumeUQ(shipment), bill.VolumeUQInfo.Value);
			AssertEquals("ManifestQtyInfo", GetManifestQty(shipment), bill.ManifestQtyInfo.Value);
			AssertEquals("ManifestUQInfo", GetManifestUQ(shipment), bill.ManifestUQInfo.Value);
		}

		protected void AssertSynchroniseJobDocAddress(JobDocAddress source, IManifestBillAddress destination)
		{
			AssertNotEquals(source, destination);
			AssertEquals("JobDocAddress", source.E2_OA_Address, destination.OA_AddressInfo.Value);

			source.E2_OA_Address = JobDocAddress.New(Shipment).PK;
			AssertEquals("JobDocAddress Synchronised automatically when source changed", source.E2_OA_Address, destination.OA_AddressInfo.Value);
		}

		protected virtual void AssertDocAddressesSynchronisedResult(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
		{
			AssertSynchroniseJobDocAddress(shipment.ConsignorDocumentaryAddress, bill.ForeignShipper);
			AssertSynchroniseJobDocAddress(shipment.NotifyPartyDocumentaryAddress, bill.NotifyParty1);
			AssertSynchroniseJobDocAddress(shipment.ConsigneeDocumentaryAddress, bill.Consignee);
		}

		protected virtual void AssertSpecficPropertiesSynchronisedResult(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
		{
		}

		protected virtual ZString GetMasterBillNumber(ForwardingShipment shipment) => shipment.JS_HouseBill;
		protected abstract ZString GetPortOfLading(ForwardingShipment shipment);
		protected abstract ZString GetPlaceOfReceipt(ForwardingShipment shipment);
		protected abstract ZString GetLastForeignPort(ForwardingShipment shipment);
		protected virtual ZDecimal GetWeight(ForwardingShipment shipment) => shipment.JS_ActualWeight;
		protected virtual ZString GetWeightUQ(ForwardingShipment shipment) => shipment.JS_UnitOfWeight;
		protected virtual ZDecimal GetVolume(ForwardingShipment shipment) => shipment.JS_ActualVolume;
		protected virtual ZString GetVolumeUQ(ForwardingShipment shipment) => shipment.JS_UnitOfVolume;
		protected virtual ZInt GetManifestQty(ForwardingShipment shipment) => shipment.JS_OuterPacks;
		protected virtual ZString GetManifestUQ(ForwardingShipment shipment) => shipment.JS_F3_NKPackType;

		protected ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Shipment.Consols.AddNew();
					consol.JK_AgentType = Core.Constants.AgentType.Direct;
					consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				}
				return consol;
			}
		}
		ForwardingConsol consol;

		protected ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					shipment.JS_HouseBill = "House123";
					shipment.JS_ActualWeight = 100;
					shipment.JS_UnitOfWeight = "KG";
					shipment.JS_ActualVolume = 50;
					shipment.JS_UnitOfVolume = "M3";
					shipment.JS_OuterPacks = 10;
					shipment.JS_F3_NKPackType = "UT";
					Shipment.ConsignorDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
					Shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
					Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		protected abstract IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol);

		protected abstract BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment);

		public Type TestsInstancesOfType => GetManifestBillSynchroniser(GetManifestBill(Consol), Shipment).GetType();
	}
}
