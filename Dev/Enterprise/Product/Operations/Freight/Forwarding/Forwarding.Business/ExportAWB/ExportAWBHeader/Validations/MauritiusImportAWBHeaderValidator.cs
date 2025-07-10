using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class MauritiusImportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public MauritiusImportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();

			if (Parent.EH_ConsigneeTraderNo.IsEmpty && Parent.IsMAWB)
			{
				var mauritiusConsigneeImportsBRNRequiredMessage = Res.GetString("e0084164-0bbe-45a3-9854-f9a82963aed1", "The Consignee's BRN(Business Registration Number) is required for imports to Mauritius to comply with Manifest reporting.");
				Parent.EH_ConsigneeTraderNoInfo.AddWarning(mauritiusConsigneeImportsBRNRequiredMessage);
			}
		}

		protected override void CheckEH_AlsoNotifyTraderNo()
		{
			base.CheckEH_AlsoNotifyTraderNo();

			if (Parent.EH_AlsoNotifyTraderNo.IsEmpty && Parent.IsMAWB)
			{
				var mauritiusNotifyPartyBRNRequiredMessage = Res.GetString("b17b7318-8263-4d74-82cb-d70e7340763b", "The Notify Party's BRN(Business Registration Number) is required for imports to Mauritius to comply with Manifest reporting.");
				Parent.EH_AlsoNotifyTraderNoInfo.AddWarning(mauritiusNotifyPartyBRNRequiredMessage);
			}
		}

		public bool IsApplicable()
		{
			return Parent.DestinationCountryCode == Core.Constants.CountryCodes.Mauritius;
		}
	}
}

