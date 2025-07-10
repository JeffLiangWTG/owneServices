using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ContainerDetailsForTest : ContainerDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public ContainerDetailsForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			EditButtonDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			this.ContainerContents = new System.Web.UI.HtmlControls.HtmlGenericControl();
			base.OrdersGrid = new ZGrid();
			base.DocumentsGrid = new ZGrid();

			base.MasterBillCaption = new ZTextLabel();
			base.MasterBillLabel = new ZTextLabel();

			base.ContainerStatusCaption = new ZTextLabel();
			base.ContainerStatusLabel = new ZTextLabel();

			base.PlannedWeightCaption = new ZTextLabel();
			base.PlannedWeightLabel = new ZTextLabel();

			base.QuarantineCaption = new ZTextLabel();
			base.QuarantineLabel = new ZTextLabel();

			base.PortTransportRefCaption = new ZTextLabel();
			base.PortTransportRefLabel = new ZTextLabel();

			base.SequenceCaption = new ZTextLabel();
			base.SequenceLabel = new ZNumericLabel();

			base.PickupCaption = new ZTextLabel();
			base.PickupLabel = new ZTextLabel();

			base.DeliverCaption = new ZTextLabel();
			base.DeliverLabel = new ZTextLabel();

			base.SpecialInstructionCaption = new ZTextLabel();
			base.SpecialInstructionLabel = new ZTextLabel();

			base.ConsoleNumberCaption = new ZTextLabel();
			base.ConsoleNumberLabel = new ZTextLabel();

			base.VerifiedByCompanyCaption = new ZTextLabel();
			base.VerifiedByCompanyLabel = new ZTextLabel();

			base.VerifiedByPersonCaption = new ZTextLabel();
			base.VerifiedByPersonLabel = new ZTextLabel();

			base.VerifiedByPhoneCaption = new ZTextLabel();
			base.VerifiedByPhoneLabel = new ZTextLabel();

			base.VerifiedByEmailCaption = new ZTextLabel();
			base.VerifiedByEmailLabel = new ZTextLabel();

			base.EditContainer = new Button();
		}

		public void ForTest_RunOnLoad()
		{
			this.OnLoad(new EventArgs());
		}

		public void ForTest_RunOnPreBind()
		{
			this.OnPreBind();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public void SetupAuthorisedContentForTest()
		{
			SetupAuthorisedContent(true);
		}

		public void SetupOrdersGridForTest()
		{
			SetupOrdersGrid(OrdersGrid);
		}

		public new ZGrid OrdersGrid
		{
			get { return base.OrdersGrid; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return ContainerForTest;
		}

		public TrackingContainer ContainerForTest
		{
			get
			{
				if (containerForTest == null)
				{
					containerForTest = Factory.NewWithValidTestData<TrackingContainer>();
					Factory.Save();
				}
				return containerForTest;
			}
			set
			{
				containerForTest = value;
				LoadOrCreateDataSource();
			}
		}
		TrackingContainer containerForTest;

		public new ZGrid DocumentsGrid
		{
			get { return base.DocumentsGrid; }
		}

		public new ZTextLabel MasterBillCaption
		{
			get { return base.MasterBillCaption; }
		}

		public new ZTextLabel MasterBillLabel
		{
			get { return base.MasterBillLabel; }
		}

		public new ZTextLabel ContainerStatusCaption
		{
			get { return base.ContainerStatusCaption; }
		}

		public new ZTextLabel ContainerStatusLabel
		{
			get { return base.ContainerStatusLabel; }
		}

		public new ZTextLabel PlannedWeightCaption
		{
			get { return base.PlannedWeightCaption; }
		}

		public new ZTextLabel PlannedWeightLabel
		{
			get { return base.PlannedWeightLabel; }
		}

		public new ZTextLabel QuarantineCaption
		{
			get { return base.QuarantineCaption; }
		}

		public new ZTextLabel QuarantineLabel
		{
			get { return base.QuarantineLabel; }
		}

		public new ZTextLabel PortTransportRefCaption
		{
			get { return base.PortTransportRefCaption; }
		}

		public new ZTextLabel PortTransportRefLabel
		{
			get { return base.PortTransportRefLabel; }
		}

		public new ZTextLabel SequenceCaption
		{
			get { return base.SequenceCaption; }
		}

		public new ZNumericLabel SequenceLabel
		{
			get { return base.SequenceLabel; }
		}

		public new ZTextLabel PickupCaption
		{
			get { return base.SequenceCaption; }
		}

		public new ZTextLabel PickupLabel
		{
			get { return base.PickupLabel; }
		}

		public new ZTextLabel DeliverCaption
		{
			get { return base.DeliverCaption; }
		}

		public new ZTextLabel DeliverLabel
		{
			get { return base.DeliverLabel; }
		}

		public new ZTextLabel SpecialInstructionCaption
		{
			get { return base.SpecialInstructionCaption; }
		}

		public new ZTextLabel SpecialInstructionLabel
		{
			get { return base.SpecialInstructionLabel; }
		}

		public new ZTextLabel ConsoleNumberCaption
		{
			get { return base.ConsoleNumberCaption; }
		}

		public new ZTextLabel ConsoleNumberLabel
		{
			get { return base.ConsoleNumberLabel; }
		}

		public new ZTextLabel VerifiedByCompanyCaption
		{
			get { return base.VerifiedByCompanyCaption; }
		}

		public new ZTextLabel VerifiedByCompanyLabel
		{
			get { return base.VerifiedByCompanyLabel; }
		}

		public new ZTextLabel VerifiedByPersonCaption
		{
			get { return base.VerifiedByPersonCaption; }
		}

		public new ZTextLabel VerifiedByPersonLabel
		{
			get { return base.VerifiedByPersonLabel; }
		}

		public new ZTextLabel VerifiedByPhoneCaption
		{
			get { return base.VerifiedByPhoneCaption; }
		}

		public new ZTextLabel VerifiedByPhoneLabel
		{
			get { return base.VerifiedByPhoneLabel; }
		}

		public new ZTextLabel VerifiedByEmailCaption
		{
			get { return base.VerifiedByEmailCaption; }
		}

		public new ZTextLabel VerifiedByEmailLabel
		{
			get { return base.VerifiedByEmailLabel; }
		}

		public new Button EditContainer
		{
			get { return base.EditContainer; }
		}
	}
}
