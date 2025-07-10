using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class AvailableEDocList : CodeDescriptionPairList
	{
		readonly List<ZString> extensionFilter;
		readonly IStorageDocsBaseCollection[] eDocCollections;
		public List<IeDoc> AvailableList;

		public AvailableEDocList()
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AvailableEDocList(List<ZString> filter, params IStorageDocsBaseCollection[] eDocCollections)
		{
			this.extensionFilter = filter;
			this.eDocCollections = eDocCollections;

			InitializeEDocsList();
		}

		void InitializeEDocsList()
		{
			AvailableList = new List<IeDoc>();
			foreach (IStorageDocsBaseCollection eDocs in eDocCollections)
			{
				if (eDocs != null)
				{
					foreach (IeDoc eDoc in eDocs)
					{
						if (!eDoc.IsDeleted)
						{
							var fileExtension = Path.GetExtension(eDoc.FileName).Replace(".", "");
							if (!extensionFilter.Any() || extensionFilter.Any(e => e.EqualsIgnoringCase(fileExtension)))
							{
								AddEdocPair(eDoc, AvailableList);
							}
						}
					}
				}
			}
		}

		protected void AddEdocPair(IeDoc eDoc, List<IeDoc> availableList)
		{
			var bizO = eDoc.ParentMain as IStorageMain;
			if (bizO != null)
			{
				var pair = GetEDocListPair(eDoc, bizO);
				AddPair(eDoc.UniqueKey, pair.Code, pair.Description);
				availableList.Add(eDoc);
			}
		}

		protected virtual CodeDescriptionPair GetEDocListPair(IeDoc eDoc, IStorageMain bizO)
		{
			return new CodeDescriptionPair(bizO.DocumentOwnerDescription.Trim() + " - " + eDoc.DocType.PadRight(3) + "-" + eDoc.FileName, Res.GetString("EC773E09-AD53-48F6-B66B-33024D74E45C", "Added: {0} - {1}", eDoc.DateAdded.ToShortDateString(), eDoc.Description));
		}
	}
}
