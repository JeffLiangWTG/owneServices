using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public partial class HVLVISFMessagesSendForm : ZChildForm
	{
		public HVLVISFMessagesSendForm(HVLVISFMetaHeader header) : base(header)
		{
			InitializeComponent();
		}

		public IEnumerable<CusISFHeader> SelectedCusISFHeaders
		{
			get
			{
				return isfHeadersGrid.List.Cast<HVLVISFMessagesSendWrapper>()
					.Where(wrapper => wrapper.ShouldSend)
					.Select(wrapper => (CusISFHeader)wrapper.RelatedJob)
					.ToList();
			}
		}

		void ButtonSelectAll_Click(object sender, System.EventArgs e)
		{
			var messagesSendWrappersInGrid = isfHeadersGrid.List.Cast<HVLVISFMessagesSendWrapper>();

			SelectOrDeselectAll(messagesSendWrappersInGrid);
		}

		void SelectOrDeselectAll(IEnumerable<HVLVISFMessagesSendWrapper> messagesSendWrappers)
		{
			var messagesSendWrappersSelected = messagesSendWrappers.Where(r => r.ShouldSend);
			var shouldSelectAll = messagesSendWrappers.Count() != messagesSendWrappersSelected.Count();

			messagesSendWrappers.ForEach(c => c.ShouldSend = shouldSelectAll);
		}

		void ButtonSend_Click(object sender, System.EventArgs e)
		{
			var selectedRows = isfHeadersGrid.List.Cast<HVLVISFMessagesSendWrapper>().Where(wrapper => wrapper.ShouldSend);

			if (selectedRows.Any())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("e94d92c3-41ec-4888-90b9-29d37d24f603", "No Job Number has been flagged for submission."));
			}
		}

		void ButtonCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
