using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business
{
	public enum CurrentQueryMode
	{
		Name,
		Code,
		Pattern
	}

	public class MergeOrgHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public MergeOrgHeader(BusinessObjectFactory factory, OrgHeader oldOrg)
			: this(factory, oldOrg, null)
		{ }

		public MergeOrgHeader(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg)
			: base(factory)
		{
			if (oldOrg != null)
			{
				OldOrganisation = oldOrg;
				ValidateOldOrganisation();
			}
			if (newOrg != null)
			{
				NewOrganisationPk = newOrg.PK;
			}
		}

		public MergeOrgHeader(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg, DbConnection externalConnection)
			: this(factory, oldOrg, newOrg)
		{
			this.externalConnection = externalConnection;
		}

		#endregion

		readonly DbConnection externalConnection;

		#region Organisations

		OrgHeader oldOrganisation;
		public OrgHeader OldOrganisation
		{
			get { return oldOrganisation; }
			set
			{
				if (oldOrganisation == null)
				{
					oldOrganisation = value;
					ValidateOldOrganisation();
					if (NewOrganisation != null)
					{
						SetOldOrgAddressesAndContactsCollectionNewOrganisation();
						CheckMergingSameOrg(OldOrganisation.PK, NewOrganisationPk, NewOrganisationPkInfo);
						ValidateOrgFlags();
					}
				}
			}
		}

		OrgHeader newOrganisation;
		public OrgHeader NewOrganisation
		{
			get { return newOrganisation; }
		}

		public ZString NewOrganisationCode { get; private set; }

		#endregion

		#region Delete Old Organisation

		public string DeleteError { get; internal set; }

		#endregion

		#region Move References Error

		public SqlException AddOverlappingDatesException { get; internal set; }

		#endregion

		#region ConflictingStorageDocsPK

		public InvalidOperationException ConflictingStorageDocsPKException { get; internal set; }

		#endregion

		#region Concurrency

		public ZSaveConcurrencyException ConcurrencyException { get; internal set; }

		#endregion

		#region HasToLoadSimilarOrgs

		public bool HasToLoadSimilarOrgs { get; set; }

		#endregion

		#region Org Collections

		OrgHeaderCollection oldOrgsCollectionByName;

		[ChildEditable(true)]
		public OrgHeaderCollection OldOrgsCollectionByName
		{
			get
			{
				if (NewOrganisation == null || !HasToLoadSimilarOrgs)
				{
					return new OrgHeaderCollection(Factory);
				}

				if (oldOrgsCollectionByName == null)
				{
					var localOldOrgsCollectionByName = new OrgHeaderCollection(Factory);
					localOldOrgsCollectionByName.Load(QueryByName);
					oldOrgsCollectionByName = localOldOrgsCollectionByName;
					oldOrgsCollectionByName.CountChanged += new CollectionCountChangedEventHandler(oldOrgsCollection_CountChanged);
					foreach (OrgHeader org in oldOrgsCollectionByName)
					{
						org.DestroyAndReloadCompanyData();
					}
					oldOrgsCollectionByName.SuspendValidation();
				}
				return oldOrgsCollectionByName;
			}
		}

		OrgHeaderCollection oldOrgsCollectionByCode;

		[ChildEditable(true)]
		public OrgHeaderCollection OldOrgsCollectionByCode
		{
			get
			{
				if (NewOrganisation == null || !HasToLoadSimilarOrgs)
				{
					return new OrgHeaderCollection(Factory);
				}

				if (oldOrgsCollectionByCode == null)
				{
					var localOldOrgsCollectionByCode = new OrgHeaderCollection(Factory);
					localOldOrgsCollectionByCode.Load(QueryByCode);
					oldOrgsCollectionByCode = localOldOrgsCollectionByCode;

					oldOrgsCollectionByCode.CountChanged += new CollectionCountChangedEventHandler(oldOrgsCollection_CountChanged);
					foreach (OrgHeader org in oldOrgsCollectionByCode)
					{
						org.DestroyAndReloadCompanyData();
					}
					oldOrgsCollectionByCode.SuspendValidation();
				}
				return oldOrgsCollectionByCode;
			}
		}

		OrgHeaderCollection oldOrgsCollectionByPattern;

		[ChildEditable(true)]
		public OrgHeaderCollection OldOrgsCollectionByPattern
		{
			get
			{
				if (NewOrganisation == null || !HasToLoadSimilarOrgs)
				{
					return new OrgHeaderCollection(Factory);
				}

				if (oldOrgsCollectionByPattern == null)
				{
					SetCollectionsByPatterns();
				}
				return oldOrgsCollectionByPattern;
			}
		}

		bool IsSettingCollectionsByPatterns;

		void SetCollectionsByPatterns()
		{
			if (NewOrganisation != null && !IsSettingCollectionsByPatterns)
			{
				IsSettingCollectionsByPatterns = true;
				try
				{
					oldOrgsCollectionByPattern = new OrgHeaderCollection(Factory);
					oldOrgsCollectionByPattern.CountChanged -= new CollectionCountChangedEventHandler(oldOrgsCollection_CountChanged);
					oldOrgsCollectionByPattern.CountChanged += new CollectionCountChangedEventHandler(oldOrgsCollection_CountChanged);

					SetOrgsCollectionByPatternsCore();

					foreach (OrgHeader org in oldOrgsCollectionByPattern)
					{
						org.DestroyAndReloadCompanyData();
					}
					oldOrgsCollectionByPattern.SuspendValidation();
				}
				finally
				{
					IsSettingCollectionsByPatterns = false;
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 void SetOrgsCollectionByPatternsCore()
		{
			NewOrganisation.SimilarOrgMatches.MaximumResultsToShow = MaxResultsCore.ToZInt();
			if (NewOrganisation.SimilarOrgMatches.Count == 0)
			{
				if (CurrentMatchThresholdCode == OrgMatchThresholds.Codes.Low)
				{
					NewOrganisation.SimilarOrgFinder.FindMaximumSimilarOrganisations();
				}
				else
				{
					NewOrganisation.SimilarOrgFinder.FindSimilarOrganisations();
				}
			}
			OldOrgsCollectionByPattern.RemoveAll();
			OldOrgAddressCollectionForSimilarOrgsByPattern.RemoveAll();
			OldOrgContactCollectionForSimilarOrgsByPattern.RemoveAll();
			if (NewOrganisation.SimilarOrgMatches.Count > 0)
			{
				List<ZGuid> addedOrgPks = new List<ZGuid>();
				List<OrgHeader> addedOrgs = new List<OrgHeader>();
				SuspendOldOrgsCollection_CountChanged = true;
				try
				{
					foreach (OrgPatternMatch match in NewOrganisation.SimilarOrgMatches)
					{
						if (!addedOrgPks.Contains(match.Header.PK))
						{
							addedOrgPks.Add(match.Header.PK);
							addedOrgs.Add(match.Header);
							OldOrgsCollectionByPattern.Add(match.Header);
						}
					}
				}
				finally
				{
					SuspendOldOrgsCollection_CountChanged = false;
				}
				if (addedOrgs.Count > 0)
				{
					OrgAddressCollection colAdr = new OrgAddressCollection(Factory, new ZQuery(OrgAddressSchema.OA_OH, NewOrganisation.PK));
					colAdr.Load();
					OrgContactCollection colCnt = new OrgContactCollection(Factory, new ZQuery(OrgContactSchema.OC_OH, NewOrganisation.PK));
					colCnt.Load();
					foreach (OrgHeader org in addedOrgs)
					{
						MergeOrgAddressCollection tmpAdr = new MergeOrgAddressCollection(Factory, org, NewOrganisation, colAdr);
						CurrentOrgAddressCollection.AddRange(tmpAdr);
						MergeOrgContactCollection tmpCnt = new MergeOrgContactCollection(Factory, org, NewOrganisation, colCnt);
						CurrentOrgContactCollection.AddRange(tmpCnt);
					}
					CurrentOrgAddressCollection.SetParentCollection();
					CurrentOrgContactCollection.SetParentCollection();
				}
			}
		}

		bool SuspendOldOrgsCollection_CountChanged { get; set; }

		void SetOrgsCollectionByNameOrCode()
		{
			CurrentOrgAddressCollection.RemoveAll();
			CurrentOrgContactCollection.RemoveAll();
			CurrentOrgHeaderCollection.Load(CurrentMode == CurrentQueryMode.Code ? QueryByCode : QueryByName);
		}

		void oldOrgsCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && !SuspendOldOrgsCollection_CountChanged)
			{
				OrgHeader newOldOrg = e.BizObject as OrgHeader;
				if (newOldOrg != null)
				{
					MergeOrgAddressCollection tmpAdr = new MergeOrgAddressCollection(Factory, newOldOrg, NewOrganisation);
					foreach (MergeOrgAddress adr in tmpAdr)
					{
						if (!ContainsByRef(CurrentOrgAddressCollection, adr.OldAddressPK))
						{
							CurrentOrgAddressCollection.Add(adr);
						}
					}
					MergeOrgContactCollection tmpCnt = new MergeOrgContactCollection(Factory, newOldOrg, NewOrganisation);
					foreach (MergeOrgContact cnt in tmpCnt)
					{
						if (!ContainsByRef(CurrentOrgContactCollection, cnt.OldContactPK))
						{
							CurrentOrgContactCollection.Add(cnt);
						}
					}
					CurrentOrgAddressCollection.SetParentCollection();
					CurrentOrgContactCollection.SetParentCollection();
				}
			}
			else if (e.ItemRemoved)
			{
				List<MergeOrgAddress> addressesToRemove = CurrentOrgAddressCollection.Where(x => x.OldObject.IsDeleted).Cast<MergeOrgAddress>().ToList();
				foreach (MergeOrgAddress adr in addressesToRemove)
				{
					CurrentOrgAddressCollection.Remove(adr);
				}
				List<MergeOrgContact> contactsToRemove = CurrentOrgContactCollection.Where(x => x.OldObject.IsDeleted).Cast<MergeOrgContact>().ToList();
				foreach (MergeOrgContact cnt in contactsToRemove)
				{
					CurrentOrgContactCollection.Remove(cnt);
				}
				CurrentOrgAddressCollection.SetParentCollection();
				CurrentOrgContactCollection.SetParentCollection();
			}
		}

		bool ContainsByRef<T>(MergeOrgElementCollection<T> col, ZGuid pk) where T : NonPersistentBusinessObject, IMergeOrgElement
		{
			foreach (IMergeOrgElement obj in col)
			{
				if (obj.OldObject.PK == pk)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Queries

		ZQuery QueryByName
		{
			get
			{
				ZQuery query = new ZQuery();
				query.MaximumRows = MaxResultsCore.ToZInt();
				query.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Equal, NewOrganisation.OH_FullName);
				query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, NewOrganisation.PK);
				return query;
			}
		}

		ZQuery QueryByCode
		{
			get
			{
				ZQuery query = new ZQuery();
				query.MaximumRows = MaxResultsCore.ToZInt();
				query.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Contains, NewOrganisation.OH_Code);
				query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, NewOrganisation.PK);
				return query;
			}
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string NewOrganisationPk = "NewOrganisationPk";
			public const string OldOrganisationCode = "OldOrganisationCode";
			public const string OldOrganisationName = "OldOrganisationName";
		}

		#endregion

		#region Properties

		#region OldOrganisationCode

		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString OldOrganisationCode
		{
			get { return OldOrganisation == null ? ZString.Empty : OldOrganisation.OH_Code; }
		}

		public ZPropertyInfo OldOrganisationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OldOrganisationCode); }
		}

		public StringCollectionX GetOldOrganisationCodes(OrgHeaderCollection collection)
		{
			StringCollectionX result = new StringCollectionX();
			Array.ForEach(collection.ToArray<OrgHeader>(), delegate(OrgHeader org)
			{ result.Add(org.OH_Code); });
			return result;
		}

		#endregion

		#region OldOrganisationName

		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		public ZString OldOrganisationName
		{
			get { return OldOrganisation == null ? ZString.Empty : OldOrganisation.OH_FullName; }
		}

		public ZPropertyInfo OldOrganisationNameInfo
		{
			get { return GetZPropertyInfo(Schema.OldOrganisationName); }
		}

		#endregion

		#region NewOrganisationPk

		ZGuid newOrganisationPk;
		[List("ActiveOrganisations")]
		public ZGuid NewOrganisationPk
		{
			get { return newOrganisationPk; }
			set
			{
				if (value != newOrganisationPk)
				{
					SetNonPersistentPropertyValue(NewOrganisationPkInfo, ref newOrganisationPk, value);
					newOrganisation = Factory.Load<OrgHeader>(newOrganisationPk);
					NewOrganisationCode = newOrganisation?.OH_Code ?? ZString.Empty;
					if (OldOrganisation != null)
					{
						SetOldOrgAddressesAndContactsCollectionNewOrganisation();
					}
					if (!IsValidationSuspended)
					{
						ValidateNewOrganisationPk();
					}
				}
			}
		}

		public ZPropertyInfo NewOrganisationPkInfo
		{
			get
			{
				var result = GetZPropertyInfo(Schema.NewOrganisationPk);
				result.HumanReadableName = Res.GetString("86cdd16a-bd22-40d7-960e-f9ce5b6b783d", "New Organization");
				return result;
			}
		}

		ZGuid organisationPkForBinding = ZGuid.Empty;
		[List("ActiveOrganisations")]
		public ZGuid OrganisationPkForBinding
		{
			get
			{
				return organisationPkForBinding;
			}
			set
			{
				SetNonPersistentPropertyValue(OrganisationPkForBindingInfo, ref organisationPkForBinding, value);
			}
		}

		public ZPropertyInfo OrganisationPkForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationPkForBinding)); }
		}

		#endregion

		#region Organization contacts collection

		#region for single old org

		MergeOrgContactCollection oldOrgContactCollection;

		public MergeOrgContactCollection OldOrgContactCollection
		{
			get
			{
				if (oldOrgContactCollection == null && OldOrganisation != null)
				{
					oldOrgContactCollection = new MergeOrgContactCollection(Factory, OldOrganisation, null);
					RegisterEditableChildObject(oldOrgContactCollection);
				}

				return oldOrgContactCollection;
			}
		}

		MergeOrgContactCollection contactCollectionWithoutDummyContact;

		public MergeOrgContactCollection ContactCollectionWithoutDummyContact
		{
			get
			{
				if (contactCollectionWithoutDummyContact == null && OldOrganisation != null)
				{
					contactCollectionWithoutDummyContact = new MergeOrgContactCollection(Factory, OldOrganisation, null);
					RegisterEditableChildObject(contactCollectionWithoutDummyContact);
				}

				return contactCollectionWithoutDummyContact;
			}
		}

		#endregion

		#region for old org collection

		MergeOrgContactCollection oldOrgContactCollectionForSimilarOrgsByName;

		[ChildEditable(true)]
		public MergeOrgContactCollection OldOrgContactCollectionForSimilarOrgsByName
		{
			get
			{
				if (oldOrgContactCollectionForSimilarOrgsByName == null)
				{
					oldOrgContactCollectionForSimilarOrgsByName = GetOrgContactCollectionForSimilarOrgs(OldOrgsCollectionByName);
				}

				return oldOrgContactCollectionForSimilarOrgsByName;
			}
		}

		MergeOrgContactCollection oldOrgContactCollectionForSimilarOrgsByCode;

		[ChildEditable(true)]
		public MergeOrgContactCollection OldOrgContactCollectionForSimilarOrgsByCode
		{
			get
			{
				if (oldOrgContactCollectionForSimilarOrgsByCode == null)
				{
					oldOrgContactCollectionForSimilarOrgsByCode = GetOrgContactCollectionForSimilarOrgs(OldOrgsCollectionByCode);
				}

				return oldOrgContactCollectionForSimilarOrgsByCode;
			}
		}

		MergeOrgContactCollection oldOrgContactCollectionForSimilarOrgsByPattern;

		[ChildEditable(true)]
		public MergeOrgContactCollection OldOrgContactCollectionForSimilarOrgsByPattern
		{
			get
			{
				if (oldOrgContactCollectionForSimilarOrgsByPattern == null)
				{
					oldOrgContactCollectionForSimilarOrgsByPattern = new MergeOrgContactCollection(Factory);
					SetCollectionsByPatterns();
				}

				return oldOrgContactCollectionForSimilarOrgsByPattern;
			}
		}

		MergeOrgContactCollection GetOrgContactCollectionForSimilarOrgs(OrgHeaderCollection masterCollection)
		{
			MergeOrgContactCollection result = new MergeOrgContactCollection(Factory);
			if (masterCollection != null)
			{
				foreach (OrgHeader org in masterCollection)
				{
					MergeOrgContactCollection tmp = new MergeOrgContactCollection(Factory, org, NewOrganisation);
					result.AddRange(tmp);
				}
				RegisterEditableChildObject(result);
				result.SetParentCollection();
			}

			return result;
		}

		#endregion

		#endregion

		#region Organization addresses collection

		#region for single old org

		protected MergeOrgAddressCollection oldOrgAddressesCollection;

		public MergeOrgAddressCollection OldOrgAddressesCollection
		{
			get
			{
				if (oldOrgAddressesCollection == null && OldOrganisation != null)
				{
					oldOrgAddressesCollection = new MergeOrgAddressCollection(Factory, OldOrganisation, null);
					RegisterEditableChildObject(oldOrgAddressesCollection);
				}

				return oldOrgAddressesCollection;
			}
		}

#if DEBUG
		protected virtual
#endif
 void SetOldOrgAddressesAndContactsCollectionNewOrganisation()
		{
			OldOrgAddressesCollection.SetNewOrganisation(NewOrganisation);
			OldOrgAddressesCollection.RefreshBinding();
			OldOrgContactCollection.SetNewOrganisation(NewOrganisation);
			OldOrgContactCollection.RefreshBinding();
			ContactCollectionWithoutDummyContact.SetNewOrganisation(newOrganisation);
			ContactCollectionWithoutDummyContact.RefreshBinding();
		}

		#endregion

		#region for old org collection

		MergeOrgAddressCollection oldOrgAddressCollectionForSimilarOrgsByName;

		[ChildEditable(true)]
		public MergeOrgAddressCollection OldOrgAddressCollectionForSimilarOrgsByName
		{
			get
			{
				if (oldOrgAddressCollectionForSimilarOrgsByName == null)
				{
					oldOrgAddressCollectionForSimilarOrgsByName = GetOrgAddressCollectionForSimilarOrgs(OldOrgsCollectionByName);
				}

				return oldOrgAddressCollectionForSimilarOrgsByName;
			}
		}

		MergeOrgAddressCollection oldOrgAddressCollectionForSimilarOrgsByCode;

		[ChildEditable(true)]
		public MergeOrgAddressCollection OldOrgAddressCollectionForSimilarOrgsByCode
		{
			get
			{
				if (oldOrgAddressCollectionForSimilarOrgsByCode == null)
				{
					oldOrgAddressCollectionForSimilarOrgsByCode = GetOrgAddressCollectionForSimilarOrgs(OldOrgsCollectionByCode);
				}

				return oldOrgAddressCollectionForSimilarOrgsByCode;
			}
		}

		MergeOrgAddressCollection oldOrgAddressCollectionForSimilarOrgsByPattern;

		[ChildEditable(true)]
		public MergeOrgAddressCollection OldOrgAddressCollectionForSimilarOrgsByPattern
		{
			get
			{
				if (oldOrgAddressCollectionForSimilarOrgsByPattern == null)
				{
					oldOrgAddressCollectionForSimilarOrgsByPattern = new MergeOrgAddressCollection(Factory);
					SetCollectionsByPatterns();
				}

				return oldOrgAddressCollectionForSimilarOrgsByPattern;
			}
		}

		MergeOrgAddressCollection GetOrgAddressCollectionForSimilarOrgs(OrgHeaderCollection masterCollection)
		{
			MergeOrgAddressCollection result = new MergeOrgAddressCollection(Factory);
			if (masterCollection != null && masterCollection.Count > 0)
			{
				foreach (OrgHeader org in masterCollection)
				{
					MergeOrgAddressCollection tmp = new MergeOrgAddressCollection(Factory, org, NewOrganisation);
					result.AddRange(tmp);
				}
				RegisterEditableChildObject(result);
				result.SetParentCollection();
			}

			return result;
		}

		#endregion

		#endregion

		#region Collections for FindBoxes

		#region ActiveOrganisations

		protected OrganisationsFindBoxCollection fActiveOrganisations;
		public OrganisationsFindBoxCollection ActiveOrganisations
		{
			get
			{
				if (fActiveOrganisations == null)
				{
					fActiveOrganisations = new OrganisationsFindBoxCollection(Factory);
				}
				return fActiveOrganisations;
			}
		}

		#endregion

		#endregion

		#region Pattern Search Parameters

		#region Thresholds collection

		CodeDescriptionPairList matchThresholdsList;
		public CodeDescriptionPairList MatchThresholdsList
		{
			get
			{
				if (matchThresholdsList == null)
				{
					matchThresholdsList = new CodeDescriptionPairList();
					matchThresholdsList.AddPair(OrgMatchThresholds.Codes.Low, OrgMatchThresholds.Descriptions.Low);
					matchThresholdsList.AddPair(OrgMatchThresholds.Codes.Medium, OrgMatchThresholds.Descriptions.Medium);
					matchThresholdsList.AddPair(OrgMatchThresholds.Codes.High, OrgMatchThresholds.Descriptions.High);
					matchThresholdsList.AddPair(OrgMatchThresholds.Codes.Extreme, OrgMatchThresholds.Descriptions.Extreme);
				}
				return matchThresholdsList;
			}
		}

		ZString currentMatchThresholdCode = OrgMatchThresholds.Codes.Medium;

		[List("MatchThresholdsList")]
		public ZString CurrentMatchThresholdCode
		{
			get { return currentMatchThresholdCode; }
			set { currentMatchThresholdCode = value; }
		}

		ZString currentMatchThresholdCodeCore = OrgMatchThresholds.Codes.Medium;
		ZString CurrentMatchThresholdCodeCore
		{
			get { return currentMatchThresholdCodeCore; }
			set
			{
				if (currentMatchThresholdCodeCore != value)
				{
					currentMatchThresholdCodeCore = value;
					NewOrganisation.PatternMatchesForThisOrg.MatchThresholdOverride = currentMatchThresholdCode;
				}
			}
		}

		#endregion

		#region Max Results

		ZDecimal maxResults = 100;
		public ZDecimal MaxResults
		{
			get { return maxResults; }
			set { maxResults = value; }
		}

		ZDecimal maxResultsCore = 100;
		ZDecimal MaxResultsCore
		{
			get { return maxResultsCore; }
			set
			{
				if (maxResultsCore != value)
				{
					maxResultsCore = value;
				}
			}
		}

		#endregion

		#endregion

		#region Currents

		public CurrentQueryMode CurrentMode { get; set; }

		public OrgHeaderCollection CurrentOrgHeaderCollection
		{
			get
			{
				switch (CurrentMode)
				{
					case CurrentQueryMode.Code:
						return OldOrgsCollectionByCode;
					case CurrentQueryMode.Name:
						return OldOrgsCollectionByName;
					case CurrentQueryMode.Pattern:
						return OldOrgsCollectionByPattern;
				}
				return null;
			}
		}

		public MergeOrgAddressCollection CurrentOrgAddressCollection
		{
			get
			{
				if (CurrentMode == CurrentQueryMode.Name)
				{
					RegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByName);
					if (oldOrgAddressCollectionForSimilarOrgsByCode != null)
					{
						UnRegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByCode);
					}
					if (oldOrgAddressCollectionForSimilarOrgsByPattern != null)
					{
						UnRegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByPattern);
					}
					return OldOrgAddressCollectionForSimilarOrgsByName;
				}
				else if (CurrentMode == CurrentQueryMode.Code)
				{
					if (oldOrgAddressCollectionForSimilarOrgsByName != null)
					{
						UnRegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByName);
					}
					RegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByCode);
					if (oldOrgAddressCollectionForSimilarOrgsByPattern != null)
					{
						UnRegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByPattern);
					}
					return OldOrgAddressCollectionForSimilarOrgsByCode;
				}
				else
				{
					if (oldOrgAddressCollectionForSimilarOrgsByName != null)
					{
						UnRegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByName);
					}
					if (oldOrgAddressCollectionForSimilarOrgsByCode != null)
					{
						UnRegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByCode);
					}
					RegisterEditableChildObject(OldOrgAddressCollectionForSimilarOrgsByPattern);
					return OldOrgAddressCollectionForSimilarOrgsByPattern;
				}
			}
		}

		public MergeOrgContactCollection CurrentOrgContactCollection
		{
			get
			{
				if (CurrentMode == CurrentQueryMode.Name)
				{
					RegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByName);
					if (oldOrgContactCollectionForSimilarOrgsByCode != null)
					{
						UnRegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByCode);
					}
					if (oldOrgContactCollectionForSimilarOrgsByPattern != null)
					{
						UnRegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByPattern);
					}
					return OldOrgContactCollectionForSimilarOrgsByName;
				}
				else if (CurrentMode == CurrentQueryMode.Code)
				{
					if (oldOrgContactCollectionForSimilarOrgsByName != null)
					{
						UnRegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByName);
					}
					RegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByCode);
					if (oldOrgContactCollectionForSimilarOrgsByPattern != null)
					{
						UnRegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByPattern);
					}
					return OldOrgContactCollectionForSimilarOrgsByCode;
				}
				else
				{
					if (oldOrgContactCollectionForSimilarOrgsByName != null)
					{
						UnRegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByName);
					}
					if (oldOrgContactCollectionForSimilarOrgsByCode != null)
					{
						UnRegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByCode);
					}
					RegisterEditableChildObject(OldOrgContactCollectionForSimilarOrgsByPattern);
					return OldOrgContactCollectionForSimilarOrgsByPattern;
				}
			}
		}

		#endregion

		#region Additional Messages

		public string AdditionalMessages
		{
			get
			{
				string result = string.Empty;
				foreach (string[] entry in MergedOrders)
				{
					if (string.IsNullOrEmpty(result))
					{
						result += "\n\r" + Res.GetString("dbdc82f4-a8a5-4887-9a09-9ff3ac07e7e3", "The following order numbers have been changed as part of the merge:");
					}
					result += "\n\r" + Res.GetString("6dda8d66-4b4b-43d5-882d-f36d271bf335", "{0}'s Order {1} changed to {2} by merge", entry[0], entry[1], entry[2]);
				}
				return result;
			}
		}

		#endregion

		#region Merged Orders

		List<string[]> mergedOrders;

		internal List<string[]> MergedOrders
		{
			get
			{
				if (mergedOrders == null)
				{
					mergedOrders = new List<string[]>();
				}
				return mergedOrders;
			}
		}

		#endregion

		#endregion

		#region Post changes of pattern parameters

		public void PostChanges(bool collectionChanged)
		{
			if (collectionChanged || MaxResultsCore != MaxResults || CurrentMatchThresholdCodeCore != CurrentMatchThresholdCode)
			{
				MaxResultsCore = MaxResults;
				CurrentMatchThresholdCodeCore = CurrentMatchThresholdCode;
				if (CurrentMode == CurrentQueryMode.Pattern)
				{
					SetOrgsCollectionByPatternsCore();
				}
				else
				{
					SetOrgsCollectionByNameOrCode();
				}
			}
		}

		#endregion

		#region SaveFactories

		public ITransactionParticipant[] SaveFactories
		{
			get
			{
				if (fSaveFactories == null)
				{
					fSaveFactories =
						new ITransactionParticipant[]
						{
							GetOrganisationMerger()
						};
				}

				return fSaveFactories;
			}
		}

		protected virtual OrganisationMerger GetOrganisationMerger()
		{
			return externalConnection == null ? new OrganisationMerger(this) : new OrganisationMerger(this, externalConnection);
		}

		ITransactionParticipant[] fSaveFactories;

		#endregion

		#region Validation

		public void ValidateNewOrganisationPk()
		{
			NewOrganisationPkInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NewOrganisationPkInfo);
			ListValidation.ErrorIfInvalidPK(NewOrganisationPkInfo, ActiveOrganisations, ResString.GetMultilingualString("02ff49e1-18b7-4bcb-864b-bf777cf84946", "You cannot merge into Inactive Organization. Either mark your New Organization as Active, or choose another one."));

			CheckSystemDefinedOrg(NewOrganisationPk, NewOrganisationPkInfo);
			if (OldOrganisation != null)
			{
				CheckMergingSameOrg(OldOrganisation.PK, NewOrganisationPk, NewOrganisationPkInfo);
			}
			ValidateOrgFlags();
			ValidateWarehousePartAttributes();
			CheckForDuplicateVoyageAccounts();
			ValidateCusPermitHeader();
		}

		void ValidateCusPermitHeader()
		{
			if (OldOrganisation != null && NewOrganisationPk.IsValid)
			{
				var oldPermitHeaderList = Factory.Load<ICommonCusPermitHeader>(new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, OldOrganisation.PK));
				foreach (var oldPermitHeader in oldPermitHeaderList)
				{
					var findDuplicatesQuery = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, NewOrganisationPk);
					findDuplicatesQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, oldPermitHeader.CPH_RN_NKCountryCode);
					findDuplicatesQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, oldPermitHeader.CPH_Number);
					findDuplicatesQuery.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, oldPermitHeader.CPH_StartDate);
					findDuplicatesQuery.AddToFilter(CusPermitHeaderSchema.CPH_Type, oldPermitHeader.CPH_Type);
					findDuplicatesQuery.AddToFilter(CusPermitHeaderSchema.CPH_SubType, oldPermitHeader.CPH_SubType);
					var duplicateData = Factory.LoadTop1<ICommonCusPermitHeader>(findDuplicatesQuery);

					if (duplicateData != null)
					{
						NewOrganisationPkInfo.AddError(Res.GetString("0562669d-da23-4238-aae8-63ebb7f1eb84", "Permit Number <{0}> already exists for organization {1}, please amend Permit Number or organization on this permit prior to merge", duplicateData.CPH_Number, NewOrganisation?.OH_Code));
					}
				}
			}
		}

		void CheckForDuplicateVoyageAccounts()
		{
			if (OldOrganisation != null && NewOrganisationPk.IsValid)
			{
				var voyageAccountsOldOrg = Factory.Load<Freight.Integration.Agency.IVoyageAccount>(new ZQuery(JobVoyAccountSchema.NA_OH, OldOrganisation.PK));
				var voyageAccountsNewOrg = Factory.Load<Freight.Integration.Agency.IVoyageAccount>(new ZQuery(JobVoyAccountSchema.NA_OH, NewOrganisationPk));

				var duplicates = voyageAccountsOldOrg.Concat(voyageAccountsNewOrg)
					.Cast<BusinessObject>()
					.GroupBy(voyAccount => new
					{
						VoyagePK = voyAccount[JobVoyAccountSchema.NA_JV],
						CompanyPK = voyAccount[JobVoyAccountSchema.NA_GC]
					})
					.Where(g => g.Count() > 1);

				if (duplicates.Any())
				{
					NewOrganisationPkInfo.AddError(Res.GetString("f38ef23d-211f-4604-ae73-c1af13ac52dc", "Both organizations contain voyage account records for the same vessel/voyage and company. You must resolve this before merging."));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public void ValidateWarehousePartAttributes()
		{
			if (NewOrganisation != null && OldOrganisation != null && !NewOrganisation.IsDeleted && !OldOrganisation.IsDeleted)
			{
				var oldOrgMiscServ = OldOrganisation.MiscServ;
				var newOrgMiscServ = NewOrganisation.MiscServ;
				if (!oldOrgMiscServ.OM_IMPartAttrib1Name.EqualsIgnoringCase(newOrgMiscServ.OM_IMPartAttrib1Name)
					|| oldOrgMiscServ.OM_IMPartAttrib1Type != newOrgMiscServ.OM_IMPartAttrib1Type
					|| !oldOrgMiscServ.OM_IMPartAttrib2Name.EqualsIgnoringCase(newOrgMiscServ.OM_IMPartAttrib2Name)
					|| oldOrgMiscServ.OM_IMPartAttrib2Type != newOrgMiscServ.OM_IMPartAttrib2Type
					|| !oldOrgMiscServ.OM_IMPartAttrib3Name.EqualsIgnoringCase(newOrgMiscServ.OM_IMPartAttrib3Name)
					|| oldOrgMiscServ.OM_IMPartAttrib3Type != newOrgMiscServ.OM_IMPartAttrib3Type
					|| oldOrgMiscServ.OM_IMUseExpiryDate != newOrgMiscServ.OM_IMUseExpiryDate
					|| oldOrgMiscServ.OM_IMUsePackingDate != newOrgMiscServ.OM_IMUsePackingDate
					|| oldOrgMiscServ.OM_IMUseSerialNumber != newOrgMiscServ.OM_IMUseSerialNumber)
				{
					NewOrganisationPkInfo.AddError(Res.GetString("f408797b-ad64-4bbd-8c1f-7fcf00edb1a0", "Cannot merge the two Organizations because the Warehouse Part Attributes do not match."));
				}
			}
		}

		public void ValidateOldOrganisation()
		{
			if (OldOrganisation != null)
			{
				OldOrganisationCodeInfo.ClearAllNotifications();
				CheckSystemDefinedOrg(OldOrganisation.PK, OldOrganisationCodeInfo);
			}
		}

		void ValidateDuplicateProducts()
		{
			if (OldOrganisation == null || NewOrganisation == null)
			{
				return;
			}

			const int maxProductsToDisplay = 10;
			var sql = FormattableString.Invariant($@"
				-- parts that will have duplicates (excluding self-duplicates) after merge
				select top {maxProductsToDisplay + 1} OP_PartNum
				from dbo.OrgSupplierPart
				inner join dbo.OrgPartRelation on OU_OP=OP_PK 
				where OP_IsActive=1
				and OU_Relationship in ('OWN', 'BTH')
				and OU_OH in (@OldOrgPK, @NewOrgPK)
				and OP_PartNum in
				(
					-- parts that will change owner during merge
					select OP_PartNum
					from dbo.OrgSupplierPart
					inner join dbo.OrgPartRelation on OU_OP=OP_PK 
					where OP_IsActive=1
					and OU_Relationship in ('OWN', 'BTH')
					and OU_OH in (@OldOrgPK)
				)
				group by OP_PartNum
				having count(DISTINCT OP_PK) > 1
			");
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@OldOrgPK", OldOrganisation.PK, OrgPartRelationSchema.OU_OH);
			parameters.Add("@NewOrgPK", NewOrganisation.PK, OrgPartRelationSchema.OU_OH);
			var duplicateProducts = new DynamicBusinessObjectCollection(Factory);
			duplicateProducts.Load(sql, parameters);
			if (duplicateProducts.Count > 0)
			{
				var message = Res.GetString(
					"DF71BE6C-FE81-477B-85EB-2078BAE31A0E",
					"Merge will result in duplicate products associated with the new organization. Product codes are:"
				);
				message += " " + string.Join(", ", duplicateProducts.Take(maxProductsToDisplay).Select(p => p[OrgSupplierPartSchema.OP_PartNum]));
				if (duplicateProducts.Count > maxProductsToDisplay)
				{
					message += ",...";
				}

				OldOrganisationCodeInfo.AddError(message);
			}
		}

		protected virtual (ZGuid companyPK, ZGuid orgPK, ZString fan)[] GetFANsForOrgs(ZGuid[] orgPks) => ObjectFactory.Get<ZA.IZACustomsRegistry>().GetFANsForOrgs(orgPks);

		void ValidateOrgFinancialAccountNumberMappings()
		{
			var orgHeaders = CurrentOrgHeaderCollection?.Cast<OrgHeader>().ToList() ?? new List<OrgHeader>();
			if (OldOrganisation?.PK.IsValid ?? false)
			{
				orgHeaders.Add(OldOrganisation);
			}

			if (orgHeaders.Any())
			{
				var orgPks = orgHeaders.Select(org => org.PK).ToArray();
				var orgFan = GetFANsForOrgs(orgPks).FirstOrDefault();
				if (orgFan.companyPK.IsValid)
				{
					var company = Factory.Load<GlbCompany>(orgFan.companyPK);
					var orgHeader = orgHeaders.First(org => org.PK == orgFan.orgPK);

					var errorMessage = Res.GetString("612FE332-FB1F-4D87-BF85-197E4A1D184E",
													"A FAN mapping {0} exists for organization {1} in South Africa Company {2}, please amend FAN list in Registry for this organization prior to merge",
													orgFan.fan, orgHeader.OH_Code, company.GC_Code);

					OldOrganisationCodeInfo.AddError(errorMessage);
					if (orgHeader != OldOrganisation)
					{
						orgHeader.AddRowError(errorMessage);
					}
				}
			}
		}

		void ValidateOrgCusAccount()
		{
			if (OldOrganisation != null)
			{
				var oldAccountList = Factory.Load<OrgCusAccount>(new ZQuery(OrgCusAccountSchema.CZ_OH, OldOrganisation.PK));
				if (oldAccountList.Length > 0)
				{
					OldOrganisationCodeInfo.AddError(Res.GetString("3a6cc4ee-1763-4d0a-a9ba-fdd7f50012e3", "The old organization has been setup with Account Numbers, please remove the Account Numbers from the old organization prior to merge."));
				}
			}
		}

		public void ValidateOrgFlags()
		{
			if (NewOrganisation != null && OldOrganisation != null && !NewOrganisation.IsDeleted && !OldOrganisation.IsDeleted)
			{
				List<string> flags = new List<string>();
				if (!OrganisationRegistry.Instance.AllowMergeIgnoringARAP.Value)
				{
					CheckARAPInAllCompanies(flags);
				}
				CheckControllingBranchValidIfRequired();
				if (flags.Count > 0)
				{
					NewOrganisationPkInfo.AddError(Res.GetString("04cd43b2-ed7c-49e8-b369-9ab0042ccaaa", "The old organization has been setup with the following Organization Types. It can only be merged into another organization that is also setup with these types below:") + "\r\n" + string.Join(", ", flags.ToArray()));
				}
			}
		}

		void CheckControllingBranchValidIfRequired()
		{
			HashSet<string> companyNamesWhereControllingBranchRequired = new HashSet<string>();

			foreach (OrgCompanyData data in OldOrganisation.CompanyDataCollection.ToArray())
			{
				if (data.GetRequiredFields().RequireBranch)
				{
					companyNamesWhereControllingBranchRequired.Add(data.Company.GC_Name);
				}
			}

			foreach (OrgCompanyData newData in NewOrganisation.CompanyDataCollection.ToArray())
			{
				if (!companyNamesWhereControllingBranchRequired.Contains(newData.Company.GC_Name))
				{
					continue;
				}

				if (newData.Header != null && !IsOrgProxyForAnyBranch(newData) && !newData.OB_GB_ControllingBranch.IsValid)
				{
					AddControllingBranchError(newData.Company.GC_Name);
				}
				companyNamesWhereControllingBranchRequired.Remove(newData.Company.GC_Name);
			}

			foreach (string companyName in companyNamesWhereControllingBranchRequired)
			{
				//anything left over will not have a controlling branch in new org
				AddControllingBranchError(companyName);
			}
		}

		void AddControllingBranchError(string companyName)
		{
			NewOrganisationPkInfo.AddError(Res.GetString("4dcfb61f-695a-430a-be8f-db5047d4df48",
						"For company {0}, the new organization lacks a Controlling Branch set up, but the old organization has organization type(s) specified in the registry setting 'Organization Required Fields' to require a Controlling Branch. Please enter a Controlling Branch.",
						companyName));
		}

		void CheckARAPInAllCompanies(List<string> flags)
		{
			var newDatas = new Dictionary<ZGuid, OrgCompanyData>();
			foreach (var group in NewOrganisation.CompanyDataCollection.Cast<OrgCompanyData>().GroupBy(data => data.OB_GC))
			{
				newDatas.Add(group.Key, group.First());
			}
			foreach (var data in OldOrganisation.CompanyDataCollection.Cast<OrgCompanyData>())
			{
				string creditor = Res.GetString("5b55ef45-56fb-45c4-943c-37a86d23eb6e", "Payable ({0})", data.Company.GC_Name);
				string debtor = Res.GetString("4942eec9-91e5-4c14-a3f8-ec1d7b1eab11", "Receivable ({0})", data.Company.GC_Name);
				if (data.OB_IsCreditor && newDatas.ContainsKey(data.OB_GC) && !newDatas[data.OB_GC].OB_IsCreditor)
				{
					flags.Add(creditor);
				}
				if (data.OB_IsDebtor && newDatas.ContainsKey(data.OB_GC) && !newDatas[data.OB_GC].OB_IsDebtor)
				{
					flags.Add(debtor);
				}
				// if org does not have data for that company, you still cannot merge. If you have problems with it, see Jenny.
				if (data.OB_IsCreditor && !newDatas.ContainsKey(data.OB_GC))
				{
					flags.Add(creditor);
				}
				if (data.OB_IsDebtor && !newDatas.ContainsKey(data.OB_GC))
				{
					flags.Add(debtor);
				}
			}
		}

		bool IsOrgProxyForAnyBranch(OrgCompanyData data)
		{
			return data.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbBranch)), new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, data.Header.PK));
		}

		void CheckSystemDefinedOrg(ZGuid checkPK, ZPropertyInfo infoForErrors)
		{
			if (checkPK != ZGuid.Empty)
			{
				OrgHeader checkOrg = new BusinessObjectFactory().Load<OrgHeader>(checkPK);
				if (checkOrg != null && checkOrg.IsSystemDefinedOrganisation)
				{
					infoForErrors.AddError(CannotMergeSystemDefinedOrg);
				}
			}
		}

		void CheckMergingSameOrg(ZGuid oldPK, ZGuid newPK, ZPropertyInfo infoForErrors)
		{
			if (oldPK == newPK)
			{
				infoForErrors.AddError(CannotMergeSameOrganisation);
			}
		}

		public void ValidateOldOrganisations()
		{
			List<BusinessObject> objWithError = new List<BusinessObject>();
			CheckDuplicates(objWithError, CurrentOrgAddressCollection, new MergeOrgAddressComparer());
			CheckDuplicates(objWithError, CurrentOrgContactCollection, new MergeOrgContactComparer());
			foreach (BusinessObject obj in objWithError)
			{
				(obj as IMergeOrgElement).ActionInfo.AddError(Res.GetString("2f995971-12a3-46e0-a7c4-959e79d6bc25", "There is already an address, on the New organization or to be merged from an existing organization, that has the same address details as this one. Please select the Merge action instead."));
			}
		}

		void CheckDuplicates(List<BusinessObject> objWithError, BusinessObjectCollection col, IMergeOrgElementComparer cmp)
		{
			if (col == null || col.Count == 0)
			{
				return;
			}

			foreach (BusinessObject obj in col)
			{
				foreach (BusinessObject objToCompare in col)
				{
					if (obj != null && objToCompare != null && !obj.IsDeleted && !objToCompare.IsDeleted)
					{
						if (cmp.Compare(obj as IMergeOrgElement, objToCompare as IMergeOrgElement) == 1)
						{
							if (!objWithError.Contains(obj))
							{
								objWithError.Add(obj);
							}
							if (!objWithError.Contains(objToCompare))
							{
								objWithError.Add(objToCompare);
							}
						}
					}
				}
			}
		}

		public void ValidateBarcodeValidationRule()
		{
			var sql = @"
SELECT DISTINCT BVR_TargetField
FROM
(
	SELECT
		BRS_PK,
		ISNULL(CASE WHEN BRS_OH_Buyer IS NOT NULL THEN @NewOrgPk ELSE BRS_OH_Buyer END, '00000000-0000-0000-0000-000000000000') AS Buyer,
		ISNULL(CASE WHEN BRS_OH_Supplier IS NOT NULL THEN @NewOrgPk ELSE BRS_OH_Supplier END, '00000000-0000-0000-0000-000000000000') AS Supplier,
		BRS_RelatedEntityId AS RelatedEntity,
		BVR_TargetField
	FROM 
		dbo.BarcodeRuleSet
		JOIN dbo.BarcodeValidationRule ON BVR_BRS_RuleSet = BRS_PK
	WHERE 
		(BRS_OH_Buyer IN (SELECT * FROM @OrgPKs) OR BRS_OH_Supplier IN (SELECT * FROM @OrgPKs))
) DuplicateRules 
GROUP BY Buyer, Supplier, RelatedEntity, BVR_TargetField
HAVING COUNT(*) > 1
ORDER BY BVR_TargetField";

			var orgPks = new List<ZGuid> { NewOrganisationPk };
			if (OldOrganisation != null)
			{
				orgPks.Add(OldOrganisation.PK);
			}

			foreach (var org in CurrentOrgHeaderCollection)
			{
				orgPks.Add(org.PK);
			}

			var duplicateRules = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@OrgPks", orgPks, OrgHeaderSchema.PK, isTableValued: true),
				ZSqlParameter.New("@NewOrgPk", NewOrganisationPk, OrgHeaderSchema.PK)
			};
			duplicateRules.Load(sql, sqlParams);
			if (duplicateRules.Count > 0)
			{
				var duplicateTargetFields = string.Join(", ", duplicateRules.Select(r => r["BVR_TargetField"]));
				NewOrganisationPkInfo.AddError(Res.GetString("bb604327-b4ca-46d1-93b8-0227dd05e24b", "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): {0}", duplicateTargetFields));
			}
		}

		abstract class IMergeOrgElementComparer : Comparer<IMergeOrgElement>
		{
			public override int Compare(IMergeOrgElement x, IMergeOrgElement y)
			{
				return x.Action == MergeOrgAddress.ActionAdd && y.Action == MergeOrgAddress.ActionAdd ? 1 : 0;
			}
		}

		class MergeOrgAddressComparer : IMergeOrgElementComparer
		{
			public override int Compare(IMergeOrgElement x, IMergeOrgElement y)
			{
				MergeOrgAddress adr1 = x as MergeOrgAddress;
				MergeOrgAddress adr2 = y as MergeOrgAddress;
				if (adr1 == null || adr2 == null || adr1.IsDeleted || adr2.IsDeleted || adr1.OldObject.IsDeleted || adr2.OldObject.IsDeleted)
				{
					return 0;
				}

				return base.Compare(x, y) == 1
					&& adr1.OldAddressAddress1 == adr2.OldAddressAddress1
					&& adr1.OldAddressCity == adr2.OldAddressCity
					&& adr1.PK != adr2.PK
					? 1 : 0;
			}
		}

		class MergeOrgContactComparer : IMergeOrgElementComparer
		{
			public override int Compare(IMergeOrgElement x, IMergeOrgElement y)
			{
				MergeOrgContact cnt1 = x as MergeOrgContact;
				MergeOrgContact cnt2 = y as MergeOrgContact;
				if (cnt1 == null || cnt2 == null)
				{
					return 0;
				}

				return base.Compare(x, y) == 1 && cnt1.OldContactName == cnt2.OldContactName && cnt1.PK != cnt2.PK ? 1 : 0;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNewOrganisationPk();
			ValidateOldOrganisation();
			ValidateOldOrganisations();
			ValidateDuplicateProducts();
			ValidateOrgFinancialAccountNumberMappings();
			ValidateOrgCusAccount();
			ValidateBarcodeValidationRule();
		}

		#endregion

		static string CannotMergeSystemDefinedOrg
		{
			get { return Res.GetString("818914cf-a9ae-4b74-85ca-d9282e419ea2", "You cannot merge this organization because it is a special, system-defined organization."); }
		}
		static string CannotMergeSameOrganisation
		{
			get { return Res.GetString("eff739ed-6afa-4d0e-99c7-57e7a160545e", "The \"Old\" organization is the same as the \"New\" organization You cannot merge an organization with itself."); }
		}
	}
}
