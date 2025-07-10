using System;
using System.ComponentModel;
using Enterprise.Customs.GUI;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.SG.V4.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class SGTN41DeclarationUserControl : BaseCustomsEntryUserControl
	{
		public SGTN41DeclarationUserControl()
		{
			InitializeComponent();
		}

		public override Customs.Business.BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				base.JobDeclaration = value;
				if (JobDeclaration != null)
				{
					UnHookValueChanged(JobDeclaration as JobDeclaration);
					HookValueChanged(JobDeclaration as JobDeclaration);
					ShowOrHideCertificateOfOriginTabPage();
					AdjustForTradeNetVersion();
				}
			}
		}

		void HookValueChanged(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_MessageTypeInfo.ValueChanged += new EventHandler(JE_MessageTypeInfo_ValueChanged);
				declaration.JE_MessageSubTypeInfo.ValueChanged += new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
			}
		}

		void UnHookValueChanged(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= new EventHandler(JE_MessageTypeInfo_ValueChanged);
				declaration.JE_MessageSubTypeInfo.ValueChanged -= new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
			}
		}

		void JE_MessageSubTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideCertificateOfOriginTabPage();
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideCertificateOfOriginTabPage();
		}

		void ShowOrHideCertificateOfOriginTabPage()
		{
			if (CertificateOfOriginTabPage != null)
			{
				CertificateOfOriginTabPage.TabVisible = JobDeclaration != null && (JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.COO || (JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.OUT && JobDeclaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKO));
			}
		}

		void ShowOrHideOtherTaxField()
		{
			if (JobDeclaration is JobDeclaration declaration && TotalOtherTaxCalcEdit != null && TotalOtherTaxLabel != null)
			{
				TotalOtherTaxCalcEdit.Visible = declaration.IsTradeNet4Point1;
				TotalOtherTaxLabel.Visible = declaration.IsTradeNet4Point1;
			}
		}

		void AdjustForTradeNetVersion()
		{
			if (StartDateLabel != null)
			{
				StartDateLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2EEC8A4C-7D3F-4DF5-BF8C-50E6410F0C7B", "Start Date:");
			}
			ShowOrHideOtherTaxField();
		}

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<SGTN41DeclarationUserControl>().Result;
		}

		#endregion

		void CPCViewEditButton_Click(object sender, EventArgs e)
		{
			SGCPC cpc = null;

			if (CPCGrid.ListManager != null)
			{
				cpc = (SGCPC)CPCGrid.ListManager.GetCurrent();
			}

			if (cpc == null)
			{
				Globals.Message.ShowInformation("Please select (highlight) the appropriate CPC line.", "Edit CPC Line");
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new CPCForm(cpc));
			}
		}
	}
}
