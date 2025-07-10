using System;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class CycleCountLocationFact : InputFactWithUserDefinedProperties, ICycleCountLocationFact
	{
		public CycleCountLocationFact(
			ICycleCountLocationCoreFact location,
			IOrganisationFact client,
			ICycleCountProductFact product,
			decimal stockOnHandForThisProduct)
		{
			PK = Guid.NewGuid();
			WrappedLocation = new FactJoin<ICycleCountLocationCoreFact>(Argument.NotNull(location, nameof(location)));
			Client = new FactLeftJoin<IOrganisationFact>(client);
			Product = new FactLeftJoin<ICycleCountProductFact>(product);
			StockOnHandForThisProduct = stockOnHandForThisProduct;
		}

		public FactJoin<ICycleCountLocationCoreFact> WrappedLocation { get; }

		public FactLeftJoin<IOrganisationFact> Client { get; }

		public FactLeftJoin<ICycleCountProductFact> Product { get; }

		public decimal StockOnHandForThisProduct { get; }

		public Guid PK { get; }

		public Guid LocationPK => WrappedLocation.Fact.PK;

		public string LocationTypeCode => WrappedLocation.Fact.LocationTypeCode;

		public string LocationClass => WrappedLocation.Fact.LocationClass;

		public string AreaName => WrappedLocation.Fact.AreaName;

		public string RowName => WrappedLocation.Fact.RowName;

		public string LocationStatus => WrappedLocation.Fact.LocationStatus;

		public int Column => WrappedLocation.Fact.Column;

		public int Level => WrappedLocation.Fact.Level;

		public int Tray => WrappedLocation.Fact.Tray;

		public bool CycleCountTaskExists
		{
			get => WrappedLocation.Fact.CycleCountTaskExists;
			set => WrappedLocation.Fact.CycleCountTaskExists = value;
		}

		public int CycleCountPathSequence => WrappedLocation.Fact.CycleCountPathSequence;

		public int RowPathSequence => WrappedLocation.Fact.RowPathSequence;

		public string PickMethod => WrappedLocation.Fact.PickMethod;

		public string LocationString => WrappedLocation.Fact.LocationString;

		public int LocationStringSortIndex => WrappedLocation.Fact.LocationStringSortIndex;

		public DateTime? InventoryLastChangedDate => WrappedLocation.Fact.InventoryLastChangedDate;

		public DateTime? CycleCountLastPerformedDate => WrappedLocation.Fact.CycleCountLastPerformedDate;

		public decimal StockOnHand => WrappedLocation.Fact.StockOnHand;

		public bool HasCommittedStock => WrappedLocation.Fact.HasCommittedStock;
	}
}
