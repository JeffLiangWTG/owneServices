using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class CreateBulkContainerDetentionForm : ZChildForm
	{
		public CreateBulkContainerDetentionForm(BulkDetentionHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		public void PerformSelectAllClick()
		{
			selectAllButton.PerformClick();
		}

		public void PerformUnselectAllClick()
		{
			unselectAllButton.PerformClick();
		}

		public void PerformNewClick()
		{
			createNew.PerformClick();
		}

		public void PerformCloseClick()
		{
			closeButton.PerformClick();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("e2f92c59-2d21-452d-9de6-02be6c5474f6", "filter"), Res.GetString("7ce1d87b-71c7-43a5-9a9b-ebdf8ef3c922", "find"), Res.GetString("cb120749-9c91-49fc-81d9-1a10be5f840a", "found"), includeIgnoreOption);
		}

		BulkDetentionHeader Header
		{
			get { return (BulkDetentionHeader)BusinessEntity; }
		}

		void unselectAllButton_Click(object sender, EventArgs e)
		{
			Header.SelectUnselectAll(false);
		}

		void selectAllButton_Click(object sender, EventArgs e)
		{
			Header.SelectUnselectAll(true);
		}

		void createNew_Click(object sender, EventArgs e)
		{
			Header.RunPreSaveValidation();

			if (Header.HasErrors)
			{
				ShowErrorsDialog();
			}
			else if (!Header.HasSelectedChildren())
			{
				Globals.Message.Show(Res.GetString("15464afe-77fc-4d7f-ac6b-d6b2a534ce20", "No detentions selected"));
			}
			else
			{
				BusinessObjectFactory createFactory = new BusinessObjectFactory();
				bool success = false;

				Header.CreateDetentionInvoice(createFactory);
				try
				{
					createFactory.Save();
					success = true;
				}
				catch (ZSaveException exception)
				{
					HandleSaveException(exception);
				}

				if (success)
				{
					Globals.Message.Show(
						Res.GetString("4e7d99f4-0ade-4641-9804-1d5e0f8c02a2", "{0} detention job(s) created.", Header.GeneratedJobs),
						Res.GetString("919c10cc-c687-48a1-800b-0c58ec4d94cb", "Saved."),
						MessageBoxButtons.OK,
						DialogResult.OK);
				}
			}
		}

		void createBulkDetentionInvoicesControl_FindClicked(object sender, EventArgs e)
		{
			Header.RunPreSaveValidation();
			if (Header.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Header.Find();
			}
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}


