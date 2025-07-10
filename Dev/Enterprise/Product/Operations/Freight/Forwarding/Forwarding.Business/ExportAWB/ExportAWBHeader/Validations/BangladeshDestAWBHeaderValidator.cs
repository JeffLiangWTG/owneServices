using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class BangladeshDestAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public BangladeshDestAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.DestinationCountryCode == Core.Constants.CountryCodes.Bangladesh;
		}

		#region EH_CustomsValue

		bool GoodsValueRequired => (Parent.IsIndirectHAWB || Parent.IsDirectMAWB) && FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.Value;

		protected override void CheckEH_CustomsValue()
		{
			base.CheckEH_CustomsValue();

			if (Parent.EH_CustomsValue.IsEmpty && GoodsValueRequired)
			{
				Parent.EH_CustomsValueInfo.AddMessageError(Res.GetString("db7db985-97f2-40c0-8919-e602f76719a2", "Goods Value (per the commercial invoice) is required for imports to Bangladesh."));
			}
		}

		#endregion
	}
}
