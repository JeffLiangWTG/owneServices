using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CreditCheckResult
	{
		public bool IsAllowedToProceed => Args?.IsAllowedToProceed ?? true;
		public ZString Message => Args?.Message ?? ZString.Empty;
		public bool IsExternalSystemUsed => Args?.IsExternalAccountingSystemUsed ?? false;
		internal SecurityLoginEventArgsForDocumentApproval Args { get; set; }
	}

	public static class CreditCheckAndDPSHelper
	{
		public static CreditCheckResult RunCheck(ICreditControlledDocumentDelivery docDeliveryObj, bool runCreditCheck, string defaultApprovalRequestReason)
		{
			var result = new CreditCheckResult();
			var creditManager = new DocumentDeliveryCreditControlManager();
			var creditMessage = creditManager.GetDocumentDeliveryStatusForCreditManagement((BusinessObject)docDeliveryObj, Res.GetString("3473b4f2-9c81-4afd-ac66-aa28eccd20cf", "message"), ZGuid.Empty, runCreditCheck && IsCreditCheckEnabled);

			if (!creditMessage.IsEmpty)
			{
				result.Args = creditManager.GetSecurityLoginEventArgs(defaultApprovalRequestReason);
				result.Args.IsCustomsSubmission = true;
			}

			return result;
		}

		public static void ProcessOverride(CreditCheckResult creditCheckResult, ICreditControlledDocumentDelivery bizObj, ZString caption)
		{
			if (!creditCheckResult.IsAllowedToProceed)
			{
				var overrider = ObjectFactory.Get<ICreditCheckAndDPSOverride>();
				overrider.ProcessOverride(creditCheckResult.Args, bizObj, caption);

				if (creditCheckResult.Args.IsAllowedToProceed && bizObj is EnterpriseBusinessObject eBizObj)
				{
					var log = eBizObj.Logs.AddNew(Events.HoldStatusOverride, creditCheckResult.Args.MessageToShowWhenNotAllowed.GetUnresolvedString());
					var factory = eBizObj.Factory;
					var authorisingUser = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, creditCheckResult.Args.AuthorisingStaffLogin));
					if (authorisingUser != null)
					{
						log.SL_GS_NKUser = authorisingUser.GS_Code;
					}
				}
			}
		}

		static bool IsCreditCheckEnabled => Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.CreditCheckOnMessageSend.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
	}
}
