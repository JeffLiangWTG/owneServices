using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer
{
	public class LogSubscriptionActionWrapper : IUniversalActionInfo
	{
		public LogSubscriptionActionWrapper(BusinessObject parent)
		{
			Argument.NotNull(parent, "parent");

			PurposeCode = ZString.Empty;
			TriggerEventCode = AutoEvents.EditedARecordCode;
			TriggerDescription = ZString.Empty;
			TriggerType = TriggerType.Manual;
			TriggerScheduledDate = ZDateTimeOffset.Empty;
			TriggerActualDate = ZDateTimeOffset.Now;
			TriggerReference = ZString.Empty;
			ParentBO = parent;
			FactoryForProcessing = parent.Factory;
			RecipientRoleDetails = Array.Empty<RecipientRoleDetail>();
		}

		public ZString ActionType { get; private set; }
		public ZString PurposeCode { get; private set; }

		public RecipientRoleDetail[] RecipientRoleDetails { get; private set; }
		public ZString TriggerEventCode { get; private set; }
		public ZString TriggerDescription { get; private set; }
		public ZInt TriggerCount { get; private set; }
		public TriggerType TriggerType { get; private set; }
		public ZDateTimeOffset TriggerScheduledDate { get; private set; }
		public ZDateTimeOffset TriggerActualDate { get; private set; }
		public ZString TriggerReference { get; private set; }
		public IStmALog TriggeringEvent { get; private set; }
		public INotifications Notifications { get; set; }
		public BusinessObject ParentBO { get; private set; }
		public BusinessObjectFactory FactoryForProcessing { get; private set; }
		public IOrgHeader RecipientOrganization => null;
		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode) => throw new NotImplementedException("If this happens there is something wrong with the grouping code in WorkflowTriggerActionManager");
	}
}
