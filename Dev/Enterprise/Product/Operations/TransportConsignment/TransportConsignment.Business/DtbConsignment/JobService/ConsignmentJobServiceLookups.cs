using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	public class ConsignmentJobServiceLookups : JobServiceLookups
	{
		public ConsignmentJobServiceLookups(ConsignmentJobService parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GetNewJobServiceType_List()
		{
			var result = base.GetNewJobServiceType_List();

			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in TransportRegistry.Instance.LandTransportJobServices.Value)
			{
				result.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return result;
		}
	}
}
