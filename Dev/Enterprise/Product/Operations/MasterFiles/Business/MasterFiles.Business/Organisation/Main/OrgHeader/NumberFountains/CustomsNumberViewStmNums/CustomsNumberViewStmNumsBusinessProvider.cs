using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.NumberFountain;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CustomsNumberViewStmNumsBusinessProvider : NonPersistentBusinessObject
	{
		protected CustomsNumberViewStmNumsBusinessProvider(BusinessObjectFactory factory, ZString providerKey, ZGuid ownerPk) : base(factory)
		{
			Parent = GetParentCore(factory, ownerPk);
			ProviderKey = providerKey;
		}

		public readonly ICustomsNumberViewStmNumsParent Parent;
		public readonly ZString ProviderKey;

		public CustomsNumberViewStmNumsCollection CustomsNumbers => customsNumbers ?? (customsNumbers = new CustomsNumberViewStmNumsCollection(this));
		CustomsNumberViewStmNumsCollection customsNumbers;

		public CustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => customsNumberWrappers ?? (customsNumberWrappers = NewCustomsNumberWrappers());
		CustomsNumberViewStmNumsWrapperCollection customsNumberWrappers;

		protected virtual CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers() => new CustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);

		public CustomsNumberStmNumberRangeCollection NumberRanges => numberRanges ?? (numberRanges = new CustomsNumberStmNumberRangeCollection(this));
		CustomsNumberStmNumberRangeCollection numberRanges;

		public CustomsNumberViewStmNumsSetting GetSetting(ZString rangeType)
		{
			var key = GetSettingCacheKeyCore(rangeType);
			return Factory.GetCachedValue(key, () => GetSettingCore(rangeType));
		}

		protected virtual ZString GetSettingCacheKeyCore(ZString rangeType)
		{
			return FormattableString.Invariant($"CustomsNumberViewStmNumsSetting-{rangeType}");
		}

		protected abstract CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType);

		public bool TryGetNextCustomsNumber(BusinessObjectFactory factory, IEnumerable<CustomsNumberViewStmNumsWrapper> wrappers, out ZString customsNumber)
		{
			customsNumber = "";
			foreach (var wrapper in wrappers)
			{
				if (TryGetNextCustomsNumber(factory, wrapper.StmNums, out customsNumber))
				{
					return true;
				}
			}
			return false;
		}

		bool TryGetNextCustomsNumber(BusinessObjectFactory factory, CustomsNumberViewStmNums stmNums, out ZString customsNumber)
		{
			if (stmNums.SN_AvailableNumbers > ZLong.Zero)
			{
				var numberFountain = stmNums.TryGetNumberFountain();
				if (numberFountain != null)
				{
					long? currentNextNumber = null;
					while (true)
					{
						try
						{
							var nextNumber = numberFountain.GetNext(factory);
							var nextCustomsNumber = stmNums.GenerateCustomsNumber(nextNumber);
							if (!stmNums.IsNumberUsed(nextCustomsNumber))
							{
								customsNumber = nextCustomsNumber;
								return true;
							}
							else if (!currentNextNumber.HasValue)
							{
								currentNextNumber = nextNumber;
							}
							else if (nextNumber == currentNextNumber.Value)
							{
								break;
							}
						}
						catch (NumberFountainMaximumValueReachedException)
						{
							break;
						}
					}
				}
			}
			customsNumber = default;
			return false;
		}

		public ZString GetReasonForNotAbleToModify(CustomsNumberViewStmNumsWrapper wrapper) => GetReasonForNotAbleToModifyCore(wrapper);
		protected virtual ZString GetReasonForNotAbleToModifyCore(CustomsNumberViewStmNumsWrapper wrapper) => ZString.Empty;

		public CustomsNumberViewStmNumsWrapper GetOrCreateWrapper(CustomsNumberViewStmNums stmNums)
		{
			CustomsNumberViewStmNumsWrapper result = null;
			if (stmNums != null)
			{
				result = (CustomsNumberViewStmNumsWrapper)stmNums.Factory.Load(WrapperType, stmNums.PK);
				if (result == null)
				{
					result = CreateWrapperCore(stmNums);
				}
			}
			return result;
		}

		protected virtual CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums) => new CustomsNumberViewStmNumsWrapper(stmNums);

		protected virtual Type WrapperType => typeof(CustomsNumberViewStmNumsWrapper);

		public void SetupRelatedDataAndNotification()
		{
			ClearRelatedDataAndNotification();
			SetupRelatedDataAndNotificationCore();
		}

		protected virtual void SetupRelatedDataAndNotificationCore()
		{
			Parent.RegisterEditableChildObject(NumberRanges);
		}

		public void ClearRelatedDataAndNotification() => ClearRelatedDataAndNotificationCore();
		protected virtual void ClearRelatedDataAndNotificationCore()
		{
			Parent.UnRegisterEditableChildObject(NumberRanges);
		}

		internal ZString GetNumberRangeDetail(CustomsNumberStmNumberRange range)
		{
			var wrapper = GetOrCreateWrapper(range.GetStmNums().FirstOrDefault());
			return wrapper == null ? range.SNR_Name : wrapper.Detail;
		}

		internal protected virtual CustomsNumberViewStmNumsValidation GetNewValidation(CustomsNumberViewStmNums stmNums)
		{
			return null;
		}

		internal protected virtual CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		{
			return null;
		}

		protected abstract ICustomsNumberViewStmNumsParent GetParentCore(BusinessObjectFactory factory, ZGuid ownerPk);

		public BusinessObject GetOwner(BusinessObjectFactory factory, ZGuid ownerPk) => GetOwnerCore(factory, ownerPk);

		protected abstract BusinessObject GetOwnerCore(BusinessObjectFactory factory, ZGuid ownerPk);

		public ZString GetOwnerType(CustomsNumberViewStmNums stmNums) => GetOwnerTypeCore(stmNums);

		protected abstract ZString GetOwnerTypeCore(CustomsNumberViewStmNums stmNums);

		public ZString GetOwnerForDisplay(BusinessObject owner) => GetOwnerForDisplayCore(owner);

		protected abstract ZString GetOwnerForDisplayCore(BusinessObject owner);

		public IEnumerable<ZGuid> GetOwnerPKs() => GetOwnerPKsCore();

		protected virtual IEnumerable<ZGuid> GetOwnerPKsCore()
		{
			yield return Parent.PK;
		}

		public IEnumerable<ZGuid> GetOwnerPKsMatching(CustomsNumberViewStmNums parent) => GetOwnerPKsMatchingCore(parent);

		protected virtual IEnumerable<ZGuid> GetOwnerPKsMatchingCore(CustomsNumberViewStmNums parent)
		{
			yield return parent.SN_Owner;
		}

		public bool MatchesFilter(ZGuid ownerPk)
		{
			return GetOwnerPKs().Contains(ownerPk);
		}

		protected abstract ZString GetProviderKeyCore();

		public ZString GetDetail(CustomsNumberViewStmNumsWrapper wrapper) => GetDetailCore(wrapper);

		protected virtual ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return Res.GetString("{D28BDF3A-EDEA-41D4-AD2A-4DCB0C8307BC}", "Range Type: {0}, Name: {1}", wrapper.SN_Type, wrapper.SN_FountainName);
		}

		public IBusinessObjectCollection GetOwnerCollection(BusinessObjectFactory factory, CustomsNumberViewStmNums stmNums) => GetOwnerCollectionCore(factory, stmNums);

		protected abstract IBusinessObjectCollection GetOwnerCollectionCore(BusinessObjectFactory factory, CustomsNumberViewStmNums stmNums);
	}
}
