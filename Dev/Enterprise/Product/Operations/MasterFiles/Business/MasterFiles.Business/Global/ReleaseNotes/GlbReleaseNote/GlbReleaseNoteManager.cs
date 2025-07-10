using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GlbReleaseNoteManager(BusinessObjectFactory factory, bool showNotesForAllCountries, string moduleToShowNotesFor)
			: base(factory)
		{
			BlankModule = string.IsNullOrEmpty(moduleToShowNotesFor);
			if (!BlankModule)
			{
				moduleToFilterFor = moduleToShowNotesFor;
			}
			showAllCountryNotes = showNotesForAllCountries;
		}

		public GlbReleaseNoteManager(BusinessObjectFactory factory)
			: this(factory, false, "")
		{
		}

		#region Properties

		readonly bool BlankModule;

		#region Module To Filter For
		[List("Modules")]
		[MaxLength(GlbReleaseNote.Schema.GF_CategoryMaxLength)]
		public ZString ModuleToFilterFor
		{
			get { return moduleToFilterFor; }
			set
			{
				if (moduleToFilterFor != value)
				{
					CheckMaximumLength(ModuleToFilterForInfo, value);
					SetNonPersistentPropertyValue(ModuleToFilterForInfo, ref moduleToFilterFor, value);
					ReloadReleaseNotes();
				}
			}
		}

		ZString moduleToFilterFor;

		public ZPropertyInfo ModuleToFilterForInfo
		{
			get { return GetZPropertyInfo(nameof(ModuleToFilterFor)); }
		}

		protected virtual bool ShouldFilterByModule
		{
			get { return true; }
		}

		#endregion

		#region Date To Filter After

		[BusinessObjectTestExclude]
		public ZDateTime DateToFilterAfter
		{
			get { return dateToFilterAfter; }
			set
			{
				if (dateToFilterAfter != value)
				{
					SetNonPersistentPropertyValue(DateToFilterAfterInfo, ref dateToFilterAfter, value);
					ReloadReleaseNotes();
				}
			}
		}
		ZDateTime dateToFilterAfter = GetDefaultDateAfterFilter();

		public ZPropertyInfo DateToFilterAfterInfo
		{
			get { return GetZPropertyInfo(nameof(DateToFilterAfter)); }
		}

		static ZDateTime GetDefaultDateAfterFilter()
		{
			var cutOffDate = ZDateTime.Today.AddMonths(-1);
			var enterpriseCDDate = (ZDateTime)Env.Registry.EnterpriseCDDate;
			if (enterpriseCDDate.IsValid && (enterpriseCDDate > cutOffDate))
			{
				return enterpriseCDDate;
			}

			return cutOffDate;
		}

		#endregion

		#region Date To Filter Before

		[BusinessObjectTestExclude]
		public ZDateTime DateToFilterBefore
		{
			get { return dateToFilterBefore; }
			set
			{
				if (dateToFilterBefore != value)
				{
					SetNonPersistentPropertyValue(DateToFilterBeforeInfo, ref dateToFilterBefore, value);
					ReloadReleaseNotes();
				}
			}
		}

		ZDateTime dateToFilterBefore;

		public ZPropertyInfo DateToFilterBeforeInfo
		{
			get { return GetZPropertyInfo(nameof(DateToFilterBefore)); }
		}

		#endregion

		#region Show Read Notes

		public ZBool ShowReadNotes
		{
			get { return showReadNotes; }
			set
			{
				SetNonPersistentPropertyValue(ShowReadNotesInfo, ref showReadNotes, value);
				ReloadReleaseNotes();
			}
		}

		protected ZBool showReadNotes;

		public ZPropertyInfo ShowReadNotesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowReadNotes)); }
		}

		#endregion

		#region Show Notes For All Countries

		public ZBool ShowAllCountryNotes
		{
			get { return showAllCountryNotes; }
			set
			{
				if (showAllCountryNotes != value)
				{
					SetNonPersistentPropertyValue(ShowAllCountryNotesInfo, ref showAllCountryNotes, value);
					ReloadReleaseNotes();
				}
			}
		}

		ZBool showAllCountryNotes;

		public ZPropertyInfo ShowAllCountryNotesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowAllCountryNotes)); }
		}

		#endregion

		#region Search Text

		[MaxLength(1000)]
		public ZString SearchText
		{
			get { return searchText; }
			set
			{
				if (searchText != value)
				{
					CheckMaximumLength(SearchTextInfo, value);
					SetNonPersistentPropertyValue(SearchTextInfo, ref searchText, value);
					ReloadReleaseNotes();
				}
			}
		}

		ZString searchText;

		public ZPropertyInfo SearchTextInfo
		{
			get { return GetZPropertyInfo(nameof(SearchText)); }
		}

		#endregion

		#region Do Not Display Until Next Upgrade

		public ZBool DoNotDisplayUntilNextUpgrade
		{
			get { return doNotDisplayUntilNextUpgrade; }
			set { SetNonPersistentPropertyValue(DoNotDisplayUntilNextUpgradeInfo, ref doNotDisplayUntilNextUpgrade, value); }
		}

		public ZPropertyInfo DoNotDisplayUntilNextUpgradeInfo
		{
			get { return GetZPropertyInfo(nameof(DoNotDisplayUntilNextUpgrade)); }
		}

		ZBool doNotDisplayUntilNextUpgrade = true;

		#endregion

		#region Show Updates on Login

		public ZBool DoNotDisplayUpdatesOnLogin
		{
			get { return doNotDisplayUpdatesOnLogin; }
			set { SetNonPersistentPropertyValue(DoNotDisplayUpdatesOnLoginInfo, ref doNotDisplayUpdatesOnLogin, value); }
		}

		public ZPropertyInfo DoNotDisplayUpdatesOnLoginInfo
		{
			get { return GetZPropertyInfo(nameof(DoNotDisplayUpdatesOnLogin)); }
		}

		ZBool doNotDisplayUpdatesOnLogin;

		#endregion

		#region Show Updates on Module Entry

		public ZBool DoNotDisplayUpdatesOnModuleEntry
		{
			get { return doNotDisplayUpdatesOnModuleEntry; }
			set { SetNonPersistentPropertyValue(DoNotDisplayUpdatesOnModuleEntryInfo, ref doNotDisplayUpdatesOnModuleEntry, value); }
		}

		public ZPropertyInfo DoNotDisplayUpdatesOnModuleEntryInfo
		{
			get { return GetZPropertyInfo(nameof(DoNotDisplayUpdatesOnModuleEntry)); }
		}

		ZBool doNotDisplayUpdatesOnModuleEntry;

		#endregion

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			OriginalDoNotDisplayUntilNextUpgrade = !string.IsNullOrEmpty(Env.Registry.DoNotDisplayReleaseNotesVersion) && Env.Registry.DoNotDisplayReleaseNotesVersion != null;
			if (Env.Registry.DoNotDisplayReleaseNotesVersion != null)
			{
				doNotDisplayUntilNextUpgrade = OriginalDoNotDisplayUntilNextUpgrade;
			}
			doNotDisplayUpdatesOnLogin = Env.Registry.DoNotDisplayUpdatesOnLogin;
			doNotDisplayUpdatesOnModuleEntry = Env.Registry.DoNotDisplayUpdatesOnModuleEntry;
			showReadNotes = Env.Registry.ShowReadNotes;
			if (ModuleToFilterFor.IsEmpty)
			{
				moduleToFilterFor = Env.Registry.ModuleToShowNotesFor;
			}
		}

		#endregion

		#region Clear Filter

		public void ClearFilter()
		{
			ModuleToFilterFor = "";
			SearchText = "";
			DateToFilterAfter = ZDateTime.Empty;
			DateToFilterBefore = ZDateTime.Empty;
		}

		#endregion

		#region Saving

		protected override void OnFactorySaving()
		{
			StoreDefaultsInRegistry();
			base.OnFactorySaving();
		}

		void StoreDefaultsInRegistry()
		{
			if (OriginalDoNotDisplayUntilNextUpgrade != DoNotDisplayUntilNextUpgrade)
			{
				Env.Registry.DoNotDisplayReleaseNotesVersion = DoNotDisplayUntilNextUpgrade ?
					ReleaseInfo.Instance.VersionNumber.ToString() : "";
				OriginalDoNotDisplayUntilNextUpgrade = DoNotDisplayUntilNextUpgrade;
			}

			Env.Registry.DoNotDisplayUpdatesOnLogin = doNotDisplayUpdatesOnLogin;
			Env.Registry.DoNotDisplayUpdatesOnModuleEntry = doNotDisplayUpdatesOnModuleEntry;
			Env.Registry.ShowReadNotes = showReadNotes;
			if (BlankModule)
			{
				Env.Registry.ModuleToShowNotesFor = moduleToFilterFor;
			}
		}

		ZBool OriginalDoNotDisplayUntilNextUpgrade;

		#endregion

		#region Release Notes

		public GlbReleaseNoteCollection ReleaseNotes
		{
			get
			{
				if (fReleaseNotes == null)
				{
					fReleaseNotes = new GlbReleaseNoteCollection(Factory, AdditionalReleaseNoteSectionsToShow);
					fReleaseNotes.AdditionalFilter = GetReleaseNotesFilter();
					fReleaseNotes.ApplySort(GlbReleaseNoteSchema.GF_ReleaseNoteDate.Name, ListSortDirection.Descending);
					RegisterEditableChildObject(fReleaseNotes);
				}

				return fReleaseNotes;
			}
		}

		GlbReleaseNoteCollection fReleaseNotes;

		protected virtual string[] AdditionalReleaseNoteSectionsToShow
		{
			get { return null; }
		}

		void ReloadReleaseNotes()
		{
			ReleaseNotes.AdditionalFilter = GetReleaseNotesFilter();
		}

		ZQuery GetReleaseNotesFilter()
		{
			ZQuery result = new ZQuery();

			if (!ShowAllCountryNotes)
			{
				ZQuery companyQuery = new ZQuery(GlbReleaseNoteSchema.GF_RN_NKCountryForReleaseNote, GlbReleaseNoteLookups.AllCountriesCode);
				companyQuery.AddToFilter(JoinCondition.Or, GlbReleaseNoteSchema.GF_RN_NKCountryForReleaseNote, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				result.AddToFilter(companyQuery);
			}

			if (!ModuleToFilterFor.IsEmpty && ShouldFilterByModule)
			{
				result.AddToFilter(GlbReleaseNoteSchema.GF_Category, ModuleToFilterFor);
			}

			if (!ShowReadNotes)
			{
				ZDBOnlyQuery readNotesQuery = new ZDBOnlyQuery(typeof(GlbReleaseNote));
				ZDBOnlySubQuery readNotesSubQuery = new ZDBOnlySubQuery(typeof(GlbReleaseNoteRead), GlbReleaseNoteReadSchema.GR_ReleaseNoteID, true);
				readNotesSubQuery.AddToFilter(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
				readNotesQuery.AddSubQuery(readNotesSubQuery, JoinCondition.And);
				result.AddToFilter(readNotesQuery);
			}

			if (!SearchText.IsEmpty)
			{
				if (SearchText.StartsWith("\"") && SearchText.EndsWith("\"") && (SearchText.Length != 1))
				{
					string textToFind = SearchText.Substring(1, SearchText.Length - 2);
					result.AddToFilter(GlbReleaseNoteSchema.GF_Summary, SQLComparisonOperator.Contains, textToFind);
				}
				else
				{
					ZString[] wordsToFind = SearchText.Split(' ');
					foreach (ZString word in wordsToFind)
					{
						result.AddToFilter(GlbReleaseNoteSchema.GF_Summary, SQLComparisonOperator.Contains, word);
					}
				}
			}

			if (DateToFilterAfter.IsValidSmallDateTime)
			{
				result.AddToFilter(GlbReleaseNoteSchema.GF_ReleaseNoteDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, DateToFilterAfter);
			}

			if (DateToFilterBefore.IsValidSmallDateTime)
			{
				result.AddToFilter(GlbReleaseNoteSchema.GF_ReleaseNoteDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, DateToFilterBefore);
			}

			return result;
		}

		#endregion

		#region Lookups

		#region Modules

		public CodeDescriptionPairList Modules
		{
			get { return new GlbReleaseNoteLookups(null).Categories; }
		}

		#endregion

		#endregion
	}
}
