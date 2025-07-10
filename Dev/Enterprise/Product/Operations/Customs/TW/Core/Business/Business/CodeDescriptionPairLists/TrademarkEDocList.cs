using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class TrademarkEDocList : AvailableEDocList
	{
		readonly IStorageDocsBaseCollection[] eDocCollections;

		public TrademarkEDocList(params IStorageDocsBaseCollection[] eDocCollections)
		{
			this.eDocCollections = eDocCollections;
			InitializeEDocsList();
		}

		public ZGuid DefaultDocPk { get; private set; }

		void InitializeEDocsList()
		{
			AvailableList = new List<IeDoc>();
			foreach (var eDocs in eDocCollections.Where(x => x != null))
			{
				foreach (var eDoc in eDocs.Cast<IeDoc>().Where(x => !x.IsDeleted && x.DocType == MessageConstants.DocumentTypes.TDM))
				{
					if (DefaultDocPk.IsEmpty && ((IStorageMain)eDoc.ParentMain).DocumentOwner is OrgSupplierPart)
					{
						DefaultDocPk = eDoc.UniqueKey;
					}
					AddEdocPair(eDoc, AvailableList);
				}
			}
		}
	}
}
