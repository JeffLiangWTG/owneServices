using System.Linq;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	internal partial class PersonFilterContentControl : ZUserControl
	{
		public PersonFilterContentControl()
		{
			InitializeComponent();
			BindingSource.DataSource = PersonFilterDataSource;
			BindCheckBoxTags();
			HookEvents();
		}

		PersonFilterDataSource PersonFilterDataSource => personFilterItemDataSource ?? (personFilterItemDataSource = new PersonFilterDataSource());
		PersonFilterDataSource personFilterItemDataSource;

		DeduplicationPersonResultDetail duplicationResultDetail;

		bool IsDropEditValueLegal => !string.IsNullOrEmpty(PersonFilterDataSource.PersonFilterActive)
				&& !string.IsNullOrEmpty(PersonFilterDataSource.PersonFilterEmail)
				&& !string.IsNullOrEmpty(PersonFilterDataSource.PersonFilterName)
				&& !string.IsNullOrEmpty(PersonFilterDataSource.PersonFilterPhone);

		internal void SetDataContext(DeduplicationPersonResultDetail duplicationResultDetail, PotentialDuplicatesUserControl potentialDuplicatesUserControl)
		{
			this.duplicationResultDetail = duplicationResultDetail;
			this.duplicationResultDetail.MeetFilterConditions = MeetFilterConditions;
		}

		void BindCheckBoxTags()
		{
			CheckBoxLow.Tag = ConfidenceRating.Low;
			CheckBoxHigh.Tag = ConfidenceRating.High;
			CheckBoxMedium.Tag = ConfidenceRating.Medium;
		}

		void HookEvents()
		{
			FindButton.Click += (o, e) => ExecuteFilter();
			ClearButton.Click += (o, e) => ClearFilter();
		}

		void ExecuteFilter()
		{
			if (duplicationResultDetail == null)
			{
				return;
			}
			duplicationResultDetail.FilterAndUpdate();

			if (!duplicationResultDetail.DuplicationCandidates.Any())
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("811DE724-F6DF-4BA0-AE50-99D0A0AE9A9A", "There are no records that match your search."));
			}
		}

		void ClearFilter()
		{
			PersonFilterDataSource.PersonFilterActive = PersonFilterActivesList.Descriptions.All.GetUnresolvedString();
			PersonFilterDataSource.PersonFilterEmail = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();
			PersonFilterDataSource.PersonFilterName = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();
			PersonFilterDataSource.PersonFilterPhone = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();
			PersonFilterDataSource.PersonFilterEmailKeyword = string.Empty;
			PersonFilterDataSource.PersonFilterPhoneKeyword = string.Empty;
			PersonFilterDataSource.PersonFilterNameKeyword = string.Empty;
			CheckBoxHigh.Checked = false;
			CheckBoxMedium.Checked = false;
			CheckBoxLow.Checked = false;
			ExecuteFilter();
		}

		FilterCondition GetFilterCondition(string code)
		{
			if (code == PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString())
			{
				return new FilterCondition(FilterConditionType.Contains);
			}

			return new FilterCondition(FilterConditionType.ExactMatch);
		}

		bool MeetFilterConditions(DuplicationPersonCandidate candidate)
		{
			var selectedActiveState = PersonFilterDataSource.PersonFilterActive;
			var selectedRatings = PanelCheckBoxList.Controls.OfType<ZCheckBox>().Where(x => x.Checked).Select(x => x.Tag.ToString()).ToList();
			var emailFilterCondition = GetFilterCondition(PersonFilterDataSource.PersonFilterEmail);
			var nameFilterCondition = GetFilterCondition(PersonFilterDataSource.PersonFilterName);
			var phoneFilterCondition = GetFilterCondition(PersonFilterDataSource.PersonFilterPhone);

			return IsDropEditValueLegal
				&& FilterHelper.PersonActiveFilter(candidate, selectedActiveState)
				&& FilterHelper.PersonMultiConfidenceFilter(candidate, selectedRatings)
				&& FilterHelper.PersonNameFilter(candidate.GlbPerson, nameFilterCondition, PersonFilterDataSource.PersonFilterNameKeyword)
				&& FilterHelper.PersonEmailFilter(candidate.GlbPerson, emailFilterCondition, PersonFilterDataSource.PersonFilterEmailKeyword)
				&& FilterHelper.PersonPhoneFilter(candidate.GlbPerson, phoneFilterCondition, PersonFilterDataSource.PersonFilterPhoneKeyword);
		}
	}
}
