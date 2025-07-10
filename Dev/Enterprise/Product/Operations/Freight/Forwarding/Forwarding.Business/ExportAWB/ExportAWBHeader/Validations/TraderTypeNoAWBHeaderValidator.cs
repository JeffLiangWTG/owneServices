using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class TraderTypeNoAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public TraderTypeNoAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable() => true;

		#region EH_ShipperTraderNo & EH_ShipperTradeNoType

		protected override void CheckEH_ShipperTraderNo()
		{
			base.CheckEH_ShipperTraderNo();

			var traderTypeAndNoMessage = GetShipperRequiredTraderTypeAndNoMessage(Parent.EH_ShipperTraderNoInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ShipperTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_ShipperTraderNoType()
		{
			base.CheckEH_ShipperTraderNoType();

			var traderTypeAndNoMessage = GetShipperRequiredTraderTypeAndNoMessage(Parent.EH_ShipperTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ShipperTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetShipperRequiredTraderTypeAndNoMessage(ZPropertyInfo traderInfo)
		{
			if (!traderInfo.Value.IsEmpty || OriginCountryCode.IsEmpty)
			{
				return ZString.Empty;
			}

			string result;
			if (DestinationCountryCode == Constants.CountryCodes.Egypt)
			{
				result = egyptImportsExporterRegistrationNumberRequiredMessage;
			}
			else if (OriginCountryCode == Constants.CountryCodes.Indonesia)
			{
				result = indonesiaExportsTaxNumberRequiredMessage;
			}
			else if (OriginCountryCode == Constants.CountryCodes.VietNam)
			{
				result = vietnamVATNumberRequiredMessage;
			}
			else if (OriginCountryCode == Constants.CountryCodes.Canada
				|| OriginCountryCode == Constants.CountryCodes.Morocco
				|| OriginCountryCode == Constants.CountryCodes.Bolivia
				|| OriginCountryCode == Constants.CountryCodes.Honduras
				|| OriginCountryCode == Constants.CountryCodes.India)
			{
				result = string.Empty;
			}
			else
			{
				result = GetRequiredTraderNoAndTypeMessage(OriginCountryCode, false);
			}

			return result;
		}

		string egyptImportsExporterRegistrationNumberRequiredMessage => Res.GetString("208a2dfc-abf1-4fdc-a6e9-a886182a090b", "Exporter registration number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the relevant company number against the Consignor Organization.");

		string indonesiaExportsTaxNumberRequiredMessage => Res.GetString("33183f4e-6e87-4380-91ba-373467adb6bb", "Shipper PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

		string vietnamVATNumberRequiredMessage => Res.GetString("ba90a2ff-773b-4d29-b2ea-53ed8e122bdb", "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL");

		#endregion

		#region EH_ConsigneeTraderNo & EH_ConsigneeTraderNoType

		bool RequireErrorMessageForMissingTraderNo => Parent.IsImportToBangladesh && (Parent.IsDirectMAWB || Parent.IsIndirectHAWB || (Parent.IsHAWB && Parent.Consol == null));

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();

			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessage(Parent.EH_ConsigneeTraderNoInfo);

			if (RequireErrorMessageForMissingTraderNo && !traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddMessageError(traderTypeAndNoMessage);
			}
			else if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		protected override void CheckEH_ConsigneeTraderNoType()
		{
			base.CheckEH_ConsigneeTraderNoType();

			var traderTypeAndNoMessage = GetConsigneeRequiredTraderTypeAndNoMessage(Parent.EH_ConsigneeTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetConsigneeRequiredTraderTypeAndNoMessage(ZPropertyInfo traderInfo)
		{
			if (!traderInfo.Value.IsEmpty || DestinationCountryCode.IsEmpty)
			{
				return ZString.Empty;
			}

			string result;
			if (DestinationCountryCode == Constants.CountryCodes.Egypt)
			{
				result = egyptImporterRegistrationNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.VietNam)
			{
				result = vietnamVATNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Indonesia)
			{
				result = indonesiaConsigneeImportsTaxNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Israel)
			{
				result = israelConsigneeAndNotifyPartyImportsVATNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Bangladesh)
			{
				result = bangladeshConsigneeVATNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Canada)
			{
				result = canadaCCCNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.China
				|| DestinationCountryCode == Constants.CountryCodes.Morocco
				|| DestinationCountryCode == Constants.CountryCodes.India
				|| DestinationCountryCode == Constants.CountryCodes.Bolivia
				|| DestinationCountryCode == Constants.CountryCodes.Honduras)
			{
				result = string.Empty;
			}
			else
			{
				result = GetRequiredTraderNoAndTypeMessage(DestinationCountryCode, true);
			}

			return result;
		}

		string canadaCCCNumberRequiredMessage => Res.GetString("b222e475-1ff0-2f4b-9c16-7a8df2c8ac9a", "For imports to Canada, the Sending Forwarder’s code provided by the CBSA (Canada Border Services Agency) is required for eManifest reporting. Use the Organization Registration Number type ‘CCC’ (Carrier Code) to save this code.");

		string bangladeshConsigneeVATNumberRequiredMessage => Res.GetString("b321e475-4ff0-4f4d-9c16-7a7df2c8ac9b", "The Consignee VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.");

		string indonesiaConsigneeImportsTaxNumberRequiredMessage => Res.GetString("ebb893e5-478c-49d4-a5d7-a16fa4282541", "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

		#endregion

		#region EH_AlsoNotifyTraderNo & EH_AlsoNotifyTraderNoType

		protected override void CheckEH_AlsoNotifyTraderNo()
		{
			base.CheckEH_AlsoNotifyTraderNo();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessage(Parent.EH_AlsoNotifyTraderNoInfo);
			if (!traderTypeAndNoMessage.IsEmpty && !Parent.EH_AlsoNotifyName.IsEmpty)
			{
				if (RequireErrorMessageForMissingTraderNo)
				{
					Parent.EH_AlsoNotifyTraderNoInfo.AddMessageError(traderTypeAndNoMessage);
				}
				else
				{
					Parent.EH_AlsoNotifyTraderNoInfo.AddWarning(traderTypeAndNoMessage);
				}
			}
		}

		protected override void CheckEH_AlsoNotifyTraderNoType()
		{
			base.CheckEH_AlsoNotifyTraderNoType();

			var traderTypeAndNoMessage = GetAlsoNotifyRequiredTraderTypeAndNoMessage(Parent.EH_AlsoNotifyTraderNoTypeInfo);
			if (!traderTypeAndNoMessage.IsEmpty && !Parent.EH_AlsoNotifyName.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoTypeInfo.AddWarning(traderTypeAndNoMessage);
			}
		}

		ZString GetAlsoNotifyRequiredTraderTypeAndNoMessage(ZPropertyInfo traderInfo)
		{
			if (!traderInfo.Value.IsEmpty || DestinationCountryCode.IsEmpty)
			{
				return ZString.Empty;
			}

			var result = string.Empty;
			if (DestinationCountryCode == Constants.CountryCodes.Egypt)
			{
				if (!AlsoNotifyIsEmpty)
				{
					result = egyptImporterRegistrationNumberRequiredMessage;
				}
			}
			else if (DestinationCountryCode == Constants.CountryCodes.VietNam)
			{
				result = vietnamVATNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Indonesia)
			{
				result = indonesiaAlsoNotifyPartyImportsTaxNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Israel)
			{
				result = israelConsigneeAndNotifyPartyImportsVATNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Bangladesh)
			{
				result = bangladeshNotifyPartyVATNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.Canada)
			{
				result = canadaCCCNumberRequiredMessage;
			}
			else if (DestinationCountryCode == Constants.CountryCodes.China
				|| DestinationCountryCode == Constants.CountryCodes.Morocco
				|| DestinationCountryCode == Constants.CountryCodes.India
				|| DestinationCountryCode == Constants.CountryCodes.Bolivia
				|| DestinationCountryCode == Constants.CountryCodes.Honduras)
			{
				result = string.Empty;
			}
			else
			{
				result = GetRequiredTraderNoAndTypeMessage(DestinationCountryCode, true);
			}

			return result;
		}

		bool AlsoNotifyIsEmpty
		{
			get
			{
				return Parent.EH_AlsoNotifyName.IsEmpty
					&& Parent.EH_AlsoNotifyAddress.IsEmpty
					&& Parent.EH_AlsoNotifyPlace.IsEmpty
					&& Parent.EH_AlsoNotifyCountryCode.IsEmpty;
			}
		}

		string egyptImporterRegistrationNumberRequiredMessage => Res.GetString("00030aca-bf2e-4fba-86cd-4a6d37de0d20", "VAT number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the VAT against the Organization.");

		string indonesiaAlsoNotifyPartyImportsTaxNumberRequiredMessage => Res.GetString("d4d9cd4f-fe07-41ee-a79b-60956cad30d6", "Also Notify PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

		string bangladeshNotifyPartyVATNumberRequiredMessage => Res.GetString("940b0832-8a47-4cce-8710-1753ee526718", "The Notify Party VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.");

		#endregion

		#region Implementation

		string israelConsigneeAndNotifyPartyImportsVATNumberRequiredMessage => Res.GetString("d1d13bfc-062c-4f51-a71d-5b9f8f5faebe", "VAT Number is required for imports to Israel to comply with Manifest reporting.");

		ZString OriginCountryCode => Parent.OriginCountry?.Code ?? ZString.Empty;

		ZString DestinationCountryCode => Parent.DestinationCountry?.Code ?? ZString.Empty;

		string GetRequiredTraderNoAndTypeMessage(ZString countryCode, ZBool isImport)
		{
			var formattedTypes = Parent.GetRequiredFormattedTraderTypes(countryCode);
			return formattedTypes.IsEmpty
				? string.Empty
				: Res.GetString("783c74d4-6f71-4480-a76b-3a7bdb57a8ff", "{0} is required for {1} {2}.", formattedTypes, RefCountry.LoadFromCountryCode(Parent.Factory, countryCode)?.Description ?? countryCode, isImport ? (NoResString)"imports" : (NoResString)"exports");
		}

		#endregion
	}
}
