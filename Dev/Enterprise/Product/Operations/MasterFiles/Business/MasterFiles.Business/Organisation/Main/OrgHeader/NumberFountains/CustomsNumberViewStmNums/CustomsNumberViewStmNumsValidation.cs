using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsValidation : ViewStmNumsValidation
	{
		public CustomsNumberViewStmNumsValidation(CustomsNumberViewStmNums parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSN_FountainName();
		}

		protected virtual void CheckSN_FountainName()
		{
		}

		public void ValidateSN_FountainName()
		{
			ValidateCalculatedProperty(Parent.SN_FountainNameInfo);
		}

		protected override void CheckSN_NameIsWesternEuropean()
		{
		}

		protected override void CheckSN_Value()
		{
			base.CheckSN_Value();
			ValidateSN_MaximumValue();
		}

		protected override void CheckSN_MaximumValue()
		{
			base.CheckSN_MaximumValue();
			if (Parent.IsInDatabase)
			{
				var value = Parent.SN_ValueForDisplay;
				if (value < ZDecimal.Zero || Parent.SN_MaximumValue < value)
				{
					Parent.SN_MaximumValueInfo.AddError(EndNumberCannotBeLessThanNextNumber);
				}
			}
			ValidateSN_MinimumValue();
		}

		protected override void CheckSN_MinimumValue()
		{
			base.CheckSN_MinimumValue();
			EnsureNotOverlappingOtherSequence(Parent.SN_MinimumValueInfo);
			ValidateSN_MaximumValue();
		}

		public static MultilingualString EndNumberCannotBeLessThanNextNumber
		{
			get { return ResString.GetMultilingualString("{6ACFD276-E3DA-476E-B29F-6EDD8FC79F83}", "End Number cannot be less than next number."); }
		}

		void EnsureNotOverlappingOtherSequence(ZPropertyInfo info)
		{
			var ownerPKs = Parent.Provider?.GetOwnerPKsMatching(Parent);
			if (ownerPKs != null && ownerPKs.Any())
			{
				var startNumber = Parent.SN_MinimumValue;
				var endNumber = Parent.SN_MaximumValue;
				var relatedStmNums = Parent.GetAllMatchingNameAndOwner(ownerPKs, true).FirstOrDefault(x => (x.SN_MinimumValue <= startNumber && startNumber <= x.SN_MaximumValue) || (startNumber <= x.SN_MinimumValue && x.SN_MinimumValue <= endNumber));
				if (relatedStmNums != null)
				{
					info.AddError(CustomsNumberViewStmNumsValidation.ThisRangeOverlapAnotherRange(Parent.SN_OwnerForDisplay, Parent.SN_MinimumValue, Parent.SN_MaximumValue, relatedStmNums.SN_OwnerForDisplay, relatedStmNums.SN_MinimumValue, relatedStmNums.SN_MaximumValue));
				}
			}
		}

		public static MultilingualString ThisRangeOverlapAnotherRange(ZString owner, ZLong startNumber, ZLong endNumber, ZString otherOwner, ZLong otherStartNumber, ZLong otherEndNumber)
		{
			return ResString.GetMultilingualString("{7E9C2CBC-E0E5-4E02-BF6B-210BDDA0A265}", "This range (Owner: '{0}', Start: {1}, End: {2}) overlaps range (Owner: '{3}', Start: {4}, End: {5}).", owner, startNumber, endNumber, otherOwner, otherStartNumber, otherEndNumber);
		}

		protected override void CheckSN_Owner()
		{
			if (Parent.SN_Owner.IsEmpty)
			{
				Parent.SN_OwnerInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.SN_OwnerInfo.HumanReadableName));
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.SN_OwnerInfo);
			}
		}

		protected override void CheckSN_Type()
		{
			base.CheckSN_Type();
			EnsureNoDuplicateTypeForMatchingNameAndOwner(Parent.SN_TypeInfo);
		}

		void EnsureNoDuplicateTypeForMatchingNameAndOwner(ZPropertyInfo info)
		{
			if (Parent.Setting != null && !Parent.Setting.AllowDuplicate())
			{
				var ownerPKs = Parent.Provider?.GetOwnerPKsMatching(Parent);
				if (ownerPKs != null && ownerPKs.Any() && Parent.GetAllMatchingNameAndOwner(ownerPKs, true).Any())
				{
					info.AddError(CustomsNumberViewStmNumsValidation.ThisRangeIsDuplicated(Parent.SN_OwnerForDisplay, Parent.SN_Type, Parent.SN_FountainName));
				}
			}
		}

		public static MultilingualString ThisRangeIsDuplicated(ZString owner, ZString type, ZString name)
		{
			return ResString.GetMultilingualString("{70FCA090-6C09-46B7-88F1-463E8D06D953}", "This range (Owner: '{0}', Type: {1}, Name: {2}) already exists.", owner, type, name);
		}

		protected new CustomsNumberViewStmNums Parent
		{
			get { return (CustomsNumberViewStmNums)base.Parent; }
		}
	}
}
