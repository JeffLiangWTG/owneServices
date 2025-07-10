using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PhaseDependantsSelectionDialog : ZChildForm
	{
		public PhaseDependantsSelectionDialog(PhaseDependantsWrapper phaseDependantsWrapper)
			: base(phaseDependantsWrapper)
		{
			InitializeComponent();

			DependantsTreeView.CheckBoxes = true;

			DependantsTreeView.AfterCheck += AfterCheckEventHandler;
			DependantsTreeView.BeforeSelect += BeforeSelectEventHandler;
			DependantsTreeView.AfterSelect += AfterSelectEventHandler;

			MandatoryCheckBox.GotFocus += MandatoryGotFocusEventHandler;
			MandatoryCheckBox.CheckedChanged += MandatoryCheckedChangedEventHandler;

			SetColors();
			BuildTree();

			OKButton.Enabled = false;
		}

		PhaseDependantsWrapper DependantsWrapper
		{
			get { return (PhaseDependantsWrapper)BusinessEntity; }
		}

		#region Build Tree

		Color ReadOnlyColor;
		Color MandatoryColor;
		Color UncheckedColor;

		void SetColors()
		{
			ReadOnlyColor = Color.Red;
			MandatoryColor = Color.Blue;
			UncheckedColor = SystemColors.ControlText;
		}

		void BuildTree()
		{
			PhaseDependantsProvider provider = DependantsWrapper.DependantsProvider;

			IEnumerable<IPhaseDependant> properties = provider.GetIZTypeProperties().OrderBy(GetPhaseDependantDisplayText);
			if (properties.Any())
			{
				TreeNode propertiesNode = DependantsTreeView.Nodes.Add(Res.GetString("de0d50ca-6ac2-4d2c-b47e-709f3711f6a3", "Properties"));
				propertiesNode.Tag = true;
				AddNodes(propertiesNode, properties);
			}

			IEnumerable<IPhaseDependant> childDependants = provider.GetChildDependants().OrderBy(GetPhaseDependantDisplayText);
			if (childDependants.Any())
			{
				TreeNode childDependantsNode = DependantsTreeView.Nodes.Add(Res.GetString("1322e0c1-2c52-4965-a96a-ceabc75a79d5", "Dependants"));
				childDependantsNode.Tag = false;
				AddNodes(childDependantsNode, childDependants);
			}

			var expandableDependants = provider.GetChildExpandableDependants();
			if (expandableDependants.Any())
			{
				TreeNode expandableMainNode = DependantsTreeView.Nodes.Add(Res.GetString("18a75f6c-9ef6-4753-9029-491cce5124e3", "Child Objects"));
				expandableMainNode.Tag = false;

				foreach (IPhaseDependant dependantObject in expandableDependants.Keys.OrderBy(GetPhaseDependantDisplayText))
				{
					TreeNode dependantObjectNode = AddNodeFromDependant(expandableMainNode, dependantObject);
					dependantObjectNode.Tag = true;

					IEnumerable<IPhaseDependant> objectProperties = expandableDependants[dependantObject].OrderBy(GetPhaseDependantDisplayText);
					AddNodes(dependantObjectNode, objectProperties);
				}
			}

			IEnumerable<IPhaseDependant> pluginDependants = provider.GetPlugInDependants().OrderBy(GetPhaseDependantDisplayText);
			if (pluginDependants.Any())
			{
				TreeNode pluginDependantsNode = DependantsTreeView.Nodes.Add(Res.GetString("49837d7a-e341-4cd2-afb0-8ecefba92222", "Plug-In Objects"));
				pluginDependantsNode.Tag = false;
				AddNodes(pluginDependantsNode, pluginDependants);
			}

			IEnumerable<IPhaseDependant> customWorkflowFields = provider.GetWorkflowFields().OrderBy(GetPhaseDependantDisplayText);
			if (customWorkflowFields.Any())
			{
				TreeNode workflowNode = DependantsTreeView.Nodes.Add(Res.GetString("06ef67dc-10ed-4349-b541-93118c80d7fe", "Custom Fields"));
				workflowNode.Tag = true;
				AddNodes(workflowNode, customWorkflowFields);
			}
		}

		void AddNodes(TreeNode parentNode, IEnumerable<IPhaseDependant> dependants)
		{
			foreach (IPhaseDependant dependant in dependants)
			{
				AddNodeFromDependant(parentNode, dependant);
			}
		}

		TreeNode AddNodeFromDependant(TreeNode parentNode, IPhaseDependant dependant)
		{
			TreeNode node = parentNode.Nodes.Add(GetPhaseDependantDisplayText(dependant));
			node.Tag = dependant;

			var originalDependant = DependantsWrapper.OriginalDependants.FirstOrDefault(x => x.Name == dependant.Name);
			if (originalDependant != null)
			{
				dependant.IsMandatory = originalDependant.IsMandatory;
				dependant.IsReadOnly = originalDependant.IsReadOnly;
				node.Checked = dependant.IsReadOnly;
			}

			return node;
		}

		ZString GetPhaseDependantDisplayText(IPhaseDependant dependant)
		{
			return !string.IsNullOrWhiteSpace(dependant.Description)
				? dependant.Description
				: dependant.Name;
		}

		#endregion

		#region Set Selection Results

		void SetSelectionResults()
		{
			var selectedDependants = new List<PhaseDependantsProvider.PhaseReadOnlyDependantImpl>();
			foreach (TreeNode node in DependantsTreeView.Nodes)
			{
				AddNodeIfCustomised(node, selectedDependants);
			}

			DependantsWrapper.SelectedDependants = selectedDependants.Cast<IPhaseDependant>();
		}

		void AddNodeIfCustomised(TreeNode node, List<PhaseDependantsProvider.PhaseReadOnlyDependantImpl> selectedDependants)
		{
			var dependant = node.Tag as IPhaseDependant;

			if (dependant != null && (dependant.IsReadOnly || dependant.IsMandatory))
			{
				selectedDependants.Add(new PhaseDependantsProvider.PhaseReadOnlyDependantImpl(dependant));
			}

			foreach (TreeNode childNode in node.Nodes)
			{
				AddNodeIfCustomised(childNode, selectedDependants);
			}
		}

		#endregion

		#region Mandatory

		void SetMandatoryVisiblility(bool visible)
		{
			MandatoryPanel.Visible = visible;
		}

		#endregion

		#region Events

		void AfterCheckEventHandler(object sender, TreeViewEventArgs e)
		{
			OKButton.Enabled = true;

			TreeNode node = e.Node;

			if (node.Tag is IPhaseDependant)
			{
				((IPhaseDependant)node.Tag).IsReadOnly = node.Checked;
			}

			if (node.Nodes.Count > 0)
			{
				ChangeCheckedStateOfAllChildNodes(node);
			}
			else
			{
				SetNodeColour(node);
			}
		}

		void ChangeCheckedStateOfAllChildNodes(TreeNode parentNode)
		{
			foreach (TreeNode childNode in parentNode.Nodes)
			{
				childNode.Checked = parentNode.Checked;
				ChangeCheckedStateOfAllChildNodes(childNode);
			}
		}

		void BeforeSelectEventHandler(object sender, TreeViewCancelEventArgs e)
		{
			if (!ValidateNode(DependantsTreeView.SelectedNode))
			{
				e.Cancel = true;
			}
		}

		void AfterSelectEventHandler(object sender, TreeViewEventArgs e)
		{
			var node = e.Node;
			var dependant = node.Tag as IPhaseDependant;

			if (MainStatusBar.Panels.Count > 0)
			{
				MainStatusBar.Panels[0].Text = dependant != null ? dependant.Description.ToString() : node.Text;
			}

			if (dependant == null)
			{
				SetMandatoryVisiblility(false);
			}
			else
			{
				SetMandatoryVisiblility(((bool?)node.Parent.Tag).Value);
				MandatoryCheckBox.Checked = dependant.IsMandatory;
			}
		}

		void MandatoryCheckedChangedEventHandler(object sender, EventArgs e)
		{
			var node = DependantsTreeView.SelectedNode;
			var dependent = node.Tag as IPhaseDependant;

			dependent.IsMandatory = MandatoryCheckBox.Checked;
			SetNodeColour(node);

			OKButton.Enabled = true;
		}

		void MandatoryGotFocusEventHandler(object sender, EventArgs e)
		{
			if (MainStatusBar.Panels.Count > 0 && DependantsTreeView.SelectedNode != null)
			{
				var dependant = DependantsTreeView.SelectedNode.Tag as IPhaseDependant;
				if (dependant != null)
				{
					MainStatusBar.Panels[0].Text = Res.GetString("eea4a2e0-1373-4bce-ad58-ecf2e6d3912b", "{0} Mandatory", dependant.Description);
				}
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (!ValidateNode(DependantsTreeView.SelectedNode))
			{
				return;
			}

			SetSelectionResults();
			Close();
		}

		void CancelZButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region SetNodeColour

		void SetNodeColour(TreeNode node)
		{
			var dependant = node.Tag as IPhaseDependant;
			if (node.Checked || (dependant != null && dependant.IsMandatory))
			{
				node.ForeColor = node.Checked ? ReadOnlyColor : MandatoryColor;

				var parentNode = node.Parent;
				while (parentNode != null)
				{
					parentNode.ForeColor = ReadOnlyColor;
					parentNode = parentNode.Parent;
				}
			}
			else
			{
				node.ForeColor = UncheckedColor;

				var parentNode = node.Parent;
				while (parentNode != null && !parentNode.Nodes.Cast<TreeNode>().Any(x => x.ForeColor == ReadOnlyColor || x.ForeColor == MandatoryColor))
				{
					parentNode.ForeColor = UncheckedColor;
					parentNode = parentNode.Parent;
				}
			}
		}

		#endregion

		#region Validation

		bool ValidateNode(TreeNode node)
		{
			var dependant = node?.Tag as IPhaseDependant;
			if (dependant != null && dependant.IsMandatory && dependant.IsReadOnly)
			{
				using (var notification = new ZMessageBox(
					Res.GetString("6e854efd-1e3a-4341-bbdd-a3a2a246e93b", "Read Only and Mandatory cannot both be set on the same item."),
					Res.GetString("cac8b91f-e1eb-4405-98c7-94d91936d6dd", "Validation Error"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error))
				{
					ZFormModaliser.ShowDialogAndDispose(notification);
				}

				return false;
			}

			return true;
		}

		#endregion

		#region Implementation

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion
	}
}
