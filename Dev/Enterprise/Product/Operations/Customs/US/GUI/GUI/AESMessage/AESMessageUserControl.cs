using System;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.GUI
{
	public partial class AESMessageUserControl : Customs.GUI.BaseCustomsEntryUserControl
	{
		public AESMessageUserControl()
		{
			InitializeComponent();
			sEDsGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(SEDsGrid_ColourDeciding);
		}

		void SEDsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			Business.CusEntryHeader entryHeader = e.ObjectAtRow as Business.CusEntryHeader;
			if (entryHeader != null)
			{
				switch (entryHeader.CH_Status)
				{
					case AESDirectCustomsEntryStatus.Codes.OriginalSEDClear:
					case AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear:
						e.Colour = System.Drawing.Color.LightGreen;
						break;
					case AESDirectCustomsEntryStatus.Codes.Error:
					case AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired:
						e.Colour = System.Drawing.Color.LightCoral;
						break;
				}

				if (entryHeader.US_IsDeactivated)
				{
					e.Colour = System.Drawing.Color.Gray;
				}
			}
		}
	}
}
