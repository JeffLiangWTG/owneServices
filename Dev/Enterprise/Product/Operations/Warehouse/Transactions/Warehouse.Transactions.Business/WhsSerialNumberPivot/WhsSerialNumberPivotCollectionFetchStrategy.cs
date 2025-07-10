using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumberPivotCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public WhsSerialNumberPivotCollectionFetchStrategy(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			// We only have one column in grid
			var serialNumberPivot = (WhsSerialNumberPivot)businessObjects[0];
			switch (serialNumberPivot.WSV_ParentTableCode)
			{
				case WhsDocketLineSchema.Constants.Prefix:
					var docketLine = (WhsDocketLine)((IActiveBusinessObjectCollection)Collection).Relationship.Master;
					Collection.Factory.GetCachedValue($"WhsSerialNumberPivotCollectionFetchStrategy|AddFetchHintsForParentDocketLines|{docketLine.WE_WD}", () => AddFetchHintsForParentLines(docketLine));
					break;
				case WhsAsnLineSchema.Constants.Prefix:
					var asnLine = (WhsAsnLine)((IActiveBusinessObjectCollection)Collection).Relationship.Master;
					Collection.Factory.GetCachedValue($"WhsSerialNumberPivotCollectionFetchStrategy|AddFetchHintsForParentAsnLines|{asnLine.WN_WD}", () => AddFetchHintsForParentLines(asnLine));
					break;
				default:
					break;
			}
		}

		bool AddFetchHintsForParentLines(WhsDocketLine docketLine)
			=> AddWhsSerialNumberFetchHint(docketLine.Factory, docketLine.Docket.Lines.Where(l => l.IsInDatabase).Cast<ISerialNumberParent>());

		bool AddFetchHintsForParentLines(WhsAsnLine asnLine)
			=> AddWhsSerialNumberFetchHint(asnLine.Factory, asnLine.Docket.AsnLines.Where(l => l.IsInDatabase).Cast<ISerialNumberParent>());

		static bool AddWhsSerialNumberFetchHint(BusinessObjectFactory factory, IEnumerable<ISerialNumberParent> lines)
		{
			WhsSerialNumberHelper.AddWhsSerialNumberAndPivotFetchHint(factory, lines.Where(l => l.IsSerialNumberUsed).Select(i => i.PK));

			return true;
		}
	}
}
