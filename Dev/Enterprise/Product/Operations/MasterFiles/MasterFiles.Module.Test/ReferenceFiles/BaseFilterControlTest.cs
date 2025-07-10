using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing;

public class BaseFilterControlTest : TestCaseWithFactory
{
	protected static void AssertColumn(ZDisplayGrid grid, string columnName)
	{
		var column = grid.Columns[columnName];
		AssertNotNull(columnName + " column should exist", column);
	}

	protected static void AssertNoColumn(ZDisplayGrid grid, string columnName)
	{
		var column = grid.Columns[columnName];
		AssertNull(columnName + " column should not exist", column);
	}

	protected CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
	{
		var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
		activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
		activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
		activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
		activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
		return activeTransportModes;
	}
}
