using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.Business
{
	public abstract class ManifestBillSynchroniser<TBillDest> : BusinessObjectSynchroniser
					where TBillDest : BusinessObject, IManifestBillForSynchroniser
	{
		protected ManifestBillSynchroniser(TBillDest destination, ForwardingShipment shipmentSource)
					: base(destination, shipmentSource)
		{
			var header = destination.Header;
			this.consolSource = (ForwardingConsol)header?.Consol;
		}

		public new TBillDest Destination => (TBillDest)base.Destination;

		public new ForwardingShipment Source => (ForwardingShipment)base.Source;

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.MasterBillNumberInfo, GetBillNumber, GetInfosAffectingBillNumber));
			portOfLadingSynchroniser = new FieldSynchroniser(Destination.PortOfLadingInfo, GetPortOfLading, GetInfosAffectingPortOfLading);
			Synchronisers.Add(portOfLadingSynchroniser);
			placeOfReceiptSynchroniser = new FieldSynchroniser(Destination.PlaceOfReceiptInfo, GetPlaceOfReceipt, GetInfosAffectingPlaceOfReceipt);
			Synchronisers.Add(placeOfReceiptSynchroniser);
			lastForeignPortSynchroniser = new FieldSynchroniser(Destination.LastForeignPortInfo, GetLastForeignPort, GetLastForeignPortInfo);
			Synchronisers.Add(lastForeignPortSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.WeightInfo, Source.JS_ActualWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.WeightUQInfo, Source.JS_UnitOfWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.VolumeInfo, Source.JS_ActualVolumeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.VolumeUQInfo, Source.JS_UnitOfVolumeInfo));

			AddConsigneeSynchroniser();
			AddConsignorSynchroniser();
			AddNotifyPartySynchroniser();

			consolSource.Transports.CountChanged -= Transports_CountChanged;
			consolSource.Transports.CountChanged += Transports_CountChanged;

			AddPacksSynchroniser();
		}

		protected virtual void AddConsignorSynchroniser()
		{
			if (!Env.Instance.Registry.EnableAddressValidationWebService || Source.ConsignorDocumentaryAddress.E2_ValidationStatus != AddressValidationStatus.Invalid)
			{
				consignorForeignShipperSynchroniser = GetAddressSynchroniser(Destination.ForeignShipper, Source.ConsignorDocumentaryAddress);
				Synchronisers.Add(consignorForeignShipperSynchroniser);
			}
			Source.ConsignorDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged -= SourceConsignorValidationStatusValueChanged;
			Source.ConsignorDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged += SourceConsignorValidationStatusValueChanged;
		}

		protected virtual void AddNotifyPartySynchroniser()
		{
			if (!Env.Instance.Registry.EnableAddressValidationWebService || Source.NotifyPartyDocumentaryAddress.E2_ValidationStatus != AddressValidationStatus.Invalid)
			{
				notifySynchroniser = GetAddressSynchroniser(Destination.NotifyParty1, Source.NotifyPartyDocumentaryAddress);
				Synchronisers.Add(notifySynchroniser);
			}
			Source.NotifyPartyDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged -= SourceNotifyValidationStatusValueChanged;
			Source.NotifyPartyDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged += SourceNotifyValidationStatusValueChanged;
		}

		protected virtual void AddConsigneeSynchroniser()
		{
			if (!Env.Instance.Registry.EnableAddressValidationWebService || Source.ConsigneeDocumentaryAddress.E2_ValidationStatus != AddressValidationStatus.Invalid)
			{
				consigneeSynchroniser = GetAddressSynchroniser(Destination.Consignee, Source.ConsigneeDocumentaryAddress);
				Synchronisers.Add(consigneeSynchroniser);
			}
			Source.ConsigneeDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged -= SourceConsigneeValidationStatusValueChanged;
			Source.ConsigneeDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged += SourceConsigneeValidationStatusValueChanged;
		}

		protected virtual void AddPacksSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.ManifestQtyInfo, Source.JS_OuterPacksInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ManifestUQInfo, Source.JS_F3_NKPackTypeInfo));
		}

		protected virtual IZType GetBillNumber()
		{
			var result = ZString.Empty;
			if (Destination.IsBillAlreadyOnFile)
			{
				result = Destination.MasterBillNumberInfo.Value.ToString();
			}
			else
			{
				result = Source.JS_HouseBill;
			}
			return result;
		}

		protected virtual IEnumerable<ZPropertyInfo> GetInfosAffectingBillNumber()
		{
			yield return Source.JS_HouseBillInfo;
		}

		protected abstract IZType GetPortOfLading();
		protected abstract IEnumerable<ZPropertyInfo> GetInfosAffectingPortOfLading();
		protected abstract IZType GetPlaceOfReceipt();
		protected abstract IEnumerable<ZPropertyInfo> GetInfosAffectingPlaceOfReceipt();
		protected abstract IZType GetLastForeignPort();
		protected abstract IEnumerable<ZPropertyInfo> GetLastForeignPortInfo();

		protected virtual BusinessObjectSynchroniser GetAddressSynchroniser(IManifestBillAddress address, JobDocAddress source)
		{
			return new JobDocAddressSynchroniser((JobDocAddress)address, source);
		}

		void SourceConsigneeValidationStatusValueChanged(object sender, EventArgs e)
		{
			ManageChangesToSourceAddressValidationStatus(Source.ConsigneeDocumentaryAddress, Destination.Consignee, consigneeSynchroniser, delegate(BusinessObjectSynchroniser s)
			{ consigneeSynchroniser = s; });
		}

		void SourceConsignorValidationStatusValueChanged(object sender, EventArgs e)
		{
			ManageChangesToSourceAddressValidationStatus(Source.ConsignorDocumentaryAddress, Destination.ForeignShipper, consignorForeignShipperSynchroniser, delegate(BusinessObjectSynchroniser s)
			{ consignorForeignShipperSynchroniser = s; });
		}

		void SourceNotifyValidationStatusValueChanged(object sender, EventArgs e)
		{
			ManageChangesToSourceAddressValidationStatus(Source.NotifyPartyDocumentaryAddress, Destination.NotifyParty1, notifySynchroniser, delegate(BusinessObjectSynchroniser s)
			{ notifySynchroniser = s; });
		}

		void ManageChangesToSourceAddressValidationStatus(JobDocAddress sourceAddress, IManifestBillAddress destinationAddress, BusinessObjectSynchroniser syncher, Action<BusinessObjectSynchroniser> setSyncherMethod)
		{
			if (sourceAddress.E2_ValidationStatus == AddressValidationStatus.Invalid)
			{
				if (syncher != null)
				{
					Synchronisers.Remove(syncher);
					syncher.SetEnabled(false, false);
					syncher.Dispose();
					syncher = null;
				}
			}
			else
			{
				if (syncher == null)
				{
					syncher = GetAddressSynchroniser(destinationAddress, sourceAddress);
					setSyncherMethod(syncher);
					Synchronisers.Add(syncher);
				}
				syncher.SetEnabled(this.IsEnabled, false);
				syncher.Synchronise(true);
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			consolSource.Transports.CountChanged -= Transports_CountChanged;
			lastForeignPortSynchroniser = null;
			placeOfReceiptSynchroniser = null;
			portOfLadingSynchroniser = null;
			Source.ConsigneeDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged -= SourceConsigneeValidationStatusValueChanged;
			Source.NotifyPartyDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged -= SourceNotifyValidationStatusValueChanged;
			Source.ConsignorDocumentaryAddress.E2_ValidationStatusInfo.ValueChanged -= SourceConsignorValidationStatusValueChanged;
		}

		protected virtual void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			Synchronise(lastForeignPortSynchroniser);
			Synchronise(portOfLadingSynchroniser);
			Synchronise(placeOfReceiptSynchroniser);
		}

		void Synchronise(FieldSynchroniser synchroniser)
		{
			if (synchroniser != null)
			{
				synchroniser.UpdateInfoEventsAndReSynchronise();
			}
		}

		protected abstract ConsolDataCalculator ConsolDataCalculator { get; }

		#endregion

		BusinessObjectSynchroniser notifySynchroniser;
		BusinessObjectSynchroniser consigneeSynchroniser;
		BusinessObjectSynchroniser consignorForeignShipperSynchroniser;
		FieldSynchroniser lastForeignPortSynchroniser;
		FieldSynchroniser portOfLadingSynchroniser;
		FieldSynchroniser placeOfReceiptSynchroniser;
		protected ForwardingConsol consolSource;
	}
}
