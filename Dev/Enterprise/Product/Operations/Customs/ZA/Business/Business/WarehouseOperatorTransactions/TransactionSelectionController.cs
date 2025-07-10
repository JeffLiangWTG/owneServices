using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class TransactionSelectionController : NonPersistentBusinessObject
	{
		public TransactionSelectionController(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OperatorTransactionSelectionCollection Records
		{
			get
			{
				if (records == null)
				{
					records = new OperatorTransactionSelectionCollection();
					var transactions = Factory.Load<CusWHSOperatorTransaction>(TransactionFilter);
					foreach (var tx in transactions)
					{
						var r = records.AddNew();
						r.TransactionType = tx.WOT_TransactionType;
						r.TransactionDate = tx.WOT_TransactionDate;
						r.ExportType = tx.WOT_ExportType;
						r.OwnerReference = tx.WOT_OwnerReference;
						r.ProductCode = tx.Product.OP_PartNum;
						r.BatchLineNo = tx.WOT_BatchLineNo;
						r.LineReference = tx.WOT_LineReference;
						r.WarehouseAddress = tx.Batch.WOB_OA_Warehouse;
						r.Batch = tx.Batch.PK;
						TransactionSelectionMapping.Add(r.PK, tx);
					}
				}
				return records;
			}
		}

		public IList<OperatorTransactionSelection> SelectedRecords
		{
			get
			{
				return Records.Cast<OperatorTransactionSelection>().Where(r => r.Select).ToList();
			}
		}

		OperatorTransactionSelectionCollection records;

		public bool SelectAll
		{
			get => Records.Count > 0 && Records.All(x => ((OperatorTransactionSelection)x).Select);
			set
			{
				foreach (var rec in Records.Cast<OperatorTransactionSelection>())
				{
					rec.Select = value;
				}
			}
		}

		public void ProcessSelectedRecords()
		{
			var shouldSave = false;
			foreach (var r in SelectedRecords)
			{
				if (TransactionSelectionMapping.ContainsKey(r.PK))
				{
					ProcessSelectedRecord(TransactionSelectionMapping[r.PK]);
					shouldSave = true;
				}
			}
			if (shouldSave)
			{
				Factory.Save();
			}
		}

		public virtual bool CanProceed => Records.Count > 0;

		protected abstract void ProcessSelectedRecord(CusWHSOperatorTransaction tx);

		protected abstract ZQuery TransactionFilter { get; }
		protected Dictionary<ZGuid, CusWHSOperatorTransaction> TransactionSelectionMapping { get; } = new Dictionary<ZGuid, CusWHSOperatorTransaction>();
	}
}
