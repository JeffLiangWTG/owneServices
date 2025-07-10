using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module.DialogDefault;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class DialogDefaultFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public DialogDefaultFilterControl()
		{
			InitializeComponent();
		}

		public DialogDefaultFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new DialogDefaultFilterStrip();
	}
}
