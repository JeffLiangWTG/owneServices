using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class NZCClassForm : Z2FindBoxPopupTreeViewForm
	{
		ZArchitecture.ZTextBox fullDescriptionTextBox;
		ZArchitecture.ZLabel zLabel1;
		ZArchitecture.ZLabel zLabel2;
		ZArchitecture.GUI.ZButton cancelButton;
		protected ZArchitecture.GUI.ZButton oKButton;
		internal ZArchitecture.ZTextBox statUnitTextBox;
		internal ZArchitecture.ZTextBox suppUnitTextBox;
		ZArchitecture.GUI.ZTreeView tariffTreeView;

		public NZCClassForm()
		{
		}

		public NZCClassForm(FamilyMemberCollectionForBinding collection)
			: base(collection)
		{
		}

		int fMaxTariffLength;
		public int MaxTariffLength
		{
			get { return fMaxTariffLength; }
			set { fMaxTariffLength = value; }
		}

		public NonDependentNZCClassificationSectionCollection Sections
		{
			get
			{
				if (fSections == null)
				{
					fSections = new NonDependentNZCClassificationSectionCollection(Factory);
					fSections.Load();
				}
				return fSections;
			}
		}

		#region Overrides

		protected NonDependentNZCClassificationSectionCollection fSections;

		public override string FormCaption
		{
			get { return Enterprise.Customs.NZ.GUI.Res.GetString("DB4B093C-B89B-429B-BE40-99EE6295D4D9", "Please select a tariff"); }
		}

		public override TreeView treeView
		{
			get { return tariffTreeView; }
		}

		#endregion

		#region Implementation

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		protected override void AcceptSelection()
		{
			base.AcceptSelection();

			var classification = SelectedElement as NZCClassification;
			if (classification != null)
			{
				if (MaxTariffLength > 0)
				{
					findBox.Code = classification.U0_Tariff.Left(MaxTariffLength);
				}
				else
				{
					findBox.Code = classification.U0_Tariff;
				}
			}
		}

		protected override void NavigateToCode(string code)
		{
			ITraversibleNode nearestNode = Sections.GetNearestNodeForCode(code);
			if (nearestNode != null)
			{
				CargoWise.EntityFramework.IFamilyMember[] list = nearestNode.GetHierarchy();
				LoadTopLevelCollection();
				TreeNodeCollection currentNodes = treeView.Nodes;
				if (currentNodes != null)
				{
					for (int i = list.Length - 1; i >= 0; i--)
					{
						foreach (TreeNode childNode in currentNodes)
						{
							if (childNode.Tag == list[i])
							{
								childNode.Expand();
								ShowContentsOfNode(childNode);
								treeView.SelectedNode = childNode;
								currentNodes = childNode.Nodes;
								break;
							}
						}
					}
				}
			}
		}

		protected override BusinessObjectCollection GetTopLevelCollection(BusinessObjectFactory factory)
		{
			return Sections;
		}

		protected override void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			oKButton.Enabled = !((CargoWise.EntityFramework.IFamilyMember)e.Node.Tag).HasChildren;
			base.treeView_AfterSelect(sender, e);
		}

		void oKButton_Click(object sender, EventArgs e)
		{
			AcceptSelection();
			Close();
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			CancelSelection();
			Close();
		}

		#endregion
	}
}


