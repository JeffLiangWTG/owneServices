using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARTermsCycleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ARTermsCycleCollection()
		{
		}

		public ARTermsCycleCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ARTermsCycle this[int x]
		{
			get { return (ARTermsCycle)Elements[x]; }
		}

		public new ARTermsCycle AddNew()
		{
			return (ARTermsCycle)base.AddNew();
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				if (Count > 1)
				{
					return base.AllowRemoveCore;
				}
				else
				{
					return false;
				}
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARTermsCycleCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARTermsCycle(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
