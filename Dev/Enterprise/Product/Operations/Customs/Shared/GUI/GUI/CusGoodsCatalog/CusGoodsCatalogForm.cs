using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusGoodsCatalogForm : ZTemplateForm
	{
		public CusGoodsCatalogForm() : base()
		{
			if (!this.IsDesignMode())
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public CusGoodsCatalogForm(BaseCusGoodsCatalog businessEntity)
			: base(businessEntity)
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("62A580B6-332F-45AD-B3E6-599AFF1D0B66", "Goods Catalog - {0}", BusinessEntity?.CGC_Description); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			if (!this.IsDesignMode())
			{
				WorkflowTabPage.TabRelevant = BusinessEntity.SupportsWorkflow;
				if (BusinessEntity.SupportsWorkflow)
				{
					WorkflowTabPage.Initialize(BusinessEntity);
				}

				var control = GetUserControl();
				if (control != null)
				{
					control.Dock = DockStyle.Fill;
					control.Visible = true;
					MainTabPage.Controls.Add(control);
				}
			}
		}

		protected virtual CusGoodsCatalogUserControl GetUserControl()
		{
			return new CusGoodsCatalogUserControl();
		}

		protected new BaseCusGoodsCatalog BusinessEntity
		{
			get { return (BaseCusGoodsCatalog)base.BusinessEntity; }
		}
	}
}
