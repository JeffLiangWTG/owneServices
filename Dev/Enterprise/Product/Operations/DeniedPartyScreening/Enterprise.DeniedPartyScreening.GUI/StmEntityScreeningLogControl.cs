using System;
using System.ComponentModel;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.DeniedPartyScreening.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class StmEntityScreeningLogControl : ZUserControl
	{
		public StmEntityScreeningLogControl()
		{
			InitializeComponent();

			LogsGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(LogsGrid_ColourDeciding);
		}

		#region Color Deciding

		DeniedPartyGridColorHelper GridColorHelper => gridColorHelper ?? (gridColorHelper = new DeniedPartyGridColorHelper());
		DeniedPartyGridColorHelper gridColorHelper;

		void LogsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var status = (StmEntityScreeningLog)e.ObjectAtRow;
			e.Colour = GridColorHelper.GetColorForResultStatus(status);
		}

		#endregion

		#region Binding

		public static void GetPropertyDescriptors()
		{
		}

		#endregion
	}
}
