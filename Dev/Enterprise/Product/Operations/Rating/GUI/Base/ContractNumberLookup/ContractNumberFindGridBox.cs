using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Rating.GUI
{
	internal class ContractNumberGridFindBox : ZGridFindBox, IContractNumberFindBoxPopupSupport
	{
		public IRateEntry RateEntry { get; set; }

		public string ContractNumber
		{
			get
			{
				return Code;
			}
			set
			{
				Code = value;
			}
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return new ContractNumberFindBoxPopup();
		}
	}
}
