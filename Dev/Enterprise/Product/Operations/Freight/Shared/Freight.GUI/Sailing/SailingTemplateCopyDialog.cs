using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public partial class SailingTemplateCopyDialog : ZChildForm
	{
		public SailingTemplateCopyDialog(SailingTemplateCopyCriteria criteria)
			: base(criteria)
		{
			ReferencePortBoundTextBox.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new Binding("Caption", criteria, "ReferencePortLabel", true));
		}

		#region GetTemplateCopy

		/// <summary>
		/// returns copy of voyage or null if canceled.
		/// </summary>
		public static JobVoyage GetTemplateCopy(JobVoyage voyageToCopy)
		{
			SailingTemplateCopyCriteria criteria = new SailingTemplateCopyCriteria(voyageToCopy);
			DialogResult result;

			using (SailingTemplateCopyDialog dialog = new SailingTemplateCopyDialog(criteria))
			{
				dialog.DialogResult = DialogResult.Cancel;
				result = ZFormModaliser.ShowDialogWithoutDispose(dialog);
			}

			return (result == DialogResult.OK) ? criteria.GenerateCopy() : null;
		}

		#endregion

		#region Overrides

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("3cc61b0c-e747-45e0-bac2-09ff2bc17b93", "schedule"), Res.GetString("7205849b-e572-4076-b4a3-c0f12e62321a", "copy"), Res.GetString("9178c330-e55a-4e12-bce7-3b6b8cf613d7", "copied"), includeIgnoreOption);
		}

		#endregion

		#region Events

		void CopyZButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelZButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
