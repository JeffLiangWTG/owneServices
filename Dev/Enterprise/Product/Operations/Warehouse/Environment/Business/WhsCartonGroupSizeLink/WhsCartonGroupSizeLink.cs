using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsCartonGroupSizeLink : AutoWhsCartonGroupSizeLink, ICartonDefinition
	{
		public WhsCartonGroupSizeLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region WCV_WCG

		[RelatedBusinessObject("CartonGroup")]
		public override ZGuid WCV_WCG
		{
			get { return base.WCV_WCG; }
			set { base.WCV_WCG = value; }
		}

		#endregion

		#region WCV_WCS

		[RelatedBusinessObject("CartonSize")]
		public override ZGuid WCV_WCS
		{
			get { return base.WCV_WCS; }
			set { base.WCV_WCS = value; }
		}

		#endregion

		#region WCV_OptimizationCost

		[ReadOnlyMember(nameof(IsOptimizationCostReadOnly))]
		public override ZInt WCV_OptimizationCost
		{
			get => base.WCV_OptimizationCost;
			set => base.WCV_OptimizationCost = value;
		}

		bool IsOptimizationCostReadOnly => (CartonGroup?.OptimizationMode ?? ZString.Empty) != CartonizationOptimizationModes.Codes.CustomOptimizationCosts;

		#endregion

		#region GetOptimizationCostBasedOnCartonSizeVolume

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public int GetOptimizationCostBasedOnCartonSizeVolume()
		{
			var cost = 1;
			var cartonSize = CartonSize;

			if (cartonSize != null)
			{
				var convertedVolume = GetVolumeInCC(cartonSize);
				cost = Math.Max(1, (int)Math.Min(int.MaxValue, Math.Round(convertedVolume)));
			}

			return cost;
		}

		decimal GetVolumeInCC(WhsCartonSize cartonSize) => Constants.Volume.Convert(cartonSize.WCS_Volume, cartonSize.WCS_VolumeUQ, Constants.Volume.CubicCentimeters);

		#endregion

		#region Related Business Objects

		#region CartonGroup

		public WhsCartonGroup CartonGroup
		{
			get { return Factory.Load<WhsCartonGroup>(WCV_WCG); }
		}

		#endregion

		#region CartonSize

		public WhsCartonSize CartonSize
		{
			get { return Factory.Load<WhsCartonSize>(WCV_WCS); }
		}

		#endregion

		#endregion

		#region ICartonDefinition Members

		Guid ICartonDefinition.PK => CartonSize.PK.ToGuid();

		decimal ICartonDefinition.Height => CartonSize.WCS_Height;

		decimal ICartonDefinition.Length => CartonSize.WCS_Length;

		decimal ICartonDefinition.Width => CartonSize.WCS_Width;

		string ICartonDefinition.DimensionUQ => CartonSize.WCS_DimensionUQ;

		decimal ICartonDefinition.Volume => CartonSize.WCS_Volume;

		string ICartonDefinition.VolumeUQ => CartonSize.WCS_VolumeUQ;

		decimal ICartonDefinition.MaxFillPercent => CartonSize.WCS_MaxFillPercent / 100m;

		decimal ICartonDefinition.MaxNumberOfUnits => CartonSize.WCS_MaxUnits;

		decimal ICartonDefinition.MaxWeight => CartonSize.WCS_MaxWeight;

		decimal ICartonDefinition.EmptyWeight => CartonSize.WCS_EmptyWeight;

		string ICartonDefinition.WeightUQ => CartonSize.WCS_WeightUQ;

		decimal ICartonDefinition.Cost
		{
			get
			{
				var minimiseVolume = CartonGroup.OptimizationMode == CartonizationOptimizationModes.Codes.MinimizeVolume;
				return minimiseVolume ? GetVolumeInCC(CartonSize) : WCV_OptimizationCost;
			}
		}

		#endregion
	}
}
