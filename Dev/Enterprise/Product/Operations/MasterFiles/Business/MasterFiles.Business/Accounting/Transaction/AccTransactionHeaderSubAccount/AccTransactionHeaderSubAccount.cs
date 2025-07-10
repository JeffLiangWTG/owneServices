using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("For DHL R27-4")]
	public class AccTransactionHeaderSubAccount : AutoAccTransactionHeaderSubAccount
	{
		public AccTransactionHeaderSubAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
