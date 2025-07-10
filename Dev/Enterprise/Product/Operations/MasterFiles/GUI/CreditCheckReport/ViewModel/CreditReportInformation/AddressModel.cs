using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.MasterFiles.Business;
using WTG.ROPE.Model;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	[CodeAlive("CreditReportInformationWindow, deleted in WI00756942, see respective WI's e-doc for details.")]
	public class AddressModel : ModelBase<AddressModel>
	{
		public AddressModel(CreditReportExtractAddress address, OrgAddress matchAddress, List<OrgAddress> addressList)
		{
			Address1 = address.Address;
			City = address.City;
			MatchAddress = matchAddress;
			SelectedMergeAction = MatchAddress == null ? MergeAction.Codes.Add : MergeAction.Codes.Update;
			MergeActions = MergeAction.GetAddAndUpdateActions();
			AddressCollection = new ObservableCollection<OrgAddress>(addressList);
		}

		public string Address1 { get; }

		public string City { get; }

		public Dictionary<string, string> MergeActions { get; }

		public ObservableCollection<OrgAddress> AddressCollection { get; }

		string selectedMergeAction;
		public string SelectedMergeAction
		{
			get => selectedMergeAction;
			set
			{
				selectedMergeAction = value;
				if (selectedMergeAction == MergeAction.Codes.Add)
				{
					matchAddress = null;
				}
				NotifyPropertyChanged(nameof(MatchAddress));
			}
		}

		OrgAddress matchAddress;
		public OrgAddress MatchAddress
		{
			get => matchAddress;
			set
			{
				matchAddress = value;
				selectedMergeAction = MergeAction.Codes.Update;
				NotifyPropertyChanged(nameof(SelectedMergeAction));
			}
		}
	}
}
