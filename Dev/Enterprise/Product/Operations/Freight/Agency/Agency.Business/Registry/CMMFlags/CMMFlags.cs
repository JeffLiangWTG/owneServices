using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class CMMFlags : AutoCMMFlags
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CMMFlags();
		}

		#endregion
	}
}


