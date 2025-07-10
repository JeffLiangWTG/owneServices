using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class InBondNumberRangeRegistryItem : StronglyTypedRegistryItem<InBondNumberRange>
	{
		public InBondNumberRangeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new InBondNumberRangeRegistryDataType(), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default))
		{
		}

		public InBondNumberRangeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, InBondNumberRange defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InBondNumberRangeRegistryDataType(), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, defaultValue))
		{
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var inBondNumberRange = (InBondNumberRange)base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);
			if (inBondNumberRange != null && inBondNumberRange.BranchPK != branchPK)
			{
				inBondNumberRange.BranchPK = branchPK;
			}
			return inBondNumberRange;
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			var inBondNumberRange = (InBondNumberRange)newValue;
			if (inBondNumberRange.RunOutWarningLimitNumber.IsEmpty)
			{
				inBondNumberRange.RunOutWarningLimitNumber = USCustomsDataRegistry.DefaultInBondNumberWarningLimit;
			}

			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, inBondNumberRange);
			
			if (inBondNumberRange != null)
			{
				if (inBondNumberRange.BranchPK != branchPK)
				{
					inBondNumberRange.BranchPK = branchPK;
				}

				if (inBondNumberRange.IsRangeValidForNumberFountain)
				{
					var factory = RegistryFactory.Instance;
					var companyOrBranchPK = branchPK != Guid.Empty ? branchPK : companyOrOwnerPK;
					var fountain = Env.NumberFountains.USInBondNumberFountain(companyOrBranchPK);
					var currentNextNumber = fountain.PeekPreliminary(factory);
					if (currentNextNumber > inBondNumberRange.LastNumber)
					{
						fountain.SetNext(factory, (long)inBondNumberRange.StartNumber);
					}
				}
			}
		}
	}
}
