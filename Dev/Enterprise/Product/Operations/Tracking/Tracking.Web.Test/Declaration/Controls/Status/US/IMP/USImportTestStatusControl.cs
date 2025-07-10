using System.Web.UI.HtmlControls;
using Enterprise.Tracking.Web.Declaration.US.IMP;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class USImportTestStatusControl : StatusControl
	{
		public void SetUpControlForTest()
		{
			AreaAIIStatus = new HtmlGenericControl();
			AreaBillOfLadingUpdateStatus = new HtmlGenericControl();
			AreaITStatus = new HtmlGenericControl();
			DispositionGrid = new ZGrid();
			EntrySummaryStatusGrid = new ZGrid();
			PGALineStatusGrid = new ZGrid();
			PGAStatusGrid = new ZGrid();
		}

		public HtmlGenericControl ForTest_AreaAIIStatus
		{
			get { return this.AreaAIIStatus; }
		}

		public HtmlGenericControl ForTest_AreaBillOfLadingUpdateStatus
		{
			get { return this.AreaBillOfLadingUpdateStatus; }
		}

		public HtmlGenericControl ForTest_AreaITStatus
		{
			get { return this.AreaITStatus; }
		}

		public ZGrid ForTest_DispositionGrid
		{
			get { return this.DispositionGrid; }
		}

		public ZGrid ForTest_EntrySummaryStatusGrid
		{
			get { return this.EntrySummaryStatusGrid; }
		}

		public ZGrid ForTest_PGALineStatusGrid
		{
			get { return this.PGALineStatusGrid; }
		}

		public ZGrid ForTest_PGAStatusGrid
		{
			get { return this.PGAStatusGrid; }
		}
	}
}
