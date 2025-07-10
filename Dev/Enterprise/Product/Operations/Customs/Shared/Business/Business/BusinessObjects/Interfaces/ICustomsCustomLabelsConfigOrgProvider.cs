using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICustomsCustomLabelsConfigOrgProvider : MasterFiles.Business.ICustomLabelsConfigOrgProvider
	{
		ZGuid PK { get; }
		ZString PartAttribute1 { get; }
		ZString PartAttribute2 { get; }
		ZString PartAttribute3 { get; }
		ZString SerialNumber { get; }
	}
}
