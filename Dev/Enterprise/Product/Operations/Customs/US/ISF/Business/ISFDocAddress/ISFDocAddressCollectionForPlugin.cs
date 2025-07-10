using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressCollectionForPlugin : JobDocAddressCollectionForPlugin
	{
		public ISFDocAddressCollectionForPlugin(ISFDocAddressCollection jobDocAddresses)
			: base(jobDocAddresses)
		{
		}

		public new ISFDocAddressDependentCollection JobDocAddresses
		{
			get { return (ISFDocAddressDependentCollection)base.JobDocAddresses; }
		}

		public new ISFDocAddress this[int i]
		{
			get { return (ISFDocAddress)base[i]; }
		}

		public new ISFDocAddress AddNew()
		{
			return (ISFDocAddress)base.AddNew();
		}

		public new ISFDocAddress AddNew(Type bizObjType)
		{
			return (ISFDocAddress)base.AddNew(bizObjType);
		}
	}
}
