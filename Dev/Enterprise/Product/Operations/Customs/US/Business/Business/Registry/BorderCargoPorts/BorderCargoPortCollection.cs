using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class BorderCargoPortCollection : RegistryBusinessObjectCollectionTemplate
	{
		public BorderCargoPortCollection()
			: base()
		{
		}

		public BorderCargoPortCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BorderCargoPort this[int i]
		{
			get { return (BorderCargoPort)Elements[i]; }
		}

		public new BorderCargoPort AddNew()
		{
			return (BorderCargoPort)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BorderCargoPortCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BorderCargoPort(CurrentFallbackLevel, CurrentFactory);
		}

		public ZBool ContainsPortCode(ZString portCode)
		{
			foreach (BorderCargoPort port in this)
			{
				if (port.PortCode == portCode)
				{
					return true;
				}
			}
			return false;
		}
	}
}
