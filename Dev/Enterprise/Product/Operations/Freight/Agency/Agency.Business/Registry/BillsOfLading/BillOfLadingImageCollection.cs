using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public class BillOfLadingImageCollection : RegistryBusinessObjectCollectionTemplate
	{
		public BillOfLadingImageCollection()
		{
		}

		public BillOfLadingImageCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new BillOfLadingImage this[int index]
		{
			get { return (BillOfLadingImage)Elements[index]; }
		}

		public new BillOfLadingImage AddNew()
		{
			return (BillOfLadingImage)base.AddNew();
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillOfLadingImage(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillOfLadingImageCollection(fallbackLevel, factory);
		}

		#endregion
	}
}
