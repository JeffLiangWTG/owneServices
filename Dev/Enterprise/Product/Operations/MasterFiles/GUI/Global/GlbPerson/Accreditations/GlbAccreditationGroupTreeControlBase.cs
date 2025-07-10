using System.Drawing;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbAccreditationGroupTreeControlBase : ZTreeViewControl
	{
		public GlbAccreditationGroupTreeControlBase()
		{
			InitializeComponent();

			descriptionColumn.Header = Res.GetString("D4224DD7-0664-470E-9B97-1B1288EE3755", "Description");

			commenceDateColumn.Header = Res.GetString("15CB32B3-1ACF-4D69-8A84-7426ECACC40A", "Commence Date");
			completionDateColumn.Header = Res.GetString("2BB528D3-DE65-42D0-8572-3196091EF0BB", "Completion Date");
			scoreColumn.Header = Res.GetString("942ED5F4-67F4-4CFC-8F3C-F72A6AB278D1", "Score");
			progressColumn.Header = Res.GetString("0F5BA85B-51BC-4D6D-BC41-2CFBF5CA073B", "Progress");
			competencyRulesColumn.Header = Res.GetString("F471F984-0EDE-40F0-9D92-B4FE1997C4DF", "Completion Minimum");
			applicantEmailColumn.Header = Res.GetString("4DF012BD-BE54-4D16-8252-8E3F7755B7E0", "Applicant Email");
			commentColumn.Header = Res.GetString("4DBCF2F7-068E-4671-BA11-7B07E6587C66", "Comment");

			commenceDateColumn.IsVisible = ShowColumns;
			completionDateColumn.IsVisible = ShowColumns;
			scoreColumn.IsVisible = ShowColumns;
			progressColumn.IsVisible = ShowColumns;
			progressColumn.IsVisible = ShowColumns;
			applicantEmailColumn.IsVisible = ShowColumns;
			commentColumn.IsVisible = ShowColumns;

			ShowEditButton = ShouldShowEditButton;
			ShowNewButton = ShouldShowNewButton;
		}

		protected virtual bool ShouldShowEditButton => false;
		protected virtual bool ShouldShowNewButton => false;
		protected override ResourceStringData DefaultNameOfATreeElement => Res.GetData("F2EDE910-9F8D-4F72-B95E-8C6248BD946A", "item");
		protected override ResourceStringData DefaultNameOfTreeElementsPlural => Res.GetData("2DB954A8-92B1-49BB-A083-C2C46790368A", "items");

		protected override void ExpandRequired()
		{
		}

		protected override void ShowEditForm(IBusiness bizObjToEdit)
		{
		}

		public virtual bool ShowColumns
		{
			get { return true; }
		}

		protected override void SetupTree()
		{
			base.SetupTree();

			descriptionTextBox.DrawText += Control_DrawText;
		}

		static void Control_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			var node = (GlbAccreditationTreeNode)e.Node.Tag;
			if (node != null && node.ParentNode == null)
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
			}
		}

		public new GlbAccreditationTreeModelView ModelView
		{
			get { return (GlbAccreditationTreeModelView)base.ModelView; }
		}

		protected override IZTreeModelView GetNewTreeModelView()
		{
			return new GlbAccreditationTreeModelView((ZTreeModel<GlbAccreditationTreeBizObjWrapperBase>)CurrentDataItem);
		}

		protected IGlbAccreditationTreeModel Model
		{
			get { return CurrentDataItem as IGlbAccreditationTreeModel; }
		}

		protected override ZTreeViewAdv CreateNewTreeViewAdv()
		{
			return new AccreditationTree();
		}
	}
}
