using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed partial class ContainerDetentionFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public ContainerDetentionFilterControl()
		{
			InitializeComponent();
		}

		public ContainerDetentionFilterControl(IBusinessObjectCollection collection, ContainerDetentionFilterStrip strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ContainerDetentionModuleStrip();
		}
	}
}
