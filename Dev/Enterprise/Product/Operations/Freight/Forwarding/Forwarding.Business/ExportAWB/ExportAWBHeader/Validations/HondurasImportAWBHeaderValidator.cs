using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class HondurasImportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public HondurasImportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public bool IsApplicable() => Parent.IsHondurasRTNRequired;

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();
			if (Parent.EH_ConsigneeTraderNoInfo.Value.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddMessageError(HondurasConsigneeRTNNumberRequiredMessage);
			}
		}

		string HondurasConsigneeRTNNumberRequiredMessage => Res.GetString("28181b81-d5fc-4638-b38c-ba4347e05b9e", "The Consignee's RTN number is required for inbound shipments to Honduras.");

		new ExportAWBHeader Parent => (ExportAWBHeader)base.Parent;
	}
}
