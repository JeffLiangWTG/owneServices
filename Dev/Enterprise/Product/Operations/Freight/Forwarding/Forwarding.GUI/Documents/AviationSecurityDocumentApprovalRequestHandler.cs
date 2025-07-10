using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class AviationSecurityDocumentApprovalRequestHandler : IAviationSecurityDocumentApprovalRequestHandler
	{
		public void Initialize(ISecurityLogin loginBisObject, Func<LoginDialogResult> showLoginDocumentLoginForm)
		{
			this.loginBisObject = loginBisObject;
			this.showLoginDocumentLoginForm = showLoginDocumentLoginForm;
		}

		ISecurityLogin loginBisObject;
		Func<LoginDialogResult> showLoginDocumentLoginForm;

		public bool IsRestrictedForSingleStep(ISecurityLoginEventArgsForDocumentApproval e)
		{
			return e.IsAviationSecurityFreightMovementRestricted;
		}

		public void HandleApprovalRequestForSingleStep(ISecurityLoginEventArgsForDocumentApproval e)
		{
			var loginDialogResult = showLoginDocumentLoginForm();

			var supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration();

			if (loginDialogResult == LoginDialogResult.Yes)
			{
				var factory = new BusinessObjectFactory();
				var user = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, loginBisObject.Login);

				if (user != null && supplyChainSecurityConfiguration.IsUserCertifiedForAviationSecurity(user))
				{
					e.IsAllowedToProceed = true;
					e.AuthorisingStaffLogin = loginBisObject.Login;
				}
				else
				{
					(loginBisObject as SecurityLogin).MessageToShowWhenNotPrinting = supplyChainSecurityConfiguration.AviationSecurityFreightMovementRestrictedErrorMessage;
				}
			}
		}

		public bool IsRestrictedForMultiStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			return false;
		}

		public void HandleApprovalRequestForMultiStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
		}
	}
}
