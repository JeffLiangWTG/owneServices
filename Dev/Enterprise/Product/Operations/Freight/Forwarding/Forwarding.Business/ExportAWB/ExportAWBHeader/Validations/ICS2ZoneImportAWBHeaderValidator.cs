using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ICS2ZoneImportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public ICS2ZoneImportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.IsImportToICS2Zone;
		}

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();
			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessageForDischargeInEU(Parent.EH_ConsigneeTraderNoInfo);

			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_ConsigneeTraderNoType()
		{
			base.CheckEH_ConsigneeTraderNoType();
			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessageForDischargeInEU(Parent.EH_ConsigneeTraderNoTypeInfo);

			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetConsigneeRequiredTraderTypeAndNoMessageForDischargeInEU(ZPropertyInfo traderInfo)
		{
			if (Parent.EH_IsConsigneeDeclarantForAdvanceCargoReporting)
			{
				return ZString.Empty;
			}

			if (!Parent.IsAWBOverridden)
			{
				if (Parent.ConsigneeCategory != OrgConstants.Category.NaturalPersonIndividual)
				{
					if (!traderInfo.Value.IsEmpty)
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
			return Res.GetString("5803b727-7e21-4aba-b856-2bcdc6903665", "The Consignee’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Consignee is a natural person/individual.");
		}

		protected override void CheckEH_AlsoNotifyTraderNo()
		{
			base.CheckEH_AlsoNotifyTraderNo();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessageForDischargeInICS2CountryMember(Parent.EH_AlsoNotifyTraderNoInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_AlsoNotifyTraderNoType()
		{
			base.CheckEH_AlsoNotifyTraderNoType();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessageForDischargeInICS2CountryMember(Parent.EH_AlsoNotifyTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetAlsoNotifyRequiredTraderTypeAndNoMessageForDischargeInICS2CountryMember(ZPropertyInfo traderInfo)
		{
			if (Parent.EH_IsConsigneeDeclarantForAdvanceCargoReporting)
			{
				return ZString.Empty;
			}

			var notifyPartyOrganisation = Parent.NotifyPartyDocumentaryAddress?.Organisation;
			if (notifyPartyOrganisation == null || !(notifyPartyOrganisation?.Country?.IsIcs2Member ?? false))
			{
				return ZString.Empty;
			}

			if (!Parent.IsAWBOverridden)
			{
				if (Parent.NotifyPartyCategory != OrgConstants.Category.NaturalPersonIndividual)
				{
					if (!traderInfo.Value.IsEmpty)
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}

			return Res.GetString("2BD9BB0D-78D2-45Af-AC91-C3E2F8631E21", "The Notify Party’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Notify Party is a natural person/individual.");
		}
	}
}
