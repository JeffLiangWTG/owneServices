using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageJobServiceLookups : JobServiceLookups
	{
		public CartageJobServiceLookups(CartageJobService parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GetNewJobServiceType_List()
		{
			CodeDescriptionPairList result = base.GetNewJobServiceType_List();

			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in TransportRegistry.Instance.PortTransportJobServices.Value)
			{
				result.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return result;
		}
	}
}
