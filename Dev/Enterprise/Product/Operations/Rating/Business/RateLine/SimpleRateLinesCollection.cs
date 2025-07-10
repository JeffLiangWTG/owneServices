using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class SimpleRateLinesCollection : BusinessObjectCollection<RateLine>
	{
		public SimpleRateLinesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public SimpleRateLinesCollection(List<RateLine> lineList, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var line in lineList)
			{
				this.Add(line);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			var pk = Guid.NewGuid();
			using (pk.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.AddNewCore(bizoType, pk);
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			using (row.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.CreateBusinessObjectFromRow(row);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			using (elementToDelete.MarkAsInDeletion())
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

#if DEBUG
		public bool Contains(RateLine line)
		{
			return Find(line) != null;
		}

		public RateLine Find(RateLine line)
		{
			return (RateLine)FindByPK(line.PK);
		}
#endif
	}
}

