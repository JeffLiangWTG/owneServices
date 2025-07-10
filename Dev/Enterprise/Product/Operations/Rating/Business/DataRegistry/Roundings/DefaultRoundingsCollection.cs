using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class DefaultRoundingsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DefaultRoundings this[int i]
		{
			get { return (DefaultRoundings)Elements[i]; }
		}

		public new DefaultRoundings AddNew()
		{
			return (DefaultRoundings)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultRoundingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultRoundings();
		}

		public static DefaultRoundingsCollection GetDefault()
		{
			DefaultRoundingsCollection result = new DefaultRoundingsCollection();

			result.AddNew().SetDefaults(RatingConstants.RateCategory.ALL);
			result.AddNew().SetDefaults(RatingConstants.RateCategory.WHS);

			return result;
		}
	}
}

