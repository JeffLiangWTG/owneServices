using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataTransfer
{
	public class DataTransferImpl : IDataTransfer
	{
		public DataTransferImpl()
		{
		}

		#region Import

		public bool ImportInvoices(BaseJobDeclaration toJobDec, string fileName)
		{
			bool result = false;

			if (File.Exists(fileName))
			{
				try
				{
					switch (Path.GetExtension(fileName).ToUpper())
					{
						case ".XML":
							result = DoXmlImport(toJobDec, fileName);
							break;

						default:
							result = DoFlatFileImport(toJobDec, fileName);
							break;
					}
				}
				catch (IOException)
				{
					fErrorMessage = Res.GetString("d0be2319-17e5-4f4f-96dc-ff7604820dc4", "File cannot be accessed. It might be in use by another program");
				}
				catch (ArgumentException ex)
				{
					fErrorMessage = Res.GetString("7824A420-7C10-4015-93AA-99562A29C1E4", "Error processing file ({0}).\r\nException message:{1}", fileName, ex.Message);
				}
				catch (DeveloperNotificationException exception)
				{
					throw new DeveloperNotificationException(string.Format(
@"Error occured while trying to import invoices.
{0}
File Name: {1}
File Contents: {2}", exception.Message, fileName, File.ReadAllText(fileName)), exception);
				}
			}
			else
			{
				fErrorMessage = Res.GetString("185a9361-7f98-447b-a68c-8dfcfbf45192", "The File '{0}' does not exist", fileName);
			}

			return result;
		}

		protected void DoImport(DeclarationDataImporter importer, BaseJobDeclaration toJobDec)
		{
			try
			{
				((IBusinessObjectCollection)toJobDec.Invoices).SuspendValidation();
				toJobDec.InvoiceLines.SuspendValidation();

				using (toJobDec.InvoiceLines.SuspendInvoiceLineListChanged())
				{
					importer.LogEvent += new EventHandler(Importer_LogEvent);
					importer.UnknownOrganisationCodeFound += new UnknownOrganisationCodeEventHandler(Importer_UnknownOrganisationCodeFound);

					importer.Import();
					if (ProcessCompleted != null)
					{
						ProcessCompleted(this, new EventArgs());
					}
				}
			}
			finally
			{
				((IBusinessObjectCollection)toJobDec.Invoices).ResumeValidation();
				toJobDec.InvoiceLines.ResumeValidation();
			}
		}

		protected bool DoXmlImport(BaseJobDeclaration toJobDec, string fileName)
		{
			bool result = false;

			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				fs.Position = 0;
				if (fs.Length > 0)
				{
					XmlValidator validator = new XmlValidator(CustomsXmlSchemaDefinitions.Instance.InvoicesSchema);
					NotificationBuffer notify = new NotificationBuffer();
					validator.Validate(fs, notify);

					if (!notify.HasErrors)
					{
						XmlDocument xmlDoc = GetXmlDoc(fs);
						importer = GetXmlImporter(xmlDoc, toJobDec);
						DoImport(importer, toJobDec);
						result = true;
					}
					else
					{
						fErrorMessage = Res.GetString("66cf95e1-42af-48e3-a942-cc696c2bc8f4", "XML Document is invalid.\r\n\r\n{0}", notify.AsString);
					}
				}
				else
				{
					fErrorMessage = Res.GetString("7fde88c2-85ee-432e-a0ff-a35634df5d23", "The XML file that you are importing is empty");
				}
			}
			return result;
		}

		protected bool DoFlatFileImport(BaseJobDeclaration toJobDec, string fileName)
		{
			importer = GetFlatFileImporter(fileName, toJobDec);
			DoImport(importer, toJobDec);
			return true;
		}

		#endregion

		#region Overrideable Methods

		protected virtual XmlInvoiceDataImporter GetXmlImporter(XmlDocument xmlDoc, BaseJobDeclaration toJobDec)
		{
			return new XmlInvoiceDataImporter(xmlDoc, toJobDec);
		}

		protected virtual XmlDocument GetXmlDoc(Stream xmlStream)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlStream.Position = 0;
			xmlDoc.Load(xmlStream);
			return xmlDoc;
		}

		protected virtual FlatFileInvoiceDataImporter GetFlatFileImporter(string fileName, BaseJobDeclaration toJobDec)
		{
			return new FlatFileInvoiceDataImporter(fileName, toJobDec);
		}

		#endregion

		#region Events

		public event UnknownOrganisationCodeEventHandler UnknownOrganisationCodeFound;
		public event ProcessedEventHandler Processed;
		public event EventHandler ProcessCompleted;

		void Importer_LogEvent(object sender, EventArgs e)
		{
			if (importer != null)
			{
				FireProcessedEvent(importer.PercentageComplete, importer.ProcessedRecordCount, importer.FailedRecordCount, importer.LogEntry);
			}
		}

		protected void FireProcessedEvent(int percentageComplete, int processedRecordCount, int failedRecordCount, string logEntry)
		{
			if (Processed != null)
			{
				Processed(this, new ProcessedEventArgs(percentageComplete, processedRecordCount, failedRecordCount, logEntry));
			}
		}

		void Importer_UnknownOrganisationCodeFound(object sender, UnknownOrganisationCodeEventArgs e)
		{
			if (UnknownOrganisationCodeFound != null)
			{
				UnknownOrganisationCodeFound(sender, e);
			}
		}

		#endregion

		#region Implementation

		public void CancelImport()
		{
			if (importer != null)
			{
				importer.CancelImport = true;
			}
		}

		public string ErrorMessage
		{
			get { return fErrorMessage ?? ""; }
		}
		protected string fErrorMessage;

		protected DeclarationDataImporter importer;

		#endregion
	}
}
