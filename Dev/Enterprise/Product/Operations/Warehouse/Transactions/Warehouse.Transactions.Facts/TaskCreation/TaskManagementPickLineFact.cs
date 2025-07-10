using System;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementPickLineFact : InputFactWithUserDefinedProperties, ITaskManagementPickLineFact
	{
		public TaskManagementPickLineFact(
			Guid pk,
			Guid availableInventorySplitPK,
			IWhsPick pick,
			IWhsPickAvailableInventory availableInventory,
			IOrganisationFact client,
			IProductFact product,
			ITaskManagementLocationFact location,
			string packUQ,
			string uomType,
			decimal quantity,
			int numberOfUnits,
			int numberOfPacks,
			decimal weight,
			string weightUQ,
			decimal volume,
			string volumeUQ)
		{
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(availableInventory, nameof(availableInventory));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(product, nameof(product));
			Argument.NotNull(location, nameof(location));

			PK = pk;
			AvailableInventorySplitPK = availableInventorySplitPK;
			Grouping = new FactJoin<ITaskManagementGroupingFact>(this);

			Quantity = quantity;

			Client = new FactJoin<IOrganisationFact>(client);
			Product = new FactJoin<IProductFact>(product);
			Location = new FactJoin<ITaskManagementLocationFact>(location);

			IsWorkOrder = pick.IsWorkOrderPick;
			IsCustomsTransaction = pick.IsCustomsTransaction;

			PalletID = availableInventory.PalletID;
			PartAttribute1 = availableInventory.PartAttrib1;
			PartAttribute2 = availableInventory.PartAttrib2;
			PartAttribute3 = availableInventory.PartAttrib3;
			SerialNumber = availableInventory.SerialNumber;
			ArrivalDate = availableInventory.ArrivalDate.ToDateTime();
			ExpiryDate = availableInventory.ExpiryDate.ConvertToNullableDateTime();
			PackingDate = availableInventory.PackingDate.ConvertToNullableDateTime();
			BondedEntryKey = availableInventory.BondedEntryKey;
			BondedEntryDate = availableInventory.BondedEntryDate.ConvertToNullableDateTime();
			PackUQ = Argument.NotNull(packUQ, nameof(packUQ));
			UOMType = Argument.NotNull(uomType, nameof(uomType));

			NumberOfUnits = numberOfUnits;
			NumberOfPacks = numberOfPacks;
			Weight = weight;
			WeightUQ = Argument.NotNull(weightUQ, nameof(weightUQ));
			Volume = volume;
			VolumeUQ = Argument.NotNull(volumeUQ, nameof(volumeUQ));

			SplitFactConstructor =
				(splitQuantity, splitUnits, splitPacks, splitWeight, splitVolume) =>
					new TaskManagementPickLineFact(
						Guid.NewGuid(),
						AvailableInventorySplitPK,
						pick,
						availableInventory,
						client,
						product,
						location,
						packUQ,
						uomType,
						splitQuantity,
						splitUnits,
						splitPacks,
						splitWeight,
						weightUQ,
						splitVolume,
						volumeUQ);
		}

		public Guid PK { get; }
		public Guid AvailableInventorySplitPK { get; }

		public decimal Quantity { get; private set; }

		public FactJoin<ITaskManagementGroupingFact> Grouping { get; }

		public FactJoin<IOrganisationFact> Client { get; }
		public FactJoin<IProductFact> Product { get; }
		public FactJoin<ITaskManagementLocationFact> Location { get; }

		public bool IsCustomsTransaction { get; }

		public bool IsWorkOrder { get; }

		public string PackUQ { get; }

		public string UOMType { get; }

		public string PalletID { get; }

		public string PartAttribute1 { get; }

		public string PartAttribute2 { get; }

		public string PartAttribute3 { get; }

		public string SerialNumber { get; }

		public string BondedEntryKey { get; }

		public DateTime ArrivalDate { get; }

		public DateTime? PackingDate { get; }

		public DateTime? ExpiryDate { get; }

		public DateTime? BondedEntryDate { get; }

		public int NumberOfLines => 1;

		public int NumberOfUnits { get; private set; }

		public int NumberOfPacks { get; private set; }

		public decimal Weight { get; private set; }

		public string WeightUQ { get; }

		public decimal Volume { get; private set; }

		public string VolumeUQ { get; }

		public Guid AssignedTask { get; set; }

		public ITaskManagementLineWithSplitSupportFact Split(int packs)
		{
			if (packs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(packs), packs, "Split packs must be greater than zero.");
			}
			else if (packs >= NumberOfPacks)
			{
				throw new ArgumentOutOfRangeException(nameof(packs), packs, $"Split packs must be less than total packs ({NumberOfPacks}).");
			}

			var packRatio = (decimal)packs / NumberOfPacks;
			var splitUnits = (int)(NumberOfUnits * packRatio);
			var splitWeight = Weight * packRatio;
			var splitVolume = Volume * packRatio;

			var splitFact = SplitFactConstructor(splitUnits, splitUnits, packs, splitWeight, splitVolume);

			Quantity -= splitUnits;
			NumberOfUnits -= splitUnits;
			NumberOfPacks -= packs;
			Weight -= splitWeight;
			Volume -= splitVolume;

			return splitFact;
		}

		Func<decimal, int, int, decimal, decimal, TaskManagementPickLineFact> SplitFactConstructor { get; }
	}
}
