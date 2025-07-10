using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SupportingDocumentCollection : NonPersistentBusinessObjectCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			MaxCountValidationEnable(10);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupportingDocument(Factory);
		}

		#region StorageDocs

		public CodeDescriptionPairList StorageDocs
		{
			get { return storageDocs ?? (storageDocs = new CodeDescriptionPairList()); }
		}
		CodeDescriptionPairList storageDocs;

		public void SetStorageDocs(IStorageDocsBaseCollection storageDocsCollection)
		{
			int counter = 0;
			foreach (IeDoc ieDoc in storageDocsCollection)
			{
				if (!ieDoc.IsDeleted)
				{
					var correctedFileName = KeepOnlyLegalCharactersForSGCustomsInFileName(ieDoc.FileName);
					ZString fileNameWithoutExtension = Path.GetFileNameWithoutExtension(correctedFileName);
					string fileExtension = Path.GetExtension(correctedFileName);
					if (fileNameWithoutExtension.IsEmpty)
					{
						counter++;
						var generatedDocumentNames = counter > 1 ? counter.ToString() : "";
						correctedFileName = "AttachedDocument" + generatedDocumentNames + fileExtension;
					}

					if (fileExtension.Equals(".PDF", StringComparison.OrdinalIgnoreCase)
						|| fileExtension.Equals(".TIF", StringComparison.OrdinalIgnoreCase)
						|| fileExtension.Equals(".DOC", StringComparison.OrdinalIgnoreCase)
						|| fileExtension.Equals(".DOCX", StringComparison.OrdinalIgnoreCase)
						|| fileExtension.Equals(".EMF", StringComparison.OrdinalIgnoreCase)
						|| fileExtension.Equals(".XLS", StringComparison.OrdinalIgnoreCase)
						|| fileExtension.Equals(".XLSX", StringComparison.OrdinalIgnoreCase))
					{
						string code = ieDoc.DocType + " - " + correctedFileName;
						if (StorageDocs.ContainsCode(code))
						{
							counter++;
							var adjustedDuplicateFileName = fileNameWithoutExtension + counter.ToString() + fileExtension;
							code = ieDoc.DocType + " - " + adjustedDuplicateFileName;
						}

						StorageDocs.AddPair(ieDoc.UniqueKey, code, "Date Added: " + ieDoc.DateAdded.ToString());
					}
				}
			}
		}

		ZString KeepOnlyLegalCharactersForSGCustomsInFileName(ZString fileName)
		{
			var processedFileName = fileName.KeepChars(ZString.AlphanumericCharacters + ".,:;!()[]-_ ");
			ZString result = Regex.Replace(processedFileName, @"\s+", "_");
			return result;
		}

		#endregion
	}
}
