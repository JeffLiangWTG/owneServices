using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]

	public class OrgHeaderLink : NonPersistentBusinessObject, IObsoleteValidation
	{
		readonly IUserNotification userNotification;

		public OrgHeader Organisation1 { get; }
		public OrgHeader Organisation2 { get; }

		public OrgHeaderLink(BusinessObjectFactory factory, IUserNotification userNotification, OrgHeader organisation1, OrgHeader organisation2)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(userNotification, nameof(userNotification));
			Argument.NotNull(organisation1, nameof(organisation1));
			Argument.NotNull(organisation2, nameof(organisation2));

			this.userNotification = userNotification;

			Organisation1 = organisation1;
			Organisation2 = organisation2;
		}

		ZGuid otherOrganisationPk;
		[List("OtherRelatedOrgHeaders")]
		public ZGuid OtherOrganisationPk
		{
			get { return otherOrganisationPk; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(OtherOrganisationPkInfo, ref otherOrganisationPk, value);
				}

				OtherOrganisationPkInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OtherOrganisationPkInfo
		{
			get { return GetZPropertyInfo((nameof(OtherOrganisationPk))); }
		}

		public OrgCollectionForLink OtherRelatedOrgHeaders
		{
			get
			{
				if (orgHeaders == null)
				{
					orgHeaders = new OrgCollectionForLink(Factory, Organisation1, Organisation2);
				}
				return orgHeaders;
			}
		}

		OrgCollectionForLink orgHeaders;

		#region Validation

		bool ValidateOtherOrganisation(ZGuid orgParent, ZGuid orgChild1, ZGuid orgChild2)
		{
			var validateResult = true;
			if (orgParent == ZGuid.Empty || orgParent == ZGuid.Missing)
			{
				RaiseError(NoOtherSelectionMade);
				validateResult = false;
			}
			else if (orgParent == orgChild1 || orgParent == orgChild2)
			{
				RaiseError(SelectedDupeOrg);
				validateResult = false;
			}

			return validateResult;
		}

		bool HasAnExistingRelationship(OrgHeader orgParent, OrgHeader orgChild)
		{
			var hasAnExistingRelationship = false;

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, orgParent.PK); // if orgParent is managed by orgChild then orgParent cannot be orgChild's parent
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, orgChild.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			var orgAlreadyLinked = Factory.Exists(typeof(OrgRelatedParty), query);

			var query2 = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK); // If orgChild is already a child, it cannot be added again as a child
			query2.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			var relatedParties = Factory.Load<OrgRelatedParty>(query2);
			var org2HasParent = relatedParties.Any();

			if (orgAlreadyLinked)
			{
				RaiseError(string.Format(CultureInfo.CurrentCulture, OrganisationAlreadyLinked, orgParent.OH_Code, orgChild.OH_Code));
				hasAnExistingRelationship = true;
			}
			else if (org2HasParent)
			{
				var headOrg = relatedParties.First().RelatedParty;
				RaiseError(string.Format(CultureInfo.CurrentCulture, OrganisationAlreadyIsChild, orgParent.OH_Code, orgChild.OH_Code, headOrg.OH_Code));
				hasAnExistingRelationship = true;
			}

			return hasAnExistingRelationship;
		}

		bool IsSameOrgBeingLinked(ZGuid org1, ZGuid org2)
		{
			var isSameOrgBeingLinked = org1 == org2;
			if (isSameOrgBeingLinked)
			{
				RaiseError(CannotLinkSameOrganisation);
			}

			return isSameOrgBeingLinked;
		}

		string NoOtherSelectionMade => Res.GetString("ACD5BD8D-D84B-4A8A-AB4E-98FCC3E47417", "Please choose an organization from the search box.");

		string SelectedDupeOrg => Res.GetString("A57C2C6C-1525-415C-B42E-0D65EA947060", "Your selected organization is already shown.");

		string CannotLinkSameOrganisation => Res.GetString("52299FB8-E3F8-41D1-9EA2-6A9672A0AE24", "You cannot link an organization with itself.");

		string OrganisationAlreadyLinked => Res.GetString("05225B04-DAC3-43BD-8B5C-0995A6DC8EA1", "{0} cannot be the head office of {1} as {1} is already the head office of {0}.");

		string OrganisationAlreadyIsChild => Res.GetString("A08AC32F-8B77-47D4-A310-C7216E42924D", "It is not possible for {0} to be head office of {1} as {1} already has {2} as its head office.\r\nRelationships can be managed under the Related Party tab.");

		#endregion

		#region AddToTable
		public bool TryAddNewOrgRelatedParty(OrgHeader orgParent, OrgHeader orgChild1, OrgHeader orgChild2)
		{
			var relatedSucceeded = ValidateOtherOrganisation(orgParent == null ? ZGuid.Empty : orgParent.PK, orgChild1.PK, orgChild2.PK);

			if (relatedSucceeded)
			{
				relatedSucceeded &= ValidateOrgRelatedParty(orgParent, orgChild1);
				relatedSucceeded &= ValidateOrgRelatedParty(orgParent, orgChild2);

				if (relatedSucceeded)
				{
					AddNewOrgRelatedParty(orgParent, orgChild1);
					AddNewOrgRelatedParty(orgParent, orgChild2);
				}
			}

			return relatedSucceeded;
		}

		public bool TryAddNewOrgRelatedParty(OrgHeader orgParent, OrgHeader orgChild)
		{
			var relatedSucceeded = ValidateOrgRelatedParty(orgParent, orgChild);
			if (relatedSucceeded)
			{
				AddNewOrgRelatedParty(orgParent, orgChild);
			}

			return relatedSucceeded;
		}

		public bool Save()
		{
			var result = true;
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				result = false;
				ZExceptionReporting.HandleSaveException(ex);
			}

			return result;
		}

		#endregion

		void RaiseError(string message)
		{
			userNotification.ShowError(message);
		}

		bool ValidateOrgRelatedParty(OrgHeader orgParent, OrgHeader orgChild)
		{
			return !HasAnExistingRelationship(orgParent, orgChild) && !IsSameOrgBeingLinked(orgParent.PK, orgChild.PK);
		}

		void AddNewOrgRelatedParty(OrgHeader orgParent, OrgHeader orgChild)
		{
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_RelatedParty = orgParent.PK; // parent
			relatedParty.PR_OH_Parent = orgChild.PK; // child
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			orgRelatedParties = orgRelatedParties ?? new List<OrgRelatedParty>();
			orgRelatedParties.Add(relatedParty);
		}

		List<OrgRelatedParty> orgRelatedParties;

		public void DeleteRelatedParties()
		{
			orgRelatedParties?.ForEach(x => x.Delete());
			orgRelatedParties?.Clear();
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = !Env.Security.OrgDuplicateDetectionMerge.IsAllowed;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
