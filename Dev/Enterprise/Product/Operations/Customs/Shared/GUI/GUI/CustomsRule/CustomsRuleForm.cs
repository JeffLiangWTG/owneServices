using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CustomsRuleForm : ZTemplateForm
	{
		public CustomsRuleForm()
			: base()
		{
			if (!DesignMode)
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public CustomsRuleForm(CustomsRule customsRule)
			: base(customsRule)
		{
			InitializeComponent();
		}

		public override string FormCaption => BusinessEntity.HumanReadableName;

		protected new CustomsRule BusinessEntity
		{
			get { return (CustomsRule)base.BusinessEntity; }
		}
	}
}
