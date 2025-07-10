using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEOceanManifestJobDocAddressTest : TestCaseWithFactory
	{
		public void TestSanitizedMembers()
		{
			var mockObject = new Mock<IDocAddress>();
			mockObject.Setup(m => m.AddressCaption).Returns("*AddressCaption*");
			mockObject.Setup(m => m.CountryCode).Returns("A*");
			mockObject.Setup(m => m.E2_AdditionalAddressInformation).Returns("Additional*Address*Info*");
			mockObject.Setup(m => m.E2_Address1).Returns("Address1*");
			mockObject.Setup(m => m.E2_Address2).Returns("Address2*");
			mockObject.Setup(m => m.E2_AddressType).Returns("ML*");
			mockObject.Setup(m => m.E2_City).Returns("CI*");
			mockObject.Setup(m => m.E2_CompanyName).Returns("COM*NAME");
			mockObject.Setup(m => m.E2_Fax).Returns("999*888");
			mockObject.Setup(m => m.E2_GovRegNum).Returns("1*2");
			mockObject.Setup(m => m.E2_GovRegNumType).Returns("L*R");
			mockObject.Setup(m => m.E2_PassportCountryOfIssue).Returns("U*");
			mockObject.Setup(m => m.E2_Phone).Returns("11*22");
			mockObject.Setup(m => m.E2_PortCode).Returns("R*T");
			mockObject.Setup(m => m.E2_State).Returns("S*T");
			mockObject.Setup(m => m.ParentDescription).Returns("PA*DE");
			mockObject.Setup(m => m.E2_PassportID).Returns("PASS*ID");
			mockObject.Setup(m => m.CountryDescription).Returns("AUSTRALIA*");

			IDocAddress wrapper = new SanitizedACEOceanManifestJobDocAddressWrapper(mockObject.Object);
			AssertEquals(" ADDRESSCAPTION ", wrapper.AddressCaption);
			AssertEquals("A ", wrapper.CountryCode);
			AssertEquals("ADDITIONAL ADDRESS INFO ", wrapper.E2_AdditionalAddressInformation);
			AssertEquals("ADDRESS1 ", wrapper.E2_Address1);
			AssertEquals("ADDRESS2 ", wrapper.E2_Address2);
			AssertEquals("ML ", wrapper.E2_AddressType);
			AssertEquals("CI ", wrapper.E2_City);
			AssertEquals("COM NAME", wrapper.E2_CompanyName);
			AssertEquals("999 888", wrapper.E2_Fax);
			AssertEquals("1*2", wrapper.E2_GovRegNum);
			AssertEquals("L R", wrapper.E2_GovRegNumType);
			AssertEquals("U ", wrapper.E2_PassportCountryOfIssue);
			AssertEquals("11 22", wrapper.E2_Phone);
			AssertEquals("R T", wrapper.E2_PortCode);
			AssertEquals("S T", wrapper.E2_State);
			AssertEquals("PA DE", wrapper.ParentDescription);
			AssertEquals("PASS*ID", wrapper.E2_PassportID);
			AssertEquals("AUSTRALIA ", wrapper.CountryDescription);
		}
	}
}
