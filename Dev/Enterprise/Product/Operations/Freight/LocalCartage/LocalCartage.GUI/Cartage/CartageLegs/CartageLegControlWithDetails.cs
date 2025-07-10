using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class CartageLegControlWithDetails : ZUserControl
	{
		public CartageLegControlWithDetails()
		{
			InitializeComponent();
			SetDataSourceBinding("CartageLegContainer", "BookedCtgMove+EW_JC_Container");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			SetTabVisibility(CurrentDataItem != null);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var isBoundToBizo = CurrentDataItem != null;
			if (!isBoundToBizo && RunSheetDetailsTabControl.SelectedTab != CartageLegDetailsTabPage)
			{
				RunSheetDetailsTabControl.SelectedTab = CartageLegDetailsTabPage; // bug - If the GPS Tab is selected, and find occurs, and no legs are shown in the planner, we experience a crash.
			}

			SetTabVisibility(isBoundToBizo);
		}

		void SetTabVisibility(bool showStandardTabs)
		{
			var showGPSTabs = showStandardTabs;

			if (BookedMovementTabPage.TabVisible != showStandardTabs)
			{
				BookedMovementTabPage.TabVisible = showStandardTabs;
			}

			if (ContainerOrLooseDetailsTabPage.TabVisible != showStandardTabs)
			{
				ContainerOrLooseDetailsTabPage.TabVisible = showStandardTabs;
			}

			if (GPSMessagesTabPage.TabVisible != showGPSTabs)
			{
				GPSMessagesTabPage.TabVisible = showGPSTabs;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZGuid CartageLegContainer
		{
			get { return cartageLegContainer; }
			set
			{
				if (cartageLegContainer != value)
				{
					cartageLegContainer = value;
					LooseDetailsGroupBox.Visible = value == ZGuid.Empty;
					ContainerGroupBox.Visible = value != ZGuid.Empty;
					RefrigerationGroupBox.Visible = value != ZGuid.Empty;
					ContainerOrLooseDetailsTabPage.Text = value == ZGuid.Empty ? Res.GetString("LocalCartage|RunSheet|LooseDetails", "Loose Details") : Res.GetString("LocalCartage|RunSheet|ContainerDetails", "Container Details");
				}
			}
		}
		ZGuid cartageLegContainer = ZGuid.Missing;

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<CartageLegControlWithDetails>()
			.Property("CartageLegContainer", ZGuid.Empty, false)
			.Result;
		}
	}
}
