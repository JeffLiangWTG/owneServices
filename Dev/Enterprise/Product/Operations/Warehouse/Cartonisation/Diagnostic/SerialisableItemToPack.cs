using System;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	[Serializable]
	public class SerialisableItemToPack
	{
		public decimal Quantity { get; set; }
		public string ProductName { get; set; }
		public string Location { get; set; }
		public decimal Height { get; set; }
		public decimal Length { get; set; }
		public decimal Width { get; set; }
		public string DimensionUQ { get; set; }
		public decimal Volume { get; set; }
		public string VolumeUQ { get; set; }
		public decimal Weight { get; set; }
		public string WeightUQ { get; set; }
		public bool KeepUpright { get; set; }

		public SerialisableItemToPack()
		{
		}

		public SerialisableItemToPack(DummyCartonisableItem dummy)
		{
			Quantity = dummy.Quantity;
			ProductName = dummy.ItemDefinition.ProductName;
			Location = dummy.Location;
			Height = dummy.ItemDefinition.Height;
			Length = dummy.ItemDefinition.Length;
			Width = dummy.ItemDefinition.Width;
			DimensionUQ = dummy.ItemDefinition.DimensionUQ;
			Volume = dummy.ItemDefinition.Volume;
			VolumeUQ = dummy.ItemDefinition.VolumeUQ;
			Weight = dummy.ItemDefinition.Weight;
			WeightUQ = dummy.ItemDefinition.WeightUQ;
			KeepUpright = dummy.ItemDefinition.KeepUpright;
		}
	}
}

