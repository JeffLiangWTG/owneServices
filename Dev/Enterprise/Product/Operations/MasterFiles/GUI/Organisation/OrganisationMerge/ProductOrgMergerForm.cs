using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	using Enterprise.ZArchitecture.Environment;

	public partial class ProductOrgMergerForm : ZChildForm
	{
		public ProductOrgMergerForm()
		{
		}

		public ProductOrgMergerForm(ProductOrgMerger merger)
			: base(merger)
		{
		}

		ProductOrgMerger Merger
		{
			get { return (ProductOrgMerger)BusinessEntity; }
		}

		public override string FormVerb => string.Empty;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateForm();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		void UpdateForm()
		{
			ProceedButton.Enabled = Merger.TotalDuplicateCount == 0;
			DeactivateSingleRelationButton.Enabled = Merger.SingleRelationDuplicateCount != 0;
			DeactivateMultiRelationButton.Enabled = Merger.MultiRelationDuplicateCount != 0;
			RemoveMultiRelationButton.Enabled = Merger.MultiRelationDuplicateCount != 0;

			OverallCaptionLabel.Text = Res.GetString(
				"ProductOrgMergerForm|OverallCaptionLabel",
				"Merging organization {0} into organization {1} will result in {2} duplicate product(s). Duplicate products are defined as products that have the same product code and the same owner.",
				Merger.OldOrganization?.OH_Code,
				Merger.NewOrganization?.OH_Code,
				Merger.SingleRelationDuplicateCount + Merger.MultiRelationDuplicateCount
			);

			SingleRelationLabel.Text = Res.GetString(
				"ProductOrgMergerForm|SingleRelationLabel",
				"{0} product(s) are related only to organization {1}. As this is the organization that is being merged from, these products need to be deactivated.\r\n\r\nExample: {2}",
				Merger.SingleRelationDuplicateCount,
				Merger.OldOrganization?.OH_Code,
				string.Join(", ", Merger.SingleRelationDuplicatePartNumbers)
			);

			MultiRelationLabel.Text = Res.GetString(
				"ProductOrgMergerForm|MultiRelationLabel",
				"{0} product(s) are related to organizations besides {1}. If these products are deactivated, they can no longer be used by the other organizations that they are linked to. " +
				"If relationships are removed, then only the relationship to organization {1} will be removed and all other relationships will be retained." +
				"\r\n\r\nExample: {2}",
				Merger.MultiRelationDuplicateCount,
				Merger.OldOrganization?.OH_Code,
				string.Join(", ", Merger.MultiRelationDuplicatePartNumbers)
			);
		}

		void ProceedButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelFormButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void DeactivateSingleRelationButton_Click(object sender, EventArgs e)
		{
			if (Confirm(Messages.GetDeactivateRelationConfirmation(Merger.OldOrganization?.OH_Code, Merger.SingleRelationDuplicateCount)))
			{
				Merger.DeactivateSingleRelationDuplicates();
				Merger.ReloadStats();
				UpdateForm();
			}
		}

		void DeactivateMultiRelationButton_Click(object sender, EventArgs e)
		{
			if (Confirm(Messages.GetDeactivateRelationConfirmation(Merger.OldOrganization?.OH_Code, Merger.MultiRelationDuplicateCount)))
			{
				Merger.DeactivateMultiRelationDuplicates();
				Merger.ReloadStats();
				UpdateForm();
			}
		}

		void RemoveMultiRelationButton_Click(object sender, EventArgs e)
		{
			if (Confirm(Messages.GetRemoveRelationConfirmation(Merger.OldOrganization?.OH_Code, Merger.MultiRelationDuplicateCount)))
			{
				Merger.DeleteMultiRelationDuplicateRelations();
				Merger.ReloadStats();
				UpdateForm();
			}
		}

		bool Confirm(string message)
		{
			return Globals.Message.Show(
						message,
						Messages.ConfirmationCaption,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						DialogResult.No
					) == DialogResult.Yes;
		}

		static class Messages
		{
			public static string ConfirmationCaption => Res.GetString("ProductOrgMergerForm|ConfirmationCaption", "Confirmation");

			public static string GetDeactivateRelationConfirmation(string organizationCode, int productCount)
			{
				return Res.GetString(
					"ProductOrgMergerForm|DeactivateRelationConfirmation",
					"Do you want to deactivate {1} product(s) related to organization {0}?",
					organizationCode,
					productCount
				);
			}

			public static string GetRemoveRelationConfirmation(string organizationCode, int productCount)
			{
				return Res.GetString(
					"ProductOrgMergerForm|RemoveRelationConfirmation",
					"Do you want to remove relations to organization {0} from {1} product(s)?",
					organizationCode,
					productCount
				);
			}
		}
	}
}
