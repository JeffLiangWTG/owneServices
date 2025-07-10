using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillLinkAsycudaContainer : AutoAsycudaBillLinkAsycudaContainer
	{
		public AsycudaBillLinkAsycudaContainer(AsycudaBill bill, AsycudaContainer container) : base()
		{
			Bill = Argument.NotNull(bill, nameof(bill));
			Container = Argument.NotNull(container, nameof(container));
		}

		internal AsycudaBill Bill { get; }

		AsycudaContainer Container { get; }

		protected override ZGuid GetPK() => Container.PK;

		public override ZBool Link
		{
			get => Bill.GetDivot(PK) != null;
			set
			{
				var hasChanged = Link != value;
				base.Link = value;
				if (!IsCopying && hasChanged)
				{
					if (value)
					{
						Bill.LinkContainer(PK);
					}
					else
					{
						Bill.UnlinkContainer(PK);
					}
				}
			}
		}

		public override ZString ContainerNumber => Container.ACN_ContainerNumber;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLinkAsycudaContainerLookups.ContainerTypes))]
		public override ZGuid ContainerType => Container.ACN_RC_ContainerType;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLinkAsycudaContainerLookups.EmptyFullList))]
		public override ZString ContainerMode => Container.ACN_EmptyFullIndicator;

		public override ZString Seal1 => Container.ACN_Seal1;

		public override ZString Seal2 => Container.ACN_Seal2;

		public override ZString Seal3 => Container.ACN_Seal3;

		public IEnumerable<ZString> Seals => Container.GetSeals();

		public RefContainer RefContainer => Container.ContainerType;

		public AsycudaBillLinkAsycudaContainerLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new AsycudaBillLinkAsycudaContainerLookups(this);
				}

				return lookups;
			}
		}

		AsycudaBillLinkAsycudaContainerLookups lookups;
	}
}
