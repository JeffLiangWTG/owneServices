using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedWarehouseLink : BondedWarehouseLink, Integration.IWhsBondedWarehouseLink
	{
		public WhsBondedWarehouseLink(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Nature 20s

		protected override void CreateOrUpdateInwardMovementCore(IWhsBondedWarehouseTransaction info)
		{
			WhsBondedTransactionProcessor processor = new WhsBondedTransactionProcessor(Factory, info);
			processor.Process();
		}

		protected override void UpdateDeclarationReferenceCore(ZGuid declarationPK, ZString declarationReference)
		{
			WhsBondedJobNumberUpdater jobNumberUpdater = new WhsBondedJobNumberUpdater(Factory);
			jobNumberUpdater.Update(declarationPK, declarationReference);
		}

		#endregion

		#region Nature 30s

		protected override IWhsBondedWarehouseTransaction CreateOrUpdateOutwardMovementCore(IWhsBondedWarehouseTransaction info, bool continueIfError)
		{
			return CreateOrUpdateOutwardMovement(Factory, info, continueIfError);
		}

		/// <summary>
		/// Uses a temp Factory so that no changes are made to the Warehouse Inventory System.
		/// </summary>
		protected override IWhsBondedWarehouseTransaction GetOutwardMovementDetailCore(IWhsBondedWarehouseTransaction info)
		{
			return CreateOrUpdateOutwardMovement(new BusinessObjectFactory(), info, continueIfError: false);
		}

		IWhsBondedWarehouseTransaction CreateOrUpdateOutwardMovement(BusinessObjectFactory factory, IWhsBondedWarehouseTransaction info, bool continueIfError)
		{
			var processor = new WhsBondedOutwardsProcessor();
			return processor.Process(factory, info, continueIfError);
		}

		protected override void CancelOutwardMovementCore(ZGuid declarationPK)
		{
			WhsOutwardsProcessorCancel processor = new WhsOutwardsProcessorCancel();
			processor.Cancel(Factory, declarationPK);
		}

		protected override void NotifyGoodsAreClearedForReleaseCore(ZGuid declarationPK, IWhsBondedWarehouseTransaction info)
		{
			WhsBondedExwEntryKeyUpdater entryKeyUpdater = new WhsBondedExwEntryKeyUpdater(Factory);
			entryKeyUpdater.Update(declarationPK, info);
		}

		#endregion

		#region Findboxes

		public override BusinessObjectCollection GetEntryKeyLookup(IWhsBondedWarehouseTransactionLine line)
		{
			BondedEntryKeyLookupCollection collection = new BondedEntryKeyLookupCollection(Factory);
			AddFilterBusinessObjectDefaultsToCollection(collection, line);
			return collection;
		}

		void AddFilterBusinessObjectDefaultsToCollection(WhsInventoryViewCollection collection, IWhsBondedWarehouseTransactionLine line)
		{
			AddProductFilterDefaultToCollection(collection, line);
			AddWarehouseFilterDefaultToCollection(collection, line);
			if (!line.PartAttrib1.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Bond ID 1", "Property", line.PartAttrib1));
			}

			if (!line.PartAttrib2.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Bond ID 2", "Property", line.PartAttrib2));
			}

			if (!line.EntryKey.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Entry Number", "Property", line.EntryKey));
			}

			if (line.EntryLineNumber > 0)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Entry Line Number", "Property", new ZString(line.EntryLineNumber.ToString())));
			}
		}

		public static class Schema
		{
			public const string WI_EntryKey = "WI_EntryKey";
			public const string WI_EntryLineNo = "WI_EntryLineNo";
		}

		void AddProductFilterDefaultToCollection(WhsInventoryViewCollection collection, IWhsBondedWarehouseTransactionLine line)
		{
			var product = (OrgSupplierPart)line.Product;
			if (product != null)
			{
				if (product.RelatedOrganisations.BuyerRelations.Length > 0) // TODO: pass importer on the interface
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer", "Property", product.RelatedOrganisations.BuyerRelations[0].OU_OH));
				}
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", line.Product.PK));
			}
		}

		void AddWarehouseFilterDefaultToCollection(WhsInventoryViewCollection collection, IWhsBondedWarehouseTransactionLine line)
		{
			if (line.Warehouse != null)
			{
				ZQuery filter = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, line.Warehouse.PK);
				WhsWarehouseCollection warehouseList = new WhsWarehouseCollection(Factory, filter);
				warehouseList.Load();
				if (warehouseList.Count > 0)
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", warehouseList[0].PK));
				}
			}
		}

		#endregion
	}
}
