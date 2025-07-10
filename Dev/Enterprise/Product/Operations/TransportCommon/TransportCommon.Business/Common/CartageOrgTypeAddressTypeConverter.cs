using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business
{
	public static class CartageOrgTypeAddressTypeConverter
	{
		public static DocAddressType GetDocAddressTypeFromOrgType(ZString orgType)
		{
			switch (orgType)
			{
				case OrganisationTypesList.Codes.CFS:
					return DocAddressType.LocalCartageCFS;
				case OrganisationTypesList.Codes.CTO:
					return DocAddressType.LocalCartageCTO;
				case OrganisationTypesList.Codes.CNE:
					return DocAddressType.LocalCartageImporter;
				case OrganisationTypesList.Codes.CNR:
					return DocAddressType.LocalCartageExporter;
				case OrganisationTypesList.Codes.CYD:
					return DocAddressType.LocalCartageYard;
				case OrganisationTypesList.Codes.SRV:
					return DocAddressType.LocalCartageService;
				case OrganisationTypesList.Codes.WHS:
					return DocAddressType.LocalCartageWarehouse;
				case OrganisationTypesList.Codes.MSC:
					return DocAddressType.LocalCartageMSC;

				default:
					return DocAddressType.None;
			}
		}

		public static ZString GetOrgTypeFromCartageDocAddressType(DocAddressType docAddressType)
		{
			switch (docAddressType)
			{
				case DocAddressType.LocalCartageCFS:
					return OrganisationTypesList.Codes.CFS;
				case DocAddressType.LocalCartageCTO:
					return OrganisationTypesList.Codes.CTO;
				case DocAddressType.LocalCartageImporter:
					return OrganisationTypesList.Codes.CNE;
				case DocAddressType.LocalCartageExporter:
					return OrganisationTypesList.Codes.CNR;
				case DocAddressType.LocalCartageYard:
					return OrganisationTypesList.Codes.CYD;
				case DocAddressType.LocalCartageService:
					return OrganisationTypesList.Codes.SRV;
				case DocAddressType.LocalCartageWarehouse:
					return OrganisationTypesList.Codes.WHS;
				case DocAddressType.LocalCartageMSC:
					return OrganisationTypesList.Codes.MSC;

				case DocAddressType.NonPersistent:
				case DocAddressType.None:
					return "";

				default:
					throw new NotSupportedException(docAddressType.ToString() + " needs to be added to GetOrgTypeFromCartageDocAddressType().");
			}
		}
	}
}
