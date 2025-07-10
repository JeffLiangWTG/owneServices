using System;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageLegUserControl : ZUserControl
	{
		public CartageLegUserControl()
		{
			InitializeComponent();
			new UNDGDataItemFormManager(null, "BookedCtgMove").Initialize(DGLinkLabel, DGSubstanceGuidFindBox, FlashPointCalcEdit, DGContactGuidFindBox);
		}

		CommonCartageLeg CommonCartageLeg
		{
			get { return (CommonCartageLeg)BindingSource.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (CommonCartageLeg != null)
			{
				LooseDetailsGroupBox.Visible = CommonCartageLeg.IsLoose;
				ContainerGroupBox.Visible = CommonCartageLeg.IsContainerised;
				RefrigerationGroupBox.Visible = CommonCartageLeg.IsContainerised;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.customFieldsDisplayControl1.NothingSetupMessageLabelText = Res.GetString("c1088e52-c9e4-4a8e-b8e6-5424b0ab943c", "To make use of this tab, please setup Port Transport Leg custom fields in Workflow Manager or on the Organization record.");
		}

		void OpenLocalTransportButton_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Cartage);
			var form = (CartageForm)controller.ShowEditForm(CommonCartageLeg.Cartage);
			if (form != null) // form is null if there is no security rights
			{
				form.SelectCartageLeg(CommonCartageLeg);
			}
		}
	}
}
