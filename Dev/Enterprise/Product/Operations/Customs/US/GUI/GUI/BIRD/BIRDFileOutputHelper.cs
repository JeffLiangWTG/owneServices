using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	class BIRDFileOutputHelper
	{
		public void BuildAMessage(JobDeclaration declaration, BuildMessage buildMessage, string messageTypeForFileName)
		{
			try
			{
				bool generated = false;

				if (!declaration.HasBIRDCommunicationMode())
				{
					if (ExportToFile(declaration, buildMessage, messageTypeForFileName))
					{
						generated = true;
					}
				}
				else
				{
					MQEDIMessage message = buildMessage();
					declaration.Factory.Save(); // to save a message
					generated = true;
				}

				if (generated)
				{
					Globals.Message.ShowInformation("BIRD file Exported successfully.");
				}
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		bool ExportToFile(JobDeclaration declaration, BuildMessage buildMessage, string messageTypeForFileName)
		{
			bool exported = false;

			using (var fileDialog = new ZSaveFileDialog())
			{
				fileDialog.CheckPathExists = true;
				fileDialog.Filter = "Txt Files *.txt|*.txt"; // File Extension Filter
				fileDialog.DefaultExt = "txt"; // File Extension
				fileDialog.AddExtension = true;
				fileDialog.FileName = declaration.JE_DeclarationReference + "_" + messageTypeForFileName;

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(fileDialog) == DialogResult.OK)
				{
					using (Stream toFile = fileDialog.OpenFile())
					{
						StreamWriter writer = new StreamWriter(toFile, System.Text.Encoding.UTF8);

						MQEDIMessage message = buildMessage();
						declaration.Factory.Save(); // to save a message

						writer.Write(message.EM_FormattedMessageText);
						writer.Flush();

						exported = true;
					}
				}
			}

			return exported;
		}
	}

	public delegate MQEDIMessage BuildMessage();
}
