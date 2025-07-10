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
	public class DummyCartonisableItemDefinition : NonPersistentBusinessObject, ICartonisableItemDefinition
	{
		public DummyCartonisableItemDefinition()
		{
			ProductName = "";
			DimensionUQ = "";
			VolumeUQ = "";
			WeightUQ = "";
		}

		public DummyCartonisableItemDefinition(decimal height, decimal length, decimal width, decimal volume, decimal weight, string dimensionUQ, string volumeUQ, string weightUQ, bool keepUpright)
		{
			Height = height;
			Length = length;
			Width = width;
			DimensionUQ = dimensionUQ;
			Volume = volume;
			VolumeUQ = volumeUQ;
			Weight = weight;
			WeightUQ = weightUQ;
			KeepUpright = keepUpright;
		}

		#region Properties for binding

		public ZString ProductName { get; set; }
		public ZDecimal Height { get; set; }
		public ZDecimal Length { get; set; }
		public ZDecimal Width { get; set; }
		public ZDecimal Volume { get; set; }
		public ZDecimal Weight { get; set; }

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

		#region KeepUpright

		public ZBool KeepUpright
		{
			get { return keepUpright; }
			set
			{
				keepUpright = value;
				KeepUprightInfo.RefreshBinding();
			}
		}
		ZBool keepUpright;

		public ZPropertyInfo KeepUprightInfo
		{
			get { return GetZPropertyInfo(nameof(KeepUpright)); }
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

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} ({1}x{2}x{3} {4}, {5}{6})", ProductName, Height, Width, Length, DimensionUQ, Weight, WeightUQ);
		}

		#region ICartonisableItemDefinition explicit implementation

		Guid ICartonisableItemDefinition.PK => StringToGuidMapping.GetGuidForString(ProductName);

		decimal ICartonisableItemDefinition.Height => Height;

		decimal ICartonisableItemDefinition.Length => Length;

		decimal ICartonisableItemDefinition.Width => Width;

		string ICartonisableItemDefinition.DimensionUQ => DimensionUQ;
		decimal ICartonisableItemDefinition.Volume => Volume;

		string ICartonisableItemDefinition.VolumeUQ => VolumeUQ;

		decimal ICartonisableItemDefinition.Weight => Weight;

		string ICartonisableItemDefinition.WeightUQ => WeightUQ;

		bool ICartonisableItemDefinition.KeepUpright => KeepUpright;

		#endregion
	}
}

