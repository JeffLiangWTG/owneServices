using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.HRM.Module
{
	public partial class ReviewProcessNodeFilterControl : ZFilterStripControl
	{
		[Obsolete]
		public ReviewProcessNodeFilterControl() : base()
		{
			InitializeComponent();
		}

		public ReviewProcessNodeFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
