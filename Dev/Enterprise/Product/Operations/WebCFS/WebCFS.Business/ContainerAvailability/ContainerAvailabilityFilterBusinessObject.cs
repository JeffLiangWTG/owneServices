using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.WebCFS.Business
{
	public class ContainerAvailabilityFilterBusinessObject : AutoContainerAvailabilityFilterBusinessObject
	{
		public ContainerAvailabilityFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LCV_ContainerNum = ZString.Empty;
			LCV_VoyageFlight = ZString.Empty;
			LCV_Vessel = ZString.Empty;
			FromDate = ZDateTime.Empty;
			ToDate = ZDateTime.Empty;
		}
	}
}
