using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Business
{
	public static class AdditionalReferenceHelper
	{
		public static CodeDescriptionPairList GetAdditionalReferenceNumberTypeList()
		{
			var referenceTypes = new CodeDescriptionPairList();
			foreach (var regType in TransportRegistry.Instance.AdditionalReferenceNumbers.Value)
			{
				referenceTypes.Add(regType);
			}

			return referenceTypes;
		}
	}
}
