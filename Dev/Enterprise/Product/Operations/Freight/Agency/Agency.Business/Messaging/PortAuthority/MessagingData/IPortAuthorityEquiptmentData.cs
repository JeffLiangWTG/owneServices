namespace Enterprise.Freight.Agency.Business
{
	public interface IPortAuthorityEquipmentData
	{
		string ContainerNumber { get; }
		string ContainerISOCode { get; }
		PortAuthorityContainerStatus ContainerStatus { get; }
		bool IsEmpty { get; }
	}
}
