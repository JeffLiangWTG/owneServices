using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class SellRatesDecimalsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new SellRatesDecimals this[int i]
		{
			get { return (SellRatesDecimals)Elements[i]; }
		}

		public new SellRatesDecimals AddNew()
		{
			return (SellRatesDecimals)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SellRatesDecimalsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SellRatesDecimals();
		}

		public static SellRatesDecimalsCollection GetDefault()
		{
			SellRatesDecimalsCollection result = new SellRatesDecimalsCollection();

			result.AddNew().SetDefaults(RatingConstants.RateCategory.ALL);

			return result;
		}
	}
}

