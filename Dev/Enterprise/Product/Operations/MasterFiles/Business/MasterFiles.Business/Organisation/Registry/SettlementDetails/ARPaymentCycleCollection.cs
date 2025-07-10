using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARPaymentCycleCollection : ARTermsCycleCollection
	{
		public ARPaymentCycleCollection()
		{
		}

		public ARPaymentCycleCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ARPaymentCycle this[int x]
		{
			get { return (ARPaymentCycle)Elements[x]; }
		}

		public new ARPaymentCycle AddNew()
		{
			return (ARPaymentCycle)base.AddNew();
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
			return new ARPaymentCycleCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARPaymentCycle(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
