using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PalletLabelAutoPrinter
	{
		public PalletLabelAutoPrinter(WhsInventoryView inventory)
		{
			Inventory = Argument.NotNull(inventory, "inventory");
		}

		readonly WhsInventoryView Inventory;
		StmMenuItem DocumentToPrint => Inventory.Factory.Load<StmMenuItem>(PalletLabelsGuid);
		readonly Guid PalletLabelsGuid = new Guid("DB1EE942-E732-429D-8BAD-5A82E1764BE3"); // Pallet Labels Document GUID

		#region PrintPalletLabel

		public void PrintPalletLabel(Guid printerPK, int numberOfLabelsToPrint)
		{
			if (Inventory.Factory.Load<IStmPrintQueue>(printerPK) != null)
			{
				Print(Inventory, DocumentToPrint, printerPK, numberOfLabelsToPrint);
			}
			else
			{
				OnPrintFailed(Res.GetString("72413CC4-A8C7-4CB3-AC52-1D375B11ADD5", "Invalid Printer provided."));
			}
		}

		void Print(IDocumentSupportable documentSupportable, StmMenuItem document, ZGuid printerPK, int numberOfLabelsToPrint)
		{
			if (document != null)
			{
				var showNotification = false;
				var printer = ObjectFactory.New<IDocumentPrinter>(showNotification);
				printer.Print(document.PK, printerPK, documentSupportable, numberOfLabelsToPrint);
			}
			else
			{
				OnPrintFailed(Res.GetString("CFEC620E-920F-46BA-88E2-FED562D1CFC9", "Pallet Labels document does not exist."));
			}
		}

		#endregion

		#region PrintFailed

		public event EventHandler<PrintFailedEventArgs> PrintFailed;

		void OnPrintFailed(ZString message)
		{
			PrintFailed?.Invoke(this, new PrintFailedEventArgs(message));
		}

		#endregion
	}
}
