using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class BulkMovementsForm : ZChildForm
	{
		public BulkMovementsForm(BulkMovementsHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		public void PerformCreateClick()
		{
			createButton.PerformClick();
		}

		public void PerformCloseClick()
		{
			closeButton.PerformClick();
		}

		public void PerformClearGeneratedClick()
		{
			clearGeneratedButton.PerformClick();
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void okButton_Click(object sender, EventArgs e)
		{
			BulkMovementsHeader header = (BulkMovementsHeader)BusinessEntity;

			if (header != null)
			{
				header.RunPreSaveValidation();

				if (header.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					bool success = false;

					BusinessObjectFactory factory = new BusinessObjectFactory();
					factory.Saved += (f, s) => { success |= s; };

					header.Generate(factory);

					try
					{
						factory.Save();
					}
					catch (ZSaveException ex)
					{
						HandleSaveException(ex);
					}

					if (success)
					{
						foreach (BulkMovementsChild child in header.Children)
						{
							child.IsGenerated = true;
						}
					}
				}
			}
		}

		void removeGeneratedButton_Click(object sender, EventArgs e)
		{
			BulkMovementsHeader header = (BulkMovementsHeader)BusinessEntity;

			if (header != null)
			{
				foreach (BulkMovementsChild child in header.Children.ToArray())
				{
					if (child.IsGenerated)
					{
						header.Children.Remove(child);
					}
				}
			}
		}
	}
}


