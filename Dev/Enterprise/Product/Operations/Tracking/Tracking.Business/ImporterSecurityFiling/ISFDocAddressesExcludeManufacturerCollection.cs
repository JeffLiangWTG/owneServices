using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling
{
	public class ISFDocAddressesExcludeManufacturerCollection : BusinessObjectCollection<ISFDocAddress>
	{
		public ISFDocAddressesExcludeManufacturerCollection(TrackingCusISFHeader iSFHeader)
			: base(iSFHeader.Factory)
		{
			this.JobDocAddresses = iSFHeader.DocAddresses;
			this.Master = iSFHeader;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (!JobDocAddresses.Contains(bizOAdded))
			{
				JobDocAddresses.Add(bizOAdded);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (JobDocAddresses.Contains(bizO))
			{
				JobDocAddresses.Remove(bizO);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			JobDocAddress newElement = child as JobDocAddress;
			newElement.E2_GovRegNumType = CodeTypeList.Codes.FIRMS;
			int sequence = 1;
			foreach (JobDocAddress docAddress in this)
			{
				docAddress.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
				sequence++;
			}
			newElement.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
		}

		public void ReBuild()
		{
			RemoveAllButLeaveRelationshipsIntact();
			foreach (JobDocAddress address in JobDocAddresses)
			{
				if (address.E2_AddressType != DocAddressTypes.GetCode(Factory, DocAddressType.Manufacturer) &&
					(
						Master.BookingParty.PK != address.PK &&
						Master.MainShipToParty.PK != address.PK &&
						Master.SellingParty.PK != address.PK &&
						Master.BuyingParty.PK != address.PK &&
						Master.StuffingLocation.PK != address.PK &&
						Master.Consolidator.PK != address.PK
					))
				{
					Add(address);
				}
			}
		}

		public readonly BusinessObjectCollection JobDocAddresses;
		public readonly TrackingCusISFHeader Master;
	}
}
