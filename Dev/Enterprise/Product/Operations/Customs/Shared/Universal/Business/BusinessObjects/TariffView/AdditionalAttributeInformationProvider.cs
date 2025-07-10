using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Universal
{
	public class AdditionalAttributeInformationProvider : IAdditionalAttributeInformationProvider
	{
		public AdditionalAttributeInformationProvider()
		{
		}

		internal static IAdditionalAttributeInformationProvider GetAdditionalAttributeInformationProvider(TariffView tariffView)
		{
			var factory = tariffView.Factory;
			var countryCode = tariffView.ZZ1_ZZZ_NKDataGrouping;
			return factory.GetCachedValue(countryCode, () =>
			{
				var builders = ObjectFactory.Get<Hashtable>("AdditionalAttributeInformationProvider");
				var objectHandle = (ObjectHandle)builders[countryCode.ToString()];
				return objectHandle != null ? (IAdditionalAttributeInformationProvider)objectHandle.GetObject(tariffView.Factory) : new AdditionalAttributeInformationProvider();
			});
		}

		#region IAdditionalAttributeInformationProvider Members
		public bool AdditionalDescriptionVisible => false;
		public ZString AdditionalDescription(ZString attributeName, ZString attributeValue) => ZString.Empty;
		#endregion;
	}
}
