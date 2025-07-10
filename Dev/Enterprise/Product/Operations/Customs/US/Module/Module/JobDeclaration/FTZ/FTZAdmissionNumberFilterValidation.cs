using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	public class FTZAdmissionNumberFilterValidation : ModuleTextFilterValidation
	{
		public FTZAdmissionNumberFilterValidation(FTZAdmissionNumberFilter parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateZoneID();
			ValidateYear();
			ValidateControlNumber();
		}

		public void ValidateZoneID()
		{
			ValidateCalculatedProperty(Parent.ZoneIDInfo);
		}

		public void ValidateYear()
		{
			ValidateCalculatedProperty(Parent.YearInfo);
		}

		public void ValidateControlNumber()
		{
			ValidateCalculatedProperty(Parent.ControlNumberInfo);
		}

		public override Type AutoValidationType => throw new Exception("The method or operation is not implemented.");

		protected void CheckZoneID()
		{
			if (!Parent.ZoneID.IsEmpty && Parent.ZoneID.Length != 7 && Parent.ZoneID.Length != 9)
			{
				Parent.ZoneIDInfo.AddError(ZoneIDFormat);
			}
		}

		protected void CheckYear()
		{
			if (!Parent.Year.IsEmpty && Parent.Year.Length != 2)
			{
				Parent.YearInfo.AddError(YearFormat);
			}
		}

		protected void CheckControlNumber()
		{
			if (!Parent.ControlNumber.IsEmpty && (Parent.ControlNumber.Length < 2 || Parent.ControlNumber.Length > 8))
			{
				Parent.ControlNumberInfo.AddError(ControlNumberFormat);
			}
		}

		new FTZAdmissionNumberFilter Parent => (FTZAdmissionNumberFilter)base.Parent;

		internal const string YearFormat = "Year should be 2 numeric.";
		internal const string ControlNumberFormat = "Control Number should be between 2 and 8 in length.";
		internal const string ZoneIDFormat = "Zone ID should be 7 alpha-numeric where Zone (3 numeric) + Sub Zone ID (2 alphanumeric) + Site ID (2 alphanumeric) or 9 alpha-numeric where Zone (3 numeric) + Sub Zone ID (3 alphanumeric) + Site ID (3 alphanumeric).";
	}
}
