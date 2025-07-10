using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.HRM.Module
{
	public partial class ReviewProcessFilterControl : ZFilterStripControl
	{
		[Obsolete]
		public ReviewProcessFilterControl() : base()
		{
			InitializeComponent();
		}

		public ReviewProcessFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
