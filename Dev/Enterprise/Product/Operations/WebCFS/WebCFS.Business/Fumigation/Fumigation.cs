using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.WebCFS.Business
{
	public class Fumigation : Autovw_List_Fumigation
	{
		public Fumigation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
