using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region Non Dependant WhsPickLineCollection

	public class WhsPickLineCollectionND : BusinessObjectCollection<WhsPickLine>
	{
		public WhsPickLineCollectionND(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsPickLineCollectionND(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Add

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region GetQtyPicked

		public ZDecimal GetQtyPicked()
		{
			ZDecimal result = 0m;

			foreach (WhsPickLine pickLine in this)
			{
				result += pickLine.WZ_Units;
			}

			return result;
		}

		#endregion

		#region AddPickLinesWithRemovalFromOriginalCollection

		public void AddPickLinesWithRemovalFromOriginalCollection(WhsPickableDocketLine pickableDocketLine, WhsPickLineCollectionND allPossiblePickLinesForThisPickableDocketLine)
		{
			for (int i = allPossiblePickLinesForThisPickableDocketLine.Count - 1; i >= 0; i--)
			{
				if (allPossiblePickLinesForThisPickableDocketLine[i].WZ_WE_TransactionLine == pickableDocketLine.PK)
				{
					Add(allPossiblePickLinesForThisPickableDocketLine[i]);
					allPossiblePickLinesForThisPickableDocketLine.Remove(allPossiblePickLinesForThisPickableDocketLine[i]);
				}
			}
		}

		#endregion
	}

	#endregion
}
