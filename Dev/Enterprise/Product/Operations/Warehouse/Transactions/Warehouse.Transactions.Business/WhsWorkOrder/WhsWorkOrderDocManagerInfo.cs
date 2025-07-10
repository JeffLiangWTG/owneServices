using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderDocManagerInfo : DocManagerInfo
	{
		public WhsWorkOrderDocManagerInfo(WhsWorkOrder workOrder, ZString docManagerCode)
			: base(workOrder, docManagerCode)
		{
			this.workOrder = workOrder;
		}

		protected override IStorageDocsBaseCollection GetEDocsView()
		{
			var eDocsView = new NonPersistantStorageDocsBaseCollection();

			foreach (WhsWorkOrderLine line in workOrder.AllLines)
			{
				if (line.BOM.IsTopLevelProduct && line.SupplierPart.OP_AutoPrintAssemblyInstructions)
				{
					foreach (IeDoc eDoc in line.SupplierPartDocManagerInfo.EDocsView)
					{
						if (eDoc.DocType == line.SupplierPartDocManagerInfo.DocManagerCode)
						{
							eDocsView.Add(eDoc);
						}
					}
				}
			}

			if (eDocsView.Count == 0)
			{
				eDocsView.AddRange(StorageMain.EDocsView);
			}

			return eDocsView;
		}

		readonly WhsWorkOrder workOrder;

		class NonPersistantStorageDocsBaseCollection : List<IeDoc>, IStorageDocsBaseCollection
		{
			#region IStorageDocsBaseCollection Members

			public void AddRange(IStorageDocsBaseCollection collection)
			{
				foreach (IeDoc item in collection)
				{
					this.Add(item);
				}
			}

			public IeDoc GetFromUniqueKey(Guid uniqueKey)
			{
				throw new NotImplementedException();
			}

			public IeDoc GetMostRecentEDoc(string docType)
			{
				throw new NotImplementedException();
			}

			public new void Remove(IeDoc elementToRemove)
			{
				throw new NotImplementedException();
			}

			public bool ContainsDocType(ZString docType)
			{
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}
