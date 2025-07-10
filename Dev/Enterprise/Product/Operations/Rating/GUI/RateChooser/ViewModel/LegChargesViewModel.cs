using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateChooser.ViewModel
{
	/// <summary>
	///		Defines a group of charges, i.e. inland, outland, ocean.
	/// </summary>
	public class LegChargesViewModel : ViewModelWithNotificationBase
	{
		public LegChargesViewModel(string leg, IEnumerable<ChargesViewModel> charges)
		{
			Leg = leg;
			Charges = new ObservableCollection<ChargesViewModel>(charges);
		}

		public string Leg { get; }
		public IEnumerable<ChargesViewModel> Charges { get; }
		public ChargesViewModel BaseCharges => Charges.FirstOrDefault(c => c.Group == ChargesViewModel.ChargesGroup.Base);
		public ChargesViewModel AdditionalCharges => Charges.FirstOrDefault(c => c.Group == ChargesViewModel.ChargesGroup.Additional);

		public bool HeaderVisibility
		{
			get { return !string.IsNullOrEmpty(Leg); }
		}
	}
}
