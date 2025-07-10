using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class CusStatementLineCollection : ActiveBusinessObjectCollection<CusStatementLine>
	{
		public CusStatementLineCollection(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public CusStatementLine GetStatementLineFor(ZString entryNum)
		{
			return this.FirstOrDefault(x => x.B3_EntryNum == entryNum);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
