using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business
{
	public class CusStatementLineCollection : DependentBusinessObjectCollection<CusStatementLine, CusStatementHeader>
	{
		public CusStatementLineCollection(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public new CusStatementHeader Master => base.Master;

		public CusStatementLine GetStatementLineFor(ZString entryNum, ZString entryType)
		{
			var query = new ZQuery();
			query.AddToFilter(CusStatementLineSchema.B3_EntryNum, entryNum);
			query.AddToFilter(CusStatementLineSchema.B3_EntryType, entryType);

			return Find(query).FirstOrDefault() as CusStatementLine;
		}
	}
}
