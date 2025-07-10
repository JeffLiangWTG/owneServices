using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class TSWGlbStaffWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new TSWGlbStaffWrapper(null);
		}

		public void TestTSWGlbStaffWrapper()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");
			AssertEquals("Staff Password should be Encyrpted value", "Ix1yvDKoBSDh5nWjOgrt5g==", wrapper.NZBPassword.GP_CurrentPassword);
			var wrappedGlbStaff = new TSWGlbStaffWrapper(staff);
			AssertEquals("DeclarantID", "40006206E", wrappedGlbStaff.DeclarantID);
			AssertEquals("DeclarantPinEncrypted - stored in EM_MessageOwner - used to indicate MAC is required, so must be truncated to MessageOwner Field size", "Ix1yvDKoBSDh5nWjOgrt", wrappedGlbStaff.DeclarantPinEncrypted);
		}
	}
}
