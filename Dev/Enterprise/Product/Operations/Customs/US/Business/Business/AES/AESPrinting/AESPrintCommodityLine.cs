using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AES
{
	public abstract class AESPrintCommodityLine :
		NonPersistentBusinessObject,
		IObsoleteValidation
	{
		public AESPrintCommodityLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public abstract ZString LineNo { get; }
		public abstract ZString ExportCode { get; }
		public abstract ZString TariffNo { get; }
		public abstract ZString TariffDescription { get; }
		public abstract ZDecimal Qty { get; }
		public abstract ZString UQ { get; }
		public abstract ZDecimal GrossWt { get; }
		public abstract ZDecimal Value { get; }
		public abstract ZString OriginIndicator { get; }
		public abstract ZString LicenseType { get; }
		public abstract ZString LicenseValue { get; }
		public abstract ZString VehicleIndicator { get; }
		public abstract ZString ExportLicenseNo { get; }
		public abstract ZString ECCN { get; }
		public abstract ZString ITARExemptionNo { get; }
		public abstract ZString MilitaryEquipmentIndicator { get; }
		public abstract ZString PartyCertificationIndicator { get; }
		public abstract ZString RegistrationNo { get; }
		public abstract ZString USMLCategoryCode { get; }
		public abstract ZString USMLCategoryDescription { get; }
		public abstract ZString VehicleTitleNumber { get; }
		public abstract ZString VehicleIDTypeDescription { get; }
		public abstract ZString VehicleTitleStateDescription { get; }
		public abstract ZString VehicleID { get; }
		public abstract ZDecimal DDTCQuantity { get; }
		public abstract ZString DDTCUnitOfMeasure { get; }

		public ZString DDTCQuantityWithUnitOfMeasure
		{
			get { return DDTCQuantity.ToString() + " " + DDTCUnitOfMeasure; }
		}

		public ZString ExportPGALines
		{
			get
			{
				if (!fExportPGALines.HasValue)
				{
					var stringBuilder = new ZStringBuilder();
					foreach (var pgaLine in AESPGALineGenerator.GeneratePGALines(GetPGAMessageBlocksCore()))
					{
						if (!pgaLine.IsEmpty)
						{
							stringBuilder.AppendLine(pgaLine);
						}
					}
					fExportPGALines = stringBuilder.ToString();
				}

				return fExportPGALines.Value;
			}
		}
		ZString? fExportPGALines;

		protected abstract IEnumerable<MessageBlock> GetPGAMessageBlocksCore();

		public ZString GrossWtUQ
		{
			get { return "KG"; }
		}

		public ZString LicenseTypeDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!LicenseType.IsEmpty)
				{
					var description = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, LicenseType.Trim(), Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
					if (!description.IsEmpty)
					{
						result = ExportLicenseNo + "  " + description + " (" + LicenseType.Trim() + ")";
					}
				}
				return result;
			}
		}

		public ZString GetUSMLCategoryDescription(ZString code)
		{
			return Factory.GetCachedValue<USMLCategoryCodes>().GetDescriptionFromCode(code);
		}

		public ZString GetVehicleIDTypeDescription(ZString code)
		{
			return Factory.GetCachedValue<VehicleIDTypeList>().GetDescriptionFromCode(code);
		}

		public ZString GetStateDescription(string stateCode)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(stateCode))
			{
				var description = (ZString)USStatesForVehiclesList.GetDescriptionFromCode(stateCode);
				result = !description.IsEmpty ? description + " (" + stateCode + ")" : stateCode;
			}
			return result;
		}

		CodeDescriptionPairList USStatesForVehiclesList
		{
			get { return Factory.GetCachedUSStateListForVehicles(); }
		}
	}
}
