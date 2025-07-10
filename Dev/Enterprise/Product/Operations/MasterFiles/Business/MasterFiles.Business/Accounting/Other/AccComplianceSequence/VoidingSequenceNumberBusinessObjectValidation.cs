using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class VoidingSequenceNumberBusinessObjectValidation : AutoVoidingSequenceNumberBusinessObjectValidation
	{
		public VoidingSequenceNumberBusinessObjectValidation(AutoVoidingSequenceNumberBusinessObject parent)
			: base(parent) { }

		#region Implementation

		public new VoidingSequenceNumberBusinessObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VoidingSequenceNumberBusinessObject)base.Parent; }
		}

		#endregion

		protected override void CheckVoidingToNumber()
		{
			ZDecimal fromNumber, toNumber;

			base.CheckVoidingToNumber();
			MandatoryValidation.CheckEntered(Parent.VoidingToNumberInfo);
			if (!Parent.VoidingToNumberInfo.HasErrors() && !Parent.VoidingToNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.VoidingToNumberInfo.AddError(Res.GetString("A73EDEF4-C697-4A5B-A029-F5143797E0AC", "The To number should only consist of numbers."));
			}
			if (!Parent.VoidingToNumberInfo.HasErrors() && (Parent.VoidingToNumber.Length > Parent.ComplianceSequence.XD_MaximumNumberDigits))
			{
				Parent.VoidingToNumberInfo.AddError(Res.GetString("638EBD3C-8314-456F-9F00-9C0FA1E3CEE6", "The length of To number exceeds the maximum number of digits of the sequence"));
			}

			ZDecimal.TryParse(Parent.VoidingFromNumber, out fromNumber);
			ZDecimal.TryParse(Parent.VoidingToNumber, out toNumber);
			if (!Parent.VoidingToNumberInfo.HasErrors() && (toNumber < fromNumber))
			{
				Parent.VoidingToNumberInfo.AddError(Res.GetString("626A6DB6-F2DA-44A4-961F-DDB8C332DE67", "The To number must be greater than the From number"));
			}

			if (!Parent.VoidingToNumberInfo.HasErrors() && (toNumber > Parent.ComplianceSequence.XD_EndNumber))
			{
				Parent.VoidingToNumberInfo.AddError(Res.GetString("7AE412CF-70B2-4EF5-969E-EB065A71B399", "The To number can't be greater than the last number in the sequence"));
			}
		}
	}
}
