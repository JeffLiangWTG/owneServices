using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ChinaTransitingAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public ChinaTransitingAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}
		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.IsTransitingThroughChina;
		}

		protected override void CheckEH_ShipperContactName()
		{
			base.CheckEH_ShipperContactName();

			if (Parent.EH_ShipperContactName.IsCargoIMPEmpty())
			{
				Parent.EH_ShipperContactNameInfo.AddWarning(Res.GetString("1943892c-5690-409a-8b37-984061b6df44", "Contact Name is required for China transit"));
			}
		}

		protected override void CheckEH_ShipperContactCode()
		{
			base.CheckEH_ShipperContactCode();

			if (Parent.EH_ShipperContactCode != Core.Constants.AWB.ContactCodes.TELEPHONE)
			{
				Parent.EH_ShipperContactCodeInfo.AddWarning(Res.GetString("caf6852e-4164-40a4-917f-28c24d0f8816", "Contact type \"Telephone\" is required for China transit"));
			}
		}

		protected override void CheckEH_ShipperContactDetail()
		{
			base.CheckEH_ShipperContactDetail();

			if (Parent.EH_ShipperContactDetail.IsCargoIMPEmpty())
			{
				Parent.EH_ShipperContactDetailInfo.AddWarning(Res.GetString("03eedf4f-4665-42a7-985b-03d7c3e7c5f7", "Contact telephone number is required for China transit"));
			}
		}

		protected override void CheckEH_ConsigneeContactName()
		{
			base.CheckEH_ConsigneeContactName();

			if (Parent.EH_ConsigneeContactName.IsCargoIMPEmpty())
			{
				Parent.EH_ConsigneeContactNameInfo.AddWarning(Res.GetString("1943892c-5690-409a-8b37-984061b6df44", "Contact Name is required for China transit"));
			}
		}

		protected override void CheckEH_ConsigneeContactCode()
		{
			base.CheckEH_ConsigneeContactCode();

			if (Parent.EH_ConsigneeContactCode != Core.Constants.AWB.ContactCodes.TELEPHONE)
			{
				Parent.EH_ConsigneeContactCodeInfo.AddWarning(Res.GetString("caf6852e-4164-40a4-917f-28c24d0f8816", "Contact type \"Telephone\" is required for China transit"));
			}
		}

		protected override void CheckEH_ConsigneeContactDetail()
		{
			base.CheckEH_ConsigneeContactDetail();

			if (Parent.EH_ConsigneeContactDetail.IsCargoIMPEmpty())
			{
				Parent.EH_ConsigneeContactDetailInfo.AddWarning(Res.GetString("03eedf4f-4665-42a7-985b-03d7c3e7c5f7", "Contact telephone number is required for China transit"));
			}
		}

		protected override void CheckEH_AlsoNotifyContactName()
		{
			base.CheckEH_AlsoNotifyContactName();

			if (Parent.EH_AlsoNotifyContactName.IsCargoIMPEmpty())
			{
				Parent.EH_AlsoNotifyContactNameInfo.AddWarning(Res.GetString("1943892c-5690-409a-8b37-984061b6df44", "Contact Name is required for China transit"));
			}
		}

		protected override void CheckEH_AlsoNotifyContactCode()
		{
			base.CheckEH_AlsoNotifyContactCode();

			if (Parent.EH_AlsoNotifyContactCode != Core.Constants.AWB.ContactCodes.TELEPHONE)
			{
				Parent.EH_AlsoNotifyContactCodeInfo.AddWarning(Res.GetString("caf6852e-4164-40a4-917f-28c24d0f8816", "Contact type \"Telephone\" is required for China transit"));
			}
		}

		protected override void CheckEH_AlsoNotifyContactDetail()
		{
			base.CheckEH_AlsoNotifyContactDetail();

			if (Parent.EH_AlsoNotifyContactDetail.IsCargoIMPEmpty())
			{
				Parent.EH_AlsoNotifyContactDetailInfo.AddWarning(Res.GetString("03eedf4f-4665-42a7-985b-03d7c3e7c5f7", "Contact telephone number is required for China transit"));
			}
		}
	}
}
