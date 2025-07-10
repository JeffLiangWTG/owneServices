using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailsCollection : BusinessObjectCollection<CusInBondMoveDetail>
	{
		public CusInBondMoveDetailsCollection(CusInBondHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}
		readonly CusInBondHeader header;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			var moveHeadersQuery = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			if (!header.IsInDatabase)
			{
				moveHeadersQuery.FetchOnlyFromLocalCache = true;
			}

			var moveHeaders = Factory.Load<CusInBondMoveHeader>(moveHeadersQuery);
			if (moveHeaders.Length > 0)
			{
				result.AddToFilter(CusInBondMoveDetailSchema.B9_BM, moveHeaders.Select(x => x.PK));
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		protected override bool AllowNewCore
		{
			get { return !header.ShouldSynchronise; }
		}
	}
}
