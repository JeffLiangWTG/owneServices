using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageJobServiceLookupsTest : JobServiceLookupsTest
	{
		protected override CodeDescriptionPairList GetValidJobServiceTypes()
		{
			CodeDescriptionPairList result = base.GetValidJobServiceTypes();
			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in TransportRegistry.Instance.PortTransportJobServices.Value)
			{
				result.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return result;
		}

		protected override JobService GetNewJobService()
		{
			return Factory.New<CommonBookedCtgMove>().Services.AddNew();
		}
	}
}
