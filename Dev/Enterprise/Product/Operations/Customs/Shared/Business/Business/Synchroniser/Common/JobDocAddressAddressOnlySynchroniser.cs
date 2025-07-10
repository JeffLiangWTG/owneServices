using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class JobDocAddressAddressOnlySynchroniser : BusinessObjectSynchroniser
	{
		public JobDocAddressAddressOnlySynchroniser(JobDocAddress destination, ZPropertyInfoGuid addressPKInfo)
			: base(destination, addressPKInfo.BizObj)
		{
			this.addressPKInfo = addressPKInfo;
		}
		protected readonly ZPropertyInfoGuid addressPKInfo;

		public new JobDocAddress Destination
		{
			get { return (JobDocAddress)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.E2_OA_AddressInfo, addressPKInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.E2_AddressOverrideInfo, GetAddressOverride, GetAddressOverrideInfos));
			Destination.ReadOnly = true;
		}

		IZType GetAddressOverride()
		{
			return ZBool.False;
		}

		IEnumerable<ZPropertyInfo> GetAddressOverrideInfos()
		{
			yield return Destination.E2_ParentIDInfo;
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Destination.ReadOnly = false;
		}
	}
}
