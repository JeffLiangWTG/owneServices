using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbPortDeliveryTimeForm : ZTemplateForm
	{
		public GlbPortDeliveryTimeForm(GlbPortDeliveryTime businessEntity) : base(businessEntity)
		{
		}

		protected GlbPortDeliveryTime PortDeliveryTime
		{
			get { return (GlbPortDeliveryTime)BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				if (!this.IsDesignMode())
				{
					return Res.GetString("GlbPortDeliveryTimeForm|FormCaption", "Port Delivery Time -") + " " + PortDeliveryTime.Company.GC_Name;
				}
				else
				{
					return base.FormCaption;
				}
			}
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
