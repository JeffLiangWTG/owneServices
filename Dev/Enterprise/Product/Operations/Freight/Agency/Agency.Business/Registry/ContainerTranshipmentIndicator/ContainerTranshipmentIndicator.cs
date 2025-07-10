using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class ContainerTranshipmentIndicator : AutoContainerTranshipmentIndicator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "lookup code constant")]
		public static class Keys
		{
			public const string Empty = "Empty";
			public const string BreakBulk = "BreakBulk";
			public const string Laden = "Laden";
		}

		public override void Delete()
		{
			throw new CannotDeleteException(Res.GetString("{00E12A47-48D9-4dd6-925A-96DABB9F7AF1}", "You cannot delete or disable this mode."));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ContainerTranshipmentIndicator();
		}
	}
}


