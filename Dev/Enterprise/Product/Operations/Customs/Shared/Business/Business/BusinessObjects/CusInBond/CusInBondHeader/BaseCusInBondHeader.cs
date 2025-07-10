using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class BaseCusInBondHeader : AutoCusInBondHeader, Integration.Customs.ICusInBondHeader
	{
		protected BaseCusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly CusInBondHeaderTypeDecider TypeDecider = new CusInBondHeaderTypeDecider();

		#endregion

		public Type MovementHeaderType
		{
			get { return MovementHeaderTypeCore; }
		}

		protected abstract Type MovementHeaderTypeCore { get; }

		public GlbCompany Company
		{
			get
			{
				GlbBranch branch = Branch;
				return branch == null ? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK) : branch.Company;
			}
		}

		#region RegistryBranchPK
		public Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryBranchPK();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		protected virtual Guid GetRegistryBranchPK()
		{
			var cachedValue = BH_GB;
			return cachedValue.IsValid ? cachedValue.ToGuid() : Guid.Empty;
		}
		#endregion

		#region RegistryCompanyPK
		public Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryCompanyPK();
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		protected virtual Guid GetRegistryCompanyPK()
		{
			Guid result = Guid.Empty;
			GlbBranch branch = Branch;
			if (branch == null)
			{
				result = GlbCompany.CurrentCompany.PK.ToGuid();
			}
			else if (!branch.GB_GC.IsEmpty)
			{
				result = branch.GB_GC.ToGuid();
			}
			return result;
		}
		#endregion
	}
}
