using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class DummyCartonisableItem : NonPersistentBusinessObject, ICartonisableItem
	{
		#region Constructors

		public DummyCartonisableItem() // empty constructor required for Grid in GUI
		{
			PK = Guid.NewGuid();
			Quantity = 1;
			ItemDefinition = new DummyCartonisableItemDefinition();
		}

		public DummyCartonisableItem(SerialisableItemToPack item) // this constructor is used after deserialisation
		{
			PK = Guid.NewGuid();
			Quantity = item.Quantity;
			Location = item.Location;
			ItemDefinition = new DummyCartonisableItemDefinition();
			ItemDefinition.ProductName = item.ProductName;
			ItemDefinition.Height = item.Height;
			ItemDefinition.Length = item.Length;
			ItemDefinition.Width = item.Width;
			ItemDefinition.DimensionUQ = item.DimensionUQ;
			ItemDefinition.Volume = item.Volume;
			ItemDefinition.VolumeUQ = item.VolumeUQ;
			ItemDefinition.Weight = item.Weight;
			ItemDefinition.WeightUQ = item.WeightUQ;
			ItemDefinition.KeepUpright = item.KeepUpright;
		}

		public DummyCartonisableItem(Guid orderLinePK, decimal qty, ICartonisableItemDefinition product)
		{
			PK = orderLinePK;
			OriginalPK = orderLinePK;
			Quantity = qty;

			ItemDefinition = (DummyCartonisableItemDefinition)product;
		}

		#endregion

		public Guid OriginalPK { get; set; }
		public new ZGuid PK { get; set; }
		public ZString Location { get; set; }
		public ZDecimal Quantity { get; set; }
		public DummyCartonisableItemDefinition ItemDefinition { get; private set; }

		#region ToString

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} x {1}", Quantity, ItemDefinition);
		}

		#endregion

		ICartonisableItemDefinition ICartonisableItem.ItemDefinition => ItemDefinition;

		Guid ICartonisableItem.PK => PK.ToGuid();

		Guid ICartonisableItem.LocationPK => StringToGuidMapping.GetGuidForString(Location);

		decimal ICartonisableItem.Quantity => Quantity;
	}
}

