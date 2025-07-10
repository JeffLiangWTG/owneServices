using System;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	[Serializable]
	public class SerializableCartonDefinition
	{
		public SerializableCartonDefinition() { }
		public SerializableCartonDefinition(DummyCartonDefinition carton)
		{
			PK = carton.PK.ToGuid();
			CartonName = carton.CartonName;
			Height = carton.Height;
			Length = carton.Length;
			Width = carton.Width;
			Volume = carton.Volume;
			VolumeUQ = carton.VolumeUQ;
			DimensionUQ = carton.DimensionUQ;
			MaxFillPercent = carton.MaxFillPercent;
			MaxNumberOfUnits = carton.MaxNumberOfUnits;
			MaxWeight = carton.MaxWeight;
			EmptyWeight = carton.EmptyWeight;
			WeightUQ = carton.WeightUQ;
			Cost = carton.Cost;
		}
		public Guid PK { get; set; }
		public string CartonName { get; set; }
		public decimal Height { get; set; }
		public decimal Length { get; set; }
		public decimal Width { get; set; }
		public decimal Volume { get; set; }
		public string VolumeUQ { get; set; }
		public string DimensionUQ { get; set; }
		public decimal MaxFillPercent { get; set; }
		public decimal MaxNumberOfUnits { get; set; }
		public decimal MaxWeight { get; set; }
		public decimal EmptyWeight { get; set; }
		public string WeightUQ { get; set; }
		public int Cost { get; set; }
	}
}

