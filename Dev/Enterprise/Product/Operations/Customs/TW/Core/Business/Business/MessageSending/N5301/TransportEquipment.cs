using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class TransportEquipment : ITransportEquipment
	{
		public TransportEquipment(CusInBondContainer cusInBondContainer)
		{
			this.cusInBondContainer = cusInBondContainer;
		}

		readonly CusInBondContainer cusInBondContainer;

		public ZString CharacteristicCode => cusInBondContainer.GetCharacteristicCode();

		public ZString ID => cusInBondContainer.BC_ContainerNum;

		public ZString UsedCapacityCode => cusInBondContainer.GetUsedCapacityCode();

		public IEnumerable<ZString> Seals => null;
	}
}
