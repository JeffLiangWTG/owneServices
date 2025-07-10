using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class MoroccoDestAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public MoroccoDestAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.DestinationCountryCode == Core.Constants.CountryCodes.Morocco;
		}

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();
			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessageForDischargeInMorocco(Parent.EH_ConsigneeTraderNoInfo);

			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_ConsigneeTraderNoType()
		{
			base.CheckEH_ConsigneeTraderNoType();
			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessageForDischargeInMorocco(Parent.EH_ConsigneeTraderNoTypeInfo);

			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetConsigneeRequiredTraderTypeAndNoMessageForDischargeInMorocco(ZPropertyInfo traderInfo)
		{
			if (!traderInfo.Value.IsEmpty)
			{
				return ZString.Empty;
			}

			return Res.GetString("75DBEE6B-6FB8-4F89-B690-C764637FCBB1", "The Consignee's ICE number is required for inbound shipments to Morocco.");
		}

		protected override void CheckEH_AlsoNotifyTraderNo()
		{
			base.CheckEH_AlsoNotifyTraderNo();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessageForDischargeInMorocco(Parent.EH_AlsoNotifyTraderNoInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_AlsoNotifyTraderNoType()
		{
			base.CheckEH_AlsoNotifyTraderNoType();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessageForDischargeInMorocco(Parent.EH_AlsoNotifyTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetAlsoNotifyRequiredTraderTypeAndNoMessageForDischargeInMorocco(ZPropertyInfo traderInfo)
		{
			if (Parent.NotifyPartyDocumentaryAddress?.Organisation == null)
			{
				return ZString.Empty;
			}

			if (!traderInfo.Value.IsEmpty)
			{
				return ZString.Empty;
			}

			return Res.GetString("EA12D59A-F42F-4E69-B9CD-1A4475B1B952", "The Notify Party’s ICE number is required for inbound shipments to Morocco.");
		}
	}
}
