using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AttachedDocumentDataObjectWriter : IAttachedDocumentDataObjectWriter
	{
		public AttachedDocument[] GenerateAttachedDocuments(bool metaDataOnly, params IeDoc[] eDocs)
		{
			var attachedDocumentCollection = new List<AttachedDocument>();
			foreach (var eDoc in eDocs)
			{
				var attachedDocument = GenerateAttachedDocument(metaDataOnly, eDoc);

				attachedDocumentCollection.Add(attachedDocument);
			}

			return attachedDocumentCollection.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public AttachedDocument GenerateAttachedDocument(bool metaDataOnly, IeDoc eDoc, IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues = null)
		{
			var attachedDocument = new AttachedDocument();
			var type = new DocumentType();
			type.Code = eDoc.DocType;
			type.Description = eDoc.Description;
			var source = new CodeDescriptionPair();
			source.Code = eDoc.DocSource;
			source.Description = eDoc.DocSourceDescription;
			attachedDocument.Type = type;
			attachedDocument.Source = source;
			attachedDocument.FileName = eDoc.FileName;
			attachedDocument.IsPublished = eDoc.IsPublished;
			attachedDocument.DocumentID = eDoc.UniqueKey.ToString();
			attachedDocument.FileSizeInBytes = (ZInt)(eDoc.FileSizeInMB * 1024 * 1024);

			if (!metaDataOnly)
			{
				// NOTE: It would be better performance if we could redirect the ImageData stream directly here, however the current SQL stream implementation only allows
				// one stream open per connection. If this changes, then the following copy can be removed.
				attachedDocument.ImageData = eDoc.GetImageDataReader().CopyToSubStreamableStreamAndCloseStream();
			}

			attachedDocument.VisibleCompanyCode = eDoc.VisibleCompanyCode;
			attachedDocument.VisibleBranchCode = eDoc.VisibleBranchCode;
			attachedDocument.VisibleDepartmentCode = eDoc.VisibleDepartmentCode;
			attachedDocument.SaveDateUTC = eDoc.LastEdited.IsValid ? eDoc.LastEdited : eDoc.DateAdded;

			var staff = (eDoc as IFactoryProvider)?.Factory?.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, eDoc.LastEditedUser);
			if (staff != null)
			{
				attachedDocument.SavedBy = new Staff { Code = staff.GS_Code, Name = staff.GS_FullName };
			}

			if (contextValues != null && contextValues.Any())
			{
				attachedDocument.ContextCollection = contextValues.Select(o => new Context { Type = new ContextType { Type = o.Key.Type, Description = o.Key.Description }, Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(o.Value) }).ToList();
			}
			return attachedDocument;
		}

		IAttachedDocument[] IAttachedDocumentDataObjectWriter.GenerateAttachedDocuments(bool metaDataOnly, params IeDocBase[] eDocs)
		{
			return this.GenerateAttachedDocuments(metaDataOnly, eDocs.Cast<IeDoc>().ToArray()).ToArray<IAttachedDocument>();
		}

		IAttachedDocument IAttachedDocumentDataObjectWriter.GenerateAttachedDocument(bool metaDataOnly, IeDoc eDoc, IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return this.GenerateAttachedDocument(metaDataOnly, eDoc, contextValues);
		}
	}
}
