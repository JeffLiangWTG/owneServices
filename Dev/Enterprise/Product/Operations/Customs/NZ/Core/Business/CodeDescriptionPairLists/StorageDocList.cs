using System.IO;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class StorageDocList : CodeDescriptionPairList
	{
		public StorageDocList()
		{ }

		public const string FileTooBigIndicator = " ** File Too Large **";

		public void SetupDocList(IStorageDocsBaseCollection storageDocsCollection)
		{
			foreach (IeDoc ieDoc in storageDocsCollection)
			{
				if (!ieDoc.IsDeleted)
				{
					try
					{
						var fileExtension = Path.GetExtension(ieDoc.FileName).ToUpper();
						if (IsAcceptedFileType(fileExtension))
						{
							var eDoc = ieDoc as StorageFile;
							if (eDoc != null && attachmentTooBig(eDoc.SC_ImageData.Length))
							{
								AddPair(ieDoc.UniqueKey, ieDoc.DocType.PadRight(3) + "-" + ieDoc.FileName, "Added: " + ieDoc.DateAdded.ToString() + " - " + ieDoc.Description + " - " + FileTooBigIndicator);
							}
							else
							{
								AddPair(ieDoc.UniqueKey, ieDoc.DocType.PadRight(3) + "-" + ieDoc.FileName, "Added: " + ieDoc.DateAdded.ToString() + " - " + ieDoc.Description);
							}
						}
					}
					catch (System.ArgumentException) { }
				}
			}
		}

		bool IsAcceptedFileType(string fileExtension)
		{
			return fileExtension.Equals(".PDF")
				|| fileExtension.Equals(".CSV")
				|| fileExtension.Equals(".TIF")
				|| fileExtension.Equals(".GIF")
				|| fileExtension.Equals(".JPG")
				|| fileExtension.Equals(".JPEG")
				|| fileExtension.Equals(".DOCX")
				|| fileExtension.Equals(".PNG")
				|| fileExtension.Equals(".XLSX");
		}

		bool attachmentTooBig(ZInt fileSize)
		{
			int maximumAttachmentFileSizeAllowed = NZCustomsDataRegistry.Instance.MaxMessageAttachmentSize.Value;
			return fileSize > maximumAttachmentFileSizeAllowed;
		}
	}
}
