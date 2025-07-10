using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TetsCusStatementLine : BaseCusStatementLine
	{
		public TetsCusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
