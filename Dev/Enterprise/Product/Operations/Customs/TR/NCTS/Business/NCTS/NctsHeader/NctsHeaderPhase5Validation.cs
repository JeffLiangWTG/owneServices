
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsHeaderPhase5Validation : EU.NCTS.Business.NctsHeaderPhase5Validation, INctsHeaderValidation
	{
		public NctsHeaderPhase5Validation(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateStampDutyStatus();
			ValidateStampDuty();
		}

		public void ValidateStampDutyStatus()
		{
			ValidateCalculatedProperty(Parent.StampDutyStatusInfo);
		}

		protected void CheckStampDutyStatus()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.StampDutyStatusInfo);
		}

		public void ValidateStampDuty()
		{
			ValidateCalculatedProperty(Parent.StampDutyInfo);
		}

		protected void CheckStampDuty()
		{
			if (Parent.StampDutyStatus == StampDutyStatusCodeList.Codes.D3 && Parent.StampDuty.IsEmpty)
			{
				Parent.StampDutyInfo.AddMessageError(Res.GetString("F3B20628-4E00-48CC-A20E-933946BDE6C0", "Please enter Stamp Duty if Stamp Duty Status Code value is 3."));
			}
		}
	}
}
