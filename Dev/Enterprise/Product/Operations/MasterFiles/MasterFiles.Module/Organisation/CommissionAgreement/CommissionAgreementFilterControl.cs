using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class CommissionAgreementFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public CommissionAgreementFilterControl()
		{
			InitializeComponent();
		}

		public CommissionAgreementFilterControl(IBusinessObjectCollection collection, CommissionAgreementFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
