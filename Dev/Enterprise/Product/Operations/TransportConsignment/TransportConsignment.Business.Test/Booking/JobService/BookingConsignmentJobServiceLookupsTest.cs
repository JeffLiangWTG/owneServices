using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class BookingConsignmentJobServiceLookupsTest : JobServiceLookupsTest
	{
		protected override CodeDescriptionPairList GetValidJobServiceTypes()
		{
			var result = base.GetValidJobServiceTypes();

			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in TransportRegistry.Instance.LandTransportJobServices.Value)
			{
				result.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return result;
		}

		protected override JobService GetNewJobService()
		{
			return Factory.New<DtbBookingConsignment>().Services.AddNew();
		}
	}
}
