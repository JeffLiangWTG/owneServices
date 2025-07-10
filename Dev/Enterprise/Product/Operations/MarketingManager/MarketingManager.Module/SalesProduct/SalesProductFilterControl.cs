using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.Module
{
	public partial class SalesProductFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a filter business object, this constructor is just for the designer", true)]
		public SalesProductFilterControl()
		{
		}

		public SalesProductFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
