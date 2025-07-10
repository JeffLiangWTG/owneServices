using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveProductSummaryCollection : NonPersistentBusinessObjectCollection<WhsReceiveProductSummary>, ICollection, IEnumerable<BusinessObject>, IEnumerable
	{
		public WhsReceiveProductSummaryCollection(BusinessObjectFactory factory, WhsReceive receive)
			: base(factory)
		{
			Receive = Argument.NotNull(receive, nameof(receive));
			BuildCollection(receive.Lines);
		}

		void BuildCollection(IEnumerable<WhsDocketLine> docketLines)
		{
			var asnLines = Receive.AsnLines?.Cast<WhsAsnLine>().ToArray();
			var receiveLines = docketLines?.Cast<WhsReceiveLine>().ToArray();
			var hasAsnLines = asnLines?.Length > 0;
			var productSummary = new Dictionary<ZString, (ZGuid ProductPk, ZString ProductDescription, ZDecimal ExpectedQty, ZDecimal ReceivedQty)>();
			SetProductSummaryForAsnLines(asnLines, productSummary);
			SetProductSummaryForReceiveLines(receiveLines, productSummary, hasAsnLines);
			BuildCollection(productSummary, !hasAsnLines);
			NeedsRefresh = receiveLines == null && asnLines == null;
		}

		void BuildCollection(Dictionary<ZString, (ZGuid ProductPk, ZString ProductDescription, ZDecimal ExpectedQty, ZDecimal ReceivedQty)> productSummary, bool isBlindReceive)
		{
			productSummary.ForEach(ps =>
			{
				var productPk = ps.Value.ProductPk;
				var clientPk = Receive.Client?.PK ?? ZGuid.Empty;
				var warehousePk = Receive.Warehouse?.PK ?? ZGuid.Empty;
				var productCode = ps.Key;
				var productDesc = ps.Value.ProductDescription;
				var receiveCategory = Receive.WD_ReceiveCategory;
				Add(new WhsReceiveProductSummary(Factory, clientPk, warehousePk, productPk, isBlindReceive, productCode, productDesc, receiveCategory, ps.Value.ExpectedQty, ps.Value.ReceivedQty));
			});
		}

		void SetProductSummaryForAsnLines(WhsAsnLine[] asnLines, Dictionary<ZString, (ZGuid ProductPk, ZString ProductDescription, ZDecimal ExpectedQty, ZDecimal ReceivedQty)> productSummary)
		{
			asnLines.ForEach(asn =>
				{
					var product = asn.SupplierPart;
					if (productSummary.TryGetValue(product.OP_PartNum, out var quantities))
					{
						quantities.ExpectedQty += asn.WN_Quantity;
						productSummary[product.OP_PartNum] = quantities;
					}
					else
					{
						productSummary.Add(product.OP_PartNum, (asn.WN_OP, product.OP_Desc, asn.WN_Quantity, ZDecimal.Zero));
					}
				}
			);
		}

		void SetProductSummaryForReceiveLines(WhsReceiveLine[] receiveLines, Dictionary<ZString, (ZGuid ProductPk, ZString ProductDescription, ZDecimal ExpectedQty, ZDecimal ReceivedQty)> productSummary, bool hasAsnLines)
		{
			receiveLines.ForEach(rl =>
				{
					if (productSummary.TryGetValue(rl.ProductCode, out var quantities))
					{
						quantities.ReceivedQty += rl.WE_TransactionQuantity;
						if (!hasAsnLines)
						{
							quantities.ExpectedQty += rl.WE_ClientOrderedUnits;
						}
						productSummary[rl.ProductCode] = quantities;
					}
					else
					{
						var expectedQty = hasAsnLines ? ZDecimal.Zero : rl.WE_ClientOrderedUnits;
						productSummary.Add(rl.ProductCode, (rl.WE_OP, rl.ProductDesc, expectedQty, rl.WE_TransactionQuantity));
					}
				}
			);
		}

		WhsReceive Receive { get; }

		void RebuildCollection(IEnumerable<WhsDocketLine> docketLines)
		{
			using (new SemaphoreManager(RebuildingCollectionSemaphore))
			{
				ClearCollection();
				BuildCollection(docketLines);
			}
		}

		public void ClearCollection()
		{
			try
			{
				using (new SemaphoreManager(RebuildingCollectionSemaphore))
				{
					if (Count > 0)
					{
						RemoveAndDeleteAll();
					}
				}
			}
			finally
			{
				NeedsRefresh = true;
			}
		}

		public bool NeedsRefresh
		{
			get { return !IsLoaded; }
			set { IsLoaded = !value; }
		}

		#region RebuildingCollectionSemaphore

		Semaphore RebuildingCollectionSemaphore
		{
			get { return rebuildingCollectionSemaphore ?? (rebuildingCollectionSemaphore = new Semaphore()); }
		}

		Semaphore rebuildingCollectionSemaphore;

		#endregion

		#region SuspendRebuild

		/// <summary>
		/// Do NOT use this method as this will prevent the Collection from updating from enumeration or checking the Count.
		/// This is only used in the GUI for when the Grid Control is Disposed.
		/// </summary>
		public IDisposable SuspendRebuild()
		{
			bool previousState = NeedsRefresh;
			return new DisposableAction(() => NeedsRefresh = false, () => NeedsRefresh = previousState);
		}

		#endregion

		#region ICollection

		int ICollection.Count
		{
			get { return Count; }
		}

		public new int Count
		{
			get
			{
				if (NeedsRefresh && !RebuildingCollectionSemaphore.IsSuspended)
				{
					RebuildCollection(Receive.Lines);
				}

				return base.Count;
			}
		}

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<BusinessObject> GetEnumerator()
		{
			if (NeedsRefresh && !RebuildingCollectionSemaphore.IsSuspended)
			{
				RebuildCollection(Receive.Lines);
			}

			return new BusinessObjectCollectionEnumerator(this);
		}

		#endregion

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
		}

		protected override bool AllowNewCore => false;

		#endregion

	}
}
