using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class BoliviaExportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public BoliviaExportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public bool IsApplicable() => Parent.IsExportFromBolivia && Parent.IsBolivianNITRequired;

		protected override void CheckEH_ShipperTraderNo()
		{
			base.CheckEH_ShipperTraderNo();
			if (Parent.EH_ShipperTraderNoInfo.Value.IsEmpty)
			{
				string boliviaShipperNITNumberRequiredMessage = Res.GetString("fc041056-8e58-4030-93a9-e12c470db195", "The Consignor's NIT number is required for shipments from Bolivia.");
				Parent.EH_ShipperTraderNoInfo.AddMessageError(boliviaShipperNITNumberRequiredMessage);
			}
		}

		new ExportAWBHeader Parent => (ExportAWBHeader)base.Parent;
	}
}
