using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	[Serializable]
	public sealed class DelayAlertDocumentDeliveryJob : AutoDocumentDeliveryJob
	{
		public DelayAlertDocumentDeliveryJob(IDocumentSupportable businessObject, bool isExportDA)
			: base(businessObject, false, GetCommandPK(businessObject, isExportDA))
		{
			if (businessObject == null)
			{
				throw new ArgumentNullException(nameof(businessObject));
			}
		}

#if NETFRAMEWORK
		DelayAlertDocumentDeliveryJob(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		static ZGuid GetCommandPK(IDocumentSupportable businessObjectToDeliver, bool isExportDA)
		{
			const string ImportDelayAlert = "Delay Alert";
			const string ExportDelayAlert = "Export Delay Alert";

			string documentName = isExportDA ? ExportDelayAlert : ImportDelayAlert;

			BusinessObject bizo = (BusinessObject)businessObjectToDeliver;
			DocumentZQuery query = new DocumentZQuery(businessObjectToDeliver.DocumentSupporter.BusinessContext, documentName);
			DocumentCommand command = bizo.Factory.LoadTop1<DocumentCommand>(query)
									  ?? throw new InvalidOperationException(string.Format("No \"{0}\" document for business context {1}", documentName, businessObjectToDeliver.DocumentSupporter.BusinessContext));

			return command.PK;
		}

		public bool HasRecipients
		{
			get
			{
				DocumentPrintSet printSet = new DocumentPrintSet(DocumentCommand, null);

				for (int i = 0; i < printSet.Count; i++)
				{
					DocumentPack docPack = printSet[i];
					DeliveryInstructions instructions = GetDeliveryInstructions(docPack);

					if (instructions.Recipients.Count > 0)
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
