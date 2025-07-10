using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefComplianceListUserControlForTest : RefComplianceListUserControl
	{
		public ZTextBox ListCodeTextBox_Exposed => base.ListCodeTextBox;
		public ZTextBox ListTypeTextBox_Exposed => base.ListTypeTextBox;
		public ZTextBox PublisherJurisdictionTextBox_Exposed => base.PublisherJurisdictionTextBox;

		public ZTextBox ListNameTextBox_Exposed => base.ListNameTextBox;
		public ZTextBox PublisherNameTextBox_Exposed => base.PublisherNameTextBox;
		public ZTextBox MainSourceTextBox_Exposed => base.MainSourceTextBox;
		public ZTextBox SecondarySourceTextBox_Exposed => base.SecondarySourceTextBox;

		public ZTextBox IntegrationDateTextBox_Exposed => base.IntegrationDateTextBox;
		public ZTextBox ModificationDateTextBox_Exposed => base.ModificationDateTextBox;

		public ZCheckBox IsSystemCheckBox_Exposed => base.IsSystemCheckBox;
		public ZCheckBox IsActiveCheckBox_Exposed => base.IsActiveCheckBox;

		public ZTextBox ListDescriptionTextBox_Exposed => base.ListDescriptionTextBox;
		public ZTextBox PublisherDetailsTextBox_Exposed => base.PublisherDetailsTextBox;

		public ZCheckBox IsExcludedCheckBox_Exposed => base.IsExcludedCheckBox;
	}
}
