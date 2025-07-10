using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ChinaImportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public ChinaImportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.IsImportToChina;
		}

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();
			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessageForDistinationInChina(Parent.EH_ConsigneeTraderNoInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_ConsigneeTraderNoType()
		{
			base.CheckEH_ConsigneeTraderNoType();
			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessageForDistinationInChina(Parent.EH_ConsigneeTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetConsigneeRequiredTraderTypeAndNoMessageForDistinationInChina(ZPropertyInfo traderInfo)
		{
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
			return Res.GetString("5803b727-7e22-4a4a-b856-2bcdc6913664", "The Consignee’s USCI (Unified Social Credit Identifier) is required for imports to China.");
		}

		protected override void CheckEH_ConsigneeContactName()
		{
			base.CheckEH_ConsigneeContactName();

			if (Parent.EH_ConsigneeContactName.IsCargoIMPEmpty())
			{
				Parent.EH_ConsigneeContactNameInfo.AddWarning(Res.GetString("dfd0d6ad-2c11-4e5b-b28a-62b83119eda7", "Contact Name is required for China imports"));
			}
		}

		protected override void CheckEH_ConsigneeContactCode()
		{
			base.CheckEH_ConsigneeContactCode();

			if (!Parent.EH_ConsigneeContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric)
				&& Parent.EH_ConsigneeContactCode != Core.Constants.AWB.ContactCodes.TELEPHONE)
			{
				Parent.EH_ConsigneeContactCodeInfo.AddWarning(Res.GetString("ca2a5990-aaa0-4ba6-84b7-ed9fa1c8ec10", "Contact type \"Telephone\" is required for China imports"));
			}
		}

		protected override void CheckEH_ConsigneeContactDetail()
		{
			base.CheckEH_ConsigneeContactDetail();

			if (Parent.EH_ConsigneeContactDetail.IsCargoIMPEmpty())
			{
				Parent.EH_ConsigneeContactDetailInfo.AddWarning(Res.GetString("4867bb8a-c25b-4fc1-8393-da09311482ac", "Contact telephone number is required for China imports"));
			}
		}

		protected override void CheckEH_AlsoNotifyContactName()
		{
			base.CheckEH_AlsoNotifyContactName();

			if (Parent.EH_AlsoNotifyContactName.IsCargoIMPEmpty())
			{
				Parent.EH_AlsoNotifyContactNameInfo.AddWarning(Res.GetString("dfd0d6ad-2c11-4e5b-b28a-62b83119eda7", "Contact Name is required for China imports"));
			}
		}

		protected override void CheckEH_AlsoNotifyContactCode()
		{
			base.CheckEH_AlsoNotifyContactCode();

			if (Parent.EH_AlsoNotifyContactCode != Core.Constants.AWB.ContactCodes.TELEPHONE
				&& !Parent.EH_AlsoNotifyContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				Parent.EH_AlsoNotifyContactCodeInfo.AddWarning(Res.GetString("ca2a5990-aaa0-4ba6-84b7-ed9fa1c8ec10", "Contact type \"Telephone\" is required for China imports"));
			}
		}

		protected override void CheckEH_AlsoNotifyContactDetail()
		{
			base.CheckEH_AlsoNotifyContactDetail();

			if (Parent.EH_AlsoNotifyContactDetail.IsCargoIMPEmpty())
			{
				Parent.EH_AlsoNotifyContactDetailInfo.AddWarning(Res.GetString("4867bb8a-c25b-4fc1-8393-da09311482ac", "Contact telephone number is required for China imports"));
			}
		}

		protected override void CheckEH_AlsoNotifyTraderNo()
		{
			base.CheckEH_AlsoNotifyTraderNo();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessageForDistinationInChina(Parent.EH_AlsoNotifyTraderNoInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_AlsoNotifyTraderNoType()
		{
			base.CheckEH_AlsoNotifyTraderNoType();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessageForDistinationInChina(Parent.EH_AlsoNotifyTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetAlsoNotifyRequiredTraderTypeAndNoMessageForDistinationInChina(ZPropertyInfo traderInfo)
		{
			if (Parent.NotifyPartyDocumentaryAddress?.Organisation == null)
			{
				return string.Empty;
			}

			if (Parent.EH_AlsoNotifyCountryCode != Constants.CountryCodes.China)
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
			return Res.GetString("5803b727-7e22-4aab-b856-2bcdc6903654", "The Notify Party’s USCI (Unified Social Credit Identifier) is required for imports to China.");
		}
	}
}
