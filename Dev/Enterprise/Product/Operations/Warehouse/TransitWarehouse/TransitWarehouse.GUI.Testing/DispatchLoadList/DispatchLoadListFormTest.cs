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
	[TestedType(typeof(DispatchLoadListForm))]
	public class DispatchLoadListFormTest : ZFormBasherTest
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = (DispatchLoadListForm)GetFormToBash())
			{
				AssertEquals("Dispatch Load List", form.FormCaption);

				((WhsItemDispatchLoadList)form.BusinessEntity).WDL_JobID = "CN001";
				AssertEquals("Dispatch Load List CN001", form.FormCaption);
			}
		}

		#endregion

		#region TestFormHasDocumentPlugIn

		public void TestFormHasDocumentPlugIn()
		{
			using (var form = (DispatchLoadListForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region Open in Browser

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (DispatchLoadListForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);
				AssertEquals(@"This Dispatch Load List cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var dll = Factory.New<WhsItemDispatchLoadList>();

			using (var form = new DispatchLoadListForm(dll))
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);

				AssertEquals("Should not show error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(dll, glowPortalsUri: "address");
		}

		public static void AssertOpenedOnTheWeb(WhsItemDispatchLoadList dtu, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/DispatchLoadList", uri.AbsolutePath);
			AssertEquals(dtu.PK.ToString(), entityPK);
		}

		#endregion

		#region PlugIns

		public void TestPlugIns()
		{
			using (var form = (DispatchLoadListForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Apportionment));
			}
		}

		#endregion

		#region Implementation

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		protected override Form GetFormToBashCore()
		{
			var form = new DispatchLoadListForm(Factory.New<WhsItemDispatchLoadList>());
			form.ControllerID = ControllerIDs.WhsItemDispatchLoadList;
			return form;
		}

		#endregion
	}
}
