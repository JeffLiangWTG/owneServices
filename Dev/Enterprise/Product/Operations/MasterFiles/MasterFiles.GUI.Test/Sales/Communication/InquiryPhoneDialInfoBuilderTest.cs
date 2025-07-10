using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class InquiryPhoneDialInfoBuilderTest : TestCaseWithFactory
	{
		public void TestGetDefaultDialInfo()
		{
			var inquiry = Factory.New<SalesEnquiry>();

			var builder = new CommunicationFormPhoneDiallerUserControl.InquiryPhoneDialInfoBuilder();
			var dialInfo = builder.GetDefaultDialInfo(inquiry);
			AssertNull(dialInfo);

			inquiry.O1_Phone = "02 12345678";
			dialInfo = builder.GetDefaultDialInfo(inquiry);
			AssertEquals("Work", dialInfo.Description);
			AssertEquals("02 12345678", dialInfo.Number);
		}

		public void TestGetAlternativePhoneDialInfos()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_Phone = "02 12345678";
			inquiry.O1_Mobile = "04 12345678";

			var builder = new CommunicationFormPhoneDiallerUserControl.InquiryPhoneDialInfoBuilder();
			var actualAlternativePhoneDialInfos = builder.GetAlternativePhoneDialInfos(inquiry);

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Work (02 12345678)",
					"Mobile (04 12345678)"
				},
				actualAlternativePhoneDialInfos.Select(info => string.Format("{0} ({1})", info.Description, info.Number)).ToArray());
		}
	}
}
