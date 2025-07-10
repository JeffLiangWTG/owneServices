using System;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class ProcessTaskFilterControl : ZFilterStripControl
	{
		public ProcessTaskFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			FilteredGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(FilteredGrid_ColourDeciding);
			InitializeBMSColumns();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ProcessTaskFilterStrip();
		}

		internal static string[] BMSRestrictedColumnNames
		{
			get
			{
				return new[]
				{
					"ProcessHeader+DoNotStartBeforeDateLocal",
					"ProcessHeader+AgreedDeliveryDateLocal",
					"ProcessHeader+LastTransferDateLocal",
					"EffectiveTaskNudge"
				};
			}
		}

		void InitializeBMSColumns()
		{
			if (!ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				var columns = Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>()
					.Where(c => BMSRestrictedColumnNames.Contains(c.ColumnName)).ToArray();

				foreach (var column in columns)
				{
					grid.ColumnStyles.Remove(column);
				}
			}
		}

		#region Grid Colours

		internal void FilteredGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			ProcessTask task = (ProcessTask)e.ObjectAtRow;
			if (task.IsBehindSchedule)
			{
				e.Colour = Color.LightSalmon;
			}
			else if (task.IsSuspended)
			{
				e.Colour = Color.Yellow;
			}
		}

		#endregion
	}
}
