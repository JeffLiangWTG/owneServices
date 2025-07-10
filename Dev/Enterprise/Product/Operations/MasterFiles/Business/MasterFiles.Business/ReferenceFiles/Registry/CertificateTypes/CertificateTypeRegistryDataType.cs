using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	[RegistryEditor("Enterprise.MasterFiles.GUI.CertificateTypeRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class CertificateTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CertificateTypeCollection>
	{
		public CertificateTypeRegistryDataType(CertificateTypeCollection defaultValue)
			: base(defaultValue)
		{
		}
	}
}
