using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	sealed class EditLinerAndAgencyContainerForTest : EditLinerAndAgencyContainer
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public EditLinerAndAgencyContainerForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			SaveContainer = new Button();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			this.ContainerContents = new System.Web.UI.HtmlControls.HtmlGenericControl();

			base.EstimatedFullDeliveryDateEditBox = new ZDateEdit();
			base.EmptyReadyForReturnDateEditBox = new ZDateEdit();
			base.EmptyReturnReqByDateEditBox = new ZDateEdit();
			base.DeliveryDateEditBox = new ZDateEdit();
			base.EmptyReturnedOnDateEditBox = new ZDateEdit();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public void SetupAuthorisedContentForTest()
		{
			SetupAuthorisedContent(true);
		}

		public new ZDateEdit EstimatedFullDeliveryDateEditBox
		{
			get { return base.EstimatedFullDeliveryDateEditBox; }
		}

		public new ZDateEdit EmptyReadyForReturnDateEditBox
		{
			get { return base.EmptyReadyForReturnDateEditBox; }
		}

		public new ZDateEdit EmptyReturnReqByDateEditBox
		{
			get { return base.EmptyReturnReqByDateEditBox; }
		}

		public new ZDateEdit DeliveryDateEditBox
		{
			get { return base.DeliveryDateEditBox; }
		}

		public new ZDateEdit EmptyReturnedOnDateEditBox
		{
			get { return base.EmptyReturnedOnDateEditBox; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return ContainerForTest;
		}

		public LinerAndAgencyContainer ContainerForTest
		{
			get
			{
				if (containerForTest == null)
				{
					containerForTest = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
					Factory.Save();
				}
				return containerForTest;
			}
		}
		LinerAndAgencyContainer containerForTest;
	}
}
