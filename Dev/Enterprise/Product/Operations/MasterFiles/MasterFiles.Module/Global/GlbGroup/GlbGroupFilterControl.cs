using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class GlbGroupFilterControl : ZFilterStripControl
	{
		public GlbGroupFilterControl()
		{
			InitializeComponent();
		}

		public GlbGroupFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new StaffSecurityFilterStrip();
		}

		protected override bool CanAcceptDataCore(IDataObject dataObject)
		{
			if (dataObject.GetDataPresent(GridRowsDataObject.DataFormatType) && dataObject is GridRowsDataObject gridRowsDataObject)
			{
				return gridRowsDataObject.Elements.All(bizo => bizo.BaseBusinessObject is GlbStaff);
			}

			return false;
		}

		protected override void AcceptDataCore(IDataObject dataObject, BusinessObject targetBizO)
		{
			var factory = targetBizO.Factory;
			var glbStaff = ((GridRowsDataObject)dataObject).Elements.Select(bizo => factory.ImportFromAnotherFactorySafe((GlbStaff)bizo.BaseBusinessObject));
			((GlbGroup)targetBizO)?.Staff.AddRange(glbStaff);
		}
	}
}
