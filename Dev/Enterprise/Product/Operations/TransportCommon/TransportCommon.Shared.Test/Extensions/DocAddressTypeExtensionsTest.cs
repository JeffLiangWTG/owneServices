using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Shared.Testing
{
	public class DocAddressTypeExtensionsTest : TestCase
	{
		#region TestGetOrganisationType

		public void TestGetOrganisationType()
		{
			AssertEquals(OrganisationTypes.Services, DocAddressType.LocalCartageCFS.GetOrganisationType());
			AssertEquals(OrganisationTypes.Services, DocAddressType.LocalCartageCTO.GetOrganisationType());
			AssertEquals(OrganisationTypes.Consignor, DocAddressType.LocalCartageExporter.GetOrganisationType());
			AssertEquals(OrganisationTypes.Consignee, DocAddressType.LocalCartageImporter.GetOrganisationType());
			AssertEquals(OrganisationTypes.Services, DocAddressType.LocalCartageYard.GetOrganisationType());
			AssertEquals(OrganisationTypes.None, DocAddressType.LocalCartageService.GetOrganisationType());
		}

		#endregion

		#region TestGetOrganisationSubType

		public void TestGetOrganisationSubType()
		{
			AssertEquals(OrganisationsSubTypeList.Codes.ContainerFreightStation, DocAddressType.LocalCartageCFS.GetOrganisationSubType());
			AssertEquals(OrganisationsSubTypeList.Codes.ContainerTerminalOperator, DocAddressType.LocalCartageCTO.GetOrganisationSubType());
			AssertEquals("", DocAddressType.LocalCartageExporter.GetOrganisationSubType());
			AssertEquals("", DocAddressType.LocalCartageImporter.GetOrganisationSubType());
			AssertEquals(OrganisationsSubTypeList.Codes.ContainerYard, DocAddressType.LocalCartageYard.GetOrganisationSubType());
			AssertEquals("", DocAddressType.LocalCartageService.GetOrganisationSubType());
		}

		#endregion

		#region TestGetDocAddressTypeFromEventReferenceFacility

		public void TestGetDocAddressTypeFromEventReferenceFacility()
		{
			AssertEquals(DocAddressType.LocalCartageCFS, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Constants.Facilities.Code.Depot));
			AssertEquals(DocAddressType.LocalCartageCTO, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Constants.Facilities.Code.Terminal));
			AssertEquals(DocAddressType.LocalCartageExporter, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Constants.Facilities.Code.Consignor));
			AssertEquals(DocAddressType.LocalCartageImporter, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Constants.Facilities.Code.Consignee));
			AssertEquals(DocAddressType.LocalCartageWarehouse, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Constants.Facilities.Code.Warehouse));
			AssertEquals(DocAddressType.LocalCartageYard, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Constants.Facilities.Code.ContainerYard));
			AssertEquals("Other", DocAddressType.None, DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility("XYZ"));
		}

		#endregion

		#region TestGetEventReferenceFacilityFromDocAddress

		public void TestGetEventReferenceFacilityFromDocAddress()
		{
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();

			AssertEquals(Constants.Facilities.Code.Depot, DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalCartageCFS)));
			AssertEquals(Constants.Facilities.Code.Terminal, DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalCartageCTO)));
			AssertEquals(Constants.Facilities.Code.Consignor, DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalCartageExporter)));
			AssertEquals(Constants.Facilities.Code.Consignee, DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalCartageImporter)));
			AssertEquals(Constants.Facilities.Code.Warehouse, DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalCartageWarehouse)));
			AssertEquals(Constants.Facilities.Code.ContainerYard, DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalCartageYard)));
			AssertEquals("Local Client", DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(JobDocAddress.New(dummy, DocAddressType.LocalClient)));
		}

		#endregion
	}
}
