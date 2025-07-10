using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class GlbAccreditationAttemptFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public GlbAccreditationAttemptFilterControl()
		{
			InitializeComponent();
		}

		public GlbAccreditationAttemptFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new GlbAccreditationAttemptFilterStrip();
		}
	}
}
