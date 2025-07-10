using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	sealed class LinerAndAgencyContainerDetailsForTest : LinerAndAgencyContainerDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public LinerAndAgencyContainerDetailsForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			EditButtonDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			this.ContainerContents = new System.Web.UI.HtmlControls.HtmlGenericControl();

			base.DocumentsGrid = new ZGrid();

			base.DynamicCaption = new ZTextLabel();
			base.DynamicLabel = new ZTextLabel();

			base.JobNumberCaption = new ZTextLabel();
			base.JobNumberLabel = new ZTextLabel();

			base.ContainerStatusCaption = new ZTextLabel();
			base.ContainerStatusLabel = new ZTextLabel();

			base.GrossWeighCaption = new ZTextLabel();
			base.GrossWeightLabel = new ZTextLabel();

			base.PickupCaption = new ZTextLabel();
			base.PickupLabel = new ZTextLabel();

			base.DeliverCaption = new ZTextLabel();
			base.DeliverLabel = new ZTextLabel();

			base.CommodityCaption = new ZTextLabel();
			base.CommodityLabel = new ZCodeFindBoxLabel();

			base.GoodValueCaption = new ZTextLabel();
			base.GoodValueLabel = new ZTextLabel();

			base.CurrencyCaption = new ZTextLabel();
			base.CurrencyLabel = new ZTextLabel();

			base.NetWeightCaption = new ZTextLabel();
			base.NetWeightLabel = new ZTextLabel();

			base.TareWeightCaption = new ZTextLabel();
			base.TareWeightLabel = new ZTextLabel();

			base.IsShipperCheckBox = new ZCheckBox();
			base.ISEmptyCheckBox = new ZCheckBox();
			base.IsDamagedCheckBox = new ZCheckBox();

			base.PaymentTermCaption = new ZTextLabel();
			base.PaymentTermLabel = new ZTextLabel();

			base.ServiceLevelCaption = new ZTextLabel();
			base.ServiceLevelLabel = new ZTextLabel();

			base.ShipperRefCaption = new ZTextLabel();
			base.ShipperRefLabel = new ZTextLabel();

			base.OrderRefCaption = new ZTextLabel();
			base.OrderRefLabel = new ZTextLabel();

			base.GoodsDescriptionCaption = new ZTextLabel();
			base.GoodsDescriptionLabel = new ZTextLabel();

			base.IsControlledAtmosphereCheckBox = new ZCheckBox();
			base.IsChillerCheckBox = new ZCheckBox();
			base.IsFreezerCheckBox = new ZCheckBox();

			base.TemperatureCaption = new ZTextLabel();
			base.TemperatureLabel = new ZTextLabel();

			base.HumidityCaption = new ZTextLabel();
			base.HumidityLabel = new ZTextLabel();

			base.TempRecordCaption = new ZTextLabel();
			base.TempRecordLabel = new ZTextLabel();

			base.AirVentCaption = new ZTextLabel();
			base.AirVentLabel = new ZTextLabel();

			base.ClipOnUnitNumberCaption = new ZTextLabel();
			base.ClipOnUnitNumberLabel = new ZTextLabel();

			base.EmptyPickupFromCaption = new ZTextLabel();
			base.EmptyPickupFromLabel = new ZTextLabel();

			base.EmptyReleaseNumberCaption = new ZTextLabel();
			base.EmptyReleaseNumberLabel = new ZTextLabel();

			base.EmptyReleaseFromCaption = new ZTextLabel();
			base.EmptyReleaseFromLabel = new ZTextLabel();

			base.EmptyReturnedToCaption = new ZTextLabel();
			base.EmptyReturnedToLabel = new ZTextLabel();

			base.VerifiedByCompanyCaption = new ZTextLabel();
			base.VerifiedByCompanyLabel = new ZTextLabel();

			base.VerifiedByPersonCaption = new ZTextLabel();
			base.VerifiedByPersonLabel = new ZTextLabel();

			base.VerifiedByPhoneCaption = new ZTextLabel();
			base.VerifiedByPhoneLabel = new ZTextLabel();

			base.VerifiedByEmailCaption = new ZTextLabel();
			base.VerifiedByEmailLabel = new ZTextLabel();

			base.RefrigrationCaption = new ZTextLabel();

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
			set
			{
				containerForTest = value;
				LoadOrCreateDataSource();
			}
		}
		LinerAndAgencyContainer containerForTest;

		public new ZGrid DocumentsGrid
		{
			get { return base.DocumentsGrid; }
		}

		public new ZTextLabel ContainerStatusCaption
		{
			get { return base.ContainerStatusCaption; }
		}

		public new ZTextLabel ContainerStatusLabel
		{
			get { return base.ContainerStatusLabel; }
		}

		public new ZTextLabel DynamicCaption
		{
			get { return base.DynamicCaption; }
		}

		public new ZTextLabel DynamicLabel
		{
			get { return base.DynamicLabel; }
		}

		public new ZTextLabel JobNumberCaption
		{
			get { return base.JobNumberCaption; }
		}

		public new ZTextLabel JobNumberLabel
		{
			get { return base.JobNumberLabel; }
		}

		public new ZTextLabel GrossWeighCaption
		{
			get { return base.GrossWeighCaption; }
		}

		public new ZTextLabel GrossWeightLabel
		{
			get { return base.GrossWeightLabel; }
		}

		public new ZTextLabel CommodityCaption
		{
			get { return base.CommodityCaption; }
		}

		public new ZCodeFindBoxLabel CommodityLabel
		{
			get { return base.CommodityLabel; }
		}

		public new ZTextLabel GoodValueCaption
		{
			get { return base.GoodValueCaption; }
		}

		public new ZTextLabel GoodValueLabel
		{
			get { return base.GoodValueLabel; }
		}

		public new ZTextLabel CurrencyCaption
		{
			get { return base.CurrencyCaption; }
		}

		public new ZTextLabel CurrencyLabel
		{
			get { return base.CurrencyLabel; }
		}

		public new ZTextLabel NetWeightCaption
		{
			get { return base.NetWeightCaption; }
		}

		public new ZTextLabel NetWeightLabel
		{
			get { return base.NetWeightLabel; }
		}
		public new ZTextLabel TareWeightCaption
		{
			get { return base.TareWeightCaption; }
		}

		public new ZTextLabel TareWeightLabel
		{
			get { return base.TareWeightLabel; }
		}

		public new ZCheckBox IsShipperCheckBox
		{
			get { return base.IsShipperCheckBox; }
		}

		public new ZCheckBox IsFreezerCheckBox
		{
			get { return base.IsFreezerCheckBox; }
		}

		public new ZCheckBox IsChillerCheckBox
		{
			get { return base.IsChillerCheckBox; }
		}

		public new ZCheckBox IsControlledAtmosphereCheckBox
		{
			get { return base.IsControlledAtmosphereCheckBox; }
		}

		public new ZCheckBox ISEmptyCheckBox
		{
			get { return base.ISEmptyCheckBox; }
		}

		public new ZCheckBox IsDamagedCheckBox
		{
			get { return base.IsDamagedCheckBox; }
		}

		public new ZTextLabel PaymentTermCaption
		{
			get { return base.PaymentTermCaption; }
		}

		public new ZTextLabel PaymentTermLabel
		{
			get { return base.PaymentTermLabel; }
		}

		public new ZTextLabel ServiceLevelCaption
		{
			get { return base.ServiceLevelCaption; }
		}

		public new ZTextLabel ServiceLevelLabel
		{
			get { return base.ServiceLevelLabel; }
		}

		public new ZTextLabel ShipperRefCaption
		{
			get { return base.ShipperRefCaption; }
		}

		public new ZTextLabel ShipperRefLabel
		{
			get { return base.ShipperRefLabel; }
		}

		public new ZTextLabel OrderRefCaption
		{
			get { return base.OrderRefCaption; }
		}
		public new ZTextLabel OrderRefLabel
		{
			get { return base.OrderRefLabel; }
		}

		public new ZTextLabel GoodsDescriptionCaption
		{
			get { return base.GoodsDescriptionCaption; }
		}
		public new ZTextLabel GoodsDescriptionLabel
		{
			get { return base.GoodsDescriptionLabel; }
		}
		public new ZTextLabel TemperatureCaption
		{
			get { return base.TemperatureCaption; }
		}
		public new ZTextLabel TemperatureLabel
		{
			get { return base.TemperatureLabel; }
		}

		public new ZTextLabel HumidityCaption
		{
			get { return base.HumidityCaption; }
		}
		public new ZTextLabel HumidityLabel
		{
			get { return base.HumidityLabel; }
		}

		public new ZTextLabel TempRecordCaption
		{
			get { return base.TempRecordCaption; }
		}
		public new ZTextLabel TempRecordLabel
		{
			get { return base.TempRecordLabel; }
		}

		public new ZTextLabel AirVentCaption
		{
			get { return base.AirVentCaption; }
		}
		public new ZTextLabel AirVentLabel
		{
			get { return base.AirVentLabel; }
		}

		public new ZTextLabel ClipOnUnitNumberCaption
		{
			get { return base.ClipOnUnitNumberCaption; }
		}

		public new ZTextLabel ClipOnUnitNumberLabel
		{
			get { return base.ClipOnUnitNumberLabel; }
		}

		public new ZTextLabel EmptyPickupFromCaption
		{
			get { return base.EmptyPickupFromCaption; }
		}
		public new ZTextLabel EmptyPickupFromLabel
		{
			get { return base.EmptyPickupFromLabel; }
		}

		public new ZTextLabel EmptyReleaseNumberCaption
		{
			get { return base.EmptyReleaseNumberCaption; }
		}
		public new ZTextLabel EmptyReleaseNumberLabel
		{
			get { return base.EmptyReleaseNumberLabel; }
		}

		public new ZTextLabel EmptyReleaseFromCaption
		{
			get { return base.EmptyReleaseFromCaption; }
		}
		public new ZTextLabel EmptyReleaseFromLabel
		{
			get { return base.EmptyReleaseFromLabel; }
		}

		public new ZTextLabel EmptyReturnedToCaption
		{
			get { return base.EmptyReturnedToCaption; }
		}

		public new ZTextLabel EmptyReturnedToLabel
		{
			get { return base.EmptyReturnedToLabel; }
		}

		public new ZTextLabel PickupCaption
		{
			get { return base.PickupCaption; }
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

		public new ZTextLabel RefrigrationCaption
		{
			get { return base.RefrigrationCaption; }
		}

		public new Button EditContainer
		{
			get { return base.EditContainer; }
		}
	}
}
