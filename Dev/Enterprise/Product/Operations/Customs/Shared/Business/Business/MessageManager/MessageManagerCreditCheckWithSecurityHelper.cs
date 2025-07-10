using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business
{
	public class MessageManagerCreditCheckWithSecurityHelper
	{
		#region ctor
		public MessageManagerCreditCheckWithSecurityHelper(BaseJobDeclaration declaration, ZString defaultApprovalRequestReason)
			: this(declaration, false, defaultApprovalRequestReason)
		{
		}

		public MessageManagerCreditCheckWithSecurityHelper(BaseJobDeclaration declaration, bool isCreditCheckForVCM = false, string defaultApprovalRequestReason = null)
		{
			this.declaration = declaration;
			this.isCreditCheckForVCM = isCreditCheckForVCM;
			this.defaultApprovalRequestReason = defaultApprovalRequestReason ?? ZString.Empty;
			SetAdditionalConditionPreCreditCheck(PreDefinedAdditionalConditions.AutoRateDSB);
		}

		#endregion

		public delegate string AdditionalConditionPreCreditCheck(BaseJobDeclaration declaration);

		public void SetAdditionalConditionPreCreditCheck(params AdditionalConditionPreCreditCheck[] additionalConditions)
		{
			this.additionalConditions = additionalConditions;
		}

		public static class PreDefinedAdditionalConditions
		{
			public static AdditionalConditionPreCreditCheck AutoRateDSB
			{
				get
				{
					return (BaseJobDeclaration declaration) =>
					{
						var result = string.Empty;
						var integrator = new InvoicePostingAccountingIntegrator();
						var autoBillingResult = integrator.IntegrateIfNecessary(new JobDeclarationIAccIntegrationDataProviderForPreCreditCheck(declaration));
						if (integrator.HasExceptionDuringIntegration)
						{
							result = Res.GetString("A368B034-7BEB-4EC6-BCB4-5BDDC10D5A9A", "Auto-rating of Customs Disbursement failed:\r\n\r\n{0}", autoBillingResult.Message);
						}
						return result;
					};
				}
			}
		}

		/// <summary>
		/// Looks at the shared registry, looks at the declaration, looks at outstanding PIA AR invoices, looks at Denied Party Status but DOES NOT make any decisions about whether the
		/// message you are trying to send is one that should incur a credit check.  You must do that.
		/// </summary>
		public bool IsCreditCheckOKToSend
		{
			get
			{
				var isCreditCheckEnabled = isCreditCheckForVCM ? declaration.IsCreditCheckEnabledForValidateCustomsMessaging
					: Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.CreditCheckOnMessageSend.GetValueWithoutFallback(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
				return PerformCreditCheck(isCreditCheckEnabled);
			}
		}

		public bool IsDeniedPartyOKToSend
		{
			get
			{
				return PerformCreditCheck(false);
			}
		}

		bool PerformCreditCheck(bool creditCheckEnabled)
		{
			ReasonForNotAllowed = null;

			if (declaration != null)
			{
				if (creditCheckEnabled && additionalConditions != null)
				{
					foreach (var additionalCheck in additionalConditions)
					{
						ReasonForNotAllowed = additionalCheck(declaration);
						if (!string.IsNullOrEmpty(ReasonForNotAllowed))
						{
							break;
						}
					}
				}

				if (string.IsNullOrEmpty(ReasonForNotAllowed))
				{
					var result = CreditCheckAndDPSHelper.RunCheck(declaration, creditCheckEnabled, defaultApprovalRequestReason);
					if (!result.Message.IsEmpty)
					{
						if (Globals.CanShowDialogs)
						{
							CreditCheckAndDPSHelper.ProcessOverride(result, declaration, declaration.CreditRestrictionMessageCaption);
						}

						ReasonForNotAllowed = result.Message;
						IsCreditCheckDoneOutsideCW1 = result.IsExternalSystemUsed;
					}
				}
			}

			return string.IsNullOrEmpty(ReasonForNotAllowed);
		}

		public static bool CheckDeniedParty(BaseJobDeclaration dec)
		{
			var creditCheckManager = new MessageManagerCreditCheckWithSecurityHelper(dec);

			bool result = creditCheckManager.IsDeniedPartyOKToSend;
			if (!result)
			{
				dec.MessageInitiator?.NotifyUserOfAnInvalidOperation(creditCheckManager.ReasonForNotAllowed);
			}

			return result;
		}

		#region Fields

		public string ReasonForNotAllowed { get; private set; }
		public bool IsCreditCheckDoneOutsideCW1 { get; private set; }

		readonly BaseJobDeclaration declaration;
		readonly ZString defaultApprovalRequestReason;
		readonly ZBool isCreditCheckForVCM;
		IEnumerable<AdditionalConditionPreCreditCheck> additionalConditions;

		#endregion
	}

	public class JobDeclarationIAccIntegrationDataProviderForPreCreditCheck : JobDeclarationIAccIntegrationDataProvider
	{
		public JobDeclarationIAccIntegrationDataProviderForPreCreditCheck(BaseJobDeclaration declaration)
			: base(ChargePosterBehaviours.AutoRateDSB, declaration.EntryHeadersForPreCreditCheck.Select(x => x.PK), declaration.PK, declaration.IsIntegrationWithAccountingSupported, declaration.IsValidatingCustomsMessaging ? declaration.Factory : new BusinessObjectFactory())
		{
			declarationDocumentDelivery = declaration;
		}

		readonly ICreditControlledDocumentDelivery declarationDocumentDelivery;

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			base.OnIntegratedWithAccountingSuccessfully();
			if (declarationDocumentDelivery.OrganisationsForCreditChecks != null)
			{
				foreach (var org in declarationDocumentDelivery.OrganisationsForCreditChecks)
				{
					org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				}
			}
		}
	}
}
