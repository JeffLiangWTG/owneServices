using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class EntryCensusWarningOverrideValidation : AutoEntryCensusWarningOverrideValidation
	{
		public EntryCensusWarningOverrideValidation(AutoEntryCensusWarningOverride parent)
			: base(parent)
		{
		}

		protected override void CheckEntryLinePK()
		{
			base.CheckEntryLinePK();

			if (Parent.EntryLine == null)
			{
				Parent.EntryLinePKInfo.AddError(EntryLineShouldBeSelected);
			}
			else
			{
				CheckIfEntryLineHasMoreThan7ConditionCodes();
			}
		}

		internal const string EntryLineShouldBeSelected = "Select a valid merged entry summary line.";

		void CheckIfEntryLineHasMoreThan7ConditionCodes()
		{
			var parentObjectEntryLine = Parent.EntryLine;

			int numberOfConditionCode = 0;

			if (parentObjectEntryLine != null)
			{
				foreach (EntryCensusWarningOverride cwo in Parent.coll)
				{
					var cwoEntryLine = cwo.EntryLine;

					if (cwoEntryLine != null)
					{
						if (cwoEntryLine == parentObjectEntryLine || cwoEntryLine.CL_LineNumber == parentObjectEntryLine.CL_LineNumber)
						{
							numberOfConditionCode++;

							if (numberOfConditionCode > 7)
							{
								Parent.EntryLinePKInfo.AddMessageError(MoreThan7ConditionCodes);
								break;
							}
						}
					}
				}
			}
		}

		internal const string MoreThan7ConditionCodes = "You have entered more than 7 condition codes for the same entry line. Messages only allow up to 7 condition and override codes for a set of entry line and its secondary lines.";

		protected override void CheckConditionCode()
		{
			base.CheckConditionCode();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ConditionCodeInfo, Parent.Lookups.ConditionCodeList);

			if (!Parent.ConditionCode.IsEmpty)
			{
				CheckDuplicateConditionCodes();
			}
		}

		void CheckDuplicateConditionCodes()
		{
			var parentObjectEntryLine = Parent.EntryLine;

			if (parentObjectEntryLine != null)
			{
				foreach (EntryCensusWarningOverride cwo in Parent.coll)
				{
					var cwoEntryLine = cwo.EntryLine;

					if (cwoEntryLine != null)
					{
						if (cwoEntryLine == parentObjectEntryLine || cwoEntryLine.CL_LineNumber == parentObjectEntryLine.CL_LineNumber)
						{
							if (cwo != Parent && cwo.ConditionCode == Parent.ConditionCode)
							{
								Parent.ConditionCodeInfo.AddMessageError(DuplicateCensusWarningCodeNowAllowed);
								break;
							}
						}
					}
				}
			}
		}

		internal const string DuplicateCensusWarningCodeNowAllowed = "Duplicate census warning condition codes are not allowed.";

		protected override void CheckOverrideCode()
		{
			base.CheckOverrideCode();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.OverrideCodeInfo, Parent.Lookups.CensusOverrideList);
		}

		#region Implementation

		public new EntryCensusWarningOverride Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (EntryCensusWarningOverride)base.Parent; }
		}

		#endregion
	}
}
