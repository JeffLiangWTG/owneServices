using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ManualDataExportForm : ZChildForm
	{
		public ManualDataExportForm(BusinessObjectFactory factory, IWorkflowProvider parent, UniversalDataType dataType, IEnumerable<IEDICommunicationsMode> communicationModes = null, IUniversalXmlSchema schemaOverride = null)
			: this(factory, new[] { parent }, dataType, communicationModes, schemaOverride)
		{
		}

		public ManualDataExportForm(BusinessObjectFactory factory, IWorkflowProvider[] parents, UniversalDataType dataType, IEnumerable<IEDICommunicationsMode> communicationModes = null, IUniversalXmlSchema schemaOverride = null)
			: base(new ManualDataExport(factory, parents, dataType, communicationModes, schemaOverride))
		{
			dataExportBO = (ManualDataExport)DataSource;
			dataExportBO.Calc_RecipientTypeInfo.ValueChanged += Calc_RecipientTypeInfo_ValueChanged;
			InitializeComponent();
			originalHeight = this.Size.Height;
			ChangeVisibility();
		}

		internal readonly ManualDataExport dataExportBO;

		ManualDataExportForm()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				throw new InvalidOperationException("This constructor is only for use with the Designer.");
			}
		}

		public override string FormHeading
		{
			get { return Res.GetString("210050a9-0b14-4051-ba41-e167d7ac7ba9", "Manually {0}", dataExportBO.SendDataDescription); }
		}

		void SendAndCloseButton_Click(object sender, EventArgs e)
		{
			dataExportBO.RunPreSaveValidation();

			if (dataExportBO.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("f63d112a-8eaf-4197-8b86-b6968bc7dc0c",
					@"You cannot send a {0} without filling in all mandatory fields.

Please fix all validation errors and try again.", dataExportBO.DataTypeName), dataExportBO.SendDataDescription);
			}
			else
			{
				SendData();
			}
		}

		void SendData()
		{
			using (var progressForm = new ManualDataExportProgressForm(dataExportBO.DataTypeName))
			{
				new ManualDataExportProgressFormSendOnShow(dataExportBO).Apply(progressForm);

				progressForm.Closed += (s, e) => Close();
				progressForm.Shown += (s, e) => Hide();

				ZFormModaliser.ShowDialogWithoutDispose(progressForm);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ChangeVisibility()
		{
			var showOtherRecipient = dataExportBO.IsOtherRecipient;
			var showRecipientService = dataExportBO.IsRecipientServiceAvailable;
			var reduceFormHeightBy = !showOtherRecipient ? 44 : 0;
			reduceFormHeightBy += !showRecipientService ? 23 : 0;

			RecipientPKGuidFindBox.Visible = showOtherRecipient;
			RecipientTypeDropEdit.Visible = showOtherRecipient;
			RecipientServiceDropEdit.Visible = showRecipientService;
			Size = ControlDpiScalingHelper.NewScaledSize(this.Size.Width, originalHeight - ControlDpiScalingHelper.ScaleToCurrentDpiY(reduceFormHeightBy), false);
		}
		readonly int originalHeight;

		void Calc_RecipientTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibility();
		}

		void DisposeCore()
		{
			if (dataExportBO != null)
			{
				dataExportBO.Calc_RecipientTypeInfo.ValueChanged -= Calc_RecipientTypeInfo_ValueChanged;
				dataExportBO.Dispose();
			}
		}
	}
}
