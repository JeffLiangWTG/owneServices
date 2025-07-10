using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NotificationGroupValidatorTest : TestCaseWithFactory
	{
		public void TestIsValidForOutgoingMail()
		{
			var staffCollection = new GlbStaffCollection(Factory);

			IGlbGroup group = null;
			Assert(!group.IsValidForOutgoingMail(Factory));

			var groupMock = new Mock<IGlbGroup>();
			group = groupMock.Object;
			groupMock.SetupSequence(m => m.Staff).Returns((IList)null);
			groupMock.Setup(m => m.Staff).Returns(staffCollection);

			Assert(!group.IsValidForOutgoingMail(Factory));

			var staff = staffCollection.AddNew();
			Assert(!group.IsValidForOutgoingMail(Factory));

			staff.GS_EmailAddress = "invalid email address";
			Assert(!group.IsValidForOutgoingMail(Factory));

			staff.GS_EmailAddress = "test@test.com";
			Assert(group.IsValidForOutgoingMail(Factory));

			groupMock.VerifyAll();
			groupMock.Verify(m => m.Staff, Times.Exactly(8));
		}

		public void TestArgumentNullExceptionThrown()
		{
			var group = new Mock<IGlbGroup>(MockBehavior.Strict);
			AssertExceptionThrown(typeof(ArgumentNullException), () => { group.Object.IsValidForOutgoingMail(null); });
		}
	}
}
