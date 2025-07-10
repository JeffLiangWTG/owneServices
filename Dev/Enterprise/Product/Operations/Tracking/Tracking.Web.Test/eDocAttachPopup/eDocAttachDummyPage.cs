using System;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class eDocAttachDummyPage : eDocAttachPage
	{
		public eDocAttachDummyPage()
			: base()
		{
			UnauthorisedLabel = new ZTextLabel();
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();

			LoadOrCreateDataSource();
		}

		internal ZTextLabel UnauthorisedLabelInternal
		{
			get { return UnauthorisedLabel; }
			set { UnauthorisedLabel = value; }
		}

		internal HtmlGenericControl UnauthorisedDivInternal
		{
			get { return UnauthorisedDiv; }
			set { UnauthorisedDiv = value; }
		}

		internal HtmlGenericControl AuthorisedContentInternal
		{
			get { return AuthorisedContent; }
			set { AuthorisedContent = value; }
		}

		internal HtmlInputHidden maximumFileSizeHiddenInternal
		{
			get { return maximumFileSizeHidden; }
			set { maximumFileSizeHidden = value; }
		}

		internal HtmlInputHidden maximumFileSizeMessageHiddenInternal
		{
			get { return maximumFileSizeMessageHidden; }
			set { maximumFileSizeMessageHidden = value; }
		}

		public void OnLoadForTest()
		{
			OnLoad(EventArgs.Empty);
		}

		protected override BusinessObject GetNewDataSource() => Factory.New<DummyBusinessObjectWithUploadSupport>();

		protected override void OnBeforeDataSourceFactorySaved()
		{
			BeforeDataSourceFactorySaved?.Invoke(this, EventArgs.Empty);
			base.OnBeforeDataSourceFactorySaved();
		}

		public event EventHandler BeforeDataSourceFactorySaved;
	}
}
