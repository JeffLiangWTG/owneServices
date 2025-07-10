using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class RunSheetSecurityGUIProvider : IRunSheetSecurityQueryProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.SetValue<IRunSheetSecurityQueryProvider>(() => new RunSheetSecurityGUIProvider());
			}
		}

		public static void Unregister(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.RemoveValue<IRunSheetSecurityQueryProvider>();
			}
		}

		RunSheetSecurityGUIProvider() { }

		void IRunSheetSecurityQueryProvider.TryAuthorise(CommonCartageLeg leg)
		{
			if (leg != null)
			{
				ProcessTask customsCleared = null;

				var cartage = leg.Cartage;
				if (cartage != null && cartage.IsImportOrDestination && !Env.Security.LocalTransportRunSheetCustomsClearanceControl.IsAllowed)
				{
					var iWorkflowProvider = cartage.CartageParent as IWorkflowProvider;
					customsCleared = iWorkflowProvider != null ? iWorkflowProvider.WorkflowItems.Milestones.Cast<ProcessTask>().FirstOrDefault(m => m.P9_SE_NKMilestoneEvent == Events.CustomsCleared.Code) : null;
				}

				var requiresSecurityOverride = customsCleared != null && customsCleared.P9_ActualDate.IsEmpty;
				var e = requiresSecurityOverride ? OKToBypassSecurity(leg) : null;
				var authorise = !requiresSecurityOverride || e.IsAllowedToProceed;

				if (authorise)
				{
					var authorisedBy = e != null ? e.AuthorisingStaffLogin : ZString.Empty;
					leg.AuthoriseRunSheet(authorisedBy);
				}
			}
		}

		SecurityLoginEventArgs OKToBypassSecurity(CommonCartageLeg leg)
		{
			var msg = GetSecurityMessage(leg);
			var e = ShowLogin(msg);
			while (!e.IsAllowedToProceed && !e.WasCancelled)
			{
				Globals.Message.ShowError(Res.GetString("611a08cc-b3aa-43ae-a224-e48ca40de0c3", "The login name and password were incorrect or the user does not have sufficient security rights to override Customs Cleared Security."));
				e = ShowLogin("");
			}

			return e;
		}

		SecurityLoginEventArgs ShowLogin(ZString msg)
		{
			var e = msg.IsEmpty ? new SecurityLoginEventArgsWithCustomMessageBox((NoResString)string.Empty, (NoResString)string.Empty, (s) => s.LocalTransportRunSheetCustomsClearanceControl, new CustomMessageBoxCallback((a, b) => ZDialogResult.Yes)) : new SecurityLoginEventArgs((NoResString)msg, (NoResString)string.Empty, (s) => s.LocalTransportRunSheetCustomsClearanceControl);

			var provider = ObjectFactory.New<ISecurityLoginProvider>(Res.GetString("9b937e1e-4cc8-4101-b7ac-498010c00eaf", "Customs Cleared Security"));
			provider.ShowDocumentLoginForDocuments(e);
			return e;
		}

		ZString GetSecurityMessage(CommonCartageLeg leg)
		{
			var parentID = leg.Cartage.CartageParent.UniqueConsignmentID;
			return Res.GetString("6f9eea65-69e9-4ada-9f19-0a283654971b", @"The leg is linked to job '{0}' that has not been Customs Cleared. Attaching this leg to the Run Sheet is normally prevented until '{0}' has been Customs Cleared. You may contact a user/manager (Customs Clearance Controller) with security to override and allow this leg to be attached to the Run Sheet.

Do you wish to override the security and attach."
				, parentID);
		}
	}
}
