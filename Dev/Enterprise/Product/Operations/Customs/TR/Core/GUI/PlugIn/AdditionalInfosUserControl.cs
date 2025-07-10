using System.Collections.Generic;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.GUI.PlugIn
{
	public partial class AdditionalInfosUserControl : EU.GUI.PlugIn.AdditionalInfosUserControl
	{
		public AdditionalInfosUserControl()
		{
			InitializeComponent();
			ChangeColumnsInGrid();
		}

		void ChangeColumnsInGrid()
		{
			using (AdditionalInfosGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AdditionalInfosGrid.ReOrderColumns(ReorderedColumnsSequence);
				AdditionalInfosGrid.SetAllAvailability(false);
				AdditionalInfosGrid.SetAvailability(true, ReorderedColumnsSequence);
			}
		}

		string[] ReorderedColumnsSequence
		{
			get
			{
				if (reorderedColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						CusSupportingInfoSchema.Constants.CSI_Description
					};
					reorderedColumnsSequence = columnList.ToArray();
				}
				return reorderedColumnsSequence;
			}
		}
		string[] reorderedColumnsSequence;
	}
}
