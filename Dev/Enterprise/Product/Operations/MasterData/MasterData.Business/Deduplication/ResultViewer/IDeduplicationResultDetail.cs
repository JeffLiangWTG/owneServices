using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.MasterData.Business
{
	public interface IDeduplicationResultDetail : ISupportNotifyPropertyChanged
	{
		bool ReloadRequired { get; set; }

		SecurityCheckpoint ViewEntityFormCheckPoint { get; }

		ZGuid SelectedCandidatePK { get; set; }

		string MasterCode { get; }

		string CandidateCode { get; }

		bool IsEmpty { get; }

		bool IsLoadingData { get; set; }

		int CandidatesCount { get; }

		void LoadCandidatesAndUpdateSelectedItem();

		bool IsExcludingInactiveResults { get; set; }

		bool IsShowIgnoredResults { get; set; }

		bool IsExcludingOtherCountries { get; set; }

		bool ShowCheckBox { get; set; }

		bool ShowSelectAll { get; set; }

		void SelectOrDeSelectAll();

		bool ShowMergeAllSelectedButton { get; }

		bool EnableMergeAllSelectedButton { get; set; }

		void MergeAllSelected();

		bool ConfirmMergeCandidateIntoMaster(IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter);

		bool ConfirmMergeMasterIntoCandidate(IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter);

		bool ShowIgnoreAllButton { get; }

		bool ShowIgnoreButton { get; }

		bool ShowNotMatchButton { get; }

		void IgnoreOrNotMatch(string status, bool forAll);

		bool ShowRemoveIgnoresButton { get; }

		bool ShowRemoveAllIgnoresButton { get; set; }

		void RemoveIgnores(bool forAll);

		bool ShowToggleExclusionButton { get; }

		bool ShowDeactivateMasterButton { get; }

		bool ShowDeactivateButton { get; }

		bool ShowOpenMasterButton { get; }

		bool ShowOpenTargetButton { get; }

		bool ShowLinkButton { get; }

		WaterMarkType WaterMark { get; }

		void OpenMaster();

		void OpenTarget(string parameter);

		void DeactivateMaster();

		void DeactivateTarget();

		void ExecuteLink();

		void ToggleExclusion();

		void CandidateBizOFormClosedHandler(BusinessObject bizO);

		IEnumerable<DuplicationModelDetailGroup> GetSelectedCandidateDetails();
	}
}
