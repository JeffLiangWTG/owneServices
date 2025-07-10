using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgAddressesFilterControl : ZFilterStripControl
	{
		public OrgAddressesFilterControl(BusinessObjectFactory factory) : this(new OrgAddressCollection(factory), new OrgAddressesFilterBusinessObject()) { }

		public OrgAddressesFilterControl(IBusinessObjectCollection gridCollection, OrgAddressesFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			CopyColumnsFromAddressesUserControl();
		}

		void CopyColumnsFromAddressesUserControl()
		{
			using (var addressesUserControl = new AddressesUserControl())
			{
				var grid = addressesUserControl.GetType().GetField("OrgAddressBoundGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(addressesUserControl) as ZGrid;
				FilteredGrid.ColumnStyles.AddRange(grid.ColumnStyles);
			}
		}
	}
}
