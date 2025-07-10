using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class FreightStatusQuery : IFreightStatusQuery
	{
		public FreightStatusQuery()
		{
			StatusRequestCode = AIMFreightStatusRequestCodes.Codes.RequestAllInformationForSingleBill;
		}

		public ZString StatusRequestCode { get; set; }
	}
}
