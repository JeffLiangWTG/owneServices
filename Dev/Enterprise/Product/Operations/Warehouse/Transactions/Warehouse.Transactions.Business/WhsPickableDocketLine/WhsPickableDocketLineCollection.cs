using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickableDocketLineCollection : WhsDocketLineCollection
	{
		protected WhsPickableDocketLineCollection(WhsPickableDocket master)
			: base(master)
		{
		}

		protected WhsPickableDocketLineCollection(WhsPickableDocket master, ZQuery filter)
			: base(master, filter)
		{
		}

		protected WhsPickableDocketLineCollection(WhsPickableDocketLine master, ZQuery filter, SchemaGuidColumn fk)
			: base(master, filter, fk)
		{
		}

		public new WhsPickableDocketLine this[int index] => (WhsPickableDocketLine)base[index];

		public new WhsPickableDocketLine AddNew() => (WhsPickableDocketLine)base.AddNew();

		public WhsPickableDocketLine FindLine(ZShort lineNo) => lineNo == 0 ? null : (WhsPickableDocketLine)this.FirstOrDefault(l => l.WE_LineNo == lineNo);

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			return
				property.Name == StagingLocationBOMPropertyName
				? new LocationComparer<WhsPickableDocketLine>(property, direction, pickableDocketLine => pickableDocketLine.StagingLocationBOM)
				: base.GetSortComparerForProperty(property, direction);
		}

		internal static string StagingLocationBOMPropertyName => $"{nameof(WhsPickableDocketLine.StagingLocationBOM)}+{nameof(WhsPickableDocketLine.PK)}";
	}
}
