using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE20Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var expConsignSE20 = new ASESE20()
			{
				ReferenceIdentifierQualifier = ReferenceIdentifierCodeList.Codes.ExpressConsignmentShipment
			};

			var splitRelSE20 = new ASESE20()
			{
				ReferenceIdentifierQualifier = ReferenceIdentifierCodeList.Codes.SplitShipmentReleaseElectionCode,
				ReferenceIdentifier = SplitShipmentReleaseCodeList.Codes.RequestSpecialPermit
			};

			var examSiteSE20 = new ASESE20()
			{
				ReferenceIdentifierQualifier = ReferenceIdentifierCodeList.Codes.ElectedExamSite,
				ReferenceIdentifier = "A002"
			};

			var nonAMSSE20 = new ASESE20()
			{
				ReferenceIdentifierQualifier = ReferenceIdentifierCodeList.Codes.NonAMSBillOfLading
			};

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)expConsignSE20).Update(declaration, notifications);
			AssertEquals("Y", declaration.US_ExpConsign);
			((IBIRDHeaderRecord)splitRelSE20).Update(declaration, notifications);
			AssertEquals(SplitShipmentReleaseCodeList.Codes.RequestSpecialPermit, declaration.US_SESplitRel);
			((IBIRDHeaderRecord)examSiteSE20).Update(declaration, notifications);
			AssertEquals("A002", declaration.US_US_NKCentralizedExamSite);
			((IBIRDHeaderRecord)nonAMSSE20).Update(declaration, notifications);
			AssertEquals(true, declaration.US_NonAMS);
		}
	}
}
