using System;
using System.ComponentModel;
using Enterprise.Customs.GUI;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.SG.V4.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class SGDeclarationUserControl : BaseCustomsEntryUserControl
	{
		public SGDeclarationUserControl()
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
				StartDateLabel.Text = "Removal Start Date:";
			}

			ShowOrHideOtherTaxField();
		}

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<SGDeclarationUserControl>().Result;
		}

		#endregion
	}
}
