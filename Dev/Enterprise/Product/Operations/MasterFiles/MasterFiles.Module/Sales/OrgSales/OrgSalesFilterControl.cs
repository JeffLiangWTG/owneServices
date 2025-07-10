using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgSalesFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public OrgSalesFilterControl()
		{
			InitializeComponent();
		}

		public OrgSalesFilterControl(IBusinessObjectCollection gridCollection, OrgSalesFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
