using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgMatchApproval : AutoOrgMatchApproval
	{
		protected OrgMatchApproval(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P2_MatchUser1), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P2_MatchUser2), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P2_OH_MatchOrg1), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P2_OH_MatchOrg2), ConcurrencyPolicy.Ignore);
		}

		public static readonly OrgMatchApprovalTypeDecider TypeDecider = new OrgMatchApprovalTypeDecider();

		public static class MatchStatus
		{
			public static string Unmatched
			{
				get { return Res.GetString("d1c05d8b-4b05-4eea-b756-c79353d046a8", "Unmatched"); }
			}
			public static string PartiallyMatchedByCurrentUser
			{
				get { return Res.GetString("d1336166-ae4d-4614-bf93-ea52b284fde7", "Partially matched by you"); }
			}
			public static string PartiallyMatched
			{
				get { return Res.GetString("7b954649-9c67-42f8-b6dc-c2c3a9e671e2", "Partially matched"); }
			}
			public static string MatchMadeWithConflict
			{
				get { return Res.GetString("c32d42e5-9567-4db4-8193-efb008f14c45", "Match made with conflict"); }
			}
			public static string NoMatchFound
			{
				get { return Res.GetString("bb074be2-8676-4527-a7d5-ed3752edc32b", "No match found"); }
			}
			public static string MatchApproved
			{
				get { return Res.GetString("a3b51e92-bd31-40dd-b595-a77000bd1f16", "Match Approved"); }
			}
		}

		#region Loader

		public new class Loader : OrgMatchApprovalLoader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion

		#region DeleteRelatedMatchApprovalsAndAddresses

		public static void DeleteRelatedMatchApprovalsAndAddresses(BusinessObject parent)
		{
			OrgMatchApproval[] allMatchApprovalsForParent = new Loader(parent.Factory).LoadAll(parent.PK);
			foreach (OrgMatchApproval matchApproval in allMatchApprovalsForParent)
			{
				matchApproval.AddressToBeMatched.Delete();
				matchApproval.Delete();
			}
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (MatchType != null)
			{
				P2_MatchType = MatchType.Code;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d403ab3d-ab05-4e71-b741-1080e4916803", "Organization Match Approval"); }
		}

		protected override OrgMatchApprovalLookups GetNewLookups()
		{
			return new OrgMatchApprovalLookups(this);
		}

		#endregion

		#region Property Overrides

		public override ZGuid P2_OH_MatchOrg1
		{
			get { return base.P2_OH_MatchOrg1; }
			set
			{
				if (value != P2_OH_MatchOrg1)
				{
					base.P2_OH_MatchOrg1 = value;
					P2_OH_ManuallySelectedOrganisationInfo.RefreshBinding();
					if (IsApprovedWithoutConflict)
					{
						OnMatchApproved(value);
					}
				}
			}
		}

		public override ZGuid P2_OH_MatchOrg2
		{
			get { return base.P2_OH_MatchOrg2; }
			set
			{
				if (value != P2_OH_MatchOrg2)
				{
					base.P2_OH_MatchOrg2 = value;
					P2_OH_ManuallySelectedOrganisationInfo.RefreshBinding();
					if (IsApprovedWithoutConflict)
					{
						OnMatchApproved(value);
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		public OrgPatternMatchAddress AddressToBeMatched
		{
			get
			{
				if (fAddressToBeMatched == null)
				{
					ZQuery filter = new ZQuery(OrgPatternMatchAddressSchema.PK, P2_ParentID);
					filter.IsNoLock = false;

					fAddressToBeMatched = (OrgPatternMatchAddress)Factory.LoadTop1(typeof(OrgPatternMatchAddress), filter);
				}
				return fAddressToBeMatched;
			}
		}
		OrgPatternMatchAddress fAddressToBeMatched;

		internal void CopyDetailsFromParent()
		{
			P2_Reference = ParentReference;
			AddressToBeMatched.P3_Code = ParentOwnerCode.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_Code.MaxLength);
			AddressToBeMatched.P3_CompanyName = ParentCompanyName.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_CompanyName.MaxLength);
			AddressToBeMatched.P3_Address1 = ParentStreet.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_Address1.MaxLength);
			AddressToBeMatched.P3_Address2 = ParentStreet2.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_Address2.MaxLength);
			AddressToBeMatched.P3_City = ParentCity.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_City.MaxLength);
			AddressToBeMatched.P3_State = ParentState.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_State.MaxLength);
			AddressToBeMatched.P3_PostCode = ParentPostCode.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_PostCode.MaxLength);
			AddressToBeMatched.P3_Phone = ParentPhone.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_Phone.MaxLength);
			AddressToBeMatched.P3_Fax = ParentFax.SubstringSafe(0, OrgPatternMatchAddressSchema.P3_Fax.MaxLength);
		}

		#region SimilarOrgMatchesSortedByRank

		public SimilarOrgMatchForApprovalCollection SimilarOrgMatchesSortedByRank
		{
			get
			{
				if (fSimilarOrgMatchesSortedByRank == null)
				{
					OrgPatternMatchCollection loadedSimilarOrgPatternMatches = LoadSimilarOrgMatches();
					loadedSimilarOrgPatternMatches.CountChanged += new CollectionCountChangedEventHandler(OnLoadedSimilarOrgPatternMatches_CountChanged);
					fSimilarOrgMatchesSortedByRank = new SimilarOrgMatchForApprovalCollection(this, loadedSimilarOrgPatternMatches);
				}
				return fSimilarOrgMatchesSortedByRank;
			}
		}
		SimilarOrgMatchForApprovalCollection fSimilarOrgMatchesSortedByRank;

		void OnLoadedSimilarOrgPatternMatches_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OrgPatternMatchCollection collection = (OrgPatternMatchCollection)sender;
			collection.CountChanged -= new CollectionCountChangedEventHandler(OnLoadedSimilarOrgPatternMatches_CountChanged);
			RefreshSimilarOrgMatches();
		}

		void RefreshSimilarOrgMatches()
		{
			fSimilarOrgMatchesSortedByRank = null;
			this.OnElementReset();
		}

		OrgPatternMatchCollection LoadSimilarOrgMatches()
		{
			BusinessObjectFactory newFactoryForTemporaryOrg = new BusinessObjectFactory();
			UnsavableTemporaryOrganisation temporaryOrg = (UnsavableTemporaryOrganisation)newFactoryForTemporaryOrg.New(typeof(UnsavableTemporaryOrganisation));
			CopyDetailsToOrganisation(temporaryOrg);

			temporaryOrg.PatternMatchesForThisOrg.MatchThresholdOverride = OrgMatchThresholds.Codes.Low;
			temporaryOrg.SimilarOrgFinder.FindSimilarOrganisations();
			temporaryOrg.SimilarOrgMatches.Sort(OrgPatternMatch.Schema.OS_Rank, ListSortDirection.Ascending);

			OrgPatternMatchCollection result = new OrgPatternMatchCollection(newFactoryForTemporaryOrg);
			foreach (OrgPatternMatch patternMatch in temporaryOrg.SimilarOrgMatches)
			{
				ZQuery filter = new ZQuery(OrgPatternMatchSchema.OS_OH, patternMatch.OS_OH);

				if (result.Find(filter).Length == 0)
				{
					result.Add(patternMatch);
					if (result.Count > 7)
					{
						break;
					}
				}
			}
			return result;
		}

#if DEBUG
		internal
#endif
			class UnsavableTemporaryOrganisation : OrgHeader
		{
			public UnsavableTemporaryOrganisation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				SetDefaultValuesForTemporaryOrganisation();
			}

			public override bool IsSavedByFactory
			{
				get { return false; }
			}
		}

		#endregion

		#endregion

		#region Bound Organisation Properties

		#region OwnerCode

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_CodeMaxLength)]
		public ZString OwnerCode
		{
			get { return fOwnerCode.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_Code : fOwnerCode; }
			set
			{
				CheckMaximumLength(OwnerCodeInfo, value);
				fOwnerCode = value;
				RefreshSimilarOrgMatches();
				OwnerCodeInfo.RefreshBinding();
			}
		}
		ZString fOwnerCode;

		public ZPropertyInfo OwnerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OwnerCode)); }
		}

		#endregion

		#region CompanyName

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_CompanyNameMaxLength)]
		public ZString CompanyName
		{
			get { return fCompanyName.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_CompanyName : fCompanyName; }
			set
			{
				CheckMaximumLength(CompanyNameInfo, value);
				fCompanyName = value;
				CompanyNameInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fCompanyName;

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyName)); }
		}

		#endregion

		#region Street

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_Address1MaxLength)]
		public ZString Street
		{
			get { return fStreet.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_Address1 : fStreet; }
			set
			{
				CheckMaximumLength(StreetInfo, value);
				fStreet = value;
				StreetInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fStreet;

		public ZPropertyInfo StreetInfo
		{
			get { return GetZPropertyInfo(nameof(Street)); }
		}

		#endregion

		#region Street2

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_Address2MaxLength)]
		public ZString Street2
		{
			get { return fStreet2.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_Address2 : fStreet2; }
			set
			{
				CheckMaximumLength(Street2Info, value);
				fStreet2 = value;
				Street2Info.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fStreet2;

		public ZPropertyInfo Street2Info
		{
			get { return GetZPropertyInfo(nameof(Street2)); }
		}

		#endregion

		#region City

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_CityMaxLength)]
		public ZString City
		{
			get { return fCity.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_City : fCity; }
			set
			{
				CheckMaximumLength(CityInfo, value);
				fCity = value;
				CityInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fCity;

		public ZPropertyInfo CityInfo
		{
			get { return GetZPropertyInfo(nameof(City)); }
		}

		#endregion

		#region State

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_StateMaxLength)]
		public ZString State
		{
			get { return fState.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_State : fState; }
			set
			{
				CheckMaximumLength(StateInfo, value);
				fState = value;
				StateInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fState;

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		#endregion

		#region PostCode

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_PostCodeMaxLength)]
		public ZString PostCode
		{
			get { return fPostCode.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_PostCode : fPostCode; }
			set
			{
				CheckMaximumLength(PostCodeInfo, value);
				fPostCode = value;
				PostCodeInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fPostCode;

		public ZPropertyInfo PostCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PostCode)); }
		}

		#endregion

		#region Phone

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_PhoneMaxLength)]
		public ZString Phone
		{
			get { return fPhone.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_Phone : fPhone; }
			set
			{
				CheckMaximumLength(PhoneInfo, value);
				fPhone = value;
				PhoneInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fPhone;

		public ZPropertyInfo PhoneInfo
		{
			get { return GetZPropertyInfo(nameof(Phone)); }
		}

		#endregion

		#region Fax

		[BusinessObjectTestExclude]
		[MaxLength(OrgPatternMatchAddress.Schema.P3_FaxMaxLength)]
		public ZString Fax
		{
			get { return fFax.IsEmpty && AddressToBeMatched != null ? AddressToBeMatched.P3_Fax : fFax; }
			set
			{
				CheckMaximumLength(FaxInfo, value);
				fFax = value;
				FaxInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fFax;

		public ZPropertyInfo FaxInfo
		{
			get { return GetZPropertyInfo(nameof(Fax)); }
		}

		#endregion

		#region MasterBill (get only)

		public ZString MasterBill
		{
			get { return ParentMasterBill; }
		}

		public ZPropertyInfo MasterBillInfo
		{
			get { return GetZPropertyInfo(nameof(MasterBill)); }
		}

		#endregion

		#region UNLoco

		[BusinessObjectTestExclude()]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString UNLoco
		{
			get { return fUNLoco.IsEmpty && Parent != null ? ParentUNLOCO : fUNLoco; }
			set
			{
				CheckMaximumLength(UNLocoInfo, value);
				fUNLoco = value;
				UNLocoInfo.RefreshBinding();
				RefreshSimilarOrgMatches();
			}
		}
		ZString fUNLoco;

		public ZPropertyInfo UNLocoInfo
		{
			get { return GetZPropertyInfo(nameof(UNLoco)); }
		}

		#endregion

		#region OrganisationType (get only)

		public abstract ZString OrganisationType { get; }
		public ZPropertyInfo OrganisationTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationType)); }
		}

		#endregion

		#endregion

		#region Parent Organisation Properties Mapping

		protected abstract ZString ParentReference { get; }

		protected virtual ZString ParentMasterBill
		{
			get { return ""; }
		}

		protected abstract ZString ParentOwnerCode { get; }
		protected abstract ZString ParentCompanyName { get; }
		protected abstract ZString ParentStreet { get; }
		protected abstract ZString ParentStreet2 { get; }
		protected abstract ZString ParentCity { get; }
		protected abstract ZString ParentUNLOCO { get; }
		protected abstract ZString ParentState { get; }
		protected abstract ZString ParentPostCode { get; }
		protected abstract ZString ParentPhone { get; }
		protected abstract ZString ParentFax { get; }

		#endregion

		#region Marking an Organisation as Matched

		#region Match

		public void Match(ZGuid orgToMatchPK)
		{
			Match(orgToMatchPK, GlbStaff.CurrentUser.GS_Code);
		}

		public void Match(ZGuid orgToMatchPK, string currentUserInitials)
		{
			CheckCanMatch();

			bool useSlot1 = false;
			bool useSlot2 = false;

			if (P2_MatchUser1 == currentUserInitials)
			{
				useSlot1 = true;
			}
			else if (P2_MatchUser2 == currentUserInitials)
			{
				useSlot2 = true;
			}
			else if (P2_MatchUser1.IsEmpty)
			{
				useSlot1 = true;
			}
			else if (P2_MatchUser2.IsEmpty)
			{
				useSlot2 = true;
			}

			if (useSlot1)
			{
				P2_OH_MatchOrg1 = orgToMatchPK;
				P2_MatchUser1 = currentUserInitials;
			}
			else if (useSlot2)
			{
				P2_OH_MatchOrg2 = orgToMatchPK;
				P2_MatchUser2 = currentUserInitials;
			}
		}

		public void MatchAndSaveAtomically(ZGuid orgToMatchPK)
		{
			InvokeMatchMethodInOtherFactoryAndSave("Match", orgToMatchPK);
		}

		#endregion

		#region NotifyNoMatchFound

		public void NotifyNoMatchFound()
		{
			NotifyNoMatchFound(GlbStaff.CurrentUser.GS_Code);
		}

		public void NotifyNoMatchFound(string currentUserInitials)
		{
			CheckCanMatch();
			Match(ZGuid.Empty, currentUserInitials);
		}

		public void NotifyNoMatchFoundAndSaveAtomically()
		{
			InvokeMatchMethodInOtherFactoryAndSave("NotifyNoMatchFound");
		}

		#endregion

		#region ApproveMatchBySupervisor

		public void ApproveMatchBySupervisor(OrgHeader match)
		{
			if (!P2_OH_MatchOrg1.IsEmpty && P2_OH_MatchOrg1 == P2_OH_MatchOrg2)
			{
				throw new MatchingException("This organisation match has already been approved without conflict.");
			}

			if (P2_OH_MatchOrg1 != match.PK)
			{
				P2_OH_MatchOrg1 = match.PK;
				P2_MatchUser1 = GlbStaff.CurrentUser.GS_Code;
			}
			if (P2_OH_MatchOrg2 != match.PK)
			{
				P2_OH_MatchOrg2 = match.PK;
				P2_MatchUser2 = GlbStaff.CurrentUser.GS_Code;
			}
		}

		public void ApproveMatchBySupervisorAndSaveAtomically(OrgHeader match)
		{
			InvokeMatchMethodInOtherFactoryAndSave("ApproveMatchBySupervisor", match);
		}

		#endregion

		[Serializable]
		public class MatchingException : Exception
		{
			public MatchingException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected MatchingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		void CheckCanMatch()
		{
			if (!CanMatchByCurrentUser)
			{
				throw new MatchingException("This organisation match has already been approved");
			}
		}

		void InvokeMatchMethodInOtherFactoryAndSave(string methodName, params object[] args)
		{
			for (int i = 0; i < 3; i++)
			{
				try
				{
					using (var manager = Db.Connection.BeginTransactionWithManager())
					{
						TryInvokeMatchMethodInOtherFactoryAndSave(methodName, args);
						manager.CommitTransaction();
						break;
					}
				}
				catch (ZSaveConcurrencyException)
				{
					OnConcurrencyException();
					if (i == 2)
					{
						throw;
					}
					continue;
				}
			}
		}

		void TryInvokeMatchMethodInOtherFactoryAndSave(string methodName, params object[] args)
		{
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			OrgMatchApproval matchApprovalInOtherFactory = (OrgMatchApproval)otherFactory.Load(GetType(), PK);
			if (matchApprovalInOtherFactory != null)
			{
				OnAfterReloadBeforeMatchApproved(matchApprovalInOtherFactory);
				try
				{
					typeof(OrgMatchApproval).InvokeMember(methodName, BindingFlags.InvokeMethod, null, matchApprovalInOtherFactory, args);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException is MatchingException)
					{
						throw ex.InnerException;
					}
					throw;
				}
				otherFactory.Save();
			}
		}

#if DEBUG
		protected virtual
#endif
 void OnAfterReloadBeforeMatchApproved(OrgMatchApproval matchApprovalInOtherFactory)
		{
		}

#if DEBUG
		protected virtual
#endif
 void OnConcurrencyException()
		{
		}

		#endregion

		#region Match Org / User Name Bound Properties

		#region P2_MatchOrgCode1

		public ZString P2_MatchOrgCode1
		{
			get { return GetMatchOrgCode(P2_MatchUser1, MatchOrg1); }
		}

		public ZPropertyInfo P2_MatchOrgCode1Info
		{
			get { return GetZPropertyInfo(nameof(P2_MatchOrgCode1)); }
		}

		#endregion

		#region P2_MatchOrgCode2

		public ZString P2_MatchOrgCode2
		{
			get { return GetMatchOrgCode(P2_MatchUser2, MatchOrg2); }
		}

		public ZPropertyInfo P2_MatchOrgCode2Info
		{
			get { return GetZPropertyInfo(nameof(P2_MatchOrgCode2)); }
		}

		#endregion

		#region P2_MatchUserFullName1

		public ZString P2_MatchUserFullName1
		{
			get { return GetMatchUserFullName(P2_MatchUser1); }
		}

		public ZPropertyInfo P2_MatchUserFullName1Info
		{
			get { return GetZPropertyInfo(nameof(P2_MatchUserFullName1)); }
		}

		#endregion

		#region P2_MatchUserFullName2

		public ZString P2_MatchUserFullName2
		{
			get { return GetMatchUserFullName(P2_MatchUser2); }
		}

		public ZPropertyInfo P2_MatchUserFullName2Info
		{
			get { return GetZPropertyInfo(nameof(P2_MatchUserFullName2)); }
		}

		#endregion

		ZString GetMatchOrgCode(ZString matchUserInitials, OrgHeader matchedOrg)
		{
			ZString result;
			if (matchUserInitials.IsEmpty)
			{
				result = "-";
			}
			else if (matchedOrg == null)
			{
				result = (NoResString)"-None Found-";
			}
			else
			{
				result = matchedOrg.OH_Code;
			}
			return result;
		}

		ZString GetMatchUserFullName(ZString matchUserInitials)
		{
			GlbStaff user = (GlbStaff)Factory.LoadFromNaturalKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, matchUserInitials);
			return (user == null) ? "-" : (string)user.GS_FullName;
		}

		#endregion

		#region P2_MatchStatus

		public ZString P2_MatchStatus
		{
			get
			{
				string result = "";
				string currentUserInitials = Env.CurrentUser.Initials.Trim();

				bool matchedByAtLeast1User = !P2_MatchUser1.IsEmpty || !P2_MatchUser2.IsEmpty;
				bool matchedByZeroOrOneUsers = P2_MatchUser1.IsEmpty || P2_MatchUser2.IsEmpty;

				if (P2_MatchUser1.IsEmpty && P2_MatchUser2.IsEmpty)
				{
					result = MatchStatus.Unmatched;
				}
				else if (
					matchedByZeroOrOneUsers &&
					(P2_MatchUser1 == currentUserInitials || P2_MatchUser2 == currentUserInitials))
				{
					result = MatchStatus.PartiallyMatchedByCurrentUser;
				}
				else if (matchedByAtLeast1User && matchedByZeroOrOneUsers)
				{
					result = MatchStatus.PartiallyMatched;
				}
				else if (!P2_MatchUser1.IsEmpty && !P2_MatchUser2.IsEmpty)
				{
					if (P2_OH_MatchOrg1 != P2_OH_MatchOrg2)
					{
						result = MatchStatus.MatchMadeWithConflict;
					}
					else if (P2_OH_MatchOrg1.IsEmpty && P2_OH_MatchOrg2.IsEmpty)
					{
						result = MatchStatus.NoMatchFound;
					}
					else if (P2_OH_MatchOrg1 == P2_OH_MatchOrg2 && P2_OH_MatchOrg1 != ZGuid.Empty)
					{
						result = MatchStatus.MatchApproved;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo P2_MatchStatusInfo
		{
			get { return GetZPropertyInfo(nameof(P2_MatchStatus)); }
		}

		#endregion

		#region Manually Selected Organisation

		[BusinessObjectTestExclude()]
		[List("Lookups.MatchOrg1s")]
		[ReadOnlyMember(nameof(IsApprovedWithoutConflict))]
		public ZGuid P2_OH_ManuallySelectedOrganisation
		{
			get { return fP2_OH_ManuallySelectedOrganisation; }
			set
			{
				SetNonPersistentPropertyValue(P2_OH_ManuallySelectedOrganisationInfo, ref fP2_OH_ManuallySelectedOrganisation, value);
				if (P2_OH_ManuallySelectedOrganisation.IsValid)
				{
					MatchAndSaveAtomically(P2_OH_ManuallySelectedOrganisation);
				}
			}
		}
		ZGuid fP2_OH_ManuallySelectedOrganisation;

		public ZPropertyInfo P2_OH_ManuallySelectedOrganisationInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(P2_OH_ManuallySelectedOrganisation));
			}
		}

		#endregion

		#region IsApproved / ApprovedOrgMatch

		public bool IsApproved
		{
			get { return !P2_OH_MatchOrg1.IsEmpty && !P2_OH_MatchOrg2.IsEmpty; }
		}

		protected bool IsApprovedWithoutConflict
		{
			get { return !P2_OH_MatchOrg1.IsEmpty && P2_OH_MatchOrg1 == P2_OH_MatchOrg2; }
		}

		bool CanMatchByCurrentUser
		{
			get { return !IsApprovedByOtherUsers && !IsApprovedWithoutConflict; }
		}

		public virtual bool IsApprovedByOtherUsers
		{
			get
			{
				bool result = !P2_MatchUser1.IsEmpty && P2_MatchUser1 != GlbStaff.CurrentUser.GS_Code;
				result = result && !P2_MatchUser2.IsEmpty && P2_MatchUser2 != GlbStaff.CurrentUser.GS_Code;
				return result;
			}
		}

		public OrgHeader ApprovedOrgMatch
		{
			get { return IsApproved ? MatchOrg1 : null; }
		}

		#endregion

		#region UpdatedWithDataRefresh Event

		public event EventHandler UpdatedWithDataRefresh;

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (UpdatedWithDataRefresh != null)
			{
				UpdatedWithDataRefresh(this, EventArgs.Empty);
			}
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new MatchApprovalUniqueIndexFailureHandler(); }
		}

		class MatchApprovalUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return OrgMatchApprovalSchema.Constants.Indexes.NR_UX__P2_ParentID_P2_MatchType; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("7d2ad808-2a9b-4d0e-b859-51ca8e45d84b", "Another user has already made changes to this record. You must re-open this form and re-apply your changes to continue."),
					Res.GetString("59a7e5f9-d886-4a40-ae0e-d03b7d625df1", "Another user has changed this record"));
			}
		}

		#endregion

		public BusinessObject Parent
		{
			get
			{
				BusinessObject result = null;
				if (MatchType != null && MatchType.ParentType != null)
				{
					result = Factory.Load(MatchType.ParentType, ParentPK);
					if (result == null)
					{
						result = Factory.GetNull(MatchType.ParentType);
					}
				}
				return result;
			}
		}

		ZGuid ParentPK
		{
			get { return AddressToBeMatched.P3_ParentID; }
		}

		public virtual bool IsCurrentUserSupervisor
		{
			get { return IsTheCurrentUserSupervisor; }
		}

		public static bool IsTheCurrentUserSupervisor
		{
			get { return Env.Security.OrgMatchApprovalSupervisor.IsAllowed; }
		}

		public bool ShouldCreateTemporaryOrganisation()
		{
			bool bothMatchUsersAgreeOnNoMatch =
				!P2_MatchUser1.IsEmpty && !P2_MatchUser2.IsEmpty &&
				P2_OH_MatchOrg1.IsEmpty && P2_OH_MatchOrg2.IsEmpty;
			bool noOrganisationMatchesFound = SimilarOrgMatchesSortedByRank.Count == 0;

			return bothMatchUsersAgreeOnNoMatch || noOrganisationMatchesFound;
		}

		public virtual void CopyDetailsToOrganisation(OrgHeader organisation)
		{
			organisation.OH_FullName = CompanyName.SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength);
			organisation.MainAddress.OA_Address1 = Street.SubstringSafe(0, OrgAddressSchema.OA_Address1.MaxLength);
			organisation.MainAddress.OA_Address2 = Street2.SubstringSafe(0, OrgAddressSchema.OA_Address2.MaxLength);
			organisation.MainAddress.OA_City = City.SubstringSafe(0, OrgAddressSchema.OA_City.MaxLength);
			ZString rememberOH_Code = organisation.OH_Code;
			organisation.OH_RL_NKClosestPort = ParentUNLOCO;
			if (!rememberOH_Code.IsEmpty)
			{
				organisation.OH_Code = rememberOH_Code;
			}
			organisation.MainAddress.OA_State = State;
			organisation.MainAddress.OA_PostCode = PostCode;
			organisation.MainAddress.OA_Phone = Phone;
			organisation.MainAddress.OA_Fax = Fax;
		}

		public abstract OrgMatchApprovalType MatchType { get; }

		protected virtual void OnMatchApproved(ZGuid orgMatchPK)
		{
			AddressToBeMatched.P3_OH_MatchOrg = orgMatchPK;
			P2_RelatedDateForPatternMatch = ZDateTime.Now;
		}
	}
}
