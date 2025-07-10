using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Shared
{
	public static class DocAddressTypeExtensions
	{
		#region GetOrganisationType

		public static OrganisationTypes GetOrganisationType(this DocAddressType docAddressType)
		{
			OrganisationTypes result;

			switch (docAddressType)
			{
				case DocAddressType.LocalCartageCFS:
				case DocAddressType.LocalCartageCTO:
				case DocAddressType.LocalCartageYard:
					result = OrganisationTypes.Services;
					break;
				case DocAddressType.LocalCartageExporter:
					result = OrganisationTypes.Consignor;
					break;
				case DocAddressType.LocalCartageImporter:
					result = OrganisationTypes.Consignee;
					break;
				default:
					result = OrganisationTypes.None;
					break;
			}

			return result;
		}

		#endregion

		#region GetOrganisationSubType

		public static string GetOrganisationSubType(this DocAddressType docAddressType)
		{
			string result;

			switch (docAddressType)
			{
				case DocAddressType.LocalCartageCFS:
					result = OrganisationsSubTypeList.Codes.ContainerFreightStation;
					break;
				case DocAddressType.LocalCartageCTO:
					result = OrganisationsSubTypeList.Codes.ContainerTerminalOperator;
					break;
				case DocAddressType.LocalCartageYard:
					result = OrganisationsSubTypeList.Codes.ContainerYard;
					break;
				default:
					result = "";
					break;
			}

			return result;
		}

		#endregion

		#region GetDocAddressTypeFromEventReferenceFacility

		public static DocAddressType GetDocAddressTypeFromEventReferenceFacility(string facilityCode)
		{
			switch (facilityCode)
			{
				case Constants.Facilities.Code.Depot:
					return DocAddressType.LocalCartageCFS;
				case Constants.Facilities.Code.Terminal:
					return DocAddressType.LocalCartageCTO;
				case Constants.Facilities.Code.Consignor:
					return DocAddressType.LocalCartageExporter;
				case Constants.Facilities.Code.Consignee:
					return DocAddressType.LocalCartageImporter;
				case Constants.Facilities.Code.Warehouse:
					return DocAddressType.LocalCartageWarehouse;
				case Constants.Facilities.Code.ContainerYard:
					return DocAddressType.LocalCartageYard;
				default:
					return DocAddressType.None;
			}
		}

		#endregion

		#region GetEventReferenceFacilityCodeFromDocAddress

		public static ZString GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress address)
		{
			if (address == null)
			{
				return "";
			}

			switch (address.DocAddressType)
			{
				case DocAddressType.LocalCartageCFS:
					return Constants.Facilities.Code.Depot;
				case DocAddressType.LocalCartageCTO:
					return Constants.Facilities.Code.Terminal;
				case DocAddressType.LocalCartageExporter:
					return Constants.Facilities.Code.Consignor;
				case DocAddressType.LocalCartageImporter:
					return Constants.Facilities.Code.Consignee;
				case DocAddressType.LocalCartageWarehouse:
					return Constants.Facilities.Code.Warehouse;
				case DocAddressType.LocalCartageYard:
					return Constants.Facilities.Code.ContainerYard;
				default:
					return address.AddressCaption;
			}
		}

		#endregion
	}
}
