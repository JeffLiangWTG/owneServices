using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ContainmentBarrierInvocationForm : ZChildForm, IContainmentBarrierResponseView
	{
		public ContainmentBarrierInvocationForm(ContainmentBarrierViewModel viewModel)
			: base(viewModel)
		{
			this.viewModel = viewModel;

			InitializeComponent();
			ContainmentBarrierInvocationControl.Build(viewModel);
		}

		readonly ContainmentBarrierViewModel viewModel;

		internal bool HasReceivedResponse { get; private set; }

		internal void CommitResponse()
		{
			ValidateAll(ValidationType.Full);

			if (viewModel.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				HasReceivedResponse = true;
				viewModel.CommitResponse();
				Close();
			}
		}

		#region ZForm Overrides

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (ContainmentBarrierInvocationControl.IterationGroupBoxVisible)
			{
				EnterpriseFormLookStrategy.SavePositionAndSize(this);
			}

			if (viewModel.Response == null || !HasReceivedResponse)
			{
				viewModel.Response = ContainmentBarrierResponses.Canceled;
			}

			base.OnClosing(e);
		}

		protected internal void ShowErrorsDialog()
		{
			base.ShowErrorsDialog();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				viewModel.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region IContainmentBarrierResponseView Members

		ContainmentBarrierResponses IContainmentBarrierResponseView.GetResponseFromUser()
		{
#if DEBUG
			if (showNonModally_ForTest)
			{
				Show();
			}
			else
#endif
			{
#if !WINZOR
				if (Owner == null && viewModel?.ContainmentBarrierTask != null)
				{
					foreach (var openedFrom in ZApplication.GetOpenForms())
					{
						if (openedFrom is ZForm parentForm &&
							parentForm.DataSource is IWorkflowProvider workflowProvider &&
							workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>().Any(task => task.PK == viewModel.ContainmentBarrierTask.PK))
						{
							Owner = parentForm;
						}
					}
				}
#endif
				ZFormModaliser.ShowDialogAndDispose(this);
			}

			return viewModel.Response ?? ContainmentBarrierResponses.None;
		}

#if DEBUG
		public static IDisposable ShowNonModally_ForTest()
		{
			showNonModally_ForTest = true;

			return new CargoWise.Common.DisposableAction(() => showNonModally_ForTest = false);
		}

		[ThreadStatic]
		static bool showNonModally_ForTest;
#endif

		#endregion

		#region For Test
#if DEBUG

		public ZPanel OutcomeChoicePanel_ForTest
		{
			get { return ContainmentBarrierInvocationControl.OutcomeChoicePanel; }
		}

		public ContainmentBarrierInvocationUserControl GetFormControl()
		{
			return this.ContainmentBarrierInvocationControl;
		}

#endif
		#endregion
	}
}
