using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressCollectionForPlugin : BusinessObjectCollection<JobDocAddress>
	{
		public JobDocAddressCollectionForPlugin(BusinessObjectCollection jobDocAddresses)
			: base(jobDocAddresses.Factory)
		{
			this.JobDocAddresses = jobDocAddresses;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (!JobDocAddresses.Contains(bizOAdded))
			{
				JobDocAddresses.Add(bizOAdded);
			}

			var docAddress = bizOAdded as JobDocAddress;
			if (docAddress != null && docAddress.Address != null && !docAddress.Address.OA_IsActive)
			{
				docAddress.Validation.ValidateE2_OA_Address();
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

		public void ReBuild()
		{
			RemoveAllButLeaveRelationshipsIntact();
			foreach (JobDocAddress address in JobDocAddresses)
			{
				if (address.E2_OA_Address.IsValid)//Overriden will have a misc address
				{
					Add(address);
				}
			}
		}

		public readonly BusinessObjectCollection JobDocAddresses;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public IBusiness HostParentBizo { get; set; }
	}
}
