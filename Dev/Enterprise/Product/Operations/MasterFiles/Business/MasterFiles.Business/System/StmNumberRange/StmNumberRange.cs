using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmNumberRange : AutoStmNumberRange
	{
		public StmNumberRange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
