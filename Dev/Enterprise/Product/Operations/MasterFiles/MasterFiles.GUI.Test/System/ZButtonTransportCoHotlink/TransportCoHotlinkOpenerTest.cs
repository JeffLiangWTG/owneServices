using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TransportCoHotlinkOpenerTest : TestCaseWithFactory
	{
		public void TestOpenWebSite()
		{
			ZString cartageTrackingDesc = TransportCoHotlinkOpener.Instance.CartageTrackingDescription;

			AssertEquals("No Transport Company has been selected.", TransportCoHotlinkOpener.Instance.OpenWebSite("refNo", null));
			AssertEquals("No Transport.OrgWebURL exists with type WHT.",
				"The Transport Company Moo does not have a " + cartageTrackingDesc + " web address.", TransportCoHotlinkOpener.Instance.OpenWebSite("refNo", TransportCo));

			AssertOpeningPageReturnsMessage("", "www.blah.com", "The Transport Reference is empty.");
			AssertOpeningPageReturnsMessage("", "www.blah.com", "The Transport Reference is empty.");
			AssertOpeningPageReturnsMessage("refNo", "", "The Transport Company Moo does not have a " + cartageTrackingDesc + " web address.");
			AssertOpeningPageReturnsMessage("refNo", "badUrl", "The Transport Company Moo does not have a valid " + cartageTrackingDesc + " web address.");
			AssertOpeningPageReturnsMessage("refNo", "www.blah.com", "The " + cartageTrackingDesc + " web address for transport company Moo is missing the text (*CargoWiseREF*).");

			// test valid case
			AssertOpeningPageReturnsMessage("refNo", "www.blah.com?ref=(*CargoWiseREF*)", "");
		}

		void AssertOpeningPageReturnsMessage(string refNo, string url, string expectedMessage)
		{
			OrgWebURL[] cartageUrls = TransportCo.OrgWebURLs.FindByUrlType(OrgWebUrlList.Codes.CartageTracking);
			OrgWebURL cartageUrl = (cartageUrls.Length > 0) ? cartageUrls[0] : TransportCo.OrgWebURLs.AddNew(OrgWebUrlList.Codes.CartageTracking);

			cartageUrl.PU_URL = url;
			AssertEquals(expectedMessage, TransportCoHotlinkOpener.Instance.OpenWebSite(refNo, TransportCo));
		}

		#region Implementation

		OrgHeader TransportCo
		{
			get
			{
				if (fTransportCo == null)
				{
					fTransportCo = Factory.New<OrgHeader>();
					fTransportCo.OH_FullName = "Moo";
				}
				return fTransportCo;
			}
		}

		OrgHeader fTransportCo;

		#endregion
	}
}
