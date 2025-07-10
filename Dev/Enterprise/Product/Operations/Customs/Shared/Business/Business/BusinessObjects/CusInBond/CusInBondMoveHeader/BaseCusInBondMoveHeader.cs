using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class BaseCusInBondMoveHeader : AutoCusInBondMoveHeader, Integration.Customs.ICusInBondMoveHeader, ICusGoodsLocationTypeSupporter
	{
		protected BaseCusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new CusInBondMoveHeaderTypeDecider();

		#endregion

		#region MovementDetailType

		public Type MovementDetailType => MovementDetailTypeCore;

		protected abstract Type MovementDetailTypeCore { get; }

		#endregion

		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid BM_BH
		{
			get { return base.BM_BH; }
			set
			{
				ZGuid oldValue = BM_BH;
				base.BM_BH = value;
				if (!IsCopying && oldValue != BM_BH && !IsRowDeletedOrDetachedOrNull)
				{
					MajorMarkAsNeedingValidationCore();
				}
			}
		}

		protected virtual void MajorMarkAsNeedingValidationCore()
		{
			BaseCusInBondHeader parent = Header;
			if (parent != null)
			{
				parent.MarkAsNeedingValidation();
			}
			MarkAsNeedingValidationIncludingChildren();
		}

		public BaseCusInBondHeader Header => Factory.Load<BaseCusInBondHeader>(BM_BH);

		public GlbBranch HeaderBranch
		{
			get
			{
				BaseCusInBondHeader header = Header;
				return header == null ? null : header.Branch;
			}
		}

		public Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryCompanyPK;
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		Guid GetRegistryCompanyPK
		{
			get
			{
				var header = Header;
				return header == null ? GlbCompany.CurrentCompany.PK.ToGuid() : header.RegistryCompanyPK;
			}
		}

		public Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryBranchPK;
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		Guid GetRegistryBranchPK
		{
			get
			{
				var header = Header;
				return header == null ? GlbBranch.CurrentBranch.PK.ToGuid() : header.RegistryBranchPK;
			}
		}

		#region ICusGoodsLocationTypeSupporter

		Type ICusGoodsLocationTypeSupporter.GoodsLocationType => GoodsLocationTypeCore;

		protected virtual Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

		#endregion
	}
}
