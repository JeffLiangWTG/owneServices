using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI
{
	public interface IBondedWarehouseMenuItemsCreatorSupporter
	{
		bool TopLevelBusinessObjectHasChanges();

		BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter);

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		ContinueWithSave FireSaveButton();
	}

	public interface IBondedWarehouseMenuItemsForGridCreatorSupporter : IBondedWarehouseMenuItemsCreatorSupporter
	{
		bool IsEnabled { get; }

		ZGrid GetGrid();
		IDeclarationWarehouseIntegrationSupporter GetSupporter(object data);
	}
}
