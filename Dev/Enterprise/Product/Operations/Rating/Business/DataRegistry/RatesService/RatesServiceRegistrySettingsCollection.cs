using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class RatesServiceRegistrySettingsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new RatesServiceRegistrySettings this[int i] => (RatesServiceRegistrySettings)Elements[i];
		public new RatesServiceRegistrySettings AddNew() => (RatesServiceRegistrySettings)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new RatesServiceRegistrySettingsCollection();
		protected override BusinessObject CreateNonPersistentBusinessObject() => new RatesServiceRegistrySettings();

		public static RatesServiceRegistrySettingsCollection GetEnabled()
		{
			return GetDefault(true);
		}

		public static RatesServiceRegistrySettingsCollection GetDisabled()
		{
			return GetDefault(false);
		}

		public static RatesServiceRegistrySettingsCollection GetDefault(bool isSubscriptionEnabled)
		{
			var result = new RatesServiceRegistrySettingsCollection();

			result.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, isSubscriptionEnabled));
			result.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose, isSubscriptionEnabled));

			return result;
		}
	}
}
