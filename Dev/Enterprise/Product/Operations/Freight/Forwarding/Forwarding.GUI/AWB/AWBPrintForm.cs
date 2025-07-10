using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class AWBPrintForm : ZChildForm
	{
		public AWBPrintForm(ConsolAWBActions businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		void AWBActions_ShowMessageOnGUI(object sender, ShowMessageOnGUIEventArgs e)
		{
			Globals.Message.ShowError(e.Message, e.Title);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (AWBActions != null)
			{
				AWBActions.ShowMessageOnGUI -= AWBActions_ShowMessageOnGUI;
			}
			base.SetDataBinding(dataSource, dataMember);

			if (AWBActions != null)
			{
				AWBActions.ShowMessageOnGUI += AWBActions_ShowMessageOnGUI;
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("FC35759C-16FE-4E08-875E-986A90B9F79A", "document"), Res.GetString("C411B038-7D86-4352-AF9E-C92EC1ACBD10", "print"), Res.GetString("5628ec90-f3b8-43f3-85ea-6e2ca5527948", "printed"), includeIgnoreOption);
		}

		ConsolAWBActions AWBActions
		{
			get { return (ConsolAWBActions)BusinessEntity; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			AWBActions.RunPreSaveValidation();

			if (AWBActions.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				bool proceed = true;

				if (AWBActions.HasWarnings)
				{
					proceed = Globals.Message.Show(Res.GetString("de11ba63-eaea-4256-873b-20730d4744d8", "There are warnings.") +
						"\r\n\r\n" + AWBActions.Notifications.GetWarnings().ToUniqueMessageListString() + "\r\n\r\n" +
						Res.GetString("ac916a84-ed8b-48f0-86a2-ae15c873d3b6", "Are you sure you want to proceed?"),
						Res.GetString("ab9fa534-9556-448a-8d1f-ce6446192cc4", "Warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK &&
						IsCreditControlledDeliveryOverriden();
				}

				if (proceed && AWBActions.PerformAllActions())
				{
					DialogResult = DialogResult.OK;
				}
			}
		}

		bool IsCreditControlledDeliveryOverriden()
		{
			bool result = true;

			string creditControlledDocumentName = AWBActions.FindFirstInvalidCreditControlledDocumentName();

			if (!String.IsNullOrEmpty(creditControlledDocumentName))
			{
				DocumentCommand documentCommand = AWBActions.GetDocumentCommand(creditControlledDocumentName);

				using (var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
				{
					guiManager.Initialise(documentCommand.Parent as ICreditControlledBusinessObject);
					DocumentSupporterDataState dataState = documentCommand.Parent.DocumentSupporter.GetDataStateBeforeRun(documentCommand);
					if (dataState != null && !dataState.IsValid)
					{
						if (!String.IsNullOrEmpty(dataState.ErrorMessage))
						{
							Globals.Message.ShowError(dataState.ErrorMessage, Res.GetString("477c654c-5a57-41d5-9c4a-4b20d09208db", "Unable to process your request"));
						}

						result = false;
					}
				}
			}

			return result;
		}
	}
}
