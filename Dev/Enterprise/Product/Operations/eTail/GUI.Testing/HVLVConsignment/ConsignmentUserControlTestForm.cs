using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI.Testing
{
	public class ConsignmentUserControlTestForm : ZForm
	{
		public HVLVConsignmentUserControl ConsignmentUserControl { get; }

		public ZGrid ConsignmentsGrid => ((ZModuleButtonGrid)Controls.Find("consignmentsGrid", true).SingleOrDefault())?.InnerGrid;
		public ZGrid ItemsGrid => (ZGrid)Controls.Find("itemsGrid", true).SingleOrDefault();
		public ZGrid ItemLinesGrid => (ZGrid)Controls.Find("itemLinesGrid", true).SingleOrDefault();

		public ConsignmentUserControlTestForm(IHVLVConsignmentCollectionParent dataSource)
			: base(dataSource)
		{
			ConsignmentUserControl = new HVLVConsignmentUserControl();
			Controls.Add(ConsignmentUserControl);
			ConsignmentUserControl.ShowAttachDetachButton = true;
		}

		public ConsignmentUserControlTestForm()
			: this(null)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				ConsignmentUserControl.Dispose();
			}
		}
	}
}
