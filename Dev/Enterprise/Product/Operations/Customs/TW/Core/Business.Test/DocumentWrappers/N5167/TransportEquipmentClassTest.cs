using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.Testing
{
	public sealed class TransportEquipmentClassTest : ITransportEquipment
	{
		public ZString CharacteristicCode => ZString.Empty;
		public ZString ID => "ctn no.123";
		public ZString UsedCapacityCode => ZString.Empty;
		public IEnumerable<ZString> Seals => null;
	}
}
