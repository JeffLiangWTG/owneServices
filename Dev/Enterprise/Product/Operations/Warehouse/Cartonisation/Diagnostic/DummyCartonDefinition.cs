using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class DummyCartonDefinition : NonPersistentBusinessObject, ICartonDefinition
	{
		#region Constructors

		public DummyCartonDefinition() // empty constructor required for Grid in GUI
		{
			DimensionUQ = "";
			VolumeUQ = "";
			WeightUQ = "";
			Cost = 1;
		}

		public DummyCartonDefinition(SerializableCartonDefinition carton) // this constructor is used after deserialisation
		{
			CartonName = carton.CartonName;
			Height = carton.Height;
			Length = carton.Length;
			Width = carton.Width;
			DimensionUQ = carton.DimensionUQ;
			Volume = carton.Volume;
			VolumeUQ = carton.VolumeUQ;
			MaxFillPercent = carton.MaxFillPercent;
			MaxNumberOfUnits = carton.MaxNumberOfUnits;
			MaxWeight = carton.MaxWeight;
			WeightUQ = carton.WeightUQ;
			EmptyWeight = carton.EmptyWeight;
			Cost = carton.Cost;
		}

		public DummyCartonDefinition(decimal length, decimal width, decimal height, decimal maxWeight, decimal emptyWeight, decimal maxVolume, decimal maxNumberOfUnits, decimal maxFillPercent, string dimensionUQ, string volumeUQ, string weightUQ, int cost)
		{
			Height = height;
			Length = length;
			Width = width;
			DimensionUQ = dimensionUQ;
			Volume = maxVolume;
			VolumeUQ = volumeUQ;
			MaxFillPercent = maxFillPercent;
			MaxNumberOfUnits = maxNumberOfUnits;
			MaxWeight = maxWeight;
			WeightUQ = weightUQ;
			EmptyWeight = emptyWeight;
			Cost = cost;
		}

		#endregion

		#region Properties for binding

		public ZString CartonName { get; set; }
		public ZDecimal Height { get; set; }
		public ZDecimal Length { get; set; }
		public ZDecimal Width { get; set; }
		public ZDecimal Volume { get; set; }
		public ZDecimal MaxFillPercent { get; set; }
		public ZDecimal MaxNumberOfUnits { get; set; }
		public ZDecimal MaxWeight { get; set; }
		public ZDecimal EmptyWeight { get; set; }
		public ZInt Cost { get; set; }

		#region VolumeUQ

		[List("VolumeUQList")]
		public ZString VolumeUQ
		{
			get { return volumeUQ; }
			set
			{
				volumeUQ = value;
				VolumeUQInfo.RefreshBinding();
			}
		}
		ZString volumeUQ;

		public ZPropertyInfo VolumeUQInfo
		{
			get { return GetZPropertyInfo(nameof(VolumeUQ)); }
		}

		#endregion

		#region DimensionUQ

		[List("DimensionUQList")]
		public ZString DimensionUQ
		{
			get { return dimensionUQ; }
			set
			{
				dimensionUQ = value;
				DimensionUQInfo.RefreshBinding();
			}
		}

		ZString dimensionUQ;

		public ZPropertyInfo DimensionUQInfo
		{
			get { return GetZPropertyInfo(nameof(DimensionUQ)); }
		}

		#endregion

		#region WeightUQ

		[List("WeightUQList")]
		public ZString WeightUQ
		{
			get { return weightUQ; }
			set
			{
				weightUQ = value;
				WeightUQInfo.RefreshBinding();
			}
		}

		ZString weightUQ;

		public ZPropertyInfo WeightUQInfo
		{
			get { return GetZPropertyInfo(nameof(WeightUQ)); }
		}

		#endregion

		#endregion

		#region List for binding to Diagnostics form

		public List<ICodeDescription> VolumeUQList
		{
			get { return UQHelper.VolumeUQList; }
		}

		public List<ICodeDescription> WeightUQList
		{
			get { return UQHelper.WeightUQList; }
		}

		public List<ICodeDescription> DimensionUQList
		{
			get { return UQHelper.DimensionUQList; }
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} ({1}x{2}x{3} {4}  {5}{6}) Max Weight = {7}{8} Cost = {9}", CartonName, Height, Width, Length, DimensionUQ, Volume, VolumeUQ, MaxWeight, WeightUQ, Cost);
		}

		#endregion

		#region ICartonDefinition explicit implementation

		Guid ICartonDefinition.PK
		{
			get { return base.PK.ToGuid(); }
		}

		decimal ICartonDefinition.Height
		{
			get { return Height; }
		}

		decimal ICartonDefinition.Length
		{
			get { return Length; }
		}

		decimal ICartonDefinition.Width
		{
			get { return Width; }
		}

		string ICartonDefinition.DimensionUQ
		{
			get { return DimensionUQ; }
		}

		decimal ICartonDefinition.Volume
		{
			get { return Volume; }
		}

		string ICartonDefinition.VolumeUQ
		{
			get { return VolumeUQ; }
		}

		decimal ICartonDefinition.MaxFillPercent
		{
			get { return MaxFillPercent; }
		}

		decimal ICartonDefinition.MaxNumberOfUnits
		{
			get { return MaxNumberOfUnits; }
		}

		decimal ICartonDefinition.MaxWeight
		{
			get { return MaxWeight; }
		}

		decimal ICartonDefinition.EmptyWeight
		{
			get { return EmptyWeight; }
		}

		string ICartonDefinition.WeightUQ
		{
			get { return WeightUQ; }
		}

		decimal ICartonDefinition.Cost
		{
			get { return Cost; }
		}

		#endregion
	}
}

