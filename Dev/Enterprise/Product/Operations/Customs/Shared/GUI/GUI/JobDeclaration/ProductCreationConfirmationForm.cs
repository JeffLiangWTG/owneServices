using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ProductCreationConfirmationForm : ZChildForm
	{
		public ProductCreationConfirmationForm(BaseJobDeclaration declaration)
			: base(declaration)
		{
			DialogResult = DialogResult.None;
#if DEBUG
			TypeDescriptor.AddAttributes(BehaviourLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		BaseJobDeclaration Declaration
		{
			get { return (BaseJobDeclaration)BusinessEntity; }
		}

		DeclarationForProductCreationHelperCollection productCreationHelperCollection;

		public override string FormHeading
		{
			get { return Res.GetString("333da72a-d51a-4d59-96ad-4cf49cf1af87", "New Or Inactive Products Found"); }
		}

		public override string FormCaption
		{
			get { return Res.GetString("31b40ced-0b93-4038-85c9-67f7017e1071", "Products for which there is no match in the system have been identified on this customs declaration.\r\nIf a matching product is found that is inactive, it will be re-activated else a new product will be created.\r\nAll products will be created with the following related organizations:"); }
		}

		#region Events

		#region Accessibility Events

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.FormBorderStyle = FormBorderStyle.Sizable;
			CreateProductCreationHelperCollection();
			ConfigureButtonVisiblity();
			SelectDefaultOptionToBeChecked();
			ConfigureWarningLabelVisiblity();
			ConfigureConfirmButtonEnabled();
		}

		protected void CreateProductCreationHelperCollection()
		{
			productCreationHelperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
		}

		protected void ConfigureConfirmButtonEnabled()
		{
			ConfirmButton.Enabled = !(NoOptionsAreSelected && AtLeastOneOptionIsAvailable);
		}

		void ConfigureWarningLabelVisiblity()
		{
			WarningLabel.Visible = productCreationHelperCollection.ShouldShowWarning;
		}

		void ConfigureButtonVisiblity()
		{
			InformationLabel.Text = FormCaption;

			ProductRelationOptionForImporter.Text = productCreationHelperCollection.ProductRelationOptionForImporterText;
			ProductRelationOptionForImporter.Visible = ProductRelationOptionForImporter.Text != ZString.Empty;

			ProductRelationOptionForSupplier.Text = productCreationHelperCollection.ProductRelationOptionForSupplierText;
			ProductRelationOptionForSupplier.Visible = ProductRelationOptionForSupplier.Text != ZString.Empty;
		}

		void SelectDefaultOptionToBeChecked()
		{
			SetOptionEnable(false);
			var productCreationHelper = productCreationHelperCollection.PrimaryProductCreationHelper;
			var defaultOption = productCreationHelper?.CalculateDefaultOptionForCreateProduct() ?? ProductRelationDefaultOption.None;
			switch (defaultOption)
			{
				case ProductRelationDefaultOption.OptionForImporter:
					ProductRelationOptionForImporter.Checked = true;
					break;
				case ProductRelationDefaultOption.OptionForSupplier:
					ProductRelationOptionForSupplier.Checked = true;
					break;
			}

			if (NoOptionsAreSelected)
			{
				var impOptionVisible = ProductRelationOptionForImporter.Visible;
				var supOptionVisible = ProductRelationOptionForSupplier.Visible;
				if (impOptionVisible && !supOptionVisible)
				{
					ProductRelationOptionForImporter.Checked = true;
				}
				else if (supOptionVisible && !impOptionVisible)
				{
					ProductRelationOptionForSupplier.Checked = true;
				}
			}

			SetOptionEnable(productCreationHelper?.CanChangeTheDefaultOption ?? true);
			BehaviourLabel.Text = productCreationHelper?.BehaviorDefaultedFromText ?? ZString.Empty;
		}

		void SetOptionEnable(bool enable)
		{
			ProductRelationOptionForImporter.Enabled = enable;
			ProductRelationOptionForSupplier.Enabled = enable;
		}

		ProductRelationDefaultOption ProductRelationOptionChosen
		{
			get
			{
				return
					ProductRelationOptionForImporter.Checked
						? ProductRelationDefaultOption.OptionForImporter
						: ProductRelationOptionForSupplier.Checked
							? ProductRelationDefaultOption.OptionForSupplier
							: ProductRelationDefaultOption.None;
			}
		}

		bool NoOptionsAreSelected => !ProductRelationOptionForImporter.Checked && !ProductRelationOptionForSupplier.Checked;
		bool AtLeastOneOptionIsAvailable => ProductRelationOptionForImporter.Visible || ProductRelationOptionForSupplier.Visible;

		#endregion

		#region Click Events

		void ProductRelationOption_Click(object sender, EventArgs e)
		{
			ConfigureConfirmButtonEnabled();
		}

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			const int maxProductsToDisplay = 10;
			var duplicateProducts = productCreationHelperCollection.Create(ProductRelationOptionChosen);

			if (duplicateProducts.Any())
			{
				Globals.Message.ShowError(
					Res.GetString("{4A246B8B-DFCD-43BE-AAA8-FE61E1172E72}",
						@"The following products were marked for creation but have not been created as they would create duplicates. This could be caused by the following
1. Product already exists for this Owner but with a different Supplier. To fix this, remove all the Suppliers, or add the new Supplier to the existing product (F4, filter on Importer only and then edit the product).
2. These products could have been subsequently created by another user. To fix this, erase and re-enter the product code to synchronize the newly created product with the invoice line, or use a different product code."
					)
					+ "\r\n\r\n"
					+ string.Join("\r\n", duplicateProducts.OrderBy(p => p).Take(maxProductsToDisplay))
					+ (duplicateProducts.Count > maxProductsToDisplay ? "\r\n..." : "")
				);
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#endregion
	}
}
