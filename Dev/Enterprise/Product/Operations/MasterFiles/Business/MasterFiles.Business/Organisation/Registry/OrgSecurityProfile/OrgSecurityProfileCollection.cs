using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OrgSecurityProfileCollection : RegistryBusinessObjectCollectionTemplate<OrgSecurityProfile>
	{
		public OrgSecurityProfileCollection()
			: base(null, null)
		{
		}

		protected override bool AllowNewCore => false;

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgSecurityProfileCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgSecurityProfile();
		}

		#endregion Implementation
	}
}
