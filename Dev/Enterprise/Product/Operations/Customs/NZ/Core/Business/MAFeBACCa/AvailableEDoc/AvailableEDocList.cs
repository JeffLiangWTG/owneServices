using System;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public class AvailableEDocList : CodeDescriptionPairList
	{
		public AvailableEDocList()
		{
		}

		public AvailableEDocList(IStorageDocsBaseCollection eDocs)
			: this()
		{
			var allowedFileExtensions = ImageToPDFConverter.GetAllowedFileExtensions();

			foreach (IeDoc eDoc in eDocs)
			{
				if (!eDoc.IsDeleted)
				{
					try
					{
						var fileExtension = Path.GetExtension(eDoc.FileName);
						if (allowedFileExtensions.Contains(fileExtension.ToUpper()))
						{
							AddPair(eDoc.UniqueKey, eDoc.DocType.PadRight(3) + "-" + eDoc.FileName, "Added: " + eDoc.DateAdded.ToShortDateString() + " - " + eDoc.Description);
						}
					}
					catch (ArgumentException)
					{
						//Skip invalid files
					}
				}
			}
		}

		public ZString GetFileNameFromPK(ZGuid pk)
		{
			var element = Elements.FirstOrDefault(eDoc => new ZGuid(eDoc.PK) == pk);
			return element != null ? new ZString(element.Code).SubstringSafe(4) : ZString.Empty;
		}
	}
}
