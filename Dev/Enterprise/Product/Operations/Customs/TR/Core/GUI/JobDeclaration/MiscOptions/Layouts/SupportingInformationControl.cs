using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using AdditionalInfo = Enterprise.Customs.TR.Business.Declaration.AdditionalInfo;

namespace Enterprise.Customs.TR.GUI
{
	public partial class SupportingInformationControl : EU.GUI.SupportingInformationControl
	{
		public SupportingInformationControl()
		{
			InitializeComponent();
			ReorderTabPages();
		}

		protected override Type GetSupportingDocumentsUserControlType() => typeof(SupportingDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControl);

		protected override ColumnWidth[] GetAdditionalInfosColumnWidths()
		{
			return new[]
			{
				new ColumnWidth(AdditionalInfo.Schema.CSI_Description, 520)
			};
		}

		void ReorderTabPages()
		{
			SupportingInformationTabControl.TabPages.Remove(TariffQuestionsTabPage);
			SupportingInformationTabControl.TabPages.Insert(TariffQuestionsTabPage, 0);
			SupportingInformationTabControl.TabPages.Remove(SupportingDocumentTabPage);
			SupportingInformationTabControl.TabPages.Insert(SupportingDocumentTabPage, 1);
			SupportingInformationTabControl.TabPages.Remove(AdditionalInfoTabPage);
			SupportingInformationTabControl.TabPages.Insert(AdditionalInfoTabPage, 2);
			SupportingInformationTabControl.TabPages.Remove(PreviousDocumentTabPage);
			SupportingInformationTabControl.TabPages.Insert(PreviousDocumentTabPage, 3);
		}
	}
}
