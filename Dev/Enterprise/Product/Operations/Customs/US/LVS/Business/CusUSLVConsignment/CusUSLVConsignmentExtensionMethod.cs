using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public static class CusUSLVConsignmentExtensionMethod
	{
		public static ZString AllocateEntryNumbersForConsignments(this List<CusUSLVConsignmentForMessaging> consignmentsToAllocate, CusUSLVClearance clearance)
		{
			var reasonForUnableToAllocate = ZString.Empty;
			var consignmentsCount = consignmentsToAllocate.Count;
			if (clearance != null && consignmentsCount > 0)
			{
				if (clearance.ULH_EntryFilerCode.IsEmpty)
				{
					reasonForUnableToAllocate = ACEEntryStmNumsSetting.EntryFilerCodeIsRequiredForEntryNumberAllocation;
				}
				else
				{
					var allocator = GetCustomsEntryNumberAllocator(clearance);
					if (allocator == null)
					{
						reasonForUnableToAllocate = ZString.Format(ACEEntryStmNumsSetting.EntryNumberRangeNotSetup, clearance.Branch.GB_Code, clearance.ULH_EntryFilerCode, clearance.Branch.Company.GC_Code);
					}
					else
					{
						if (allocator.TotalAvailableNumbers < consignmentsCount)
						{
							if (allocator.Wrapper.IsBranchLevel)
							{
								reasonForUnableToAllocate = ZString.Format(NotEnoughAvailableEntryNumbersForBranch, clearance.Branch.GB_Code, clearance.ULH_EntryFilerCode);
							}
							else
							{
								reasonForUnableToAllocate = ZString.Format(NotEnoughAvailableEntryNumbersForCompany, clearance.Branch.Company.GC_Code, clearance.ULH_EntryFilerCode);
							}
						}
						else
						{
							allocator.NumberToAllocate = consignmentsCount;
							var entryNumbers = allocator.AllocateNumbers();
							for (var index = 0; index < entryNumbers.Length; index++)
							{
								consignmentsToAllocate[index].Consignment.CE_EntryNum = entryNumbers[index];
							}
						}
					}
				}
			}

			return reasonForUnableToAllocate;
		}
		internal const string NotEnoughAvailableEntryNumbersForBranch = "There is not enough entry numbers in Branch '{0}' (Entry Filer Code:'{1}').\r\nPlease either increase the Branch's 'Entry Range' or contact Customs for a new range of numbers.";
		internal const string NotEnoughAvailableEntryNumbersForCompany = "There is not enough entry numbers in Company '{0}' (Entry Filer Code:'{1}').\r\nPlease either increase the Company's 'Entry Range' or contact Customs for a new range of numbers.'";

		static BorderLineReleaseAllocator GetCustomsEntryNumberAllocator(CusUSLVClearance clearance)
		{
			BorderLineReleaseAllocator allocator = null;
			var entryStmNumberSetting = ACEEntryStmNumsSetting.New(clearance.Branch, clearance.ULH_EntryFilerCode);
			var wrapper = entryStmNumberSetting.GetFirstAvailableOrLastSequenceWrapper(clearance.Branch);
			if (wrapper != null)
			{
				allocator = new BorderLineReleaseAllocator(wrapper);
			}

			return allocator;
		}
	}
}
