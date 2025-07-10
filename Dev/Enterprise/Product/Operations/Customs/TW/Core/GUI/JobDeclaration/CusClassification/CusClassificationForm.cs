using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
{
	public class CusClassificationForm : Customs.GUI.BaseClassificationForm
	{
		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
		}

		protected override Customs.GUI.BaseClassificationUserControl GetUserControl()
		{
			return new CusClassificationUserControl();
		}
	}
}
