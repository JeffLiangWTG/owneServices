using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class PhaseDependantsSelectionDialogTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			PhaseDependantsWrapper wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), GetOriginalSelected());
			using (PhaseDependantsSelectionDialogForTesting dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				dialog.Show();
				AssertEquals("Treeview checkboxes enabled", true, dialog.DependantsTreeView.CheckBoxes);
				AssertEquals("OK button disabled", false, dialog.OKButton.Enabled);

				TreeNodeCollection nodes = dialog.DependantsTreeView.Nodes;

				string[] expectedTopLevelNodes = new[] { "Properties", "Dependants", "Child Objects" };
				AssertContainsExactElementsInAnyOrder("Top level nodes", expectedTopLevelNodes, nodes.Cast<TreeNode>().Select(node => node.Text));

				AssertNodesBranch(FindNode(nodes, "Properties"), new[] { "Aragorn", "Gimli", "Legolas" }, new[] { "Gimli" });
				AssertNodesBranch(FindNode(nodes, "Dependants"), new[] { "Gandalf", "Boromir" }, new[] { "Gandalf" });
				AssertNodesBranch(FindNode(nodes, "Child Objects"), new[] { "Hobbits" }, Array.Empty<string>());

				TreeNode childObjectsNode = FindNode(nodes, "Child Objects");
				AssertNodesBranch(FindNode(childObjectsNode.Nodes, "Hobbits"), new[] { "Frodo", "Sam" }, new[] { "Frodo" });
			}
		}

		void AssertNodesBranch(TreeNode parentNode, string[] expectedNodes, string[] expectedCheckedNodes)
		{
			IEnumerable<TreeNode> nodes = parentNode.Nodes.Cast<TreeNode>();

			AssertContainsExactElementsInAnyOrder("Expected nodes", expectedNodes, nodes.Select(node => node.Text));
			AssertContainsExactElementsInAnyOrder("Checked nodes", expectedCheckedNodes, nodes.Where(node => node.Checked).Select(node => node.Text));

			foreach (string expectedNodeName in expectedNodes)
			{
				var node = nodes.First(x => x.Text == expectedNodeName);
				if (node.Nodes.Count == 0)
				{
					var nodeTag = node.Tag as IPhaseDependant;
					var nodeTagName = !string.IsNullOrWhiteSpace(nodeTag.Description)
						? nodeTag.Description
						: nodeTag.Name;

					AssertEquals("Node has proper IPhaseReadOnlyDependant tag", expectedNodeName, nodeTagName);
				}
			}
		}

		TreeNode FindNode(TreeNodeCollection nodes, string nodeText)
		{
			return nodes.Cast<TreeNode>().FirstOrDefault(node => node.Text == nodeText);
		}

		public void TestCancelButtonDoesNotAffectWrapper()
		{
			PhaseDependantsWrapper wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), GetOriginalSelected());
			using (PhaseDependantsSelectionDialogForTesting dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				dialog.Show();
				AssertEquals("Precondition", null, wrapper.SelectedDependants);

				dialog.CancelButton.PerformClick();
				AssertEquals("No changes", null, wrapper.SelectedDependants);
			}
		}

		public void TestOKButtonIsSettingSelectionResultsOnWrapper()
		{
			PhaseDependantsWrapper wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), GetOriginalSelected());
			using (PhaseDependantsSelectionDialogForTesting dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				dialog.Show();
				AssertEquals("Precondition", null, wrapper.SelectedDependants);

				dialog.OKButton.Enabled = true;
				dialog.OKButton.PerformClick();

				ZString[] expectedSelection = new ZString[] { "Gandalf", "Gimli", "Hobbits.Frodo" };
				AssertContainsExactElementsInAnyOrder("Selected nodes set in wrapper", expectedSelection, wrapper.SelectedDependants.Select(x => x.Name));
			}
		}

		public void TestCheckingParentNodeChangesStateOfChildNodes()
		{
			PhaseDependantsWrapper wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), GetOriginalSelected());
			using (PhaseDependantsSelectionDialogForTesting dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				dialog.Show();
				TreeNode topNode = dialog.DependantsTreeView.Nodes[0];

				topNode.Checked = true;
				AssertEquals("All children are checked", true, topNode.Nodes.Cast<TreeNode>().All(node => node.Checked));

				topNode.Checked = false;
				AssertEquals("All children are unchecked", true, topNode.Nodes.Cast<TreeNode>().All(node => !node.Checked));
			}
		}

		public void TestAddWorkflowFields()
		{
			var existingTemplatesQuery = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, DummyWorkflowDescriptor.Instance.Code);
			var existingTemplates = Factory.Load<ProcessTaskTemplate>(existingTemplatesQuery);

			foreach (var template in existingTemplates)
			{
				template.P0_IsSystem = false;
				template.Delete();
			}

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "custom field C";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			var def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "custom field A";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "custom field B";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var duplicateDef = template2.GenCustomColumnDefinitions.AddNew();
			duplicateDef.XC_Name = "custom field A";
			duplicateDef.XC_Type = AddOnColumnDataType.Codes.Datetime;

			Factory.Save();

			var wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), Enumerable.Empty<IPhaseDependant>());

			using (var dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				dialog.Show();

				var customFieldsNode = dialog.DependantsTreeView
					.Nodes.Cast<TreeNode>()
					.FirstOrDefault(node => node.Text == "Custom Fields");

				AssertNotNull("should create custom fields node", customFieldsNode);

				AssertArrayEqualsByElements("custom fields", new[]
				{
						"custom field A (DAT)",
						"custom field A (INT)",
						"custom field B (DAT)",
						"custom field C (STR)"
					},
				customFieldsNode.Nodes.Cast<TreeNode>().Select(node => node.Text).ToArray());
			}
		}

		public void TestValidateNode()
		{
			var wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), GetOriginalSelected());
			using (var dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				var topNode = dialog.DependantsTreeView.Nodes[0];
				var node = topNode.Nodes[0];
				var setting = node.Tag as IPhaseDependant;

				setting.IsReadOnly = false;
				setting.IsMandatory = false;

				dialog.Show();

				dialog.DependantsTreeView.SelectedNode = node;
				node.Checked = true;
				dialog.MandatoryCheckBox.Checked = true;
				dialog.DependantsTreeView.SelectedNode = topNode;

				AssertEquals("MessageBox", "Validation Error", ZFormModaliser.LastFormShownDialogForTest.Text);
				Assert("The problem node is still selected", node.IsSelected);

				ZFormModaliser.LastFormShownDialogForTest = null;

				node.Checked = false;
				dialog.DependantsTreeView.SelectedNode = topNode;

				AssertNull("No MessageBox", ZFormModaliser.LastFormShownDialogForTest);
				Assert("TopNode is selected", topNode.IsSelected);
			}
		}

		public void TestNodeColour()
		{
			var wrapper = new PhaseDependantsWrapper(new PhaseDependantsProviderForTesting(), GetOriginalSelected());
			using (var dialog = new PhaseDependantsSelectionDialogForTesting(wrapper))
			{
				dialog.Show();

				var topNode = dialog.DependantsTreeView.Nodes[0];
				topNode.Expand();

				var node = topNode.Nodes[0];
				var setting = node.Tag as IPhaseDependant;

				dialog.DependantsTreeView.SelectedNode = node;
				node.Checked = false;

				AssertEquals(SystemColors.ControlText, node.ForeColor);

				dialog.MandatoryCheckBox.Checked = true;
				AssertEquals(Color.Blue, node.ForeColor);

				dialog.MandatoryCheckBox.Checked = false;
				AssertEquals(SystemColors.ControlText, node.ForeColor);

				node.Checked = true;
				AssertEquals(Color.Red, node.ForeColor);
			}
		}

		#region Implementation

		public class PhaseDependantsSelectionDialogForTesting : PhaseDependantsSelectionDialog
		{
			public PhaseDependantsSelectionDialogForTesting(PhaseDependantsWrapper phaseDependantsWrapper)
				: base(phaseDependantsWrapper)
			{
			}

			public new TreeView DependantsTreeView
			{
				get { return base.DependantsTreeView; }
			}

			public new ZButton OKButton
			{
				get { return base.OKButton; }
			}

			public new ZCheckBox MandatoryCheckBox
			{
				get { return base.MandatoryCheckBox; }
			}
		}

		public class PhaseDependantsProviderForTesting : PhaseDependantsProvider
		{
			protected override Type ParentType
			{
				get { return typeof(FellowshipOfTheRing); }
			}

			protected override string ParentWorkflowType
			{
				get { return "DUM"; }
			}

			public override IEnumerable<IPhaseDependant> GetChildDependants()
			{
				yield return new PropertyDependant("Gandalf", "");
				yield return new PropertyDependant("Boromir", "Boromir");
			}

			public override Dictionary<IPhaseDependant, IEnumerable<IPhaseDependant>> GetChildExpandableDependants()
			{
				PropertyDependant hobbits = new PropertyDependant("Hobbits", "");
				var hobbitsProperties = GetIZTypePropertiesPrefixedWithName("Hobbits", typeof(Hobbits));

				var result = new Dictionary<IPhaseDependant, IEnumerable<IPhaseDependant>>();
				result.Add(hobbits, hobbitsProperties);

				return result;
			}
		}

		IEnumerable<IPhaseDependant> GetOriginalSelected()
		{
			var propertyDependants = new[]
			{
				new PhaseDependantsProvider.PropertyDependant("Gandalf", "Gandalf"),
				new PhaseDependantsProvider.PropertyDependant("Gimli", "Gimli"),
				new PhaseDependantsProvider.PropertyDependant("Hobbits.Frodo", "Frodo"),
				new PhaseDependantsProvider.PropertyDependant("Hobbits.Merry", "Merry")
			};

			foreach (var propertyDependant in propertyDependants)
			{
				propertyDependant.IsReadOnly = true;
				yield return propertyDependant;
			}
		}

		class FellowshipOfTheRing
		{
			public ZString Aragorn { get; set; }
			public ZString Gimli { get; set; }
			public ZString Legolas { get; set; }
		}

		class Hobbits
		{
			public ZString Frodo { get; set; }
			public ZString Sam { get; set; }
		}

		#endregion
	}
}
