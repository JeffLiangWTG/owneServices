using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CustomsNumberViewStmNumsCompanyProvider : CustomsNumberViewStmNumsBusinessProvider
	{
		protected CustomsNumberViewStmNumsCompanyProvider(BusinessObjectFactory factory, ZString countryCode, ZGuid ownerPk) : base(factory, countryCode, ownerPk)
		{
		}

		public GlbCompany Company => (GlbCompany)Parent;
		public ZString CountryCode => ProviderKey;

		public virtual bool EnableCompanyLevel => true;
		public virtual bool EnableBranchLevel => false;

		public new CustomsNumberViewStmNumsCompanyWrapperCollection CustomsNumberWrappers => (CustomsNumberViewStmNumsCompanyWrapperCollection)base.CustomsNumberWrappers;

		protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
		{
			return new CustomsNumberViewStmNumsCompanyWrapperCollection(CustomsNumbers);
		}

		protected override Type WrapperType => typeof(CustomsNumberViewStmNumsCompanyWrapper);

		protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		{
			return new CustomsNumberViewStmNumsCompanyWrapper(stmNums);
		}

		protected override ICustomsNumberViewStmNumsParent GetParentCore(BusinessObjectFactory factory, ZGuid ownerPk)
		{
			return factory.Load<GlbCompany>(ownerPk) ?? factory.Load<GlbBranch>(ownerPk)?.Company;
		}

		protected override BusinessObject GetOwnerCore(BusinessObjectFactory factory, ZGuid ownerPk)
		{
			return (BusinessObject)factory.Load<GlbCompany>(ownerPk) ?? factory.Load<GlbBranch>(ownerPk);
		}

		protected override ZString GetOwnerTypeCore(CustomsNumberViewStmNums stmNums)
		{
			return stmNums.Owner is GlbCompany ? Res.GetString("{6FFB750B-022F-48AE-A246-90BE6C5E8E41}", "Company") : Res.GetString("{9A14A8E8-424E-4B60-9594-A25F682D1522}", "Branch");
		}

		protected override ZString GetOwnerForDisplayCore(BusinessObject owner)
		{
			var result = ZString.Empty;
			if (owner is GlbCompany company)
			{
				result = FormattableString.Invariant($"{company.GC_Code} - {company.GC_Name}");
			}
			else if (owner is GlbBranch branch)
			{
				result = FormattableString.Invariant($"{branch.GB_Code} - {branch.GB_BranchName}");
			}
			return result;
		}

		protected override ZString GetSettingCacheKeyCore(ZString rangeType)
		{
			return base.GetSettingCacheKeyCore(rangeType) + "-" + CountryCode;
		}

		protected override IEnumerable<ZGuid> GetOwnerPKsCore()
		{
			return base.GetOwnerPKsCore().Concat(Company.Branches.Select(x => x.PK));
		}

		protected override IEnumerable<ZGuid> GetOwnerPKsMatchingCore(CustomsNumberViewStmNums parent)
		{
			IEnumerable<ZGuid> ownerPKs = new[] { parent.SN_Owner };
			var enableCompanyLevel = EnableCompanyLevel;
			var enableBranchLevel = EnableBranchLevel;
			if (enableBranchLevel && enableCompanyLevel)
			{
				ownerPKs = GetOwnerPKs(true);
			}
			else if (enableBranchLevel)
			{
				ownerPKs = GetOwnerPKs(false);
			}
			else if (enableCompanyLevel)
			{
				ownerPKs = new[] { Company.PK };
			}
			return ownerPKs;
		}

		protected IEnumerable<ZGuid> GetOwnerPKs(bool includeCompany = true)
		{
			if (includeCompany)
			{
				yield return Company.PK;
			}
			foreach (var branch in Company.Branches)
			{
				yield return branch.PK;
			}
		}

		protected override ZString GetProviderKeyCore()
		{
			return CountryCode;
		}

		protected override ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper)
		{
			var owner = ZString.Empty;
			if (EnableBranchLevel)
			{
				var ownerType = EnableCompanyLevel ? GetOwnerType(wrapper.StmNums) + " - " : string.Empty;
				owner = Res.GetString("{8BDA9652-C03C-4D69-96DF-960A03A34165}", "Owner: {0}{1}, ", ownerType, wrapper.SN_OwnerForDisplay);
			}
			return owner + base.GetDetailCore(wrapper);
		}

		protected override IBusinessObjectCollection GetOwnerCollectionCore(BusinessObjectFactory factory, CustomsNumberViewStmNums stmNums)
		{
			if (stmNums.Owner is GlbCompany)
			{
				return new GlbCompanyCollection(factory);
			}
			else
			{
				var companyPk = stmNums.Provider.Parent.PK;
				var result = new GlbBranchNotCurrentCompanyRelatedCollection(factory, new ZQuery(GlbBranchSchema.GB_GC, companyPk));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Company", "Property", companyPk, false));
				return result;
			}
		}
	}
}
