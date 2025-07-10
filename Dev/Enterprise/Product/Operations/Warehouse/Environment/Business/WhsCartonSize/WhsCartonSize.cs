using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[CodeProperty(WhsCartonSizeSchema.Constants.WCS_Code), DescriptionProperty(WhsCartonSizeSchema.Constants.WCS_Code)]
	public class WhsCartonSize : AutoWhsCartonSize, IPackageTemplate
	{
		public WhsCartonSize(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region WCS_Length

		public override ZDecimal WCS_Length
		{
			get { return base.WCS_Length; }
			set
			{
				base.WCS_Length = value;

				CalculateVolume();
			}
		}

		#endregion

		#region WCS_Width

		public override ZDecimal WCS_Width
		{
			get { return base.WCS_Width; }
			set
			{
				base.WCS_Width = value;

				CalculateVolume();
			}
		}

		#endregion

		#region WCS_Height

		public override ZDecimal WCS_Height
		{
			get { return base.WCS_Height; }
			set
			{
				base.WCS_Height = value;

				CalculateVolume();
			}
		}

		#endregion

		#region WCS_Volume

		public override ZDecimal WCS_Volume
		{
			get => base.WCS_Volume;
			set
			{
				var linksToUpdate = Factory
					.Load<WhsCartonGroupSizeLink>(new ZQuery(WhsCartonGroupSizeLinkSchema.WCV_WCS, PK))
					.Where(l => l.CartonGroup.OptimizationMode == CartonizationOptimizationModes.Codes.MinimizeVolume)
					.ToArray();

				base.WCS_Volume = value;

				foreach (var link in linksToUpdate)
				{
					link.WCV_OptimizationCost = link.GetOptimizationCostBasedOnCartonSizeVolume();
				}
			}
		}

		#endregion

		#region WCS_DimensionUQ

		[List("Lookups.DimensionUQs")]
		public override ZString WCS_DimensionUQ
		{
			get { return base.WCS_DimensionUQ; }
			set
			{
				base.WCS_DimensionUQ = value;

				CalculateVolume();
			}
		}

		#endregion

		#region WCS_EmptyWeight

		public override ZDecimal WCS_EmptyWeight
		{
			get { return base.WCS_EmptyWeight; }
			set
			{
				base.WCS_EmptyWeight = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWCS_MaxWeight();
				}
			}
		}

		#endregion

		#region WCS_MaxWeight

		public override ZDecimal WCS_MaxWeight
		{
			get { return base.WCS_MaxWeight; }
			set
			{
				base.WCS_MaxWeight = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWCS_EmptyWeight();
				}
			}
		}

		#endregion

		#region WCS_WeightUQ

		[List("Lookups.WeightUQs")]
		public override ZString WCS_WeightUQ
		{
			get { return base.WCS_WeightUQ; }
			set { base.WCS_WeightUQ = value; }
		}

		#endregion

		#region WCS_VolumeUQ

		[List("Lookups.VolumeUQs")]
		public override ZString WCS_VolumeUQ
		{
			get { return base.WCS_VolumeUQ; }
			set
			{
				base.WCS_VolumeUQ = value;

				CalculateVolume();
			}
		}

		#endregion

		#region AttachedToACartonGroup

		public bool AttachedToACartonGroup
		{
			get { return Factory.LoadTop1<WhsCartonGroupSizeLink>(new ZQuery(WhsCartonGroupSizeLinkSchema.WCV_WCS, PK)) != null; }
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get { return !WCS_Code.IsEmpty ? WCS_Code : base.HumanReadableNameCore; }
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Delete

		public override void Delete()
		{
			foreach (var pivot in Factory.Load<WhsCartonGroupSizeLink>(new ZQuery(WhsCartonGroupSizeLinkSchema.WCV_WCS, PK)))
			{
				pivot.Delete();
			}

			base.Delete();
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WCS_MaxFillPercent = 80;
			WCS_MaxUnits = 999999;

			var defaultUnits = ObjectFactory.Get<IPackageDefaultUQs>();
			WCS_VolumeUQ = defaultUnits.DefaultVolumeUnit;
			WCS_DimensionUQ = defaultUnits.DefaultDimensionUnit;
			WCS_WeightUQ = defaultUnits.DefaultWeightUnit;
		}

		#endregion

		#region CalculateVolume

		void CalculateVolume()
		{
			if (CanCalculateVolume)
			{
				WCS_Volume = CalculatedVolume;
			}
		}

		public ZDecimal CalculatedVolume
		{
			get { return VolumeCalculator.Calculate(); }
		}

		public bool CanCalculateVolume
		{
			get { return IsDimensionUQValid && IsVolumeUQValid && IsDimensionsValid; }
		}

		bool IsVolumeUQValid
		{
			get { return !WCS_VolumeUQ.IsEmpty && Lookups.VolumeUQs.ContainsCode(WCS_VolumeUQ); }
		}

		bool IsDimensionsValid
		{
			get { return WCS_Length > 0 && WCS_Width > 0 && WCS_Height > 0; }
		}

		bool IsDimensionUQValid
		{
			get { return !WCS_DimensionUQ.IsEmpty && Lookups.DimensionUQs.ContainsCode(WCS_DimensionUQ); }
		}

		VolumeCalculator VolumeCalculator
		{
			get
			{
				return volumeCalculator ?? (volumeCalculator =
					new VolumeCalculator
					(
						(ZPropertyInfoDecimal)WCS_LengthInfo,
						(ZPropertyInfoDecimal)WCS_WidthInfo,
						(ZPropertyInfoDecimal)WCS_HeightInfo,
						(ZPropertyInfoString)WCS_DimensionUQInfo,
						(ZPropertyInfoString)WCS_VolumeUQInfo
					));
			}
		}

		VolumeCalculator volumeCalculator;

		#endregion

		#region IPackageTemplate Members

		ZString IPackageTemplate.DimensionUQ
		{
			get { return WCS_DimensionUQ; }
		}

		ZDecimal IPackageTemplate.Height
		{
			get { return WCS_Height; }
		}

		ZDecimal IPackageTemplate.Length
		{
			get { return WCS_Length; }
		}

		ZDecimal? IPackageTemplate.Volume
		{
			get { return WCS_Volume; }
		}

		ZString IPackageTemplate.VolumeUQ
		{
			get { return WCS_VolumeUQ; }
		}

		ZDecimal IPackageTemplate.TareWeight
		{
			get { return WCS_EmptyWeight; }
		}

		ZString IPackageTemplate.WeightUQ
		{
			get { return WCS_WeightUQ; }
		}

		ZDecimal IPackageTemplate.Width
		{
			get { return WCS_Width; }
		}

		#endregion
	}
}
