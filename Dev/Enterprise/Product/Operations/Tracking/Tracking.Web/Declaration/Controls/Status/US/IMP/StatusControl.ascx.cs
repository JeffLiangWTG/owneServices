using System.Web.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Declaration.US.IMP
{
	[ToolboxData("<{0}:StatusControl runat=server></{0}:StatusControl>")]
	public partial class StatusControl : BaseStatusControl, ISelfBindingWebControl
	{
		protected override void OnInit(System.EventArgs e)
		{
			base.OnInit(e);
			SetUpGrids();
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			SetupControl();
		}

		public void SetupControl()
		{
			if (USDeclaration != null)
			{
				AreaAIIStatus.Visible = USDeclaration.US_EnableAII;

				bool bOLVisible = !(USDeclaration.BLUStatus.IsEmpty && USDeclaration.BLUMessageStatusDate.IsEmpty);
				AreaBillOfLadingUpdateStatus.Visible = bOLVisible;

				bool iTStatusVisible = !(USDeclaration.ITStatusDate.IsEmpty &&
					USDeclaration.ITDepartureStatus.IsEmpty &&
					USDeclaration.ITArrivalStatus.IsEmpty &&
					USDeclaration.ITExportStatus.IsEmpty &&
					USDeclaration.ITTOLStatus.IsEmpty);
				AreaITStatus.Visible = iTStatusVisible;
			}
		}

		public virtual void SetUpGrids()
		{
			DispositionGrid.ColumnProvider = new CustomsDispositionColumnProvider();
			EntrySummaryStatusGrid.ColumnProvider = new CustomsEntrySummaryStatusColumnProvider();
			if (USDeclaration != null && (USDeclaration.US_CargoReleaseType == CargoReleaseTypeList.Codes.ACE || USDeclaration.US_CargoReleaseType == CargoReleaseTypeList.Codes.SE))
			{
				PGAStatusGrid.ColumnProvider = new CustomsEntryPGAStatusColumnProvider();
				PGAStatusGrid.Visible = true;
				PGALineStatusGrid.Visible = false;
			}
			else if (USDeclaration != null && (USDeclaration.US_CargoReleaseType == CargoReleaseTypeList.Codes.ACS || USDeclaration.US_CargoReleaseType == CargoReleaseTypeList.Codes.BCR || USDeclaration.US_CargoReleaseType == CargoReleaseTypeList.Codes.CR))
			{
				PGALineStatusGrid.ColumnProvider = new CustomsPGALineStatusColumnProvider();
				PGALineStatusGrid.Visible = true;
				PGAStatusGrid.Visible = false;
			}
			else if (USDeclaration != null && USDeclaration.JE_ApplicationCode == JobApplicationCodeList.Codes.ACE)
			{
				PGAStatusGrid.ColumnProvider = new CustomsEntryPGAStatusColumnProvider();
				PGAStatusGrid.Visible = true;
				PGALineStatusGrid.Visible = false;
			}
			else
			{
				PGALineStatusGrid.Visible = false;
				PGAStatusGrid.Visible = false;
			}
		}

		public JobDeclaration USDeclaration
		{
			get { return DataSource as JobDeclaration; }
		}

		public override void PreDataBind()
		{
			if (USDeclaration != null)
			{
				var refreshReleaseStatusRecords = USDeclaration.CargoReleaseRecords;
				refreshReleaseStatusRecords = USDeclaration.ENSE0Records;

				SetupControl();
				SetUpGrids();
			}
		}

		public override void Bind(object dataSource)
		{
			base.BindControl<JobDeclaration>(dataSource);
		}
	}
}
