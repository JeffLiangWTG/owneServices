using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CustomConversionFactorForm : ZChildForm
	{
		public CustomConversionFactorForm(RateLine line)
			: base(new CustomConversionFactor(line))
		{
			InitializeComponent();
		}

		public CustomConversionFactorForm()
			: base(new CustomConversionFactor(null))
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		public ZString ConversionFactor
		{
			get { return ((CustomConversionFactor)BusinessEntity).ConversionFactor; }
		}

		#region IDisposable Members

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

