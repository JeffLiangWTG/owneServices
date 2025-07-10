using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDocTypeForm : ZForm
	{
		public RefDocTypeForm(RefDocType docType)
			: base(docType)
		{
			InitializeComponent();

			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			docType.ConfirmUpdate += new ConfirmEventHandler(OnDocType_ConfirmUpdate);
		}

		public new RefDocType BusinessEntity
		{
			get { return (RefDocType)base.BusinessEntity; }
		}

		bool isInDataBase;

		#region Implementation

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = ShowDuplicatedDocTypesDialog();
				if (result == ContinueWithSave.Yes)
				{
					isInDataBase = BusinessEntity.IsInDatabase;

					if ((!BusinessEntity.IsInDatabase && BusinessEntity.RT_ForceUserToRead) ||
							(BusinessEntity.IsInDatabase && BusinessEntity.RT_ForceUserToReadInfo.HasChanges))
					{
						Globals.Message.ShowInformation(Res.GetString("5d93a6c0-388b-47ed-863e-adf235548c3d", "Allow up to 30 minutes for the 'Force User to Read' change to take effect for other users"));
					}
				}
			}
			return result;
		}

		ContinueWithSave ShowDuplicatedDocTypesDialog()
		{
			var result = ContinueWithSave.Yes;
			if (BusinessEntity.AppliesToAllCategories)
			{
				var duplicatedDocTypes = BusinessEntity.GetDuplicatedDocTypes();
				if (duplicatedDocTypes.Length > 0)
				{
					var duplicatedDocTypeCategories = string.Join(",", duplicatedDocTypes.Select(x => x.RT_ReferenceType));
					var systemDuplicatedDocType = duplicatedDocTypes.FirstOrDefault(x => x.RT_IsSystem);
					if (systemDuplicatedDocType != null)
					{
						Globals.Message.ShowError(Res.GetString("F010250C-0090-401B-B94F-E6528BBCA59D", @"There are already document types with a code of '{0}' for the following categories: {1}.
The Doc Type {0} with Category {2} is a system defined Doc Type and cannot be deleted. Please save with a category other than ALL.",
						BusinessEntity.RT_DocType,
						duplicatedDocTypeCategories,
						systemDuplicatedDocType.RT_ReferenceType));

						result = ContinueWithSave.No;
					}
					else
					{
						var message = Res.GetString("a31787a7-dc0f-4b4f-8884-6f063c4838ae", "There are already document types with a code of '{0}' for the following categories: {1}. If you proceed, these document types will be deleted. Are you sure you want to proceed?",
								BusinessEntity.RT_DocType, duplicatedDocTypeCategories);
						if (Globals.Message.ShowConfirmation(message, Res.GetString("553ed95f-d00d-4eb0-8c0b-48079f2c42c1", "Duplicate Document Types"), Res.GetString("41d89de9-1f83-4551-9bb4-430d1a0fe98d", "yes"), MessageBoxIcon.Warning) == DialogResult.OK)
						{
							BusinessEntity.FixDuplicatedDocTypes(duplicatedDocTypes);
						}
						else
						{
							result = ContinueWithSave.No;
						}
					}
				}
			}
			return result;
		}

		bool OnDocType_ConfirmUpdate(RefDocType sender, ConfirmEventArgs e)
		{
			var warning = Res.GetString("b5f116b1-6a4e-44bc-8cf2-4fbca0e6b976", "Warning");

			using (ZFormStrategy.SuppresseNewFormInTransactionWarning())
			{
				return isInDataBase && Globals.Message.Show(e.Message, warning, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
			}
		}

		#endregion

		void logMacroFormButton_Click(object sender, EventArgs e)
		{
			var eventRef = new EventReference(BusinessEntity.RT_SE_NKDocumentReceivedEvent, BusinessEntity.RT_LogMacro);
			using (var form = new EventReferenceForm(eventRef))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form, this) == DialogResult.OK)
				{
					BusinessEntity.RT_LogMacro = eventRef.CompleteText;
				}
			}
		}
	}
}
