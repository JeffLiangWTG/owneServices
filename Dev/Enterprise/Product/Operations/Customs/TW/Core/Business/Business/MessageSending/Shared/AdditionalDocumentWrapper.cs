using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class AdditionalDocumentWrapper : IAdditionalDocument
	{
		public AdditionalDocumentWrapper() { }

		public AdditionalDocumentWrapper(ZString id)
		 : this(id, 0)
		{
		}

		public AdditionalDocumentWrapper(ZString id, ZInt sequenceNumeric)
		{
			this.id = id;
			this.sequenceNumeric = sequenceNumeric;
		}

		public AdditionalDocumentWrapper(ZDateTime slaughterDateTime)
		{
			this.slaughterDateTime = slaughterDateTime;
		}

		public AdditionalDocumentWrapper(SupportingDocument supportingDocument, IeDoc ieDoc)
		{
			SupportingDocument = supportingDocument;
			IeDoc = ieDoc;
		}

		protected SupportingDocument SupportingDocument { get; }
		protected IeDoc IeDoc { get; }

		readonly ZString? id;
		readonly ZInt? sequenceNumeric;
		readonly ZDateTime slaughterDateTime;

		ZString IAdditionalDocument.ID => id ?? SupportingDocument?.DocumentNo ?? ZString.Empty;

		ZString IAdditionalDocument.Content => SupportingDocument?.Remarks ?? ZString.Empty;

		ZString IAdditionalDocument.ImageFileFormat => IeDoc != null ? Path.GetExtension(IeDoc.FileName).Substring(1).ToUpper(CultureInfo.CurrentCulture) : ZString.Empty;

		ZString IAdditionalDocument.ImageFileName => IeDoc != null ? Path.GetFileName(IeDoc.FileName) : ZString.Empty;

		ZInt IAdditionalDocument.SequenceNumeric => sequenceNumeric ?? SupportingDocument?.LineNumber ?? ZInt.Zero;

		ZLong IAdditionalDocument.SizeMeasure => IeDoc?.ImageData.Length ?? ZLong.Zero;

		ZString IAdditionalDocument.TypeCode => GetTypeCodeCore();

		protected virtual ZString GetTypeCodeCore()
		{
			return SupportingDocument?.Type ?? ZString.Empty;
		}

		ZString IAdditionalDocument.ResponsibleGovernmentAgency => SupportingDocument?.ControllingAgency ?? ZString.Empty;

		ZDate IAdditionalDocument.SlaughterDateTime => slaughterDateTime.IsValid ? slaughterDateTime.Date : ZDate.Empty;

		public IEnumerable<IAdditionalDocument> GetDocuments(IAdditionalSupportingDocument addtionalDoc)
		{
			return GetDocuments(addtionalDoc.GetSupportingDocuments().Cast<SupportingDocument>(), addtionalDoc.GetAllEDocs());
		}

		public IEnumerable<IAdditionalDocument> GetDocuments(IEnumerable<SupportingDocument> addtionalDoc, IStorageDocsBaseCollection[] allEDocs)
		{
			foreach (var supportingDocument in addtionalDoc)
			{
				var eDocGuid = supportingDocument.EDoc;
				var edoc = eDocGuid.IsValid ? allEDocs.GetFromUniqueKey(eDocGuid.ToGuid()) : null;
				yield return GenerateAdditionalDocument(supportingDocument, edoc);
			}
		}

		protected virtual IAdditionalDocument GenerateAdditionalDocument(SupportingDocument supDoc, IeDoc edoc)
		{
			return new AdditionalDocumentWrapper(supDoc, edoc);
		}
	}
}
