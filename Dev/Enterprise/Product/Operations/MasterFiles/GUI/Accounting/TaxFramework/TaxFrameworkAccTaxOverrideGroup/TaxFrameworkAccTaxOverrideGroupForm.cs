using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxFrameworkAccTaxOverrideGroupForm : AccTaxOverrideGroupFormBase
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public TaxFrameworkAccTaxOverrideGroupForm()
		{
			InitializeComponent();
		}

		public TaxFrameworkAccTaxOverrideGroupForm(AccTaxOverrideGroup taxOverrideGroup) : base(taxOverrideGroup)
		{
			taxOverrideGroup.Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
		}

		public override string FormCaption
		{
			get
			{
				return Res.GetString("DDF7985A-D678-485E-A0D6-7B1BAE759AF3", "Tax Configuration Override Group");
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();
		}
	}
}
