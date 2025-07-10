using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	sealed class TestLinerAndAgencyBasePage : LinerAndAgencyBasePage, ILinerAndAgenycBasePageForTest
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public TestLinerAndAgencyBasePage()
			: base()
		{
			VolumeCalculatorGridAddOn = new VolumeCalculatorDataGridAddOn
				(JobPackLinesSchema.Constants.JL_PackageCount,
				JobPackLinesSchema.Constants.JL_Length,
				JobPackLinesSchema.Constants.JL_Width,
				JobPackLinesSchema.Constants.JL_Height,
				JobPackLinesSchema.Constants.JL_UnitOfDimension,
				JobPackLinesSchema.Constants.JL_ActualVolume,
				JobPackLinesSchema.Constants.JL_ActualVolumeUQ);
		}

		protected override LinerAndAgencyBaseWebInterfacesHelper GetNewWebInterfacesHelper()
		{
			return null;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return true; }
		}

		protected override string NotFoundLabelText
		{
			get { return "Not Found"; }
		}

		public void SetupBookedContainersGridForTest()
		{
			BookedContainersGrid = new ZDataGrid();
			SetupBookedContainersGrid();
		}

		public ZDataGrid BookedContainersGridForTest
		{
			get { return BookedContainersGrid; }
		}

		public void SetupPackLinesGridForTest(string packMode)
		{
			if (Shipment == null)
			{
				LoadOrCreateDataSource();
			}

			Shipment.JS_PackingMode = packMode;
			PacksGrid = new ZDataGrid();
			SetupPackLinesGrid();
		}

		public ZDataGrid PacksGridForTest
		{
			get { return PacksGrid; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return Factory.New<AgencyShipment>();
		}
	}
}
