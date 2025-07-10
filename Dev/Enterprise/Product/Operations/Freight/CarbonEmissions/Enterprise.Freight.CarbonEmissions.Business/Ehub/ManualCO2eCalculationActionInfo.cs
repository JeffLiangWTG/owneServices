using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class ManualCO2eCalculationActionInfo : IUniversalActionInfo
	{
		public ManualCO2eCalculationActionInfo(BusinessObject parent)
		{
			Argument.NotNull(parent, "parent");

			PurposeCode = ZString.Empty;
			TriggerEventCode = ZString.Empty;
			TriggerDescription = ZString.Empty;
			TriggerType = TriggerType.Manual;
			TriggerScheduledDate = ZDateTimeOffset.Empty;
			TriggerActualDate = ZDateTimeOffset.Now;
			TriggerReference = ZString.Empty;
			RecipientRoleDetails = Array.Empty<RecipientRoleDetail>();
			ParentBO = parent;
			FactoryForProcessing = parent.Factory;
		}

		public ZString ActionType => default;

		public ZString PurposeCode { get; }

		public RecipientRoleDetail[] RecipientRoleDetails { get; }

		public ZString TriggerEventCode { get; }

		public ZString TriggerDescription { get; }

		public ZInt TriggerCount => default;

		public TriggerType TriggerType { get; }

		public ZDateTimeOffset TriggerScheduledDate { get; }

		public ZDateTimeOffset TriggerActualDate { get; }

		public ZString TriggerReference { get; }

		public IStmALog TriggeringEvent => null;

		public INotifications Notifications { get; set; }

		public BusinessObject ParentBO { get; }

		public BusinessObjectFactory FactoryForProcessing { get; }

		public IOrgHeader RecipientOrganization => null;

		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode) => throw new NotImplementedException("If this happens there is something wrong with the grouping code in WorkflowTriggerActionManager");
	}
}
