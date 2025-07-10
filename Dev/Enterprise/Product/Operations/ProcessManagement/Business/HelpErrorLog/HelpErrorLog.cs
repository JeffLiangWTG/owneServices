using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class HelpErrorLog : AutoHelpErrorLog
	{
		public HelpErrorLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
