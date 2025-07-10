using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class EntryProcessingPortsMappingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EntryProcessingPortsMappingCollection()
			: base()
		{
		}

		public EntryProcessingPortsMappingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new EntryProcessingPortsMapping this[int i]
		{
			get { return (EntryProcessingPortsMapping)Elements[i]; }
		}

		public new EntryProcessingPortsMapping AddNew()
		{
			return (EntryProcessingPortsMapping)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntryProcessingPortsMappingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EntryProcessingPortsMapping(CurrentFallbackLevel, CurrentFactory, this);
		}

		public ZString GetMappedProcessingPort(ZString entryPort)
		{
			foreach (EntryProcessingPortsMapping mapping in this)
			{
				if (mapping.EntryPort == entryPort)
				{
					return mapping.ProcessingPort;
				}
			}
			return ZString.Empty;
		}
	}
}
