using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.Module
{
	public class CusClassificationFilterControl : CusClassificationFilterControl<ZFilterStrip>
	{
		public CusClassificationFilterControl()
		{
		}

		public CusClassificationFilterControl(IBusinessObjectCollection gridCollection, CusClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}

	public partial class CusClassificationFilterControl<T> : ZFilterStripControl<T> where T : ZFilterStrip, new()
	{
		private readonly System.ComponentModel.Container components;

		public CusClassificationFilterControl()
		{
			InitializeComponent();
		}

		public CusClassificationFilterControl(IBusinessObjectCollection gridCollection, CusClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}

