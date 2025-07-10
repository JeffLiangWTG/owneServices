using System;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class LoginForTest : Login
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void OnLoadForTest(bool initControls = true)
		{
			if (initControls)
			{
				LoginInstructionContent = new HtmlGenericControl();
				LoginInstructionLabel = new ZTextLabelNoEncode();
				ViewShipmentDetails = new HtmlGenericControl();
				ViewContainerDetails = new HtmlGenericControl();
				QuickViewDetails = new HtmlGenericControl();
				CompanyCode = new HtmlGenericControl();
				CompanyCodeTextBox = new ZTextBox() { ID = "CompanyCodeTextBox" };
				LoginNameTextBox = new ZTextBox() { ID = "LoginNameTextBox" };
				PasswordTextBox = new ZTextBox() { ID = "PasswordTextBox" };
				ContainerNumberTextBox = new ZTextBox { ID = "ContainerNumberTextBox" };
			}
			OnLoad(EventArgs.Empty);
		}

		public ZTextBox CompanyCodeTextBoxForTest => CompanyCodeTextBox;

		public ZTextBox LoginNameTextBoxForTest => LoginNameTextBox;

		public ZTextBox PasswordTextBoxForTest => PasswordTextBox;

		public ZTextBox ContainerNumberTextBoxForTest => ContainerNumberTextBox;

		public bool CacheableForTest => Cacheable;

		public void SigninBtn_ClickForTest()
		{
			SigninBtn_Click(null, EventArgs.Empty);
		}

		public void FindBtn_ClickForTest()
		{
			FindBtn_Click(null, EventArgs.Empty);
		}

		public TrackingLoginManager LoginManForTest
		{
			get { return LoginMan; }
		}

		public ZString LoginInstructionForTest
		{
			get { return LoginInstructionLabel.Text; }
		}

		public TrackingLoginManager GetNewDataSourceForTest() => (TrackingLoginManager)GetNewDataSource();

		public HtmlGenericControl LoginInstructionContentForTest
		{
			get { return LoginInstructionContent; }
		}

		public HtmlGenericControl QuickViewDetailsForTest
		{
			get { return QuickViewDetails; }
		}

		public HtmlGenericControl ViewContainerDetailsForTest
		{
			get { return ViewContainerDetails; }
		}

		public HtmlGenericControl ViewShipmentDetailsForTest
		{
			get { return ViewShipmentDetails; }
		}

		public bool IsDataSourceInSessionForTest
		{
			get { return IsDataSourceInSession; }
		}

		public NameValueCollection RequestQueryStringForTest => RequestQueryString;
	}
}
