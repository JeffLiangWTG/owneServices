using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class JobSeaVoyageFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public JobSeaVoyageFilterControl()
		{
			InitializeComponent();
		}

		public JobSeaVoyageFilterControl(IBusinessObjectCollection collection, JobSeaVoyageFilterStrip strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new JobSeaVoyageModuleStrip();
		}
	}
}
