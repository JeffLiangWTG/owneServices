using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgCusCodeValidity))]
	public sealed class OrgCusCodeValidityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestVerificationAuthority()
		{
			var validity = Factory.New<OrgCusCodeValidity>();
			AssertEquals(ZString.Empty, validity.VerificationAuthority);

			validity.OCV_Verified = true;
			AssertEquals("USER ENTRY", validity.VerificationAuthority);

			validity.OCV_VerificationAuthority = "CBP";
			AssertEquals("CBP", validity.VerificationAuthority);

			validity.OCV_SnapShotOfWhatIsVerified = "TEST TEXT";
			AssertEquals("TEST TEXT", validity.SnapshotContextForDisplay);
		}

		public void TestSnapshotContextForDisplay()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var dunOrgCusCode = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUN111", Core.Constants.CountryCodes.UnitedStates);
			var mainAddress = organization.MainAddress;

			var orgHeaderWrapper = OrgHeaderWrapper.New(organization);
			var globalBusinessIdentifierData = new GlobalBusinessIdentifierData(orgHeaderWrapper);
			globalBusinessIdentifierData.US_OA_AddressDetails = mainAddress.PK;
			globalBusinessIdentifierData.US_GLN = "GLN222";
			globalBusinessIdentifierData.US_LEI = "LEI333";
			globalBusinessIdentifierData.SaveGlobalBusinessIdentifiers();

			var builder = new GlobalBusinessIdentifierMessageBuilder(globalBusinessIdentifierData, GlobalBusinessIdentifierMessageType.Original);
			var sentMessage = builder.Generate();
			sentMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;
			sentMessage.EM_MessageNum = "EDIEDIDAT_1";
			Factory.Save();

			var glnOrgCusCode = mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "GLN222", Core.Constants.CountryCodes.UnitedStates);
			var leiOrgCusCode = mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "LEI333", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(string.Empty, dunOrgCusCode.SnapshotContextForDisplay);
			AssertEquals(string.Empty, glnOrgCusCode.SnapshotContextForDisplay);
			AssertEquals(string.Empty, leiOrgCusCode.SnapshotContextForDisplay);

			foreach (var orgCusCode in new OrgCusCode[] { dunOrgCusCode, glnOrgCusCode, leiOrgCusCode })
			{
				orgCusCode.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified = "B003901XJ5GO                                                                    " +
"GO20DUNSDUN111                                                                  " +
"GO300124231111010GBI IS VALID                                                   " +
"GO20GLN GLN222                                                                  " +
"GO300124231111020GBI NOT FOUND                                                  " +
"GO20LEI LEI333                                                                  " +
"GO300124231111030GBI IS INACTIVE                                                " +
"GO900124231111001GBI ACCEPTED                                                   " +
"Y  3901XJ5GO                                                                    ";
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			foreach (var orgCusCode in new OrgCusCode[] { dunOrgCusCode, glnOrgCusCode, leiOrgCusCode })
			{
				var orgCusCodeLoaded = newFactory.Load<OrgCusCode>(orgCusCode.PK);
				AssertContains("DUNS : DUN111 <br />GLN : GLN222 <br />LEI : LEI333 <br /><br />Firm Name :  <br />Street Address : #1, <br />City :  <br />State :  <br /><br />Zip/Postal Code :  <br />ISO Country Code :  <br /><br />Phone :  <br />Website URL :  <br /><br />", orgCusCodeLoaded.SnapshotContextForDisplay);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Roles</th><th>&nbsp;</th></tr></thead><tr><td>Manufacturer</td><td>N</td></tr><tr><td>Shipper</td><td>N</td></tr><tr><td>Seller</td><td>N</td></tr><tr><td>Exporter</td><td>N</td></tr><tr><td>Packager</td><td>N</td></tr><tr><td>Distributor</td><td>N</td></tr></table><br />", orgCusCodeLoaded.SnapshotContextForDisplay);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>GBI Overall Status</th><th>Description</th></tr></thead><tr><td>001</td><td>GBI ACCEPTED</td></tr></table><br />", orgCusCodeLoaded.SnapshotContextForDisplay);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>GBI Ref. ID Qualifier</th><th>GBI Ref. ID</th><th>Disposition Code</th><th>Description</th></tr></thead><tr><td>DUNS</td><td>DUN111</td><td>010</td><td>GBI IS VALID</td></tr><tr><td>GLN</td><td>GLN222</td><td>020</td><td>GBI NOT FOUND</td></tr><tr><td>LEI</td><td>LEI333</td><td>030</td><td>GBI IS INACTIVE</td></tr></table><br />", orgCusCodeLoaded.SnapshotContextForDisplay);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<OrgCusCodeValidity>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.MainAddress.CustomsCodes.AddNew();
			result.OCV_OK_OrgCusCode = cusCode.PK;
			return result;
		}
	}
}
