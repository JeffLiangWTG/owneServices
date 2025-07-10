using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public interface IRequiresWorkflowSecurity
	{
		string WorkflowItemEditCheckpointCode { get; }
		string WorkflowItemJustViewCheckpointCode { get; }
		ModuleIdentifier WorkflowProviderModuleId { get; }
		bool ViewOnly { get; set; }
	}

	public static class IRequiresWorkflowSecurityExtensions
	{
		public static void CheckAndConsumeWorkflowLicenceIfAllowed<T>(this T requireWorkflowSecurity) where T : ZUserControl, IRequiresWorkflowSecurity
		{
			ModuleIdentifier moduleId = requireWorkflowSecurity.WorkflowProviderModuleId;
			Action setView = () => requireWorkflowSecurity.ViewOnly = true;
			CheckAndConsumeWorkflowLicenceIfAllowed(requireWorkflowSecurity, moduleId, setView, requireWorkflowSecurity.WorkflowItemEditCheckpointCode,
				requireWorkflowSecurity.WorkflowItemJustViewCheckpointCode);
		}

		public static void CheckAndConsumeWorkflowLicenceIfAllowed<T>(this T requireWorkflowSecurity, ModuleIdentifier moduleId, Action setView, string editCheckpointCode, string viewCheckpointCode) where T : ZUserControl
		{
			if (!requireWorkflowSecurity.IsDesignMode())
			{
				var canEdit = true;

				if (WorkflowDataRegistry.Instance.EnableCheckpointsForWorkflowSecurity.Value)
				{
					var editCheckpoint = GetSecurityCheckpoint(requireWorkflowSecurity, moduleId, editCheckpointCode);
					canEdit = editCheckpoint == null || editCheckpoint.IsAllowed;
				}

				var canView = canEdit;
				var noViewMessage = String.Empty;
				if (!canEdit)
				{
					var viewCheckpoint = GetSecurityCheckpoint(requireWorkflowSecurity, moduleId, viewCheckpointCode);
					canView = viewCheckpoint == null || viewCheckpoint.IsAllowed;
					noViewMessage = (viewCheckpoint != null) ? viewCheckpoint.ErrorMessageForNotAllowed : Res.GetString("f7e57129-6e01-4cc4-b218-1cb4c9f287e6", "You do not have permission to access this feature");
				}

				if (canEdit || canView)
				{
					ZWorkflowTabPage workflowTabPage = GetTabPageContainer<ZWorkflowTabPage>(requireWorkflowSecurity);
					if (workflowTabPage != null)
					{
						workflowTabPage.ConsumeWorkflowLicence();
					}
				}

				ZTabPage tabPage = GetTabPageContainer<ZTabPage>(requireWorkflowSecurity);
				if (tabPage != null)
				{
					if (!canEdit && canView)
					{
						tabPage.SetReadOnlyIncludingChildren();
						setView();
						ZLabel readOnlyLabel = new ZLabel();
						readOnlyLabel.Dock = DockStyle.Top;
						readOnlyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
						readOnlyLabel.Text = Res.GetString("d4abdcff-923d-4568-9a40-1656c6bcaa73", "Due to your permissions, this feature is read-only.");
						tabPage.Controls.Add(readOnlyLabel);
					}
					else if (!canEdit && !canView)
					{
						requireWorkflowSecurity.Visible = false;
						ZLabel coveringLabel = new ZLabel();
						coveringLabel.Dock = DockStyle.Fill;
						coveringLabel.Name = "SecurityDeniedLabel";
						coveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
						coveringLabel.Text = noViewMessage;
						tabPage.Controls.Add(coveringLabel);
						coveringLabel.BringToFront();
					}
				}
			}
		}

		static T GetTabPageContainer<T>(ZUserControl userControl) where T : class
		{
			Control parent = userControl.Parent;
			while (parent != null && !(parent is T))
			{
				parent = parent.Parent;
			}

			return parent as T;
		}

		static bool ThisOrAnyParentNull(Control userControl)
		{
			var control = userControl;
			while (control != null && !(control is Form))
			{
				control = control.Parent;
			}
			return control == null;
		}

		public static SecurityCheckpoint GetSecurityCheckpoint<T>(T requireWorkflowSecurity, ModuleIdentifier moduleId, string checkpoint) where T : ZUserControl
		{
			SecurityCheckpoint result = null;

			try
			{
				if (moduleId == null)
				{
					ZForm form = requireWorkflowSecurity.FindForm() as ZForm;
					if (form == null)
					{
						// Sometimes one of the control parents can be null if another control on the same form disposes
						// at a certain time which makes FindForm() return null. We don't want to throw an exception in this case.
						if (!ThisOrAnyParentNull(requireWorkflowSecurity))
						{
							ErrorReporter.ReportOnce("WorkflowSecurityChecks_CannotFindForm", "Could not find the parent ZForm for this workflow control");
						}
						return result;
					}

					ControllerID controllerID = form.ControllerID;
					if (controllerID == null)
					{
						ErrorReporter.ReportOnce("WorkflowSecurityChecks_NoControllerID", "Could not find the ZForm's ControllerID for this workflow control. If this is causing test failures on your FormBasher tests, set the ControllerID manually in GetFormToBashCore().");
						return result;
					}

					ZController controller = ZControllerFactory.Create(controllerID);
					moduleId = controller.ModuleID;
					if (moduleId == null)
					{
						ErrorReporter.ReportOnce("WorkflowSecurityChecks_NoModuleID", "Could not find the ZController's ModuleID for this workflow control. ControllerID is " + controllerID.Name + ".");
						return result;
					}
				}

				using (ZModule module = ZModuleFactory.Instance.Create(moduleId))
				{
					if (module.SecurityCheckpoint != Env.Security.None)
					{
						result = Env.Security.FindOrCreateWorkflowItemCheckpoint(module.SecurityCheckpoint, checkpoint);
						if (!module.SupportsWorkflow
#if DEBUG
 && module.ID?.ID.ToString() != "Dummy"
#endif
)
						{
							ErrorReporter.ReportOnce("WorkflowSecurityChecks_NoSecurityCheckpoint",
								string.Format("Workflow security checkpoints are not included in the Security Tree. You need to override SupportsWorkflow flag on your module ({0}) to have the Workflow SecurityCheckpoints automatically added", module.GetType().FullName));
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Catch exceptions as a precaution to ensure that the system doesn't blow up if the security checkpoint is not found
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}
	}
}
