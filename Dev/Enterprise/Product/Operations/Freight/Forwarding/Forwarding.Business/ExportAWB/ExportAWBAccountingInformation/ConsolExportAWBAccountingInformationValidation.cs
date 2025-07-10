using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBAccountingInformationValidation : ExportAWBAccountingInformationValidation
	{
		public ConsolExportAWBAccountingInformationValidation(ConsolExportAWBAccountingInformation parent)
			: base(parent)
		{
		}

		public new ConsolExportAWBAccountingInformation Parent
		{
			get { return (ConsolExportAWBAccountingInformation)base.Parent; }
		}

		protected override void CheckEA_Information()
		{
			base.CheckEA_Information();
			MandatoryValidation.WarnIfNotEntered(Parent.EA_InformationInfo);

			if (!Parent.EA_Information.IsEmpty && Parent.EA_Information.Length > 34)
			{
				Parent.EA_InformationInfo.AddWarning(Res.GetString("57B7B25C-72C1-4413-9A8D-BA05109DD75A", "Only the first 34 characters will be used for FWB messaging."));
			}
		}

		protected override void CheckEA_InformationID()
		{
			base.CheckEA_InformationID();
			if (!Parent.EA_InformationID.IsEmpty)
			{
				if (!Parent.IsItalianRegistrationCode)
				{
					ListValidation.ErrorIfInvalidCode(Parent.EA_InformationIDInfo, Parent.Lookups.AccountingCodes);
				}
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.EA_InformationIDInfo);
			}
		}
	}
}
