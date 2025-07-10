using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	static class DataObjectWriterHelper
	{
		public class FileAttributes
		{
			public ZString Name { get; set; }
			public ZString Description { get; set; }
			public ZString Code { get; set; }
			public ZBool IsPublished { get; set; }
		}

		public static void AppendPopulatePDFAttachedDocument(UniversalShipment uxmlShipment, IDocument document, FileAttributes fileAttributes, Func<bool> preConditions = null, bool applyDraftWatermark = false, IEnumerable<Context> contextCollection = null)
		{
			Argument.NotNull(uxmlShipment, "UniversalShipment");
			Argument.NotNull(fileAttributes, "FileAttributes");

			uxmlShipment.SetAttachedDocumentCollection(() =>
			{
				var attachedDocuments = uxmlShipment.AttachedDocumentCollection ?? new List<AttachedDocument>();
				if ((preConditions == null || preConditions()) && document != null)
				{
					var attachedDocument = new AttachedDocument();
					attachedDocument.IsPublished = fileAttributes.IsPublished;
					attachedDocument.FileName = $"{fileAttributes.Name}.pdf";
					attachedDocument.Type = new DocumentType
					{
						Code = fileAttributes.Code,
						Description = fileAttributes.Description
					};

					if (contextCollection != null)
					{
						attachedDocument.ContextCollection = new List<Context>(contextCollection);
					}

					var worksheet = (IWorksheet)document;
					using (var xlsStream = new MemoryStream())
					{
						var xls = worksheet.ToXlsFile();
						xls.Save(xlsStream);

						byte[] pdfStreamBytes;
						if (applyDraftWatermark)
						{
							var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
							deliveryInfo.ShowDraftWatermark = true;

							pdfStreamBytes = DocumentConverter.ConvertFromExcel(xlsStream.ToArray(), string.Empty,
								OutputFormatType.PDF, deliveryInfo.Watermark,
								ZArchitecture.Environment.ColourDepth.BlackAndWhite, false, 1m);
						}
						else
						{
							pdfStreamBytes = DocumentConverter.ConvertFromExcel(xlsStream.ToArray(), OutputFormatType.PDF,
								ZArchitecture.Environment.ColourDepth.BlackAndWhite);
						}

						var pdfStream = new MemoryStream(pdfStreamBytes);
						attachedDocument.ImageData = pdfStream.CopyToSubStreamableStreamAndCloseStream();
					}

					attachedDocuments.Add(attachedDocument);
				}

				return attachedDocuments.Any() ? attachedDocuments : null;
			});
		}
	}
}
