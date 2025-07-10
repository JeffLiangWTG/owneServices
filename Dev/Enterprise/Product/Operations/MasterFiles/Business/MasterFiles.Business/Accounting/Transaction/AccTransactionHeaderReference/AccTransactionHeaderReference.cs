using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderReference : AutoAccTransactionHeaderReference
	{
		public AccTransactionHeaderReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
