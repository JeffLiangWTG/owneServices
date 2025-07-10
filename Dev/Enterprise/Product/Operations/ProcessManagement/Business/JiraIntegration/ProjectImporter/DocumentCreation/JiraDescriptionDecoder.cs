using System.Text;
using CargoWise.Application;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business.JiraIntegration;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraDescriptionDecoder : DocumentDecoder
	{
		public JiraDescriptionDecoder(IDocumentFactory factory)
			: base(factory)
		{
		}

		public string DocumentDescription { get; private set; }

		protected override byte[] GetDocData(JiraAttachment attachment)
		{
			return Encoding.ASCII.GetBytes(attachment.DescriptionDocContent);
		}

		protected override void ProcessDocument(JiraAttachment attachment)
		{
			var fileContent = GetDocData(attachment);

			if (fileContent != null)
			{
				var doc = GetDocAddedToDocManager(fileContent, attachment);

				if (doc is StorageFile storageFile)
				{
					var hyperlinkRtf = ObjectFactory.Get<IShowEDocUrlHandler>().CreateRtf(doc, ImportedDescriptionLinkMessage);
					if (!string.IsNullOrEmpty(hyperlinkRtf))
					{
						DocumentDescription = hyperlinkRtf;
						return;
					}
				}

				DocumentDescription = FailedDescriptionLinkMessage;
			}
			else
			{
				DocumentDescription = BadDescriptionDataMessage;
			}
		}

		#region Decoding Constants

		static string ImportedDescriptionLinkMessage => Res.GetString("761A3458-942E-4F3D-A349-2BF13F838998", "Open Description In Browser");
		static string FailedDescriptionLinkMessage => Res.GetString("3E2EF733-146E-4E39-B407-1A1E99A08AF0", "Could not add description hyperlink, please see eDocs tab");
		static string BadDescriptionDataMessage => Res.GetString("7A9E969A-2F62-43A2-9894-FCF7B5907FE1", "Could not convert Jira Description to eDocs Description.");

		#endregion
	}
}
