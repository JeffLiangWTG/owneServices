using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.GUI
{
	public partial class InBondNumberSettingForBranchSpecificForm : NumberSettingForNonBranchSpecificForm
	{
		public InBondNumberSettingForBranchSpecificForm()
		{
		}

		public InBondNumberSettingForBranchSpecificForm(InBondNumberSetting setting)
			: base(setting)
		{
		}

		public new InBondNumberSetting BusinessEntity
		{
			get { return (InBondNumberSetting)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			if (InBondNumberGenerator.IsInBondNumberRangeByCompany(GlbCompany.CurrentCompany.PK))
			{
				CompanyGuidFindBox.CodeBox.Text = GlbCompany.CurrentCompany.GC_Code;
				CompanyGuidFindBox.Refresh();
				BranchGuidFindBox.ReadOnly = true;
			}
			else
			{
				CompanyGuidFindBox.ReadOnly = true;
			}
		}
	}
}
