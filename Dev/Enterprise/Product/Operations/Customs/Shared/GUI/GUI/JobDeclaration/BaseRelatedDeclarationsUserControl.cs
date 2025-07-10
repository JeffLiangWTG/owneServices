using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseRelatedDeclarationsUserControl : BaseCustomsEntryUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public BaseRelatedDeclarationsUserControl()
		{
			InitializeComponent();
			helper = new RelatedDeclarationHelper(new RelatedDeclarationPresenter());
		}

		readonly RelatedDeclarationHelper helper;

		void ButtonNew_Click(object sender, EventArgs e)
		{
			helper.CreateNewRelated(JobDeclaration);
		}

		void ButtonEdit_Click(object sender, EventArgs e)
		{
			DoEdit();
		}

		void DoEdit()
		{
			var selectedElements = GetSelectedElements();
			if (selectedElements.Length == 1)
			{
				ViewOrEditDeclaration(selectedElements[0]);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("11090d55-d83b-4010-8f8f-7698f1004501", "Please Select One Declaration to Edit"));
			}
		}

		void DoLoadParent()
		{
			ViewOrEditDeclaration(JobDeclaration.ParentRelatedDeclaration);
		}

		void RelatedDeclarationsGrid_DoubleClick(object sender, EventArgs e)
		{
			DoEdit();
		}

		protected virtual BusinessObject[] GetSelectedElements()
		{
			return RelatedDeclarationsGrid.SelectedElements;
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == ButtonNew && previousControl == ButtonEdit;
		}

		void BaseRelatedDeclarationsUserControl_Load(object sender, EventArgs e)
		{
			if (JobDeclaration != null && JobDeclaration.ParentRelatedDeclaration != null)
			{
				ParentButton.Visible = true;
			}
		}

		void ParentButton_Click(object sender, EventArgs e)
		{
			DoLoadParent();
		}

		void ViewOrEditDeclaration(BusinessObject jobDeclaration)
		{
			if (FindForm() is ZForm form && (form.DisplayMode == ODisplayMode.ReadOnly || form.DisplayMode == ODisplayMode.Delete))
			{
				helper.ViewExisting(jobDeclaration);
			}
			else
			{
				helper.EditExisting(jobDeclaration);
			}
		}
	}
}
