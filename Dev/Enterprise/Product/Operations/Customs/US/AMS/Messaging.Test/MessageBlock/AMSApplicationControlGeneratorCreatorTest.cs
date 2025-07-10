using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class AMSApplicationControlGeneratorCreatorTest : TestCaseWithFactory
	{
		[TestDate(1971, 9, 18)]
		public void TestNew()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageNum = "320989";
			var creator = new AMSApplicationControlGeneratorCreator();
			var applicationControlGenerator = creator.New(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, GlbBranch.CurrentBranch);
			AssertEquals(typeof(AMSApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(APLACR), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(APLZCR), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("ACR          MI                                                                 ", applicationControlGenerator.A.Serialise());
			AssertEquals("ZCR          MI                   00000                                         ", applicationControlGenerator.Z.Serialise());
		}
	}
}
