using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RateFormulaBox : ZGridFindBox
	{
		public RateFormulaBox()
		{
			PopupButtonReadonlyCanBeDifferent = true;
			CodeBox.ReadOnly = true;
			CodeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		}

		internal RateFormulaForm rateFormulaForm;

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}

		public IUnitListForRateFormulaEditProvider UnitListProvider;

		protected override IFindBoxPopup PopupForm
		{
			get
			{
				if (rateFormulaForm == null || rateFormulaForm.IsDisposed)
				{
					rateFormulaForm = new RateFormulaForm(new RateFormulaEditHelper(UnitListProvider), CodeBox.MaxLength);
				}
				return rateFormulaForm;
			}
		}
	}
}
