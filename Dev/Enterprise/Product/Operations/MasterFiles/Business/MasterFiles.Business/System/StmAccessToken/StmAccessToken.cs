using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmAccessToken : AutoStmAccessToken
	{
		public StmAccessToken(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
