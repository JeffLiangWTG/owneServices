using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsLineAddInfoCommercialCharge : IWarehouseCustomsLineAddInfo
	{
		public WarehouseCustomsLineAddInfoCommercialCharge(CommercialCharge charge)
		{
			this.charge = Argument.NotNull(charge, nameof(charge));
		}
		public static class Constants
		{
			public const string Type = "CCT";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Add info key string")]
			public static class AddInfoKeys
			{
				public const string ChargeType = "ChargeType";
				public const string Currency = "Currency";
				public const string Amount = "Amount";
				public const string IsDutiable = "IsDutiable";
				public const string IsGSTApplicable = "IsGSTApplicable";
				public const string IsIncludedInITOT = "IsIncludedInITOT";
				public const string IsStatisticalValueApplicable = "IsStatisticalValueApplicable";
			}
		}

		readonly CommercialCharge charge;

		ZString IWarehouseCustomsLineAddInfo.AddInfoData
		{
			get
			{
				if (!addInfoData.HasValue)
				{
					addInfoData = CreateAddInfoData();
				}
				return addInfoData.Value;
			}
		}
		ZString? addInfoData;

		ZString CreateAddInfoData()
		{
			var dictionary = new Dictionary<ZString, ZString>();
			dictionary.Add(Constants.AddInfoKeys.Amount, Business.BaseAddInfo.GetStringRepresentation(charge.Amount.GetValueOrDefault()));
			dictionary.Add(Constants.AddInfoKeys.ChargeType, charge.ChargeType.GetCodeAsUpperCase());
			dictionary.Add(Constants.AddInfoKeys.Currency, charge.Currency.GetCodeAsUpperCase());
			dictionary.Add(Constants.AddInfoKeys.IsDutiable, Business.BaseAddInfo.GetStringRepresentation(charge.IsDutiable.GetValueOrDefault()));
			dictionary.Add(Constants.AddInfoKeys.IsGSTApplicable, Business.BaseAddInfo.GetStringRepresentation(charge.IsGSTApplicable.GetValueOrDefault()));
			dictionary.Add(Constants.AddInfoKeys.IsIncludedInITOT, Business.BaseAddInfo.GetStringRepresentation(charge.IsIncludedInITOT.GetValueOrDefault()));
			dictionary.Add(Constants.AddInfoKeys.IsStatisticalValueApplicable, Business.BaseAddInfo.GetStringRepresentation(charge.IsStatisticalValueApplicable.GetValueOrDefault()));
			return AddInfoParser.Serialise(dictionary);
		}

		ZString IWarehouseCustomsLineAddInfo.Type => Constants.Type;

		ZString IWarehouseCustomsLineAddInfo.NAddInfoData => ZString.Empty;
	}
}
