using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AES
{
	public class AESHeaderCommodityLine : AESPrintCommodityLine
	{
		public AESHeaderCommodityLine(CusEntryLine entryLine)
			: base(entryLine.Factory)
		{
			this.entryLine = entryLine;
		}
		internal CusEntryLine entryLine;

		IAESTIRCommodityLineItem CommodityLine
		{
			get { return entryLine; }
		}

		IAESTIRUsedVehicle UsedVehicleLine
		{
			get { return entryLine; }
		}

		#region Common Properties

		public override ZString LineNo
		{
			get { return CommodityLine.LineNumber.ToString(); }
		}

		public override ZString ExportCode
		{
			get { return CommodityLine.ExportInformationCode; }
		}

		public override ZString TariffNo
		{
			get { return CommodityLine.ScheduleBHTSNumber; }
		}

		public override ZString TariffDescription
		{
			get { return CommodityLine.CommodityDescription; }
		}

		public override ZDecimal Qty
		{
			get { return CommodityLine.Quantity1; }
		}

		public override ZString UQ
		{
			get { return CommodityLine.UnitOfMeasure1; }
		}

		public override ZDecimal GrossWt
		{
			get { return CommodityLine.ShippingWeight; }
		}

		public override ZDecimal Value
		{
			get { return CommodityLine.ValueOfGoods; }
		}

		public override ZString OriginIndicator
		{
			get { return CommodityLine.ForeignDomesticOriginIndicator; }
		}

		#endregion

		#region License/DDTC Details

		public override ZString LicenseType
		{
			get { return CommodityLine.LicenseCodeLicenseExemptionCode; }
		}

		public override ZString LicenseValue
		{
			get { return CommodityLine.LicenseValue.ToString(); }
		}

		public override ZString ExportLicenseNo
		{
			get { return CommodityLine.ExportLicenseNumberCFRCitationAuthorizationSymbolKCP; }
		}

		public override ZString ECCN
		{
			get { return CommodityLine.ExportControlClassificationNumberECCN; }
		}

		public override ZString ITARExemptionNo
		{
			get { return CommodityLine.DDTCITARExemptionNumber; }
		}

		public override ZString MilitaryEquipmentIndicator
		{
			get { return CommodityLine.DDTCSignificantMilitaryEquipmentSMEIndicator; }
		}

		public override ZString PartyCertificationIndicator
		{
			get { return CommodityLine.DDTCEligiblePartyCertificationIndicator; }
		}

		public override ZString RegistrationNo
		{
			get { return CommodityLine.DDTCRegistrationNumber; }
		}

		public override ZString USMLCategoryCode
		{
			get { return CommodityLine.DDTCUSMLCategoryCode; }
		}

		public override ZString USMLCategoryDescription
		{
			get { return entryLine.AddInfoLookups.US_USMLCategoryCodes.GetDescriptionFromCode(CommodityLine.DDTCUSMLCategoryCode); }
		}

		public override ZDecimal DDTCQuantity
		{
			get { return CommodityLine.DDTCQuantity; }
		}

		public override ZString DDTCUnitOfMeasure
		{
			get { return CommodityLine.DDTCUnitOfMeasureCode; }
		}

		#endregion

		#region Vehicle Details

		public override ZString VehicleIndicator
		{
			get { return entryLine.IsUsedVehicle ? "Yes" : "No"; }
		}

		public override ZString VehicleTitleNumber
		{
			get { return UsedVehicleLine.VehicleTitleNumber; }
		}

		public override ZString VehicleIDTypeDescription
		{
			get { return GetVehicleIDTypeDescription(UsedVehicleLine.VehicleIDQualifier); }
		}

		public override ZString VehicleTitleStateDescription
		{
			get { return GetStateDescription(UsedVehicleLine.VehicleTitleStateCode); }
		}

		public override ZString VehicleID
		{
			get { return UsedVehicleLine.VehicleIdentificationNumberVINProductID; }
		}

		#endregion

		protected override IEnumerable<MessageBlock> GetPGAMessageBlocksCore()
		{
			return ExportPGABlocksCreator.BuildPGABlocks(CommodityLine);
		}
	}
}
