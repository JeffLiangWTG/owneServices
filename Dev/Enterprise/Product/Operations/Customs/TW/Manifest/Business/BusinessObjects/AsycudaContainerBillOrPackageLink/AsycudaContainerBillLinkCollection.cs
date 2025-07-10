using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaContainerBillLinkCollection<B, MasterB> : AsycudaContainerBillLinkCollection
		where B : AsycudaContainerBillOrPackageLink
		where MasterB : AsycudaBill
	{
		public AsycudaContainerBillLinkCollection(MasterB master)
			: base(master)
		{
		}

		public AsycudaContainerBillLinkCollection(MasterB master, ZQuery query)
			: base(master, query)
		{ }

		public new B this[int i] => (B)base[i];
		public new MasterB Master => (MasterB)base.Master;
		public new B AddNew() => (B)base.AddNew();
		public new B AddNew(Type bizOType) => (B)base.AddNew(bizOType);
	}

	public abstract class AsycudaContainerBillLinkCollection : DependentBusinessObjectCollection<AsycudaContainerBillOrPackageLink, AsycudaBill>
	{
		protected AsycudaContainerBillLinkCollection(AsycudaBill master)
			: base(master, new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, master.PK))
		{ }

		protected AsycudaContainerBillLinkCollection(AsycudaBill master, ZQuery query)
			: base(master, query.AddToFilter(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, master.PK)))
		{ }

		protected override string FkColumnName => AsycudaContainerBillOrPackageLink.Schema.APC_ABL_Bill;
	}
}
