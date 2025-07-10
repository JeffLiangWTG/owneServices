using System;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(ReceiveASNForm))]
	public class ReceiveASNFormTest : ZFormBasherTest
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = (ReceiveASNForm)GetFormToBash())
			{
				AssertEquals("Receive ASN", form.FormCaption);

				((WhsItemReceiveASN)form.BusinessEntity).WRP_ReferenceNumber = "WRP001";
				AssertEquals("Receive ASN WRP001", form.FormCaption);
			}
		}

		#endregion

		#region TestFormHasDocumentPlugIn

		public void TestFormHasDocumentPlugIn()
		{
			using (var form = (ReceiveASNForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region Open in Browser

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (ReceiveASNForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);
				AssertEquals(@"This Receive ASN cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var asn = Factory.New<WhsItemReceiveASN>();

			using (var form = new ReceiveASNForm(asn))
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);

				AssertEquals("Should not show error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(asn, glowPortalsUri: "address");
		}

		public static void AssertOpenedOnTheWeb(WhsItemReceiveASN asn, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/ReceiveASN", uri.AbsolutePath);
			AssertEquals(asn.PK.ToString(), entityPK);
		}
		#endregion

		#region Implementation

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		protected override Form GetFormToBashCore()
		{
			var form = new ReceiveASNForm(Factory.New<WhsItemReceiveASN>());
			form.ControllerID = ControllerIDs.WhsItemReceiveASN;
			return form;
		}

		#endregion
	}
}
