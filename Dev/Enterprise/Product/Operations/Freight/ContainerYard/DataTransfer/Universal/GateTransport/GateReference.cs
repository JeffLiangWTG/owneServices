using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.ContainerYard.Business
{
	sealed class GateReference : IReferencesParent
	{
		internal ZString? VehicleRegistrationNumber;
		internal List<ZString> ContainerNumbers;
		public GateReference()
		{
			ContainerNumbers = new List<ZString>();
		}
	}
}
