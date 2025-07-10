using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.LandedCosting.GUI
{
	public class LandedCostingHistoryViewPlugIn : ZPlugIn
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public const string PlugInName = "Landed Cost History";

		public LandedCostingHistoryViewPlugIn(ILandedCostHistoryMaster lCHistoryHost) : base(lCHistoryHost as IBusiness)
		{
			LCHistoryMaster = lCHistoryHost;
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Forwarder; }
		}

		public override string Name
		{
			get { return PlugInName; }
		}

		GenericLandedCostHistoryCollection fHistories;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (fHistories == null)
			{
				fHistories = new GenericLandedCostHistoryCollection(LCHistoryMaster);
				fHistories.Load();
				fHistories.SetReadOnlyIncludingChildren(true);
			}
			return fHistories;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			LandCostHistoryUserControl result = new LandCostHistoryUserControl();
			result.LCHistoryMaster = LCHistoryMaster;
			return result;
		}

		protected readonly ILandedCostHistoryMaster LCHistoryMaster;
	}
}
