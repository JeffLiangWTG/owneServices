using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderExportStatementCreator : ExportStatementCreator
	{
		public CusEntryHeaderExportStatementCreator(CusEntryHeader entryHeader, ExportStatementSetting exportStatementSetting)
			: base(exportStatementSetting)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			this.entryHeader = entryHeader;
		}

		protected override ZString GetStatementFieldTypeMessage(ZString fieldType, string countryCode)
		{
			ZString result = fieldType;
			if (Core.Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(countryCode))
			{
				result = GetUSStatementFieldTypeMessage(fieldType);
			}
			return result;
		}

		protected override string GetFieldSeparator(string countryCode)
		{
			return (Core.Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(countryCode)) ? " " : base.GetFieldSeparator(countryCode);
		}

		RefCountry USCountry
		{
			get
			{
				if (fUSCountry == null)
				{
					fUSCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates);
				}
				return fUSCountry;
			}
		}
		RefCountry fUSCountry;

		ZString GetUSStatementFieldTypeMessage(ZString fieldType)
		{
			ZString result = "";

			switch (fieldType)
			{
				case SEDStatementFieldType.Codes.AgentEIN:
					result = GetEINFromOrganisation(entryHeader.Declaration.Forwarder);
					break;
				case SEDStatementFieldType.Codes.DateOfExport:
					result = entryHeader.ExportDate.ToString("MM/dd/yyyy");
					break;
				case SEDStatementFieldType.Codes.FilerID:
					result = GetFilerID();
					break;
				case SEDStatementFieldType.Codes.ITN:
					result = entryHeader.HasBeenWithdrawn ? ZString.Empty : entryHeader.EntryNumber;
					break;
				case SEDStatementFieldType.Codes.ShipperEIN:
					result = GetEINFromOrganisation(entryHeader.Declaration.Consignor);
					break;
				case SEDStatementFieldType.Codes.ShipperEINAndFilerID:
					result = GetEINFromOrganisation(entryHeader.Declaration.Consignor) + "-" + GetFilerID();
					break;
				case SEDStatementFieldType.Codes.SRN:
					result = entryHeader.HouseBillsCommaSeparated;
					break;
				case SEDStatementFieldType.Codes.XTN:
					result = entryHeader.HasBeenWithdrawn ? ZString.Empty : entryHeader.US_XTN;
					break;
			}

			return result;
		}

		ZString GetFilerID()
		{
			return USCustomsDataRegistry.Instance.ExportEntryFilerID.GetFallBackValueAtAllLevels(entryHeader.RegistryCompanyPK, entryHeader.RegistryBranchPK, Guid.Empty).EntryFilerID.KeepAlphanumericCharacters();
		}

		ZString GetEINFromOrganisation(OrgHeader org)
		{
			if (org != null)
			{
				return org.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, USCountry).KeepAlphanumericCharacters();
			}
			return ZString.Empty;
		}

		BusinessObjectFactory Factory
		{
			get { return entryHeader.Factory; }
		}

		readonly CusEntryHeader entryHeader;
	}
}
