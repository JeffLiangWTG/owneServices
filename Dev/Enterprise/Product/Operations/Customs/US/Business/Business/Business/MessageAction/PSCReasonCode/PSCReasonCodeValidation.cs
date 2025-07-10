using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PSCReasonCodeValidation : AutoPSCReasonCodeValidation
	{
		public PSCReasonCodeValidation(AutoPSCReasonCode parent)
			: base(parent)
		{
		}

		protected override void CheckParentTypeIndicator()
		{
			base.CheckParentTypeIndicator();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ParentTypeIndicatorInfo, Parent.Lookups.ParentTypeIndicatorList);
			ValidateLineNumber();
		}

		protected override void CheckLineNumber()
		{
			base.CheckLineNumber();

			if (Parent.ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.EntryLine)
			{
				if (Parent.LineNumber == ZString.Empty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.LineNumberInfo, "line number");
				}
				else
				{
					if (Parent.entry.MergedLines.FindByFormattedLineNumber(Parent.LineNumber) == null)
					{
						Parent.LineNumberInfo.AddMessageError(NoEntryLineExistsWithLineNumber);
					}
				}
			}
			CheckMoreThan5ReasonPerEntryOrEntryLine();
		}

		void CheckMoreThan5ReasonPerEntryOrEntryLine()
		{
			var pSCReasonCodes = Parent.ParentCollection;
			if (pSCReasonCodes != null)
			{
				foreach (PSCReasonCode reasonCode in pSCReasonCodes)
				{
					if (Parent != reasonCode && Parent.ParentTypeIndicator == reasonCode.ParentTypeIndicator)
					{
						if (Parent.ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.Entry || (Parent.LineNumber != "0" && Parent.LineNumber == reasonCode.LineNumber))
						{
							Parent.LineNumberInfo.AddMessageError(DuplicatedParentType);
							break;
						}
					}
				}
			}
		}
		internal const string DuplicatedParentType = "You cannot enter more than 5 reason codes per entry or entry line.";

		internal const string NoEntryLineExistsWithLineNumber = "No entry line exists with this line number.";

		protected override void CheckReason1()
		{
			base.CheckReason1();

			ListValidation.MessageErrorIfInvalidCode(Parent.Reason1Info, Parent.Lookups.ReasonCodeList);

			if (!Parent.GetReasonCodes().Any())
			{
				Parent.Reason1Info.AddMessageError(NoReasonCodeEntered);
			}
		}
		internal const string NoReasonCodeEntered = "You have not entered any reason. Please enter at least one reason.";

		protected override void CheckReason2()
		{
			base.CheckReason2();

			ListValidation.MessageErrorIfInvalidCode(Parent.Reason2Info, Parent.Lookups.ReasonCodeList);
		}

		protected override void CheckReason3()
		{
			base.CheckReason3();

			ListValidation.MessageErrorIfInvalidCode(Parent.Reason3Info, Parent.Lookups.ReasonCodeList);
		}

		protected override void CheckReason4()
		{
			base.CheckReason4();

			ListValidation.MessageErrorIfInvalidCode(Parent.Reason4Info, Parent.Lookups.ReasonCodeList);
		}

		protected override void CheckReason5()
		{
			base.CheckReason5();

			ListValidation.MessageErrorIfInvalidCode(Parent.Reason5Info, Parent.Lookups.ReasonCodeList);
		}

		#region Implementation

		public new PSCReasonCode Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PSCReasonCode)base.Parent; }
		}

		#endregion
	}
}
