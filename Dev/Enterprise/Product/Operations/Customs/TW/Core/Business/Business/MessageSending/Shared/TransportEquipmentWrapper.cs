using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class TransportEquipmentWrapper : ITransportEquipment
	{
		public TransportEquipmentWrapper(CusContainer cusContainer)
		{
			this.cusContainer = cusContainer;
		}

		readonly CusContainer cusContainer;

		public ZString CharacteristicCode => cusContainer.GetCharacteristicCode();

		public ZString ID => cusContainer.CO_ContainerNumber;

		public ZString UsedCapacityCode => cusContainer.GetUsedCapacityCode();

		public IEnumerable<ZString> Seals => cusContainer.GetSeals();
	}
}
