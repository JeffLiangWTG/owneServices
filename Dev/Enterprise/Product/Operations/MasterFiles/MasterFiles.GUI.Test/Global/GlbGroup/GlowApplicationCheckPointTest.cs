using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class GlowApplicationCheckPointTest :  TestCase
	{
		Mock<IZSecurity> GetMockSecurity()
		{
			var mockSecurity = new Mock<IZSecurity>();
			mockSecurity.Setup(m => m.AddCheckPoint(It.IsAny<CheckpointLookupKey>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
			return mockSecurity;
		}

		GlowApplicationCheckpoint GetCheckpoint(Mock<IZSecurity> mockSecurity)
		{
			var parent = new SecurityCheckpoint("Parent", (NoResString)"Parent", null, mockSecurity.Object);
			return new GlowApplicationCheckpoint("Code", DummyModuleIDs.Dummy.Description, parent, mockSecurity.Object);
		}

		public void TestGroupWithRightsIsAllowed()
		{
			var mockSecurity = GetMockSecurity();
			mockSecurity.Setup(m => m.IsGroupAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Granted);
			var mockCheckPoint = GetCheckpoint(mockSecurity);
			AssertEquals("IsAllowed", true, mockCheckPoint.IsAllowed);
			AssertEquals("Glow Application Checkpoint Code", "GlowAppCheck_Code", mockCheckPoint.Code);
			mockSecurity.VerifyAll();
		}

		public void TestGroupWithRightsIsDenied()
		{
			var mockSecurity = GetMockSecurity();
			mockSecurity.Setup(m => m.IsGroupAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Denied);
			var mockCheckPoint = GetCheckpoint(mockSecurity);
			AssertEquals("IsAllowed", false, mockCheckPoint.IsAllowed);
			AssertEquals("Glow Application Checkpoint Code", "GlowAppCheck_Code", mockCheckPoint.Code);
			mockSecurity.VerifyAll();
		}

		public void TestGroupWithRightsIsImplicit()
		{
			var mockSecurity = GetMockSecurity();
			mockSecurity.Setup(m => m.IsGroupAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Implicit);
			var mockCheckPoint = GetCheckpoint(mockSecurity);
			AssertEquals("IsAllowed", false, mockCheckPoint.IsAllowed);
			AssertEquals("Glow Application Checkpoint Code", "GlowAppCheck_Code", mockCheckPoint.Code);
			mockSecurity.VerifyAll();
		}
	}
}
