using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsHeaderValidation : EU.NCTS.Business.NctsHeaderValidation, INctsHeaderValidation
	{
		public NctsHeaderValidation(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent
		{
			get { return (NctsHeader)base.Parent; }
		}

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
				Parent.StampDutyInfo.AddMessageError(Res.GetString("D15B3344-BB90-4971-80AF-6F1086EC7113", "Please enter Stamp Duty if Stamp Duty Status Code value is 3."));
			}
		}

		protected override void CheckBH_OH_Carrier()
		{
			base.CheckBH_OH_Carrier();

			var orgHeader = Parent.Carrier;
			var orgAddress = Parent.Carrier?.MainAddress;
			if (orgHeader != null && orgAddress != null)
			{
				var warningMessages = NctsCompanyValidationHelper.CheckOrganizationLength(orgHeader, orgAddress);
				foreach (var msg in warningMessages)
				{
					Parent.BH_OH_CarrierInfo.AddWarning(msg);
				}
			}
		}
	}
}
