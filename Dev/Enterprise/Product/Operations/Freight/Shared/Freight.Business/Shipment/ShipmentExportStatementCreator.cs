using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public class ShipmentExportStatementCreator : ExportStatementCreator
	{
		public ShipmentExportStatementCreator(CommonShipment shipment, ExportStatementSetting exportStatementSetting, string dateFormat)
			: base(exportStatementSetting)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException("Shipment");
			}
			this.Shipment = shipment;
			this.DateFormat = dateFormat;
		}

		protected override ZString GetStatementFieldTypeMessage(ZString fieldType, string countryCode)
		{
			ZString result = fieldType;
			if (Core.Constants.CountryCodes.UsaAndTerritoriesList.Contains(countryCode))
			{
				result = GetUSStatementFieldTypeMessage(fieldType, countryCode);
			}
			return result;
		}

		protected override string GetFieldSeparator(string countryCode)
		{
			return (Core.Constants.CountryCodes.UsaAndTerritoriesList.Contains(countryCode)) ? " " : base.GetFieldSeparator(countryCode);
		}

		RefCountry USTerritories(string countryCode)
		{
			if (fUSTerritory == null)
			{
				fUSTerritory = RefCountry.LoadFromCountryCode(Factory, countryCode);
			}
			return fUSTerritory;
		}
		RefCountry fUSTerritory;

		ZString GetUSStatementFieldTypeMessage(ZString fieldType, string countryCode)
		{
			ZString result = fieldType;
			switch (fieldType)
			{
				case SEDStatementFieldType.Codes.AgentEIN:
					result = GetEINFromOrganisation(Shipment.ExportBroker, countryCode);
					break;
				case SEDStatementFieldType.Codes.DateOfExport:
					result = Shipment.JS_E_DEP.ToString(DateFormat);
					break;
				case SEDStatementFieldType.Codes.FilerID:
					result = GetFilerID();
					break;
				case SEDStatementFieldType.Codes.ITN:
					result = Shipment.CustomsEntryNumber;
					break;
				case SEDStatementFieldType.Codes.ShipperEIN:
					result = GetEINFromOrganisation(Shipment.Consignor, countryCode);
					break;
				case SEDStatementFieldType.Codes.ShipperEINAndFilerID:
					result = GetEINFromOrganisation(Shipment.Consignor, countryCode) + "-" + GetFilerID();
					break;
				case SEDStatementFieldType.Codes.SRN:
					result = Shipment.JS_HouseBill;
					break;
				case SEDStatementFieldType.Codes.XTN:
					break;
				default:
					break;
			}
			return result;
		}

		ZString GetFilerID()
		{
			return ((ZString)ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerIDValue).KeepAlphanumericCharacters();
		}

		ZString GetEINFromOrganisation(OrgHeader org, string countryCode)
		{
			if (org != null)
			{
				return org.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, USTerritories(countryCode)).KeepAlphanumericCharacters();
			}
			return ZString.Empty;
		}

		BusinessObjectFactory Factory
		{
			get { return Shipment.Factory; }
		}

		readonly CommonShipment Shipment;
		readonly string DateFormat;
	}
}
