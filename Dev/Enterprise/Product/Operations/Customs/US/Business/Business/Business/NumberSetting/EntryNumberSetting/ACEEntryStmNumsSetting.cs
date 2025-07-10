using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ACEEntryStmNumsSetting
	{
		public ACEEntryStmNumsSetting(ZGuid companyPK)
			: this(companyPK, GetEntryFilerCode(companyPK))
		{
		}

		protected ACEEntryStmNumsSetting(ZGuid companyPK, ZString entryFilerCode, bool addDefaultCompanyRangeIfMissing = false)
		{
			EntryFilerCode = entryFilerCode.ToUpperInvariant();
			Company = Argument.NotNull(new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbCompany>(companyPK), "companyPK must be a valid PK");
			if (addDefaultCompanyRangeIfMissing)
			{
				AddDefaultCompanyRangeIfMissing();
			}
		}

		static ZString GetEntryFilerCode(ZGuid companyPK)
		{
			return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
		}

		public static ACEEntryStmNumsSetting New(GlbBranch branch, ZString entryFilerCode, bool addDefaultCompanyRangeIfMissing = false)
		{
			ACEEntryStmNumsSetting result = null;
			var companyPK = branch?.GB_GC ?? ZGuid.Invalid;
			if (companyPK.IsValid && !entryFilerCode.IsEmpty)
			{
				result = new ACEEntryStmNumsSetting(companyPK, entryFilerCode, addDefaultCompanyRangeIfMissing);
			}
			return result;
		}

		public const string NotEnoughAvailableEntryNumbersForBranch = "Branch '{0}' (Entry Filer Code:'{1}') has run out of entry numbers.\r\nPlease either increase the Branch's 'Entry Range' or contact Customs for a new range of numbers.";
		public const string NotEnoughAvailableEntryNumbersForCompany = "Company '{0}' (Entry Filer Code:'{1}') has run out of entry numbers.\r\nPlease contact Customs for a new range of numbers.'";
		public const string EntryNumberRangeNotSetup = "The Entry Number Range of Branch '{0}' (Entry Filer Code:'{1}') has not been setup correctly.\r\nPlease set it up in Maintain -> User Admin -> Companies -> Company '{2}' -> Number Ranges.";
		public const string EntryFilerCodeIsRequiredForEntryNumberAllocation = "Cannot allocate entry number when there is no Entry Filer code specified.";

		public static string GetAnyReasonForNotAbleToAllocateNumber(GlbBranch branch, ZString entryFilerCode)
		{
			string result = "";
			if (entryFilerCode.IsEmpty)
			{
				result = EntryFilerCodeIsRequiredForEntryNumberAllocation;
			}
			else
			{
				var stmNums = GetFirstAvailableOrLastSequenceStmNums(branch, entryFilerCode);
				if (stmNums == null || !stmNums.IsNumberFountainValid)
				{
					result = string.Format(Culture.Invariant, EntryNumberRangeNotSetup, branch?.GB_Code ?? ZString.Empty, entryFilerCode, branch?.Company?.GC_Code ?? ZString.Empty);
				}
				else if (stmNums.TotalAvailableNumbers == ZDecimal.Zero)
				{
					result = stmNums.Owner is GlbCompany ? string.Format(Culture.Invariant, NotEnoughAvailableEntryNumbersForCompany, branch?.Company?.GC_Code ?? ZString.Empty, entryFilerCode) : string.Format(Culture.Invariant, NotEnoughAvailableEntryNumbersForBranch, branch?.GB_Code ?? ZString.Empty, entryFilerCode);
				}
			}
			return result;
		}

		public static CustomsNumberViewStmNums GetFirstAvailableOrLastSequenceStmNums(GlbBranch branch, ZString entryFilerCode)
		{
			return New(branch, entryFilerCode, true)?.GetFirstAvailableOrLastSequence(branch);
		}

		public readonly ZString EntryFilerCode;
		public readonly GlbCompany Company;
		public CustomsNumberViewStmNumsBusinessProvider Provider => Company.CustomsNumberProvider;

		public bool TryGetNextCustomsNumber(BusinessObjectFactory factory, GlbBranch branch, out ZString entryNumber)
		{
			var result = false;
			entryNumber = ZString.Empty;
			if (Provider != null)
			{
				var matchingStmNums = GetOrderedCustomsEntriesWrappersMatching(branch.PK);
				if (matchingStmNums.Length == 0)
				{
					matchingStmNums = GetOrderedCustomsEntriesWrappersMatching(branch.GB_GC);
				}
				if (matchingStmNums.Length == 0)
				{
					entryNumber = ZString.Empty;
				}
				else
				{
					result = Provider.TryGetNextCustomsNumber(factory, matchingStmNums, out entryNumber);
				}
			}
			return result;
		}

		public CustomsNumberViewStmNums GetFirstAvailableOrLastSequence(GlbBranch branch)
		{
			return GetFirstAvailableOrLastSequenceWrapper(branch)?.StmNums;
		}

		public USCustomsNumberViewStmNumsWrapper GetFirstAvailableOrLastSequenceWrapper(GlbBranch branch)
		{
			USCustomsNumberViewStmNumsWrapper result = null;

			if (branch != null && Provider != null)
			{
				var matchingStmNumsWrappers = GetSequenceWrappers(branch);
				result = matchingStmNumsWrappers?.Where(x => x.SN_AvailableNumbers > ZDecimal.Zero).FirstOrDefault() ?? matchingStmNumsWrappers.LastOrDefault();
			}

			return result;
		}

		USCustomsNumberViewStmNumsWrapper[] GetSequenceWrappers(GlbBranch branch)
		{
			var matchingStmNumsWrappers = GetOrderedCustomsEntriesWrappersMatching(branch.PK);
			if (matchingStmNumsWrappers.Length == 0)
			{
				matchingStmNumsWrappers = GetOrderedCustomsEntriesWrappersMatching(branch.GB_GC);
			}
			return matchingStmNumsWrappers;
		}

		public USCustomsNumberViewStmNumsWrapper GetMatchingSequenceWrapper(GlbBranch branch, ZDecimal entryNumber)
		{
			USCustomsNumberViewStmNumsWrapper result = null;

			if (branch != null && Provider != null)
			{
				var matchingStmNumsWrappers = GetSequenceWrappers(branch);
				result = matchingStmNumsWrappers?.Where(x => x.SN_AvailableNumbers > ZDecimal.Zero && x.SN_ValueForDisplay <= entryNumber && x.SN_MaximumValue >= entryNumber).FirstOrDefault() ?? matchingStmNumsWrappers.FirstOrDefault();
			}

			return result;
		}

		#region Implementation

		USCustomsNumberViewStmNumsWrapper[] GetOrderedCustomsEntriesWrappersMatching(ZGuid ownerPK)
		{
			return GetCustomsEntriesWrappers().Where(x => x.SN_Owner == ownerPK).OrderBy(x => x.SN_SystemCreateTimeUtc).ThenBy(x => x.StmNums.Sequence).ToArray();
		}

		void AddDefaultCompanyRangeIfMissing()
		{
			if (Provider != null && !GetCustomsEntriesWrappers().Any())
			{
				var ensSetting = Provider.GetSetting(NumberRangeTypeList.Codes.CustomsEntry);
				Add(Company.PK, 1L, ensSetting.DefaultTypeRangeMax().Value);
			}
		}

		IEnumerable<USCustomsNumberViewStmNumsWrapper> GetCustomsEntriesWrappers()
		{
			return Provider.CustomsNumberWrappers.OfType<USCustomsNumberViewStmNumsWrapper>().Where(x => x.IsCustomsEntry && x.AppliesTo == EntryFilerCode);
		}

		#endregion

		USCustomsNumberViewStmNumsWrapper Add(ZGuid ownerPK, ZLong minimumValue, ZLong maximumValue)
		{
			var range = AddRange(ownerPK, minimumValue, maximumValue);
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, range.StmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, range.StmNums.SN_Owner);
			return (USCustomsNumberViewStmNumsWrapper)Provider.GetOrCreateWrapper(CustomsNumberViewStmNumsHelper.LoadTop1StmNums(Company.Factory, query));
		}

		USCustomsNumberViewStmNumsWrapper AddRange(ZGuid ownerPK, ZLong minimumValue, ZLong maximumValue)
		{
			// need to create a new stmNums in a different factory because when the data is save; the db will assign a new SN_ID and hence causing the SN_PK to be changed on the ViewStmNums
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var provider = newFactory.Load<GlbCompany>(Company.PK).CustomsNumberProvider;
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = (USCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			wrapper.AppliesTo = EntryFilerCode;
			stmNums.SN_Owner = ownerPK;
			stmNums.SN_MinimumValue = minimumValue;
			stmNums.SN_MaximumValue = maximumValue;
			try
			{
				stmNums.Factory.Save();
				var currentProvider = Provider;
				currentProvider.CustomsNumbers.RefreshFromDb();
				currentProvider.NumberRanges.RefreshFromDb();
				currentProvider.CustomsNumberWrappers.RefreshBinding();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			return wrapper;
		}

#if DEBUG
		public USCustomsNumberViewStmNumsWrapper AddForTesting(ZGuid ownerPK, ZLong minimumValue, ZLong maximumValue) => Add(ownerPK, minimumValue, maximumValue);
#endif
	}
}
