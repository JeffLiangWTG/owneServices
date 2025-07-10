using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class BoliviaImportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public BoliviaImportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public bool IsApplicable() => Parent.IsImportToBolivia && Parent.IsBolivianNITRequired;

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();
			if (Parent.EH_ConsigneeTraderNoInfo.Value.IsEmpty)
			{
				string boliviaConsigneeNITNumberRequiredMessage = Res.GetString("6a64e7f9-f1d9-401b-8c93-ef3417c26707", "The Consignee's NIT number is required for shipments to Bolivia.");
				Parent.EH_ConsigneeTraderNoInfo.AddMessageError(boliviaConsigneeNITNumberRequiredMessage);
			}
		}

		new ExportAWBHeader Parent => (ExportAWBHeader)base.Parent;
	}
}
