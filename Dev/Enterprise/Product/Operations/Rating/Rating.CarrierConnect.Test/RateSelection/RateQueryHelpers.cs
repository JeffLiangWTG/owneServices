using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.CarrierConnect.Test
{
	public class RateChargeDtoAssertion : RateChargeDto
	{
		readonly HashSet<string> propertiesWithValues = new();

		public new Guid SourcePK
		{
			get => base.SourcePK;
			set
			{
				base.SourcePK = value;
				OnValueSet(nameof(SourcePK));
			}
		}

		public new ChargeCodeDto ChargeCode
		{
			get => base.ChargeCode;
			set
			{
				base.ChargeCode = value;
				OnValueSet(nameof(ChargeCode));
			}
		}

		public new string ChargeUnit
		{
			get => base.ChargeUnit;
			set
			{
				base.ChargeUnit = value;
				OnValueSet(nameof(ChargeUnit));
			}
		}

		public new string CalculationDescription
		{
			get => base.CalculationDescription;
			set
			{
				base.CalculationDescription = value;
				OnValueSet(nameof(CalculationDescription));
			}
		}

		public new CommodityDto Commodity
		{
			get => base.Commodity;
			set
			{
				base.Commodity = value;
				OnValueSet(nameof(Commodity));
			}
		}

		public new string ContainerType
		{
			get => base.ContainerType;
			set
			{
				base.ContainerType = value;
				OnValueSet(nameof(ContainerType));
			}
		}

		public new string ContainerQuality
		{
			get => base.ContainerQuality;
			set
			{
				base.ContainerQuality = value;
				OnValueSet(nameof(ContainerQuality));
			}
		}

		public new string LocalCurrency
		{
			get => base.LocalCurrency;
			set
			{
				base.LocalCurrency = value;
				OnValueSet(nameof(LocalCurrency));
			}
		}

		public new decimal LocalAmount
		{
			get => base.LocalAmount;
			set
			{
				base.LocalAmount = value;
				OnValueSet(nameof(LocalAmount));
			}
		}

		public new string RateCurrency
		{
			get => base.RateCurrency;
			set
			{
				base.RateCurrency = value;
				OnValueSet(nameof(RateCurrency));
			}
		}

		public new decimal RateAmount
		{
			get => base.RateAmount;
			set
			{
				base.RateAmount = value;
				OnValueSet(nameof(RateAmount));
			}
		}

		void OnValueSet(string propName)
		{
			propertiesWithValues.Add(propName);
		}

		public bool HasValue(string propName) => propertiesWithValues.Contains(propName);
	}

	public class RateResultDtoAssertion
	{
		public string[] Containers { get; set; }

		public string CarrierContractNumber { get; set; }

		public string CarrierServiceLevel { get; set; }

		public OrgHeader TransportProvider { get; set; }

		public IRateEntry[] RateEntries { get; set; }

		public IRateLine[] RateLines { get; set; }

		public ICollection<RateChargeDtoAssertion> Charges { get; set; } = Array.Empty<RateChargeDtoAssertion>();

		public PerContainerCommodityDto[] PerContainerCommodity { get; set; }

		public override string ToString()
		{
			return $"Entries: {string.Join(", ", RateEntries.Select(GetEntryName))}\nLines: {string.Join(", ", RateLines.Select(GetLineName))}";

			string GetEntryName(IRateEntry entry)
				=> $"{entry.TI_RateCategory}-{entry.TI_Mode}-{entry.TI_OriginLRC}-{entry.TI_ViaLRC}-{entry.TI_DestinationLRC}";

			string GetLineName(IRateLine line)
				=> $"{line.ChargeCode?.AC_Code}-{line.TL_RateCalculator}-{line.ParentRateEntry?.ParentRatingHeader?.TH_RateType}-{line.ParentRateEntry?.Container?.RC_Code}-{line.ParentRateEntry?.TI_RH_NKCommodityCode}";
		}
	}
}
