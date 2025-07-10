using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineEBLProvider : AutoRefShippingLineEBLProvider
	{
		public RefShippingLineEBLProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
