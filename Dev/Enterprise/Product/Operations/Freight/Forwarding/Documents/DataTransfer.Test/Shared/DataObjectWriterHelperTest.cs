using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Freight.Forwarding.Documents.DataTransfer.DataObjectWriterHelper;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	class DataObjectWriterHelperTest : TestCaseWithFactory
	{
		public void TestAppendPopulatePDFAttachedDocument()
		{
			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
			var document = new DummyDocument();
			var fileAttributes = new FileAttributes()
			{
				Name = "TEST NAME",
				Description = "TEST DESCRIPTION",
				Code = "TEST CODE",
				IsPublished = false
			};

			AssertExceptionThrown<ArgumentNullException>(() => AppendPopulatePDFAttachedDocument(null, document, fileAttributes));
			AssertExceptionThrown<ArgumentNullException>(() => AppendPopulatePDFAttachedDocument(universalShipment, document, null));

			AppendPopulatePDFAttachedDocument(universalShipment, document, fileAttributes);
			AssertEquals(1, universalShipment.AttachedDocumentCollection.Count);

			var attachedDocument = universalShipment.AttachedDocumentCollection[0];

			AssertPDFAttachedDocumentsFileAttributes(attachedDocument, fileAttributes);
			AssertNotNull("ImageData", attachedDocument.ImageData);

			AppendPopulatePDFAttachedDocument(universalShipment, document, fileAttributes);
			AssertEquals(2, universalShipment.AttachedDocumentCollection.Count);

			universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
			AppendPopulatePDFAttachedDocument(universalShipment, document, fileAttributes, () => false);
			AssertNull(universalShipment.AttachedDocumentCollection);

			universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
			AppendPopulatePDFAttachedDocument(universalShipment, null, fileAttributes);
			AssertNull(universalShipment.AttachedDocumentCollection);
		}

		public static void AssertPDFAttachedDocumentsFileAttributes(AttachedDocument attachedDocument, FileAttributes fileAttributes)
		{
			CombineAssertions(() =>
			{
				AssertNotNull("attachedDocument should not be null", attachedDocument);
				AssertEquals("FileName", $"{fileAttributes.Name}.pdf", attachedDocument.FileName);
				AssertEquals("Type.Description", fileAttributes.Description, attachedDocument.Type.Description);
				AssertEquals("Type.Code", fileAttributes.Code, attachedDocument.Type.Code);
				AssertEquals("IsPublished", fileAttributes.IsPublished, attachedDocument.IsPublished);
			});
		}

		public static void AssertPDFAttachedDocumentsContextCollection(AttachedDocument attachedDocument, List<Context> contextCollection)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Context.Count", contextCollection.Count, attachedDocument.ContextCollection.Count);

				for (var i = 0; i < contextCollection.Count; i++)
				{
					var context = contextCollection[0];
					var contextInAttachment = attachedDocument.ContextCollection.FirstOrDefault(x => x.Type == context.Type.Type);

					AssertNotNull($"Context[{i}]", contextInAttachment);
					AssertEquals($"Context[{i}].Value", context.Value, contextInAttachment.Value);
				}
			});
		}

		public void TestAttachedDocumentWatermark()
		{
			var document = new DummyDocument();
			var fileAttributes = new FileAttributes()
			{
				Name = "TEST NAME",
				Description = "TEST DESCRIPTION",
				Code = "TEST CODE",
				IsPublished = false
			};

			var universalShipmentWithWatermark = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
			AppendPopulatePDFAttachedDocument(universalShipmentWithWatermark, document, fileAttributes, applyDraftWatermark: true);
			var attachedDocumentWithWatermark = universalShipmentWithWatermark.AttachedDocumentCollection[0];

			var universalShipmentWithoutWatermark = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
			AppendPopulatePDFAttachedDocument(universalShipmentWithoutWatermark, document, fileAttributes, applyDraftWatermark: false);
			var attachedDocumentWithoutWatermark = universalShipmentWithoutWatermark.AttachedDocumentCollection[0];

			Assert(attachedDocumentWithWatermark.ImageData.Length > attachedDocumentWithoutWatermark.ImageData.Length);
		}
	}
}
