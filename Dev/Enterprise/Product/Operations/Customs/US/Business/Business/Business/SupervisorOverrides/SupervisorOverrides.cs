using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public class SupervisorOverridesContext : Customs.Business.SupervisorOverridesContext
	{
		public const string SendingPaymentAuthorizationMessage = "SendingPaymentAuthorizationMessage"; // Context
	}
	public class SupervisorOverrides : Customs.Business.SupervisorOverrides
	{
		#region Constants
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Constants : Customs.Business.SupervisorOverrides.Constants
		{
			public const string AffirmationCodeLocationForDeclaration = "Declaration";
			public const string AffirmationCodeLocationForInvoiceFormat = "Invoice Header '{0}'";
			public const string AffirmationCodeLocationForFDAFormat = "Invoice Line '{0}'";
			public const string PayerAccountNumber = "AccountNo: '{0}' -> '{1}'";
			public const string DeclarationHasSpecificMessageErrors = "Sending with specific message errors:";
			public const string FTARecon = "FTA Recon: '{0}' -> '{1}'";
			public const string PaymentType = "Payment Type: '{0}' -> '{1}'";
			public const string ReconIssue = "Recon Issue: '{0}' -> '{1}'";
			public const string AIIRequestedIndicator = "AII Requested Indicator: '{0}'";
		}

		#endregion

		public SupervisorOverrides(IBusiness businessEntity, string context)
			: base(businessEntity, context)
		{
		}

		#region implementation
		public bool HasStatementAsBusinessEntity
		{
			get { return businessEntity is StatementPaymentAction; }
		}

		protected override void CheckSavingDeclaration(BaseJobDeclaration declaration)
		{
			base.CheckSavingDeclaration(declaration);
			var usJobDeclaration = declaration as JobDeclaration;
			CheckFTARecon(usJobDeclaration);
			CheckPaymentType(usJobDeclaration);
			CheckReconIssue(usJobDeclaration);
			CheckAIIRequestedIndicator(usJobDeclaration);
		}

		#endregion

		#region Declaration Defaults

		protected override void CreateMessagesCore()
		{
			base.CreateMessagesCore();

			if (HasStatementAsBusinessEntity && ContextIsSendingPaymentAuthorizationMessage)
			{
				CheckPayerAccountNumber(businessEntity);
			}
		}

		protected override void CheckMessageErrors(IBusiness businessEntity)
		{
			var declaration = businessEntity as JobDeclaration;
			var nominatedMessageErrors = OverrideData.NominatedMessageErrors;
			if (nominatedMessageErrors != null && declaration != null)
			{
				if (ContextIsSendingMessages && ((IMessageNotificationsProvider)declaration).HasMessageErrors)
				{
					var uniqueMessageList = new System.Collections.Generic.List<string>();
					uniqueMessageList.AddRange(declaration.NotificationsIncludingChildren.GetNotifications(CargoWise.EntityFramework.NotificationType.MessageError).GetUniqueMessageList());

					var shouldCheck = false;
					var messagesToLog = Constants.DeclarationHasSpecificMessageErrors;

					if (nominatedMessageErrors.Count == 0 && uniqueMessageList.Count > 0)
					{
						messagesToLog = Customs.Business.SupervisorOverrides.Constants.DeclarationHasAnyMessageErrors;
						shouldCheck = true;
					}
					else
					{
						foreach (NominatedMessageError exception in nominatedMessageErrors)
						{
							if (uniqueMessageList.Any(x => x.ToUpperInvariant().Contains(exception.FieldName.ToUpperInvariant())))
							{
								shouldCheck = true;
								messagesToLog += exception.FieldName + " - " + exception.MessageErrorText + "; ";
							}
						}
					}

					if (shouldCheck && CheckSecurityRightForCheckpointRequired(Env.Security.AllowMessageErrors))
					{
						AddMessageLog(Env.Security.AllowMessageErrors, messagesToLog);
					}
				}
			}
		}

		internal SupervisorOverrideData OverrideData
		{
			get { return USCustomsDataRegistry.Instance.SupervisorOverride.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		void CheckFTARecon(JobDeclaration declaration)
		{
			if (!declaration.US_FixRecon && EntryTypeList.IsValidForRecon(declaration.US_EntryType))
			{
				var defaultUS_NAFTAReconIndicator = ReconIssueCalculator.GetNAFTACalculated(declaration);
				var valueHasBeenChanged = declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_NAFTAReconIndicator) || !declaration.IsInDatabase;

				if (valueHasBeenChanged && declaration.US_NAFTAReconIndicator != defaultUS_NAFTAReconIndicator && CheckSecurityRightForCheckpointRequired(Env.Security.USFTAReconDefault))
				{
					AddMessageLog(Env.Security.USFTAReconDefault, string.Format(Constants.FTARecon, defaultUS_NAFTAReconIndicator, declaration.US_NAFTAReconIndicator));
				}
			}
		}

		void CheckPaymentType(JobDeclaration declaration)
		{
			if (!declaration.IsFTZAdmission && declaration.IORWrapper != null)
			{
				var defaultUS_PaymentType = declaration.IORWrapper.ZO_PaymentType;

				bool valueHasBeenChanged = declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_PaymentType) || !declaration.IsInDatabase;

				if (valueHasBeenChanged && declaration.US_PaymentType != defaultUS_PaymentType && !defaultUS_PaymentType.IsEmpty && CheckSecurityRightForCheckpointRequired(Env.Security.USPaymentTypeDefault))
				{
					AddMessageLog(Env.Security.USPaymentTypeDefault, string.Format(Constants.PaymentType, defaultUS_PaymentType, declaration.US_PaymentType));
				}
			}
		}

		void CheckReconIssue(JobDeclaration declaration)
		{
			if (!declaration.US_FixRecon && EntryTypeList.IsValidForRecon(declaration.US_EntryType))
			{
				var defaultUS_OtherReconIndicator = ReconIssueCalculator.GetReconIssueCodeCalculated(declaration);
				var valueHasBeenChanged = declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_OtherReconIndicator) || !declaration.IsInDatabase;

				if (valueHasBeenChanged && declaration.US_OtherReconIndicator != defaultUS_OtherReconIndicator &&
					!defaultUS_OtherReconIndicator.IsEmpty && CheckSecurityRightForCheckpointRequired(Env.Security.USReconIssueDefault))
				{
					AddMessageLog(Env.Security.USReconIssueDefault, string.Format(Constants.ReconIssue, defaultUS_OtherReconIndicator, declaration.US_OtherReconIndicator));
				}
			}
		}

		void CheckAIIRequestedIndicator(JobDeclaration declaration)
		{
			if (!declaration.IsFTZAdmission)
			{
				bool valueHasBeenChanged = declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_IsAIIRequested) ||
										(!declaration.IsInDatabase && declaration.US_IsAIIRequested);

				if (valueHasBeenChanged && CheckSecurityRightForCheckpointRequired(Env.Security.USAllRequestedIndicator))
				{
					AddMessageLog(Env.Security.USAllRequestedIndicator, string.Format(Constants.AIIRequestedIndicator, declaration.US_IsAIIRequested));
				}
			}
		}

		#endregion

		protected override ZBool CheckSecurityRightForCheckpointRequired(SecurityCheckpoint checkpoint)
		{
			return checkpoint.IsAllowed || AnyStaffOrGroupHasSecurityRight(checkpoint.Code);
		}

		public bool ContextIsSendingPaymentAuthorizationMessage
		{
			get { return context == SupervisorOverridesContext.SendingPaymentAuthorizationMessage; }
		}

		#region Statement Defaults
		void CheckPayerAccountNumber(IBusiness businessEntity)
		{
			if (businessEntity is StatementPaymentAction statementPaymentAction)
			{
				var accountNumber = statementPaymentAction.StatementHeader?.ImporterWrapper?.ZO_AccountNo ?? ZString.Empty;

				if (!accountNumber.IsEmpty && statementPaymentAction.PayerUnitNo != accountNumber && CheckSecurityRightForCheckpointRequired(Env.Security.USPAYERAccountNumberDefault))
				{
					AddMessageLog(Env.Security.USPAYERAccountNumberDefault, string.Format(CultureInfo.InvariantCulture, Constants.PayerAccountNumber, accountNumber, statementPaymentAction.PayerUnitNo));
				}
			}
		}
		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Customs.US.Business
{
	#region Registry Setup

	public enum TargetInRegistry { NoTarget, All, AllowMessageErrors, MergeBy, FTARecon, PaymentType, ReconIssue, PayerAccountNumber, AIIRequestedIndicator }

	#endregion
}

#endif
#endregion
