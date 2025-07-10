using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	[XmlRoot("PhaseDependants")]
	public class PhaseDependantCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PhaseDependantCollection()
		{
		}

		#region Implementation

		public new PhaseDependant AddNew()
		{
			return (PhaseDependant)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PhaseDependantCollection();
		}

		public new PhaseDependant this[int i]
		{
			get { return (PhaseDependant)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PhaseDependant();
		}

		#endregion
	}
}
