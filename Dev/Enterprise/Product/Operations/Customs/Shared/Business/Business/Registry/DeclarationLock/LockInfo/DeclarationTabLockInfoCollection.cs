using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	[XmlRoot("TabInfos")]
	public sealed class DeclarationTabLockInfoCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DeclarationTabLockInfoCollection()
		{
		}

		public DeclarationTabLockInfoCollection(DeclarationLockConfig lockConfig, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			this.lockConfig = lockConfig;
		}

		public DeclarationLockConfig LockConfig
		{
			get { return lockConfig; }
		}
		DeclarationLockConfig lockConfig;

		public new DeclarationTabLockInfo this[int i]
		{
			get { return (DeclarationTabLockInfo)Elements[i]; }
		}

		public new DeclarationTabLockInfo AddNew()
		{
			return (DeclarationTabLockInfo)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DeclarationTabLockInfoCollection(LockConfig, fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DeclarationTabLockInfo(CurrentFallbackLevel, CurrentFactory);
		}

		public DeclarationTabLockInfoCollection Clone(DeclarationLockConfig declarationLockConfig, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = (DeclarationTabLockInfoCollection)Clone(fallbackLevel, factory);
			result.lockConfig = declarationLockConfig;

			return result;
		}
	}
}
