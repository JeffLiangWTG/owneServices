using System.ComponentModel;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.DeniedPartyScreening.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class RelatedDeniedPartyScreeningStatusControl : ZUserControl
	{
		public RelatedDeniedPartyScreeningStatusControl()
		{
			InitializeComponent();
			LogsGrid.ColourDeciding += LogsGrid_ColourDeciding;
		}

		DeniedPartyGridColorHelper GridColorHelper => gridColorHelper ?? (gridColorHelper = new DeniedPartyGridColorHelper());
		DeniedPartyGridColorHelper gridColorHelper;

		void LogsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var status = (IRelatedOrgPartyScreeningStatus)e.ObjectAtRow;
			e.Colour = GridColorHelper.GetColorForResultStatus(status);
		}

		public static void GetPropertyDescriptors()
		{
		}
	}
}

