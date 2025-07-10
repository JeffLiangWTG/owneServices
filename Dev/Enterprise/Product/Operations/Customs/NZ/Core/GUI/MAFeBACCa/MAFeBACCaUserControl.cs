
namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	using System;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MAFeBACCa;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.ZArchitecture.GUI;

	public partial class MAFeBACCaUserControl : ZUserControl, IDisposable
	{
		public MAFeBACCaUserControl()
		{
			InitializeComponent();
		}

		public MAFeBACCaUserControl(MAFMessagingBO mafMessaging)
		{
			MAFMessaging = mafMessaging;
			plugInSupport = MAFMessaging.PlugInSupport;
			InitializeComponent();
			InitializeVisibility();
		}

		void InitializeVisibility()
		{
			var useIPIMessage = true;
			var isDeclaration = plugInSupport.Master is JobDeclaration;
			MessagesTabPage.TabVisible = MAFMessaging.Messages.Count > 0;
			TSWMessagesTabPage.TabVisible = useIPIMessage;
			CommentsGroupBox.Visible = isDeclaration;
			ExporterContactGroupBox.Visible = isDeclaration;
			FlagsGroupBox.Visible = isDeclaration;
			PaymentGroupBox.Location = isDeclaration ? CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 279) : CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 192);
			ImporterContactGroupBox.Location = isDeclaration ? CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 272) : CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 396);
			ImporterContactGroupBox.Text = isDeclaration ? Res.GetString("DC076786-5D02-4623-9801-7B9025A9E349", "Importer Contact Details") : Res.GetString("982245C8-6AC9-43B7-9FDC-090013120E51", "Receiving Agent Contact Details");
			AttachedFilesGroupBox.Size = isDeclaration ? CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 265) : CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 387);
			AttachedFilesGroupBox.Visible = !useIPIMessage;
			ImporterContactGroupBox.Visible = !useIPIMessage;
			DetailsGroupBox.Visible = !useIPIMessage;
			ConsignmentTypeDropEdit.ReadOnly = !isDeclaration;
			CargoTypeDropEdit.ReadOnly = !isDeclaration;
			MessagingStatusDescriptionTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 26);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(MAFMessaging, "");
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible)
			{
				MAFMessaging.RefreshBindingIncludingChildren();
				CargoTypeDropEdit.Visible = plugInSupport.CargoTypeVisible;
			}
			base.OnVisibleChanged(e);
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}

		public MAFMessagingBO MAFMessaging { get; set; }
		readonly IMAFPlugInSupport plugInSupport;
	}
}
