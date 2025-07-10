using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineMessagingRequirementType : AutoRefShippingLineMessagingRequirementType
	{
		public RefShippingLineMessagingRequirementType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
