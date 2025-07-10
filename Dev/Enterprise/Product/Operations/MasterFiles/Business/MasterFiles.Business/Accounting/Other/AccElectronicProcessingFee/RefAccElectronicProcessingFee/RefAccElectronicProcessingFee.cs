using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefAccElectronicProcessingFee : AutoRefAccElectronicProcessingFee
	{
		public RefAccElectronicProcessingFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
