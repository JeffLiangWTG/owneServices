using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;

namespace Enterprise.Customs.US.AES
{
	public class AESTIRMessageCommodityLine : AESPrintCommodityLine
	{
		public AESTIRMessageCommodityLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AESCommShipCL1XP CL1Block
		{
			get;
			set;
		}

		public AESCommShipCL2XP CL2Block
		{
			get;
			set;
		}

		public AESCommShipODTXP ODTBlock
		{
			get;
			set;
		}

		public AESCommShipEV1XP EV1Block
		{
			get;
			set;
		}

		public List<AESCommShipPGAXP> PGABlock
		{
			get
			{
				if (pGABlock == null)
				{
					pGABlock = new List<AESCommShipPGAXP>();
				}
				return pGABlock;
			}
		}
		List<AESCommShipPGAXP> pGABlock;

		#region Common Details

		public override ZString LineNo
		{
			get { return CL1Block != null ? CL1Block.LineNumber.ToString() : ""; }
		}

		public override ZString ExportCode
		{
			get { return CL1Block != null ? CL1Block.ExportInformationCode : ZString.Empty; }
		}

		public override ZString TariffNo
		{
			get { return CL2Block != null ? CL2Block.ScheduleBHTSNumber : ZString.Empty; }
		}

		public override ZString TariffDescription
		{
			get { return CL1Block != null ? CL1Block.CommodityDescription : ZString.Empty; }
		}

		public override ZDecimal Qty
		{
			get { return CL2Block != null ? CL2Block.Quantity1 : ZDecimal.Zero; }
		}

		public override ZString UQ
		{
			get { return CL2Block != null ? CL2Block.UnitOfMeasure1 : ZString.Empty; }
		}

		public override ZDecimal GrossWt
		{
			get { return CL2Block != null ? CL2Block.ShippingWeight : ZDecimal.Zero; }
		}

		public override ZDecimal Value
		{
			get { return CL2Block != null ? (ZDecimal)decimal.Round(CL2Block.ValueOfGoods, 0) : ZDecimal.Zero; }
		}

		public override ZString OriginIndicator
		{
			get { return CL1Block != null ? CL1Block.ForeignDomesticOriginIndicator : ZString.Empty; }
		}

		#endregion

		#region License/DDTC Details

		public override ZString LicenseType
		{
			get { return CL1Block != null ? CL1Block.LicenseCodeLicenseExemptionCode : ZString.Empty; }
		}

		public override ZString LicenseValue
		{
			get { return CL1Block != null ? CL1Block.LicenseValue.ToString() : string.Empty; }
		}

		public override ZString ExportLicenseNo
		{
			get { return CL2Block != null ? CL2Block.ExportLicenseNumberCFRCitationAuthorizationSymbolKCPACM : ZString.Empty; }
		}

		public override ZString ECCN
		{
			get { return CL2Block != null ? CL2Block.ExportControlClassificationNumberECCN : ZString.Empty; }
		}

		public override ZString ITARExemptionNo
		{
			get { return ODTBlock != null ? ODTBlock.DDTCITARExemptionNumber : ZString.Empty; }
		}

		public override ZString MilitaryEquipmentIndicator
		{
			get { return ODTBlock != null ? ODTBlock.DDTCSignificantMilitaryEquipmentSMEIndicator : ZString.Empty; }
		}

		public override ZString PartyCertificationIndicator
		{
			get { return ODTBlock != null ? ODTBlock.DDTCEligiblePartyCertificationIndicator : ZString.Empty; }
		}

		public override ZString RegistrationNo
		{
			get { return ODTBlock != null ? ODTBlock.DDTCRegistrationNumber : ZString.Empty; }
		}

		public override ZString USMLCategoryCode
		{
			get { return ODTBlock != null ? ODTBlock.DDTCUSMLCategoryCode : ZString.Empty; }
		}

		public override ZString USMLCategoryDescription
		{
			get { return ODTBlock != null ? GetUSMLCategoryDescription(ODTBlock.DDTCUSMLCategoryCode) : ZString.Empty; }
		}

		public override ZDecimal DDTCQuantity
		{
			get { return ODTBlock?.DDTCQuantity ?? ZDecimal.Zero; }
		}

		public override ZString DDTCUnitOfMeasure
		{
			get { return ODTBlock?.DDTCUnitOfMeasureCode ?? ZString.Empty; }
		}

		#endregion

		#region Vehicle Details

		public override ZString VehicleIndicator
		{
			get { return EV1Block != null ? "Yes" : "No"; }
		}

		public override ZString VehicleTitleNumber
		{
			get { return EV1Block != null ? EV1Block.VehicleTitleNumber : ZString.Empty; }
		}

		public override ZString VehicleIDTypeDescription
		{
			get { return EV1Block != null ? GetVehicleIDTypeDescription(EV1Block.VehicleIDQualifier) : ZString.Empty; }
		}

		public override ZString VehicleTitleStateDescription
		{
			get { return EV1Block != null ? GetStateDescription(EV1Block.VehicleTitleStateCode) : ZString.Empty; }
		}

		public override ZString VehicleID
		{
			get { return EV1Block != null ? EV1Block.VehicleIdentificationNumberVINProductID : ZString.Empty; }
		}
		#endregion

		protected override IEnumerable<MessageBlock> GetPGAMessageBlocksCore()
		{
			return PGABlock;
		}
	}
}
