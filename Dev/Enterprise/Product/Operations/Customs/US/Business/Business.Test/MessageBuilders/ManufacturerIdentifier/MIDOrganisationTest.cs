using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MIDOrganisationTest : TestCaseWithFactory
	{
		public void TestCreateMIDOrganizationIfNecessary()
		{
			const string MIDOrganizationNumber = "TESTMIDLA";
			USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			var orgAddress = new MIDOrganisation().CreateMIDOrganizationIfNecessary(MIDOrganizationNumber, Factory);
			AssertNull("OrgAddress should be null since registry is false", orgAddress);
			AssertEquals("OrgAddress count should be 0", 0, Factory.Load<OrgCusCode>(new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, MIDOrganizationNumber)).Length);

			USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			orgAddress = new MIDOrganisation().CreateMIDOrganizationIfNecessary(MIDOrganizationNumber, Factory);
			AssertEquals("OrgAddress is created since registry is true", AddressParser.GetAddressPKFromMatchingMIDCode(MIDOrganizationNumber, Factory), orgAddress.PK);
			AssertEquals("OrgAddress count should be 1", 1, Factory.Load<OrgCusCode>(new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, MIDOrganizationNumber)).Length);

			orgAddress = new MIDOrganisation().CreateMIDOrganizationIfNecessary(MIDOrganizationNumber, Factory);
			AssertNull("Same MID code is used so no OrgAddress is created", orgAddress);
			AssertEquals("OrgAddress count should still be 1", 1, Factory.Load<OrgCusCode>(new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, MIDOrganizationNumber)).Length);
		}
	}
}
