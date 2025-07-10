using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ConsolExportAWBHeader), "AWBRateLines")]
	public class ConsolExportAWBRateLine : ExportAWBRateLine
	{
		public ConsolExportAWBRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		protected override Forwarding.AWB.Business.ExportAWBRateLineValidation GetNewValidation()
		{
			return new ConsolExportAWBRateLineValidation(this);
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();

			if (!ER_ChargeableWeight.IsWithinSqlPrecisionAndScale(9, 3))
			{
				ErrorReporter.ReportOnce("Trying to save invalidated Chargeable Weight", FormattableString.Invariant($"Stacktrace: {invalidChargeableWeightStackTrace}"));
			}
		}

		public override ZString ER_RateClass
		{
			get { return base.ER_RateClass; }
			set
			{
				base.ER_RateClass = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateER_NatureAndQtyOfGoodsType();
					Validation.ValidateER_CommodityItemNumber();

					if (Master != null)
					{
						var otherULDAdditionalInfoLines = Master.AWBRateLines
																.Cast<ConsolExportAWBRateLine>()
																.Except(new[] { this })
																.Where(x => x.ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation);

						foreach (var item in otherULDAdditionalInfoLines)
						{
							item.Validation.ValidateER_RateClass();
						}
					}
				}
			}
		}

		[List("ER_CommodityItemNumberList")]
		public override ZString ER_CommodityItemNumber
		{
			get { return base.ER_CommodityItemNumber; }
			set { base.ER_CommodityItemNumber = value; }
		}

		protected override int GetER_CommodityItemNumber_MaxLength()
		{
			return ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation
					? 3
					: base.GetER_CommodityItemNumber_MaxLength();
		}

		public override ZDecimal ER_Total
		{
			get { return base.ER_Total; }
			set
			{
				base.ER_Total = value;
				if (Master != null)
				{
					Master.Validation.ValidateEH_WeightPrepaidCollect();
					Master.Validation.ValidateEH_OtherPrepaidCollect();
				}
			}
		}

		public override bool RequireHSCode
		{
			get
			{
				if (Master == null)
				{
					return false;
				}

				if (Master.HasInboundToICS2Zone || Master.IsImportToExportFromTransitingThroughUnitedArabEmirates)
				{
					return true;
				}

				var destinationCountryCode = Master.DestinationCountryCode.ToString();
				return destinationCountryCode == Constants.CountryCodes.Indonesia || destinationCountryCode == Constants.CountryCodes.Argentina;
			}
		}

		public override bool IsHSCodeLine
		{
			get
			{
				return ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			}
		}

		public override ZDecimal ER_ChargeableWeight
		{
			get => base.ER_ChargeableWeight;
			set
			{
				base.ER_ChargeableWeight = value;

				if (!value.IsWithinSqlPrecisionAndScale(9, 3))
				{
					invalidChargeableWeightStackTrace = System.Environment.StackTrace;
				}
			}
		}

		string invalidChargeableWeightStackTrace;

		[SuppressWeaklyTypedCollectionMessage]
		public IList ER_CommodityItemNumberList
		{
			get
			{
				switch (ER_RateClass)
				{
					case Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation:
						return Factory.GetCachedValue("ConsolExportAWBRateLine.ER_CommodityItemNumberListForULDAdditionalInfoLine", () => ContainerIATARateClassList.GetListForAWB());
					case Constants.AWB.RateClass.UnitLoadDeviceBasicCharge:
					case Constants.AWB.RateClass.SpecificCommodityRate:
						return new IATACommodityCodeCollection(Factory);
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoodsLithiumBattery GetNatureAndQtyOfGoodsLithiumBattery()
		{
			return new ConsolNatureAndQtyOfGoodsLithiumBattery(this);
		}
	}
}
