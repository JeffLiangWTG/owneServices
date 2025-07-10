using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Business.Testing
{
	public class CartageOrgTypeAddressTypeConverterTest : TestCase
	{
		#region TestGetDocAddressTypeFromOrgType

		public void TestGetDocAddressTypeFromOrgType()
		{
			AssertEquals(DocAddressType.LocalCartageCFS, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.CFS));
			AssertEquals(DocAddressType.LocalCartageCTO, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.CTO));
			AssertEquals(DocAddressType.LocalCartageImporter, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.CNE));
			AssertEquals(DocAddressType.LocalCartageExporter, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.CNR));
			AssertEquals(DocAddressType.LocalCartageYard, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.CYD));
			AssertEquals(DocAddressType.LocalCartageService, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.SRV));
			AssertEquals(DocAddressType.LocalCartageWarehouse, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.WHS));
			AssertEquals(DocAddressType.LocalCartageMSC, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationTypesList.Codes.MSC));

			AssertEquals(DocAddressType.None, CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType("moo"));
		}

		#endregion

		#region TestGetOrgTypeFromCartageDocAddressType

		public void TestGetOrgTypeFromCartageDocAddressType()
		{
			AssertEquals(OrganisationTypesList.Codes.CFS, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageCFS));
			AssertEquals(OrganisationTypesList.Codes.CTO, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageCTO));
			AssertEquals(OrganisationTypesList.Codes.CNE, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageImporter));
			AssertEquals(OrganisationTypesList.Codes.CNR, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageExporter));
			AssertEquals(OrganisationTypesList.Codes.CYD, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageYard));
			AssertEquals(OrganisationTypesList.Codes.SRV, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageService));
			AssertEquals(OrganisationTypesList.Codes.WHS, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageWarehouse));
			AssertEquals(OrganisationTypesList.Codes.MSC, CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.LocalCartageMSC));

			AssertEquals("", CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.NonPersistent));
			AssertEquals("", CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.None));

			AssertExceptionThrown(typeof(NotSupportedException), "Manufacturer needs to be added to GetOrgTypeFromCartageDocAddressType().",
				() => CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddressType.Manufacturer));
		}

		#endregion
	}
}
