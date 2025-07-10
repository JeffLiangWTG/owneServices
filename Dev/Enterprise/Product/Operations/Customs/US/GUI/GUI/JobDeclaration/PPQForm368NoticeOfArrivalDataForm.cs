using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class PPQForm368NoticeOfArrivalDataForm : ZChildForm
	{
		public PPQForm368NoticeOfArrivalDataForm(PPQForm368NoticeOfArrivalData bo)
			: base(bo)
		{
		}

		public new PPQForm368NoticeOfArrivalData BusinessEntity
		{
			get { return (PPQForm368NoticeOfArrivalData)base.BusinessEntity; }
		}

		void DefaultButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.LoadDefault();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.SaveData();
			Close();
		}

		void CancelAndCloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
