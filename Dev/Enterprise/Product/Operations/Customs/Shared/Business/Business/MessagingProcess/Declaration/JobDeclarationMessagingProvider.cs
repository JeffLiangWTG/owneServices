using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.Security;

namespace Enterprise.Customs.Business.MessagingProcess.Declaration
{
	public sealed class JobDeclarationMessagingProvider<TSendingObjectParent, TSendingObjectObject> : ICommonJobDeclarationProvider
		where TSendingObjectParent : JobDeclarationMessageSendingObjectParent<TSendingObjectObject>
		where TSendingObjectObject : JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessagingProvider(TSendingObjectParent parent) : this(parent, null) { }
		public JobDeclarationMessagingProvider(TSendingObjectParent parent, ISupportCreditAndDPSCheckOptions supportCreditAndDPSCheckOptions) : this(GetJobDeclarationBondedWarehouseAutomation, parent, supportCreditAndDPSCheckOptions) { }

		internal JobDeclarationMessagingProvider(Func<CusEntryHeader, MessageAction, IJobDeclarationBondedWarehouseAutomation> warehouseAutomationProvider, TSendingObjectParent parent) : this(warehouseAutomationProvider, parent, null) { }
		internal JobDeclarationMessagingProvider(Func<CusEntryHeader, MessageAction, IJobDeclarationBondedWarehouseAutomation> warehouseAutomationProvider, TSendingObjectParent parent, ISupportCreditAndDPSCheckOptions supportCreditAndDPSCheckOptions)
		{
			this.parent = parent;
			this.warehouseAutomationProvider = warehouseAutomationProvider;
			this.supportCreditAndDPSCheckOptions = supportCreditAndDPSCheckOptions;

			isWHSUniversalXMLActive = parent.ParentDeclaration.IsWHSUniversalXMLActive;
			creditCheckEnabled = Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.CreditCheckOnMessageSend.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}
		TSendingObjectParent parent { get; }
		readonly Func<CusEntryHeader, MessageAction, IJobDeclarationBondedWarehouseAutomation> warehouseAutomationProvider;
		readonly bool isWHSUniversalXMLActive;
		readonly bool creditCheckEnabled;
		readonly ISupportCreditAndDPSCheckOptions supportCreditAndDPSCheckOptions;

		BaseJobDeclaration Declaration => parent.ParentDeclaration;

		IReadOnlyCollection<MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult)
		{
			var notifications = new List<MessageSendingNotification>();

			if (isWHSUniversalXMLActive)
			{
				notifications.AddRange(RunWarehouseAutomationValidation());
			}

			return notifications;
		}

		void ISupportConfigureProcess.ConfigureProcess(ActionChain actionChain)
		{
			ConfigureAutoRateDeclarationStep(actionChain);
			ConfigureWarehouseAutomtionSteps(actionChain);
		}

		#region Warehouse Automation
		internal IReadOnlyCollection<MessageSendingNotification> RunWarehouseAutomationValidation()
		{
			var notifications = new List<MessageSendingNotification>();

			if (warehouseAutomations.Any())
			{
				foreach (TSendingObjectObject sendingObj in parent.SendingObjectsCollection)
				{
					var header = sendingObj.Header;
					if (!header.IsBondedWarehousingDisabled)
					{
						var errorMsg = header.GetMessageErrorOfRequiredFieldsForBondedWarehousing(true, true, checkEntryDetails: header.IsOutOfWarehouseWarehousing);
						if (!errorMsg.IsEmpty)
						{
							notifications.Add(new MessageSendingError(errorMsg));
						}
					}
				}

				foreach (var automation in warehouseAutomations)
				{
					var errorMsg = automation.GetPendingTransactionError();
					if (!errorMsg.IsEmpty)
					{
						notifications.Add(new MessageSendingError(errorMsg));
						break;
					}
				}
			}

			return notifications;
		}

		internal void ConfigureWarehouseAutomtionSteps(ActionChain actionChain)
		{
			if (isWHSUniversalXMLActive && warehouseAutomations.Any())
			{
				var warehouseChain = CreateWarehouseAutomationChain();
				var postActionChain = warehouseChain.FindAction(WHSPostActions);

				actionChain.FindAction(SendMessagesProcess.SendMessageActions.CreateMessages).WrapInChain(warehouseChain, postActionChain, false, ActionLink.Common);
			}
		}

		internal ActionChain CreateWarehouseAutomationChain()
		{
			var warehouseChain = new ActionChain(WHSPreActions, WarehouseAutomationPreActions);
			warehouseChain.AppendAction(WHSPostActions, WarehouseAutomationPostActions, ActionLink.Success)
						  .AppendAction(WHSRestoreActions, WarehouseAutomationRestoreActions, ActionLink.Failure);

			return warehouseChain;
		}

		internal ActionResult WarehouseAutomationPreActions(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.Success && warehouseAutomations.Any())
			{
				result.Success = warehouseAutomations.TrueForAll(x => x.ExecutePreAction());
			}

			return result;
		}

		internal ActionResult WarehouseAutomationPostActions(ActionResult previousResult)
		{
			var result = previousResult;

			if (warehouseAutomations.Any())
			{
				warehouseAutomations.ForEach(x => x.ExecutePostAction());
			}

			return result;
		}

		internal ActionResult WarehouseAutomationRestoreActions(ActionResult previousResult)
		{
			var result = previousResult;

			if (!result.Success && warehouseAutomations.Any())
			{
				warehouseAutomations.ForEach(x => x.ExecuteRestoreAction());
			}

			return result;
		}

		internal List<IJobDeclarationBondedWarehouseAutomation> warehouseAutomations => fWarehouseAutomations ?? (fWarehouseAutomations = SetupWarehouseAutomation());
		List<IJobDeclarationBondedWarehouseAutomation> fWarehouseAutomations;
		internal List<IJobDeclarationBondedWarehouseAutomation> SetupWarehouseAutomation()
		{
			var automations = new List<IJobDeclarationBondedWarehouseAutomation>();

			foreach (TSendingObjectObject sendingObj in parent.SendingObjectsCollection)
			{
				var header = sendingObj.Header;
				if (sendingObj is IJobDeclarationSendingObjectWarehouseProvider whsSupporter && whsSupporter.ShouldProcessWarehouse)
				{
					var automation = warehouseAutomationProvider(header, whsSupporter.GetMessageAction());

					if (automation.PrepareForProcessing(header.GetInventoryAutomationAction(), true))
					{
						automations.Add(automation);
					}
				}
			}

			return automations;
		}

		internal static IJobDeclarationBondedWarehouseAutomation GetJobDeclarationBondedWarehouseAutomation(CusEntryHeader header, MessageAction messageAction) => new JobDeclarationBondedWarehouseAutomation(header, messageAction);

		const string WHSPreActions = "WHSAutomationPreActions";
		const string WHSPostActions = "WHSAutomationPostActions";
		const string WHSRestoreActions = "WHSAutomationRestoreActions";
		#endregion

		#region Security
		SecurityCheckpoint ISupportSecurityCheckpoints.SendMessagesSecurityCheckpoint => null;
		SecurityCheckpoint ISupportSecurityCheckpoints.SendMessagesWithErrorsSecurityCheckpoint => Env.Security.CustomsDeclarationSendWithMessageErrors;
		SecurityCheckpoint ISupportSecurityCheckpoints.SendMessagesWithErrorsOverrideSecurityCheckpoint => Env.Security.AllowMessageErrors;
		#endregion

		#region Auto-Rate Declaration
		void ConfigureAutoRateDeclarationStep(ActionChain actionChain)
		{
			if (creditCheckEnabled)
			{
				actionChain.FindAction(SendMessagesProcess.SendMessageActions.CreateMessages).InsertActionBefore(AutoRateDeclarationAction, AutoRateDeclaration, ActionLink.Success);
			}
		}

		internal ActionResult AutoRateDeclaration(ActionResult previousResult)
		{
			var result = previousResult;

			if (creditCheckEnabled)
			{
				var autoRatingResult = MessageManagerCreditCheckWithSecurityHelper.PreDefinedAdditionalConditions.AutoRateDSB(parent.ParentDeclaration);
				if (!string.IsNullOrEmpty(autoRatingResult))
				{
					result.Success = false;
					result.Notifications.AddError(autoRatingResult);
				}
			}

			return result;
		}

		const string AutoRateDeclarationAction = "AutoRateDeclaration";
		#endregion

		#region CreditAndDPSCheck
		bool ISupportCreditAndDPSCheckOptions.MustRunCreditCheck => (supportCreditAndDPSCheckOptions?.MustRunCreditCheck ?? true) && creditCheckEnabled;
		string ISupportCreditAndDPSCheckOptions.CreditRestrictionMessageCaption => supportCreditAndDPSCheckOptions?.CreditRestrictionMessageCaption ?? Declaration.CreditRestrictionMessageCaption;
		string ISupportCreditAndDPSCheckOptions.DefaultApprovalRequestReason => supportCreditAndDPSCheckOptions?.DefaultApprovalRequestReason ?? string.Empty;
		ICreditControlledDocumentDelivery ISupportCreditAndDPSCheck.DocumentDeliveryObject => Declaration;
		#endregion
	}
}
