using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	[XmlRoot("EventInfos")]
	public sealed class DeclarationEventLockInfoCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DeclarationEventLockInfoCollection()
		{
		}

		public DeclarationEventLockInfoCollection(DeclarationLockConfig lockConfig, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			this.lockConfig = lockConfig;
		}

		public DeclarationLockConfig LockConfig
		{
			get { return lockConfig; }
		}
		DeclarationLockConfig lockConfig;

		public new DeclarationEventLockInfo this[int i]
		{
			get { return (DeclarationEventLockInfo)Elements[i]; }
		}

		public new DeclarationEventLockInfo AddNew()
		{
			return (DeclarationEventLockInfo)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DeclarationEventLockInfoCollection(LockConfig, fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DeclarationEventLockInfo(CurrentFallbackLevel, CurrentFactory);
		}

		public DeclarationEventLockInfoCollection Clone(DeclarationLockConfig declarationLockConfig, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = (DeclarationEventLockInfoCollection)Clone(fallbackLevel, factory);
			result.lockConfig = declarationLockConfig;

			return result;
		}
	}
}
