using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ContainmentBarrierInvocationUserControl : ZUserControl
	{
		public ContainmentBarrierInvocationUserControl()
		{
			InitializeComponent();
		}

		ContainmentBarrierInvocationForm ContainmentBarrierForm
		{
			get { return containmentBarrierForm ?? (containmentBarrierForm = (ContainmentBarrierInvocationForm)FindForm()); }
		}

		ContainmentBarrierInvocationForm containmentBarrierForm;

		internal bool IterationGroupBoxVisible
		{
			get { return IterationGroupBox.Visible; }
		}

		ContainmentBarrierViewModel viewModel;

		#region ZUserControl Overrides

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			ContainmentBarrierResponses? responseToPerform = null;

			if (keyData == (Keys.Control | Keys.P))
			{
				responseToPerform = ContainmentBarrierResponses.Passed;
			}
			else if (keyData == (Keys.Control | Keys.I))
			{
				responseToPerform = ContainmentBarrierResponses.IterationRequired;
			}
			else if (keyData == (Keys.Control | Keys.D))
			{
				if (viewModel.ValidResponses.HasFlag(ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource))
				{
					responseToPerform = ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource;
				}
				else if (viewModel.ValidResponses.HasFlag(ContainmentBarrierResponses.DeferredToAnotherResource))
				{
					responseToPerform = ContainmentBarrierResponses.DeferredToAnotherResource;
				}
			}

			if (responseToPerform != null && viewModel.ValidResponses.HasFlag(responseToPerform.Value))
			{
				PerformResponse(responseToPerform.Value);
				return true;
			}
			else
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			if (viewModel != null)
			{
				ShouldCreateWorkflowForIterationCheckBox.Visible = !viewModel.ShouldCreateWorkflowForIteration_ReadOnly;
			}
		}

		#endregion

		#region Build

		internal void Build(ContainmentBarrierViewModel viewBuildModel)
		{
			viewModel = viewBuildModel;

			var responses = viewModel.ValidResponses;

			var buttonStrip = new MultiActionProvidingButtonStrip<ContainmentBarrierResponses>(GetButtonActions(responses).ToArray());
			ContainmentBarrierForm.CancelButton = (IButtonControl)buttonStrip.Controls[CancelText];

			OutcomeChoicePanel.Controls.Add(buttonStrip);

			if (string.IsNullOrEmpty(viewModel.ResourceUnderReviewNK))
			{
				ResourceUnderReviewDropEdit.Select();
			}
		}

		IEnumerable<ButtonStripAction<ContainmentBarrierResponses>> GetButtonActions(ContainmentBarrierResponses responses)
		{
			if (responses.HasFlag(ContainmentBarrierResponses.Passed))
			{
				yield return GetButton(nameof(ContainmentBarrierResponses.Passed), Res.GetString("6b6a63ec-f2e9-4823-8881-4a21d7c66525", "Passed"), ResString.GetMultilingualString("c7012ef5-8e62-4307-afcf-d9d8cc7a89ff", "Marks this Containment Barrier as Passed with no additional iterations required. (CTRL-P)"), Properties.Resources.glyphicons_152_check, ContainmentBarrierResponses.Passed);
			}
			if (responses.HasFlag(ContainmentBarrierResponses.IterationRequired))
			{
				yield return GetButton(nameof(ContainmentBarrierResponses.IterationRequired), Res.GetString("1947c272-12a0-45cb-8dfc-f9ed6df2a131", "Iteration required"), ResString.GetMultilingualString("9300edfe-3423-4d62-8fa8-2aa18d0213d6", "Marks this Containment Barrier as requiring an iteration, and creates the necessary tasks. (CTRL-I)"), Properties.Resources.glyphicons_085_repeat, ContainmentBarrierResponses.IterationRequired);
			}
			if (responses.HasFlag(ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource))
			{
				yield return GetButton(nameof(ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource), Res.GetString("ab5a4195-29d0-4ac4-865c-343e7ce3420e", "Accept iteration already created"), ResString.GetMultilingualString("d545cc05-66d9-4cf6-ae2f-66d3e793c163", "Accepts the iteration already created by another resource. (CTRL-D)"), Properties.Resources.glyphicons_043_group, ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource);
			}
			else if (responses.HasFlag(ContainmentBarrierResponses.DeferredToAnotherResource))
			{
				yield return GetButton(nameof(ContainmentBarrierResponses.DeferredToAnotherResource), Res.GetString("e4afef25-f70d-4092-9f5b-fa13fa75328a", "Defer to another resource"), ResString.GetMultilingualString("59bfd7d9-c540-4d54-a180-98ef094014dc", "Defers the checking of this Containment Barrier to another resource with an open task. (CTRL-D)"), Properties.Resources.glyphicons_043_group, ContainmentBarrierResponses.DeferredToAnotherResource);
			}

			yield return GetButton(CancelText, CancelText, ResString.GetMultilingualString("b928d82e-28a9-490d-b09b-c915ad162fb3", "Cancels checking of this Containment Barrier, and returns the task to its previous status."), Properties.Resources.glyphicons_192_circle_remove, ContainmentBarrierResponses.Canceled);
		}

		static string CancelText
		{
			get { return Res.GetString("49cb7ecb-7fdb-4c71-ab76-b9ff7c90125b", "Cancel"); }
		}

		ButtonStripAction<ContainmentBarrierResponses> GetButton(string name, string text, MultilingualString tooltip, Image image, ContainmentBarrierResponses response)
		{
			var buttonAction = new ButtonStripAction<ContainmentBarrierResponses>
			{
				Name = name,
				Text = text,
				Image = image,
				Response = response,
				ToolTip = tooltip,
				FireAction = (s, e) => PerformResponse(response),
			};

			return buttonAction;
		}

		#endregion

		#region PerformResponse

		void PerformResponse(ContainmentBarrierResponses response)
		{
			viewModel.Response = response;

			if (ContainmentBarrierForm != null)
			{
				if (response == ContainmentBarrierResponses.IterationRequired)
				{
					ContainmentBarrierForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;

					if (!IterationGroupBox.Visible)
					{
						IterationGroupBox.Visible = true;
						SetPositionAndSize();
					}

					CreateIterationButton.Focus();
				}
				else
				{
					CommitResponse();
				}
			}
		}

		void SetPositionAndSize()
		{
			if (EnterpriseFormLookStrategy.FormIsSaved(ContainmentBarrierForm))
			{
				RestoreFormSize();
			}

			ContainmentBarrierForm.MinimumSize = ControlDpiScalingHelper.NewScaledSize(625, 492);
			EnsureFormIsMinimumHeight();
		}

		void RestoreFormSize()
		{
			ContainmentBarrierForm.RememberFormPosition = false;
			EnterpriseFormLookStrategy.RestorePositionAndSize(ContainmentBarrierForm);
			ContainmentBarrierForm.RememberFormPosition = true;
		}

		void EnsureFormIsMinimumHeight()
		{
			if (ContainmentBarrierForm.Height < ContainmentBarrierForm.MinimumSize.Height)
			{
				ControlDpiScalingHelper.SetHeight(ref containmentBarrierForm, ContainmentBarrierForm.MinimumSize.Height, true);
			}
		}

		void CommitResponse()
		{
			if (ContainmentBarrierForm != null)
			{
				ContainmentBarrierForm.CommitResponse();
			}
		}

		void CreateIterationButton_Click(object sender, EventArgs e)
		{
			CommitResponse();
		}

		void IterateFromWorkflowDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			IterateFromTaskDropEdit.Focus();
		}
		#endregion
	}
}
