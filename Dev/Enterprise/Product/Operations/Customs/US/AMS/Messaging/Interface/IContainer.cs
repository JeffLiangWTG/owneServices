using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface ICommonContainer
	{
		ZString ContainerEquipmentNo { get; }
		ZString SealNumber1 { get; }
		ZString SealNumber2 { get; }
		ZString ContainerEquipmentDescriptionCode { get; }
		ZInt ContainerEquipmentLength { get; }
		ZString Height { get; }
		ZString Width { get; }
		ZString ContainerEquipmentType { get; }
		ZString LoadEmptyStatusCode { get; }
		ZString TypeOfServiceCode { get; }
		IEnumerable<ICargoDescription> Commondities { get; }
		IEnumerable<IHazardousMaterial> HazardousMaterials { get; }
	}

	public interface IContainer : ICommonContainer
	{
		IEnumerable<IVINOrEmptyContainer> VINsOrEmptyContainers { get; }
	}

	public interface IACEContainer : ICommonContainer
	{
		IEnumerable<IVehicleDetails> VehicleDetails { get; }
	}
}
