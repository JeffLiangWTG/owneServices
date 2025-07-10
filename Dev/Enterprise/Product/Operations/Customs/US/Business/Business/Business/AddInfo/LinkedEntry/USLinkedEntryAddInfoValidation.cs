using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USLinkedEntryAddInfoValidation : AutoUSLinkedEntryAddInfoValidation
	{
		public USLinkedEntryAddInfoValidation(AutoUSLinkedEntryAddInfo parent) : base(parent)
		{
		}

		JobDeclaration Declaration
		{
			get { return Parent.LinkedEntry.Declaration; }
		}

		new USLinkedEntryAddInfo Parent
		{
			get { return (USLinkedEntryAddInfo)base.Parent; }
		}

		protected override void CheckUS_LE_EntryNumber()
		{
			base.CheckUS_LE_EntryNumber();

			if (Parent.US_LE_EntryNumber.IsEmpty)
			{
				if (Declaration.US_ConsolACE)
				{
					Parent.US_LE_EntryNumberInfo.AddMessageError(EntryNoCannotBeEmpty);
				}
			}
			else
			{
				var parent = Declaration;
				if (parent != null)
				{
					if (parent.LinkedEntryNumbers.OfType<LinkedEntry>().Count(x => x.US_LE_EntryNumber == Parent.US_LE_EntryNumber) > 1)
					{
						Parent.US_LE_EntryNumberInfo.AddMessageError(DuplicateEntry);
					}
				}
				EntryNumberValidator.ValidateFormatAndCheckDigit(Parent.US_LE_EntryNumberInfo, parent?.Branch, InvalidCheckDigit);
			}
		}
		internal const string EntryNoCannotBeEmpty = "Entry Number can not be empty.";
		internal const string DuplicateEntry = "Duplicate entry numbers found. Messages will be rejected by Customs.";
		internal const string InvalidCheckDigit = "Check digit appears to be incorrect, please validate.   Check digit should be ";

		protected override void CheckUS_LE_PortCode()
		{
			base.CheckUS_LE_PortCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_LE_PortCodeInfo, Parent.LinkedEntry.AddInfoLookups.SchDPortList);

			var declaration = Declaration;
			if (declaration != null && declaration.IsProtest && !declaration.Protest.US_P_FilingDDPP.IsEmpty)
			{
				if (!Parent.US_LE_PortCode.IsEmpty && Parent.US_LE_PortCode.SubstringSafe(0, 2) != declaration.Protest.US_P_FilingDDPP.SubstringSafe(0, 2))
				{
					Parent.US_LE_PortCodeInfo.AddWarning(PortDoNotMatchFilingDDPP);
				}
			}
		}
		internal const string PortDoNotMatchFilingDDPP = "This import entry was entered into a different district to the protest district. With a few exceptions, the protest or petition must be filed within the boundary of the original import districts. For example, entries entered in Chicago (3901) cannot be protested in Los Angeles (2704), but Denver(3307) entries can be protested in San Francisco(2809).";

		protected override void CheckUS_LE_Withdraw()
		{
			base.CheckUS_LE_Withdraw();

			if (Parent.US_LE_Withdraw)
			{
				var declaration = Declaration;
				if (declaration != null && declaration.IsProtest && declaration.Protest.Is181115Intervention)
				{
					Parent.US_LE_WithdrawInfo.AddMessageError(WithdrawingEntriesIsNotSupportedFor181115Intervention);
				}
			}
		}

		internal const string WithdrawingEntriesIsNotSupportedFor181115Intervention = "Withdrawing an entry is not supported for the selected Tariff Act Citation.";
	}
}
