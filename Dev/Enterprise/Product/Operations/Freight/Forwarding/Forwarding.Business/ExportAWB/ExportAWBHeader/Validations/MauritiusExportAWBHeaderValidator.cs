using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class MauritiusExportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public MauritiusExportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		protected override void CheckEH_ShipperTraderNo()
		{
			base.CheckEH_ShipperTraderNo();

			if (Parent.EH_ShipperTraderNo.IsEmpty && Parent.IsMAWB)
			{
				var mauritiusExportsBRNRequiredMessage = Res.GetString("e5fb01db-61d9-4e79-bf3f-47b952a50845", "The Shipper's BRN(Business Registration Number) is required for exports from Mauritius to comply with Manifest reporting.");
				Parent.EH_ShipperTraderNoInfo.AddWarning(mauritiusExportsBRNRequiredMessage);
			}
		}

		public bool IsApplicable()
		{
			return Parent.OriginCountryCode == Core.Constants.CountryCodes.Mauritius;
		}
	}
}

