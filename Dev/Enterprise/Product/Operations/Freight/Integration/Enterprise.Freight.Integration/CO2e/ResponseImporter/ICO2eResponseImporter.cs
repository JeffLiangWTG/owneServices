using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Integration
{
	public interface ICO2eResponseImporter
	{
		string LogMessages { get; }

		void ImportGreenHouseGasEmission(ITopLevelDataObject universalShipment, ICO2eCalculationSupporter supporter, string name = default, (decimal TotalCO2e, IEnumerable<BusinessObject> Transports) previousCO2eValue = default);
	}
}
