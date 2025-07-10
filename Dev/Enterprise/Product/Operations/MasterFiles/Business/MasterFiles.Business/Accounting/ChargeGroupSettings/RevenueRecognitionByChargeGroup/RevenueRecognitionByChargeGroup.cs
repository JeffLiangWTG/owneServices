using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class RevenueRecognitionByChargeGroup : ChargeGroupSetup<RevenueRecognitionCollection>
	{
		public RevenueRecognitionByChargeGroup()
			: base()
		{
		}

		public RevenueRecognitionByChargeGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RevenueRecognitionByChargeGroup(fallbackLevel, null);
		}
	}
}
