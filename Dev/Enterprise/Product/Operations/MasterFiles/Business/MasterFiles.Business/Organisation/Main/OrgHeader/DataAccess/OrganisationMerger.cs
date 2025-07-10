using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public enum OrganisationMergerActionOnSave
	{
		MergeAndDelete,
		MergeOnly,
		DeleteOnly
	}

	public class OrganisationMerger : SaveInTransactionActionWithMainConnection
	{
		public OrganisationMerger(MergeOrgHeader mergeOrgHeader)
			: this(mergeOrgHeader.OldOrganisation.PK, mergeOrgHeader.NewOrganisationPk, mergeOrgHeader.OldOrgAddressesCollection, mergeOrgHeader.OldOrgContactCollection)
		{
			this.mergeOrgHeader = mergeOrgHeader;
			mergeData.MergeOrgHeader = mergeOrgHeader;

			if (mergeOrgHeader != null && mergeOrgHeader.NewOrganisation != null)
			{
				foreach (OrgBrandOrRelatedName brand in mergeOrgHeader.NewOrganisation.BrandsOrRelatedNames)
				{
					brandsInitial.Add(brand.P1_RelatedName);
					mergeData.InitialBrands.Add(brand.P1_RelatedName);
				}
			}
			mergeOrgHeader.DeleteError = string.Empty;
		}

		public OrganisationMerger(MergeOrgHeader mergeOrgHeader, DbConnection connection)
			: this(mergeOrgHeader)
		{
			this.connection = connection;
			mergeData.Connection = connection;
		}

		protected OrganisationMerger(ZGuid oldOrg, ZGuid newOrg, MergeOrgAddressCollection mergeAddressCollection, MergeOrgContactCollection mergeContactsCollection)
		{
			oldOrganisation = oldOrg;
			newOrganisation = newOrg;
			this.mergeAddressCollection = mergeAddressCollection;
			this.mergeContactsCollection = mergeContactsCollection;
			mergeData = new OrganisationMergeData(oldOrg, newOrg, mergeAddressCollection, mergeContactsCollection);

			foreach (IMergeAction mergeAction in (IEnumerable)(ObjectFactory.Get("OrganisationMergeActions")))
			{
				extraMergeActions.Add(mergeAction);
			}
		}

		readonly OrganisationMergeData mergeData;

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory(Connection);
		}

		DbConnection connection;
		DbConnection Connection
		{
			get
			{
				return connection ?? (connection = Db.Connection);
			}
		}

		OrganisationMergerActionOnSave actionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;
		public OrganisationMergerActionOnSave ActionOnSave
		{
			get
			{
				return actionOnSave;
			}
			set
			{
				actionOnSave = value;
			}
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			if (mergeOrgHeader?.ConcurrencyException != null)
			{
				mergeOrgHeader.ConcurrencyException = null;
			}

			if (actionOnSave == OrganisationMergerActionOnSave.MergeOnly || actionOnSave == OrganisationMergerActionOnSave.MergeAndDelete)
			{
				foreach (var action in MergeHelper.GetAllCoreActions(mergeData))
				{
					action.Merge(mergeData.OldOrgHeader, mergeData.NewOrgHeader, actionOnSave);
				}

				PrepareToMergeOrgAddresses();

				CreateDraftCommissionAgreements();
				MergeClientPickPackParametersByWhs();
				MergeWhsDockets();
				MergeWhsSerialNumbers();
				MergeProductStyles();
				MergeWhsProductParamsByWhsAndClient();
				MergeRatingHeader();
				MergeRatingDetails();
				MergeRateOneOffCarrier();
				DeleteOldOrgPartRelations();
				DeleteWhsPutawayLocationCache();
				DeleteUniversalJobLink();
				MoveGlbGroupLinks();
				MoveOrgReferences();
				MergeOrgAddresses();
				MergeOrgContacts();
				MoveContactsAddressOverrides();
				MoveOrgRelatedParties();
				MergeOrgAppointedAgentPorts();
				MergeEDocs();
				MergeOrgStaffAssignments();
				MergeARAP();
				MergeOrgWebURLs();
				MergeBarcodeRuleSets();
				DeleteOldOrgRefOrgConsortiumPivots();
				FixAccTransactionHeaders();
				MergeSubAccountTypeOrgnisationInTransactionLine();
				MergeSubAccountTypeOrgnisationInTransactionHeader();
				FixAccTaxReturnLines();
				MergeCusPermitHeader();
				MergeOrders();
				MergeWarehouseLinks();
				DeleteRefPacks();
				DeleteOrgProductType();
				DeleteOrgAirlineMAWBStockManagement();
				DeleteCusSeaManSlotOrg();
				DeleteJobTradeLaneVoyage();
				DeleteStmMenuDocumentConfig();
				MergeOrgServiceLevels();
				UpdateCountryDatasWithApprovedLocation();
				DeleteOrgPatternMatchAddress();
				DeleteOrgMatchApproval();
				MoveOrgDocuments();
				MergeSuppressedDocuments();
				MoveOrgSubscriptions();
				MoveGenAddOnColumn();
				MoveCountryData();
				MoveGlobalChargeCodeMapPivot();
				AddMergeLogs();
				SetOldOrganisationInactive();
				RegeneratePatternMatchesForNewOrg();
				MergeJobComInvoiceHeader();
				DoAdditionalMergeActions();
				MergeJobDeclaration();
				MergeCustomFields();
				MergeCustomizations();
				MergeRatingContractNamedAccountPivots();
				MergeAllocationRouteAgentPivots();
				MergeOrgAddressAdditionalInfo();
				MergeOrgCompetitors();
				MergeOrgFlags();
				MergeOrgRefFacilities();
				MergeProductionRules();
			}
			if (actionOnSave == OrganisationMergerActionOnSave.DeleteOnly || actionOnSave == OrganisationMergerActionOnSave.MergeAndDelete)
			{
				new RetryHandler(new TimeSpan[] { TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1) }).Invoke(DeleteOldOrganization);
			}

			return ChangedTableNames.All;
		}

		void MergeCustomFields()
		{
			using (var cmd = GetCommandOnMainConnection($@"
update dbo.GenCustomAddOnValue set XV_ParentID = @NewOrgPK,
XV_SystemLastEditUser = @SystemLastEditUser,
XV_SystemLastEditTimeUtc = @SystemLastEditTimeUtc
where XV_ParentID = @OldOrgPK
and not exists (select 1 from dbo.GenCustomAddOnValue t where t.XV_ParentID = @NewOrgPK
and t.XV_Name = GenCustomAddOnValue.XV_Name
and t.XV_Type = GenCustomAddOnValue.XV_Type)"))
			{
				cmd.AddParameter("@NewOrgPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
				cmd.AddParameter("@OldOrgPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void MergeCustomizations()
		{
			using (var cmd = GetCommandOnMainConnection($@"update dbo.OrgCustomLabels set OT_OH = @NewOrgPK where OT_OH = @OldOrgPK
and not exists (select 1 from dbo.OrgCustomLabels t where t.OT_OH = @NewOrgPK
and t.OT_FieldName = OrgCustomLabels.OT_FieldName)"))
			{
				cmd.AddParameter("@NewOrgPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrgPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void DoAdditionalMergeActions()
		{
			foreach (var mergeAction in extraMergeActions)
			{
				mergeAction.Merge(OldOrgHeader, NewOrgHeader, ActionOnSave);
			}
		}

		protected virtual void DeleteOldOrganization()
		{
			try
			{
				var factory = GetNewFactory();
				factory.NameForDebugging = "Tagged ROPE team for WI00214111";
				var org = factory.Load<OrgHeader>(oldOrganisation);
				if (org != null)
				{
					org.Delete();
				}
				//fix WI00134880 - deletion of old org failing due to lingering OrgSecurity rows with matching OX_OH. Cause unknown.
				var orgSecurityRows = factory.Load<OrgSecurity>(new ZQuery(OrgSecuritySchema.OX_OH, oldOrganisation));
				foreach (var row in orgSecurityRows)
				{
					if (!row.IsDeleted)
					{
						row.Delete();
					}
				}

				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				if (mergeOrgHeader != null)
				{
					mergeOrgHeader.ConcurrencyException = ex;
				}

				throw;
			}
			catch (ZSaveException ex)
			{
				if (mergeOrgHeader != null)
				{
					StringBuilder s = new StringBuilder((NoResString)"** Error Saving Record**\r\n");
					if (ex.InnerException != null && ex.InnerException.InnerException != null)
					{
						s.Append((NoResString)"Inner Message = ");
						s.Append(ex.InnerException.InnerException.Message);
					}
					mergeOrgHeader.DeleteError = s.ToString();
				}
				throw;
			}
		}

		void RegeneratePatternMatchesForNewOrg()
		{
			if (mergeAddressCollection != null)
			{
				OrgHeader org = GetNewFactory().Load<OrgHeader>(newOrganisation);
				if (org != null)
				{
					List<IMatchingAddress> addedAddresses = GetAddedAddresses();
					var addedOrgNames = GetAddedOrgNames(addedAddresses);
					var addedBrands = GetAddedBrands(org);
					addedOrgNames.AddRange(addedBrands);
					org.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(org, addedAddresses, addedOrgNames.Count > 0 ? addedOrgNames : null);
					org.Factory.Save();
				}
			}
		}

		IEnumerable<OrganisationName> GetAddedBrands(OrgHeader org)
		{
			OrgBrandOrRelatedNameCollection col = org.BrandsOrRelatedNames;
			var result = new List<OrganisationName>(col.Count);
			foreach (OrgBrandOrRelatedName brand in col)
			{
				if (!brandsInitial.Contains(brand.P1_RelatedName))
				{
					result.Add(new OrganisationName(brand.P1_RelatedName, org.OH_Language));
				}
			}
			return result;
		}

		List<OrganisationName> GetAddedOrgNames(List<IMatchingAddress> addedAddresses)
		{
			if (addedAddresses == null)
			{
				return new List<OrganisationName>();
			}

			var result = new List<OrganisationName>(addedAddresses.Count);
			foreach (var adr in addedAddresses)
			{
				if (!adr.OA_CompanyNameOverride.IsEmpty)
				{
					result.Add(new OrganisationName(adr.OA_CompanyNameOverride, adr.OA_Language) { OrgAddressPK = adr.PK });
				}
			}
			return result;
		}

		List<IMatchingAddress> GetAddedAddresses()
		{
			List<IMatchingAddress> addedAddresses = new List<IMatchingAddress>();
			foreach (MergeOrgAddress adr in mergeAddressCollection)
			{
				if (adr.Action == MergeOrgAddress.ActionAdd)
				{
					addedAddresses.Add(adr.OldObject as IMatchingAddress);
				}
			}
			return addedAddresses.Count > 0 ? addedAddresses : null;
		}

		#region DeleteWhsPutawayLocationCache

		void DeleteWhsPutawayLocationCache()
		{
			var deleteSql = $"DELETE FROM dbo.WhsPutawayLocationCache WHERE WPC_OH_Client = @oldOrgPk";
			using (var cmd = Db.Connection.Command(deleteSql)) // We're testing tables and views that don't have Schemas in Z
			{
				cmd.AddParameter("@oldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region DeleteUniversalJobLink

		void DeleteUniversalJobLink()
		{
			var deleteSql = $"DELETE FROM dbo.StmUniversalJobLink WHERE UCL_OH_Owner = @oldOrgPk";
			using (var cmd = Db.Connection.Command(deleteSql)) // We're testing tables and views that don't have Schemas in Z
			{
				cmd.AddParameter("@oldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge dbo.OrgHeader Flags

		public void MergeOrgFlags()
		{
			if (!OldOrgHeader.IsDeleted)
			{
				string sqlText = @"
						UPDATE dbo.OrgHeader
						SET
							OH_IsNationalAccount = @IsNationalAccount,
							OH_IsGlobalAccount = @IsGlobalAccount,
							OH_IsConsignee = @IsConsignee,
							OH_IsConsignor = @IsConsignor,
							OH_IsTransportClient = @IsTransportClient,
							OH_IsWarehouseClient = @IsWarehouseClient,
							OH_IsShippingProvider = @IsShippingProvider,
							OH_IsForwarder = @IsForwarder,
							OH_IsBroker = @IsBroker,
							OH_IsMiscFreightServices = @IsMiscFreightServices,
							OH_IsCompetitor = @IsCompetitor,
							OH_IsSalesLead = @IsSalesLead,
							OH_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
							OH_SystemLastEditUser = @SystemLastEditUser
						WHERE OH_PK = @NewOrgPk";
				using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@IsNationalAccount", SqlDbType.Bit, OldOrgHeader.OH_IsNationalAccount || NewOrgHeader.OH_IsNationalAccount);
					cmd.AddParameter("@IsGlobalAccount", SqlDbType.Bit, OldOrgHeader.OH_IsGlobalAccount || NewOrgHeader.OH_IsGlobalAccount);
					cmd.AddParameter("@IsConsignee", SqlDbType.Bit, OldOrgHeader.OH_IsConsignee || NewOrgHeader.OH_IsConsignee);
					cmd.AddParameter("@IsConsignor", SqlDbType.Bit, OldOrgHeader.OH_IsConsignor || NewOrgHeader.OH_IsConsignor);
					cmd.AddParameter("@IsTransportClient", SqlDbType.Bit, OldOrgHeader.OH_IsTransportClient || NewOrgHeader.OH_IsTransportClient);
					cmd.AddParameter("@IsWarehouseClient", SqlDbType.Bit, OldOrgHeader.OH_IsWarehouseClient || NewOrgHeader.OH_IsWarehouseClient);
					cmd.AddParameter("@IsShippingProvider", SqlDbType.Bit, OldOrgHeader.OH_IsShippingProvider || NewOrgHeader.OH_IsShippingProvider);
					cmd.AddParameter("@IsForwarder", SqlDbType.Bit, OldOrgHeader.OH_IsForwarder || NewOrgHeader.OH_IsForwarder);
					cmd.AddParameter("@IsBroker", SqlDbType.Bit, OldOrgHeader.OH_IsBroker || NewOrgHeader.OH_IsBroker);
					cmd.AddParameter("@IsMiscFreightServices", SqlDbType.Bit, OldOrgHeader.OH_IsMiscFreightServices || NewOrgHeader.OH_IsMiscFreightServices);
					cmd.AddParameter("@IsCompetitor", SqlDbType.Bit, OldOrgHeader.OH_IsCompetitor || NewOrgHeader.OH_IsCompetitor);
					cmd.AddParameter("@IsSalesLead", SqlDbType.Bit, OldOrgHeader.OH_IsSalesLead || NewOrgHeader.OH_IsSalesLead);
					cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
					cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());

					cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Unapprove Commission Agreements

		public void CreateDraftCommissionAgreements()
		{
			var factory = GetNewFactory();
			factory.RefreshEnabled = false;
			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));

			var opportunitySubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			opportunitySubQuery.AddToFilter(OrgOpportunitySchema.P8_OH, oldOrganisation);

			var draftSubQuery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreement), OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, true);

			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, opportunitySubQuery, JoinCondition.And);
			query.AddSubQuery(OrgCommissionAgreementSchema.PK, draftSubQuery, JoinCondition.And);
			query.AddToFilter(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, SQLComparisonOperator.NotEqual, DBNull.Value);

			var agreements = factory.Load<OrgCommissionAgreement>(query);
			foreach (var agreement in agreements)
			{
				var draftAgreement = agreement.CreateDraft();
				if (agreement.CA0_OH_Customer == oldOrganisation)
				{
					draftAgreement.CA0_OH_Customer = newOrganisation;
				}
			}
			factory.Save();
		}

		#endregion

		#region Logging

		void AddMergeLogs()
		{
			BusinessObjectFactory logFactory = GetNewFactory();
			logFactory.RefreshEnabled = false; // Only adds and save new logs, no need to refresh changes in/from other factories.

			var oldOrg = logFactory.Load<OrgHeader>(oldOrganisation);
			var newOrg = logFactory.Load<OrgHeader>(newOrganisation);
			if (oldOrg != null && newOrg != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				newOrg.Logs.AddNew(Events.EditedARecord, FormattableString.Invariant($"Organisation {oldOrg.OH_Code} was merged into this organisation."));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				oldOrg.Logs.AddNew(MergedLogEvent, FormattableString.Invariant($"This organisation was merged into organisation {newOrg.OH_Code}.|{newOrganisation}"));
				logFactory.Save();
			}
		}

		public static Event MergedLogEvent => AutoEvents.MovedToDifferentJob;

		#endregion

		#region General OrgHeader merge

		/// <summary>
		/// Update FKs to OldOrganisation to point to the new one
		/// </summary>
		protected void MoveOrgReferences()
		{
			MoveReferences(oldOrganisation.ToGuid(), newOrganisation.ToGuid(), OrgHeaderSchema.Constants.Prefix);
		}

		/// <summary>
		/// Inactivate OldOrganisation
		/// </summary>
		protected void SetOldOrganisationInactive()
		{
			string sqlText = "UPDATE dbo.OrgHeader SET OH_IsActive = 0, OH_SystemLastEditTimeUtc = @SystemLastEditTimeUtc, OH_SystemLastEditUser = @SystemLastEditUser WHERE OH_PK = @OldOrgPk";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Addresses and Contacts

		#region Merge Addresses

		void PrepareToMergeOrgAddresses()
		{
			mergedAddresses = new Dictionary<ZGuid, ZGuid>();

			if (OrgIsDeleted())
			{
				return;
			}

			foreach (MergeOrgAddress mergeAddressInfo in mergeAddressCollection)
			{
				if (mergeAddressInfo.Action == MergeOrgAddress.ActionAdd)
				{
					var oldObject = mergeAddressInfo.OldObjectCore;
					var existingAddressPKInNewOrg = GetExistingAddressPKInNewOrg(oldObject, Connection, newOrganisation.ToGuid());
					if (existingAddressPKInNewOrg.HasValue)
					{
						mergeAddressInfo.NewObjectPK = existingAddressPKInNewOrg.Value;
						mergeAddressInfo.Action = MergeOrgAddress.ActionMerge;
					}
				}

				if (mergeAddressInfo.Action == MergeOrgAddress.ActionMerge)
				{
					mergedAddresses.Add(mergeAddressInfo.OldAddressPK, mergeAddressInfo.NewObjectPK);
				}
			}
		}

		bool OrgIsDeleted()
			=> mergeOrgHeader != null && (mergeOrgHeader.OldOrganisation.IsDeleted || mergeOrgHeader.NewOrganisation.IsDeleted);

		void MergeOrgAddresses()
		{
			if (OrgIsDeleted())
			{
				return;
			}

			foreach (MergeOrgAddress mergeAddressInfo in mergeAddressCollection)
			{
				if (mergeAddressInfo.Action == MergeOrgAddress.ActionAdd)
				{
					MoveAddressToNewOrg(mergeAddressInfo.OldObjectCore);
				}
				else
				{
					MoveReferences(mergeAddressInfo);
				}
			}
		}

		void MoveReferences(MergeOrgAddress mergeAddressInfo)
		{
			MoveReferences(mergeAddressInfo.OldAddressPK, mergeAddressInfo.NewObjectPK, "OA");
			UpdateContactOC_OA_OrgAddressFromAddress(mergeAddressInfo.NewObjectPK, mergeAddressInfo.OldAddressPK);
		}

		void MoveAddressToNewOrg(OrgAddress oldAddress)
		{
			var codes = GetAllOrgUsageComments(newOrganisation).ToArray();
			if (oldAddress.OA_Code.IsEmpty || !oldAddress.IsUniqueUsageComment(oldAddress.OA_Code, codes))
			{
				oldAddress.SetDefaultUsageComment(codes);
			}
			string sqlText = @"
				UPDATE dbo.OrgAddressCapability SET PZ_IsMainAddress = 0 WHERE PZ_OA = @OldAddressPk
				UPDATE dbo.OrgAddress SET OA_OH = @NewOrgPk, OA_Code = @OACode WHERE OA_PK = @OldAddressPk";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldAddressPk", SqlDbType.UniqueIdentifier, oldAddress.PK.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@OACode", oldAddress.OA_Code.ToString(), OrgAddressSchema.OA_Code);
				cmd.ExecuteNonQuery();
			}
		}

		internal static Guid? GetExistingAddressPKInNewOrg(OrgAddress oldAddress, DbConnection connection, Guid newOrganisationPK)
		{
			string sql = string.Format(@"
				SELECT TOP 1 OA_PK FROM dbo.OrgAddress
				WHERE {0} = @orgHeaderPK
				AND RTRIM(LTRIM({1})) = @addr1
				AND RTRIM(LTRIM({2})) = @addr2
				AND RTRIM(LTRIM({3})) = @city
				AND RTRIM(LTRIM({4})) = @state
				AND RTRIM(LTRIM({5})) = @phone
				AND RTRIM(LTRIM({6})) = @fax
				AND RTRIM(LTRIM({7})) = @mobile
				AND RTRIM(LTRIM({8})) = @email
				AND RTRIM(LTRIM({9})) = @code",
					OrgAddressSchema.OA_OH.Name, //0
					OrgAddressSchema.OA_Address1.Name, //1
					OrgAddressSchema.OA_Address2.Name, //2
					OrgAddressSchema.OA_City.Name, //3
					OrgAddressSchema.OA_State.Name, //4
					OrgAddressSchema.OA_Phone.Name, //5
					OrgAddressSchema.OA_Fax.Name, //6
					OrgAddressSchema.OA_Mobile.Name, //7
					OrgAddressSchema.OA_Email.Name, //8
					OrgAddressSchema.OA_Code.Name); //9

			using (var cmd = GetCommandOnMainConnection(sql, connection))
			{
				cmd.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, newOrganisationPK);
				cmd.AddParameterBasedOnDbColumn("@addr1", oldAddress.OA_Address1.ToString().Trim(), OrgAddressSchema.OA_Address1);
				cmd.AddParameterBasedOnDbColumn("@addr2", oldAddress.OA_Address2.ToString().Trim(), OrgAddressSchema.OA_Address2);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@city", oldAddress.OA_City.ToString().Trim(), OrgAddressSchema.OA_City);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@state", oldAddress.OA_State.ToString().Trim(), OrgAddressSchema.OA_State);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@phone", oldAddress.OA_Phone.ToString().Trim(), OrgAddressSchema.OA_Phone);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@fax", oldAddress.OA_Fax.ToString().Trim(), OrgAddressSchema.OA_Fax);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@mobile", oldAddress.OA_Mobile.ToString().Trim(), OrgAddressSchema.OA_Mobile);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@email", oldAddress.OA_Email.ToString().Trim(), OrgAddressSchema.OA_Email);
				cmd.AddParameterBasedOnDbColumn((NoResString)"@code", oldAddress.OA_Code.ToString().Trim(), OrgAddressSchema.OA_Code);

				var result = cmd.ExecuteScalar();
				return result != null ? (Guid?)result : null;
			}
		}

		IEnumerable<string> GetAllOrgUsageComments(ZGuid pk)
		{
			using (DbCommand cmd = GetCommandOnMainConnection("select OA_Code from dbo.OrgAddress where OA_OH = @OrgHeaderPK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OrgHeaderPK", SqlDbType.UniqueIdentifier, pk.ToGuid());
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader.GetString(0);
					}
				}
			}
		}

		#endregion

		#region Merge Contacts

		void MergeOrgContacts()
		{
			List<MergeOrgContact> moved = new List<MergeOrgContact>();
			foreach (MergeOrgContact mergeContactInfo in mergeContactsCollection)
			{
				if (mergeContactInfo.Action == MergeOrgContact.ActionAdd)
				{
					MoveContactToNewOrg(mergeContactInfo.OldContactPK);
					if (mergedAddresses.ContainsKey(mergeContactInfo.OldContactAddress))
					{
						UpdateContactOC_OA_OrgAddressFromContact(mergedAddresses[mergeContactInfo.OldContactAddress], mergeContactInfo.OldContactPK);
					}
					moved.Add(mergeContactInfo);
				}
				else
				{
					MoveDocumentsToNewContact(mergeContactInfo.OldContactPK.ToGuid(), mergeContactInfo.NewContactPK.ToGuid(), newOrganisation.ToGuid());
					MoveGlbGroupOrgContactLinksToNewContact(mergeContactInfo.OldContactPK.ToGuid(), mergeContactInfo.NewContactPK.ToGuid());
					MoveReferences(mergeContactInfo.OldContactPK, mergeContactInfo.NewObjectPK, "OC");
					MoveAndMergeOrgContactItems(mergeContactInfo.OldContactPK, mergeContactInfo.NewObjectPK);
				}
			}

			if (moved.Count > 0)
			{
				string cntPks = string.Empty;
				foreach (var movedContact in moved)
				{
					cntPks += cntPks.Length > 0 ? ",'" + movedContact.OldContactPK + "'" : "'" + movedContact.OldContactPK + "'";
				}
				string sql = "update dbo.OrgDocument set OD_DefaultContact = 0 where od_oc in (" + cntPks + @") 
				and 
				(
					OD_DocumentGroup in 
					(
						select OD_DocumentGroup from dbo.OrgDocument where OD_DefaultContact = 1 and od_oc in 
						(
							select oc_pk from dbo.orgcontact where oc_oh ='" + newOrganisation + @"' 
							and	oc_pk not in (" + cntPks + @")
						)
					)
					or
					OD_SU_MenuItem in 
					(
						select OD_SU_MenuItem from dbo.OrgDocument where OD_DefaultContact = 1 and od_oc in 
						(
							select oc_pk from dbo.orgcontact where oc_oh ='" + newOrganisation + @"' 
							and	oc_pk not in (" + cntPks + @")
						)
					)
				)";
				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		void MoveDocumentsToNewContact(Guid oldContactPK, Guid newContactPK, Guid newOrganization)
		{
			var sqlText = @"
DECLARE @OldContactName nvarchar(max)

SELECT @OldContactName = OC_ContactName FROM dbo.OrgContact WHERE OC_PK = @OldContactPK

SELECT OD_DocumentGroup AS ExistingDocumentGroup, OD_SU_MenuItem AS ExistingMenuItem, OD_DefaultContact AS ExistingDefaultContact, OC_ContactName AS ExistingContactName INTO #TempDocument FROM dbo.OrgDocument JOIN dbo.OrgContact ON OD_OC = OC_PK AND OC_OH = @NewOrganization

UPDATE dbo.OrgDocument SET OD_OC = @NewContactPK,
	OD_DefaultContact =
	CASE WHEN EXISTS
	(
		SELECT 1 FROM #TempDocument WHERE ((ExistingDocumentGroup <> '' AND ExistingDocumentGroup = OD_DocumentGroup) OR (ExistingMenuItem IS NOT NULL AND OD_SU_MenuItem = ExistingMenuItem)) AND ExistingDefaultContact = 1
	)
	THEN 0
	ELSE OD_DefaultContact
	END
	WHERE OD_OC = @OldContactPK AND OD_DocumentGroup <> 'NOT'
	AND
	(
		NOT EXISTS
		(
			SELECT 1 FROM #TempDocument
			WHERE
			ExistingContactName = @OldContactName
			AND
			(
				(ExistingDocumentGroup <> '' AND OD_DocumentGroup = ExistingDocumentGroup) OR (ExistingMenuItem IS NOT NULL AND ExistingMenuItem = OD_SU_MenuItem)
			)
		)
	)

UPDATE dbo.OrgDocument SET OD_OC = @NewContactPK WHERE OD_OC = @OldContactPK AND OD_DocumentGroup = 'NOT' AND NOT EXISTS (SELECT 1 FROM #TempDocument WHERE ExistingDocumentGroup = 'NOT')

IF OBJECT_ID('tempdb..#TempDocument') IS NOT NULL DROP TABLE #TempDocument
";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewContactPK", SqlDbType.UniqueIdentifier, newContactPK);
				cmd.AddParameter("@OldContactPK", SqlDbType.UniqueIdentifier, oldContactPK);
				cmd.AddParameter("@NewOrganization", SqlDbType.UniqueIdentifier, newOrganization);
				cmd.ExecuteNonQuery();
			}
		}

		#region  Move GlbGroupOrgContactLinks

		class MergeGlbGroupOrgContactLink
		{
			public MergeGlbGroupOrgContactLink(Guid pk, Guid groupPK)
			{
				PK = pk;
				GroupPK = groupPK;
			}

			public Guid PK { get; set; }
			public Guid GroupPK { get; set; }
		}

		List<MergeGlbGroupOrgContactLink> GetGlbGroupOrgContactLinks(ZGuid contactPK)
		{
			var result = new List<MergeGlbGroupOrgContactLink>();
			using (var cmd = GetCommandOnMainConnection((NoResString)"SELECT GCK_PK, GCK_GG_Group FROM dbo.GlbGroupOrgContactLink WHERE GCK_OC_Contact = @ContactPK"))
			{
				cmd.AddParameter("@ContactPK", SqlDbType.UniqueIdentifier, contactPK.ToGuid());
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						result.Add(new MergeGlbGroupOrgContactLink(rd.GetGuid(0), rd.GetGuid(1)));
					}
				}
			}
			return result;
		}

		void MoveGlbGroupOrgContactLinksToNewContact(Guid oldContactPK, Guid newContactPK)
		{
			var oldContactGroupLinks = GetGlbGroupOrgContactLinks(oldContactPK);
			var newContactGroupLinks = GetGlbGroupOrgContactLinks(newContactPK);
			foreach (var oldContactGroupLink in oldContactGroupLinks)
			{
				if (newContactGroupLinks.Any(x => x.GroupPK == oldContactGroupLink.GroupPK))
				{
					//Delete oldContactGroupLink
					using (var cmd = GetCommandOnMainConnection(@"
						DELETE FROM dbo.GlbGroupOrgContactLink WHERE GCK_PK = @OldContactGroupLinkPK"))
					{
						cmd.AddParameter("@OldContactGroupLinkPK", SqlDbType.UniqueIdentifier, oldContactGroupLink.PK);
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					//Update GCK_OC_Contact
					using (var cmd = GetCommandOnMainConnection((NoResString)@"
						UPDATE dbo.GlbGroupOrgContactLink
						SET
							GCK_OC_Contact = @NewContactPK,
							GCK_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
							GCK_SystemLastEditUser = @SystemLastEditUser
						WHERE GCK_PK = @OldContactGroupLinkPK"))
					{
						cmd.AddParameter("@NewContactPK", SqlDbType.UniqueIdentifier, newContactPK);
						cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
						cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
						cmd.AddParameter("@OldContactGroupLinkPK", SqlDbType.UniqueIdentifier, oldContactGroupLink.PK);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#endregion

		void UpdateContactOC_OA_OrgAddressFromAddress(ZGuid newFK, ZGuid oldOrgPk)
		{
			string sqlText = @"
				UPDATE dbo.OrgContact SET OC_OA_OrgAddress = @NewFK WHERE OC_OA_OrgAddress = @OldFK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@newFK", SqlDbType.UniqueIdentifier, newFK.ToGuid());
				cmd.AddParameter("@OldFK", SqlDbType.UniqueIdentifier, oldOrgPk.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void UpdateContactOC_OA_OrgAddressFromContact(ZGuid newFK, ZGuid oldContactPk)
		{
			string sqlText = @"
				UPDATE dbo.OrgContact SET OC_OA_OrgAddress = @NewFK WHERE OC_PK = @OldContactPk";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@newFK", SqlDbType.UniqueIdentifier, newFK.ToGuid());
				cmd.AddParameter("@OldContactPk", SqlDbType.UniqueIdentifier, oldContactPk.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void MoveContactToNewOrg(ZGuid oldContactPk)
		{
			string sqlText = @"
				UPDATE dbo.OrgContact SET OC_OH = @NewOrgPk WHERE OC_PK = @OldContactPk";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldContactPk", SqlDbType.UniqueIdentifier, oldContactPk.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void MoveContactsAddressOverrides()
		{
			string sqlText = @"
				UPDATE dbo.OrgContact SET OC_OH_AddressOverride = @NewOrgPK WHERE OC_OH_AddressOverride = @OldOrgPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrgPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrgPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void MoveAndMergeOrgContactItems(ZGuid oldContactPk, ZGuid newContactPk)
		{
			var updateSql = string.Format(@"
UPDATE oldItem
SET {0} = @newContactPk
FROM {1} oldItem
WHERE
	oldItem.{0} = @oldContactPk
	AND NOT EXISTS
	(
		SELECT 1
		FROM {1} newItem
		WHERE
			newItem.OI_OC = @newContactPk
			AND newItem.{2} = oldItem.{2}
			AND newItem.{3} = oldItem.{3}
			AND newItem.{4} = oldItem.{4}
	)",
						OrgContactItemSchema.Constants.OI_OC,
						OrgContactItemSchema.Constants.TableName,
						OrgContactItemSchema.Constants.OI_ContactItemType,
						OrgContactItemSchema.Constants.OI_Description,
						OrgContactItemSchema.Constants.OI_Address);

			using (var cmd = GetCommandOnMainConnection(updateSql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@oldContactPk", SqlDbType.UniqueIdentifier, oldContactPk.ToGuid());
				cmd.AddParameter("@newContactPk", SqlDbType.UniqueIdentifier, newContactPk.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		void MoveReferences(ZGuid oldPk, ZGuid newPk, string parentTableCode)
		{
			try
			{
				using (DbCommand cmd = GetCommandOnMainConnection("XT_MoveFkReferences")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.CommandTimeout = 3600; //one hour
					cmd.AddParameter("@NewParentPk", SqlDbType.UniqueIdentifier, newPk.ToGuid());
					cmd.AddParameter("@OldParentPk", SqlDbType.UniqueIdentifier, oldPk.ToGuid());
					cmd.AddParameter("@ParentTableCode", SqlDbType.Char, parentTableCode);
					cmd.AddParameter("@FkSystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser?.GS_Code.ToString() ?? User.UnKnownUserCode);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.ExecuteNonQuery();
				}
			}
			catch (SqlException ex)
			{
				var error = (NoResString)"Attempt to add an invoice which overlaps with dates in existing invoices.";
				if (mergeOrgHeader != null && ex.Message.Contains(error))
				{
					mergeOrgHeader.AddOverlappingDatesException = ex;
				}

				throw;
			}
		}

		#endregion

		#region Merge Org Appointed Agent Ports

		void MergeOrgAppointedAgentPorts()
		{
			string sql = "update dbo.OrgAppointedAgentPorts set O5_OA_AgentOfficeAddress = @NewAgentOfficeAddress where O5_OA_AgentOfficeAddress = @RawAgentOfficeAddress";
			foreach (KeyValuePair<ZGuid, ZGuid> pair in mergedAddresses)
			{
				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameterBasedOnDbColumn("@NewAgentOfficeAddress", pair.Value.ToGuid(), OrgAppointedAgentPortsSchema.O5_OA_AgentOfficeAddress);
					cmd.AddParameterBasedOnDbColumn("@RawAgentOfficeAddress", pair.Key.ToGuid(), OrgAppointedAgentPortsSchema.O5_OA_AgentOfficeAddress);
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Move Org Related Parties

		void MoveOrgRelatedParties()
		{
			string sqlText = "update dbo.OrgRelatedParty set PR_OH_RelatedParty = @NewOrganisationPK where PR_OH_RelatedParty = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}

			sqlText = "select PR_PK, case when PR_OA is null then 0 else 1 end from dbo.OrgRelatedParty where PR_OH_Parent = @OldOrganisationPK";
			Dictionary<Guid, bool> relatedPartyPKs = GetRelatedPartiesFromQuery(sqlText, ZSqlParameter.New("@OldOrganisationPK", oldOrganisation.ToGuid(), OrgRelatedPartySchema.PR_OH_Parent));

			newOrgHeader = null;
			if (relatedPartyPKs.Count > 0)
			{
				foreach (var pair in relatedPartyPKs)
				{
					if (pair.Value)
					{
						sqlText = @"update dbo.OrgRelatedParty set PR_OA = @NewMainAddressPK where PR_OH_Parent = @OldOrganisationPK and PR_PK = @PairKey
								and PR_OA in (select OA_PK from dbo.orgaddress where OA_OH = @OldOrganisationPK)";
						using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
						{
							cmd.AddParameter("@PairKey", SqlDbType.UniqueIdentifier, pair.Key);
							cmd.AddParameter("@NewMainAddressPK", SqlDbType.UniqueIdentifier, NewOrgHeader.MainAddress.PK.ToGuid());
							cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
							cmd.ExecuteNonQuery();
						}
					}

					#region SuppressResourceStringsCheckRegion
					var multiRelatedPartiesPartyTypes = ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI ? "'SRV', 'WRP'" : "'CAV', 'CAU', 'SRV', 'MAN'";
					#endregion
					sqlText = $@"update dbo.OrgRelatedParty set PR_OH_Parent = @NewOrganisationPK 
								where PR_OH_Parent = @OldOrganisationPK 
								and PR_PK = @PairKey
								and PR_PartyType <> @PartyType
								and not exists
								(
									select * from dbo.OrgRelatedParty t1
									inner join dbo.OrgRelatedParty t2 on
									t1.PR_PartyType = t2.PR_PartyType and
									t1.PR_FreightTransportMode = t2.PR_FreightTransportMode and
									t1.PR_FreightContainerMode = t2.PR_FreightContainerMode and
									t1.PR_FreightDirection = t2.PR_FreightDirection
									where (t1.PR_GC = t2.PR_GC or (t1.PR_GC is null and t2.PR_GC is null))
									and t1.PR_PK = @PairKey
									and t2.PR_PK <> @PairKey
									and t2.PR_OH_Parent = @NewOrganisationPK
									and (t1.PR_OH_RelatedParty = t2.PR_OH_RelatedParty and t1.PR_PartyType in ({multiRelatedPartiesPartyTypes}) OR t1.PR_PartyType NOT in ({multiRelatedPartiesPartyTypes}))
								)";

					using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@PairKey", SqlDbType.UniqueIdentifier, pair.Key);
						cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
						cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
						cmd.AddParameterBasedOnDbColumn("@PartyType", RelatedPartyTypeList.Codes.ManagementGrouping, OrgRelatedPartySchema.PR_PartyType);
						cmd.ExecuteNonQuery();
					}
				}

				sqlText = "delete from dbo.OrgRelatedParty where PR_OH_Parent = @OldOrganisationPK";
				using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}

		Dictionary<Guid, bool> GetRelatedPartiesFromQuery(string sql, ZSqlParameter parameter)
		{
			Dictionary<Guid, bool> result = new Dictionary<Guid, bool>();
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter(parameter);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0), reader.GetInt32(1) == 1);
					}
				}
			}
			return result;
		}

		#endregion

		#region Merge eDocs

		void MergeEDocs()
		{
			DataRow oldStorage = GetOrgStorageMain(oldOrganisation);

			if (oldStorage != null)
			{
				DataRow newStorage = GetOrgStorageMain(newOrganisation);

				if (newStorage != null)
				{
					MoveStorageDocsToNewOrg(oldStorage, newStorage);
				}
				else
				{
					MoveStorageMainToNewOrg();
				}
			}

			UpdateOtherDocRelatedData();
		}

		void MoveStorageDocsToNewOrg(DataRow oldStorage, DataRow newStorage)
		{
			int dbNumberOld = Utilities.ConvertToInt32(oldStorage[StorageMainSchema.SM_DB.Name]);
			int dbNumberNew = Utilities.ConvertToInt32(newStorage[StorageMainSchema.SM_DB.Name]);
			string dbNameOld = string.Format(storageDbFormatMask, Db.DatabaseName, dbNumberOld);
			string dbNameNew = string.Format(storageDbFormatMask, Db.DatabaseName, dbNumberNew);

			bool areOldOrgDocsUnallocated = (dbNumberOld == 0);
			bool doesNewOrgDocDbExist = CheckIfEdocsDbExists(dbNameNew);

			if (areOldOrgDocsUnallocated || doesNewOrgDocDbExist)
			{
				var oldStorageMainPk = (Guid)oldStorage[StorageMainSchema.PK.Name];

				if (areOldOrgDocsUnallocated)
				{
					var updateUnallocatedOldDocsSql = string.Format(CultureInfo.InvariantCulture, "update dbo.StorageDocs set SC_SM = '{0}' where SC_SM = @OldStorageMainPk", Guid.Empty.ToString());
					using (DbCommand cmd = GetCommandOnMainConnection(updateUnallocatedOldDocsSql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@OldStorageMainPk", SqlDbType.UniqueIdentifier, oldStorageMainPk);
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					var newStorageMainPk = (Guid)newStorage[StorageMainSchema.PK.Name];
					MoveOldOrgDocsToNewOrgDb(oldStorageMainPk, newStorageMainPk, dbNameOld, dbNameNew);
				}

				var deleteOldStorageMainSql = "delete from dbo.StorageMain where SM_PK = @OldStorageMainPk";
				using (DbCommand cmd = GetCommandOnMainConnection(deleteOldStorageMainSql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@OldStorageMainPk", SqlDbType.UniqueIdentifier, oldStorageMainPk);
					cmd.ExecuteNonQuery();
				}
			}
		}

		void MoveOldOrgDocsToNewOrgDb(Guid oldStoragePk, Guid newStoragePk, string oldDbName, string newDbName)
		{
			bool isOldDbWriteable = DocManagerUtils.IsDbWriteableForDocManager(oldDbName); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			bool isNewDbWriteable = DocManagerUtils.IsDbWriteableForDocManager(newDbName); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects

			if (!isNewDbWriteable)
			{
				string writeableDbName = ObjectFactory.Get<DocumentScanning.Integration.IDocManagerDBHelper>().GetLastWritableDatabaseName()
					?? throw new Exception("Cannot find a writeable SD database to store merged eDocs.");

				MoveOrCopyStorageDocsToAnotherDb(newDbName, writeableDbName, newStoragePk, newStoragePk, isNewDbWriteable);
				UpdateStorageMainDbNumber(newStoragePk, writeableDbName);
				newDbName = writeableDbName;
			}

			MoveOrCopyStorageDocsToAnotherDb(oldDbName, newDbName, oldStoragePk, newStoragePk, isOldDbWriteable);
		}

		void MoveOrCopyStorageDocsToAnotherDb(string sourceDb, string targetDb, Guid sourceStoragePk, Guid targetStoragePk, bool isSourceDbWriteable)
		{
			string to = string.Format("[{0}]..[{1}]", targetDb, StorageDocsSchema.Constants.TableName);
			string where = "WHERE SC_SM = @SourceStoragePK";

			if (sourceDb != targetDb)
			{
				string from = string.Format("[{0}]..[{1}]", sourceDb, StorageDocsSchema.Constants.TableName);
				string sqlText = string.Format("INSERT {0} ({1}) SELECT {1} FROM {2} {3};", to, StorageDocColumnList, from, where);

				try
				{
					using (DbCommand cmd = GetCommandOnMainConnection(sqlText))
					{
						cmd.AddParameter("@SourceStoragePK", SqlDbType.UniqueIdentifier, sourceStoragePk);
						cmd.ExecuteNonQuery();
					}
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotInsertDuplicateConstraintKey)
				{
					var exception = new InvalidOperationException(GetUserFriendlyDuplicateKeyErrorMessage(from, to, sourceStoragePk, ex));
					mergeOrgHeader.ConflictingStorageDocsPKException = exception;
					throw exception;
				}

				if (isSourceDbWriteable)
				{
					sqlText = string.Format("DELETE {0} {1};", from, where);
					using (DbCommand cmd = GetCommandOnMainConnection(sqlText))
					{
						cmd.AddParameter("@SourceStoragePK", SqlDbType.UniqueIdentifier, sourceStoragePk);
						cmd.ExecuteNonQuery();
					}
				}
			}

			if (sourceStoragePk != targetStoragePk)
			{
				string sqlText = string.Format("UPDATE {0} SET SC_SM = @TargetStoragePk {1};", to, where);
				using (DbCommand cmd = GetCommandOnMainConnection(sqlText))
				{
					cmd.AddParameter("@SourceStoragePK", SqlDbType.UniqueIdentifier, sourceStoragePk);
					cmd.AddParameter("@TargetStoragePk", SqlDbType.UniqueIdentifier, targetStoragePk);
					cmd.ExecuteNonQuery();
				}
			}
		}

		void MoveStorageMainToNewOrg()
		{
			string sqlText = "UPDATE dbo.StorageMain SET SM_ParentFK = @NewOrgPK WHERE SM_ParentFK = @OldOrgPK";

			using (DbCommand cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.AddParameter("@NewOrgPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrgPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		string GetUserFriendlyDuplicateKeyErrorMessage(string from, string to, Guid sourceStoragePK, SqlException originalException)
		{
			try
			{
				var fromAlias = (NoResString)"dis";
				var toAlias = (NoResString)"ret";

				var fromDescAlias = fromAlias + "Desc";
				var toDescAlias = toAlias + "Desc";

				var fromFileNameAlias = fromAlias + "FileName";
				var toFileNameAlias = toAlias + "FileName";

				var fromDocTypeAlias = fromAlias + "DocType";
				var toDocTypeAlias = toAlias + "DocType";

				var fromDateAlias = fromAlias + (NoResString)"Date";
				var toDateAlias = toAlias + (NoResString)"Date";

				var selectColumns = new StringCollectionX
				{
					fromAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_Desc + (NoResString)" as " + fromDescAlias,
					fromAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_FileName + (NoResString)" as " + fromFileNameAlias,
					fromAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_DocType + (NoResString)" as " + fromDocTypeAlias,
					fromAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_Date + (NoResString)" as " + fromDateAlias,
					toAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_Desc + (NoResString)" as " + toDescAlias,
					toAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_FileName + (NoResString)" as " + toFileNameAlias,
					toAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_DocType + (NoResString)" as " + toDocTypeAlias,
					toAlias + (NoResString)"." + StorageDocsSchema.Constants.SC_Date + (NoResString)" as " + toDateAlias
				};

				var whereClause = string.Format(CultureInfo.InvariantCulture, (NoResString)"WHERE {0}.{1} = @SourceStoragePK", fromAlias, StorageDocsSchema.Constants.SC_SM);
				var andClause = string.Format(CultureInfo.InvariantCulture, (NoResString)"AND {0}.{1} = {2}.{3}", fromAlias, StorageDocsSchema.Constants.PK, toAlias, StorageDocsSchema.Constants.PK);

				var sqlTextBuilder = new ZStringBuilder("SELECT " + string.Join(",", selectColumns.ToArray()));
				sqlTextBuilder.Append(" " + string.Format(CultureInfo.InvariantCulture, (NoResString)"FROM {0} as {1}, {2} as {3}", from, fromAlias, to, toAlias));
				sqlTextBuilder.Append(" " + whereClause);
				sqlTextBuilder.Append(" " + andClause);

				using (DataTable storageDocTable = new DataTable(StorageDocsSchema.Constants.TableName))
				using (DbCommand cmd = GetCommandOnMainConnection(sqlTextBuilder.ToString()))
				{
					cmd.AddParameter("@SourceStoragePK", SqlDbType.UniqueIdentifier, sourceStoragePK);

					using (var fromAdapter = cmd.NewDataAdapter())
					{
						storageDocTable.Locale = CultureInfo.InvariantCulture;
						fromAdapter.Fill(storageDocTable);
						var fromDuplicateMessages = new StringCollectionX();
						var toDuplicateMessages = new StringCollectionX();
						foreach (DataRow row in storageDocTable.Rows)
						{
							fromDuplicateMessages.Add(Res.GetString("38e7f1d8-45a4-42d9-858c-5b7d5f3f258f", "Description: {0}, File Name: {1}, Document Type: {2}, Date: {3}",
								row[fromDescAlias].ToString(),
								row[fromFileNameAlias].ToString(),
								row[fromDocTypeAlias].ToString(),
								(new ZDateTime(row[fromDateAlias])).ToShortDateString()));
							toDuplicateMessages.Add(Res.GetString("38e7f1d8-45a4-42d9-858c-5b7d5f3f258f", "Description: {0}, File Name: {1}, Document Type: {2}, Date: {3}",
								row[toDescAlias].ToString(),
								row[toFileNameAlias].ToString(),
								row[toDocTypeAlias].ToString(),
								(new ZDateTime(row[toDateAlias])).ToShortDateString()));
						}

						return Res.GetString("dfe5ad1f-dedc-450b-bffa-b858bed77dde", @"Another record already exists in database with the same primary key. If both records are identical then this can be fixed by permanently deleting the conflicting eDoc on the dissolved organization.

The value primary key must be unique on eDoc.
The duplicate values(s) are:")
							+ "\r\n\r\n" + Res.GetString("7e837aae-e054-4733-9a32-3dc452c03b98", "Dissolved Sub-Record") + "\r\n" + mergeOrgHeader.OldOrganisation.HumanReadableName
							+ "\r\n" + string.Join("\r\n", fromDuplicateMessages.ToArray())
							+ "\r\n\r\n" + Res.GetString("6a8befa0-0b46-4464-a89c-2e270ef3d87c", "Retained Sub-Record") + "\r\n" + mergeOrgHeader.NewOrganisation.HumanReadableName
							+ "\r\n" + string.Join("\r\n", toDuplicateMessages.ToArray());
					}
				}
			}
			catch (Exception)
			{
				throw originalException;
			}
		}

		void UpdateStorageMainDbNumber(Guid storagePk, string storageDbName)
		{
			int dbNumber = Convert.ToInt32(storageDbName.Substring(storageDbName.Length - 3, 3));
			var sqlText = "UPDATE dbo.StorageMain SET SM_DB = @DbNumber WHERE SM_PK = @StoragePK;";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.AddParameter("@DbNumber", SqlDbType.Int, dbNumber);
				cmd.AddParameter("@StoragePK", SqlDbType.UniqueIdentifier, storagePk);
				cmd.ExecuteNonQuery();
			}
		}

		void UpdateOtherDocRelatedData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobRequiredDocument
SET
	EQ_ParentID = @NewOrganisationPK,
	EQ_SystemLastEditTimeUtc = GETUTCDATE(),
	EQ_SystemLastEditUser = '{0}'
WHERE
	EQ_ParentID = @OldOrganisationPK and EQ_ParentTableCode = '{1}'",
				GlbStaff.CurrentUser.GS_Code.ToString(),
				OrgHeaderSchema.Constants.Prefix);
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}

			sqlText = "update dbo.StorageDocs set SC_ParentID = @NewOrganisationPK where SC_ParentID = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		DataRow GetOrgStorageMain(ZGuid orgPK)
		{
			using (DataTable storageMain = new DataTable(StorageMainSchema.Constants.TableName))
			using (DbCommand cmd = GetCommandOnMainConnection("select * from dbo.StorageMain where SM_ParentFK = @OrgPK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(storageMain);
					return storageMain.Rows.Count > 0 ? storageMain.Rows[0] : null;
				}
			}
		}

		string StorageDocColumnList
		{
			get
			{
				if (storageDocColumnList == null)
				{
					ZStringBuilder builder = new ZStringBuilder();

					foreach (SchemaColumn column in StorageDocsSchema.All)
					{
						builder.Append(column.Name);
					}

					storageDocColumnList = builder.ToStringWithDelimiterBetweenAppends(",");
				}

				return storageDocColumnList;
			}
		}

		string storageDocColumnList;

		protected bool CheckIfEdocsDbExists(string dbName)
		{
			using (var command = GetCommandOnMainConnection((NoResString)"IF EXISTS (SELECT null FROM sys.databases WHERE name = @Dbname) SELECT 1 ELSE SELECT 0;"))
			{
				command.AddParameter("@Dbname", SqlDbType.NVarChar, 128, dbName);
				var queryResult = command.ExecuteScalar();
				return Convert.ToInt32(queryResult) == 1;
			}
		}

		readonly string storageDbFormatMask = $"{{0}}{Db.SDDatabaseAffix}{{1:000}}";

		#endregion

		#region Merge Org Staff Assignments

		void MergeOrgStaffAssignments()
		{
			var oldOrg = GetNewFactory().Load<OrgHeader>(oldOrganisation);
			var newOrg = GetNewFactory().Load<OrgHeader>(newOrganisation);

			if (oldOrg != null && newOrg != null)
			{
				var oldAssignments = new OrgStaffAssignmentsCollection(oldOrg) { CompanySpecific = false };
				var newAssignments = new OrgStaffAssignmentsCollection(newOrg) { CompanySpecific = false };

				var pks = oldAssignments.Cast<OrgStaffAssignments>()
					.Where(assignment => CanMergeIntoNewStaffAssignment(assignment, newAssignments))
					.Select(assignment => assignment.PK.ToString());

				var pkCsv = "'" + string.Join("','", pks) + "'";
				if (pkCsv != "''")
				{
					var sqlText = "UPDATE dbo.OrgStaffAssignments SET O8_OH =@NewOrgPk, O8_SystemLastEditTimeUtc = GETUTCDATE(), O8_SystemLastEditUser = @SystemLastEditUser WHERE O8_OH = @OldOrgPk and O8_PK in (" + pkCsv + ")";
					using (var cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
						cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
						cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), OrgStaffAssignmentsSchema.O8_SystemLastEditUser);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		static bool CanMergeIntoNewStaffAssignment(OrgStaffAssignments proposedAssignment, OrgStaffAssignmentsCollection otherAssignments)
		{
			var willConflictWithOtherAssignment = otherAssignments.Cast<OrgStaffAssignments>().Any(assignment =>
				proposedAssignment.O8_GC == assignment.O8_GC &&
				proposedAssignment.O8_Role == assignment.O8_Role &&
				proposedAssignment.O8_Department == assignment.O8_Department &&
				proposedAssignment.O8_GS_NKPersonResponsible == assignment.O8_GS_NKPersonResponsible &&
				proposedAssignment.O8_Product == assignment.O8_Product);

			if (willConflictWithOtherAssignment)
			{
				return false;
			}

			var newAssignment = (OrgStaffAssignments)proposedAssignment.Clone();
			otherAssignments.Add(newAssignment);
			newAssignment.Header.StaffAssignments.CompanySpecific = false;
			newAssignment.RunPreSaveValidation();

			return !newAssignment.HasErrors;
		}

		#endregion

		#region Merge Rating Headers and Rate Entries

		void MergeRatingHeader()
		{
			DataTable rowsToChangeFK = new DataTable(RatingHeaderSchema.Constants.TableName);
			using (DbCommand cmd = GetCommandOnMainConnection("select * from dbo.RatingHeader where TH_OH =@OldOrgPk")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(rowsToChangeFK);
				}
			}

			foreach (DataRow row in rowsToChangeFK.Rows)
			{
				var newOrgRatingHeaderToMerge = new List<Guid>();

				using (DbCommand cmd = GetCommandOnMainConnection("select TH_PK from dbo.RatingHeader where TH_OH = @NewOrgPk and TH_GC = @TH_GC and TH_RateType = @TH_RateType and TH_QuoteNumber = @TH_QuoteNumber and TH_GlobalRateLevel = @TH_GlobalRateLevel")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
					cmd.AddParameterBasedOnDbColumn("@TH_GC", row[RatingHeaderSchema.TH_GC.Name], RatingHeaderSchema.TH_GC);
					cmd.AddParameterBasedOnDbColumn("@TH_RateType", row[RatingHeaderSchema.TH_RateType.Name], RatingHeaderSchema.TH_RateType);
					cmd.AddParameterBasedOnDbColumn("@TH_QuoteNumber", row[RatingHeaderSchema.TH_QuoteNumber.Name], RatingHeaderSchema.TH_QuoteNumber);
					cmd.AddParameterBasedOnDbColumn("@TH_GlobalRateLevel", row[RatingHeaderSchema.TH_GlobalRateLevel.Name], RatingHeaderSchema.TH_GlobalRateLevel);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							newOrgRatingHeaderToMerge.Add(reader.GetGuid(0));
						}
					}
				}

				if (newOrgRatingHeaderToMerge.Count == 1)// Should only have one result: FK_UX__TH_OH_TH_GC_TH_RateType_TH_QuoteNumber_TH_GlobalRateLevel
				{
					var oldOrgRatingHeaderPK = (Guid)row[RatingHeaderSchema.PK.Name];
					var newOrgRatingHeaderPK = newOrgRatingHeaderToMerge[0];

					LogAndDeleteOverlappingRateEntriesByRatingHeader(oldOrgRatingHeaderPK, newOrgRatingHeaderPK);
					UpdateRatingHeaderRelatedFields(oldOrgRatingHeaderPK, newOrgRatingHeaderPK);
					DeleteOldRatingHeader(oldOrgRatingHeaderPK);
				}
			}

			LogAndDeleteOverlappingRateEntriesByOrgReferences();
		}

		void UpdateRatingHeaderRelatedFields(Guid oldOrgRatingHeaderPK, Guid newOrgRatingHeaderPK)
		{
			string sql = @" update dbo.RateOneOffShipment set TT_TH = @NewFK where TT_TH = @OldFK
									update dbo.RateAttachment set TA_TH = @NewFK where TA_TH = @OldFK
									update dbo.RateEntry set TI_TH = @NewFK where TI_TH = @OldFK";

			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewFK", SqlDbType.UniqueIdentifier, newOrgRatingHeaderPK);
				cmd.AddParameter("@OldFK", SqlDbType.UniqueIdentifier, oldOrgRatingHeaderPK);
				cmd.ExecuteNonQuery();
			}
		}

		void DeleteOldRatingHeader(Guid oldOrgRatingHeaderPK)
		{
			using (DbCommand cmd = GetCommandOnMainConnection("delete from dbo.RatingHeader where TH_PK=@TH_PK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@TH_PK", SqlDbType.UniqueIdentifier, oldOrgRatingHeaderPK);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Rating Details

		void MergeRatingDetails()
		{
			var sql = @"
UPDATE dbo.OrgRateTariffLevel SET P7_OH = @NewOrg WHERE P7_OH = @OldOrg
	AND NOT EXISTS
	(
		SELECT 0
		FROM dbo.OrgRateTariffLevel B
		WHERE B.P7_OH = @NewOrg
			AND B.P7_TariffType = OrgRateTariffLevel.P7_TariffType AND B.P7_Mode = OrgRateTariffLevel.P7_Mode AND B.P7_Direction = OrgRateTariffLevel.P7_Direction AND B.P7_GC = OrgRateTariffLevel.P7_GC
	)
";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Find, Log and Delete Overlapping Rate Entries

		#region SuppressResourceStringsCheckRegion

		void LogAndDeleteOverlappingRateEntriesByRatingHeader(Guid oldOrgRatingHeaderPK, Guid newOrgRatingHeaderPK)
		{
			var selectSql = "SELECT * INTO #OldOrgRateEntries FROM dbo.RateEntry WHERE TI_TH = @oldPK;";
			var updateSql = "UPDATE #OldOrgRateEntries SET TI_TH = @newPK;";

			LogAndDeleteOverlappingRateEntriesBeforeAndAfter(selectSql, updateSql, oldOrgRatingHeaderPK, newOrgRatingHeaderPK, true, null);
		}

		void LogAndDeleteOverlappingRateEntriesByOrgReferences()
		{
			var selectSql = @"
SELECT * INTO #OldOrgRateEntries FROM dbo.RateEntry
WHERE
	TI_OH_TransportProvider			= @oldPK
	OR TI_OH_Supplier				= @oldPK
	OR TI_OH_Consignor				= @oldPK
	OR TI_OH_Consignee				= @oldPK
	OR TI_OH_ControllingCustomer	= @oldPK
";

			var updateSql = @"
UPDATE #OldOrgRateEntries SET TI_OH_TransportProvider		= @newPK WHERE TI_OH_TransportProvider		= @oldPK;
UPDATE #OldOrgRateEntries SET TI_OH_Supplier				= @newPK WHERE TI_OH_Supplier				= @oldPK;
UPDATE #OldOrgRateEntries SET TI_OH_Consignor				= @newPK WHERE TI_OH_Consignor				= @oldPK;
UPDATE #OldOrgRateEntries SET TI_OH_Consignee				= @newPK WHERE TI_OH_Consignee				= @oldPK;
UPDATE #OldOrgRateEntries SET TI_OH_ControllingCustomer		= @newPK WHERE TI_OH_ControllingCustomer	= @oldPK;
";

			if (mergedAddresses.Count > 0)
			{
				var selectBuilder = new StringBuilder(selectSql);
				var updateBuilder = new StringBuilder(updateSql);
				int paramIndex = 0;
				foreach (var addressPair in mergedAddresses)
				{
					(var oldParam, var newParam) = GetNextAddressParamNames(ref paramIndex);

					selectBuilder.AppendLine("	OR TI_OA_CartagePickupAddressOverride	= " + oldParam);
					selectBuilder.AppendLine("	OR TI_OA_CartageDeliveryAddressOverride	= " + oldParam);

					updateBuilder.AppendLine("UPDATE #OldOrgRateEntries SET TI_OA_CartagePickupAddressOverride = " + newParam + " WHERE TI_OA_CartagePickupAddressOverride = " + oldParam);
					updateBuilder.AppendLine("UPDATE #OldOrgRateEntries SET TI_OA_CartageDeliveryAddressOverride = " + newParam + " WHERE TI_OA_CartageDeliveryAddressOverride = " + oldParam);
				}

				selectSql = selectBuilder.ToString();
				updateSql = updateBuilder.ToString();
			}

			LogAndDeleteOverlappingRateEntriesBeforeAndAfter(selectSql, updateSql, oldOrganisation.ToGuid(), newOrganisation.ToGuid(), false, mergedAddresses);
			LogAndDeleteOverlappingRateEntriesInTemporaryTable(selectSql, updateSql, oldOrganisation.ToGuid(), newOrganisation.ToGuid(), false, mergedAddresses);
		}

		void LogAndDeleteOverlappingRateEntriesInTemporaryTable(string selectSql, string updateSql, Guid oldPK, Guid newPK, bool shouldUpdateLogParent, Dictionary<ZGuid, ZGuid> addresses)
		{
			var selectAndUpdateSql = selectSql + ";" + System.Environment.NewLine + updateSql;
			var detectDuplicateSql = selectAndUpdateSql + $@"
ALTER TABLE #OldOrgRateEntries DROP COLUMN TI_RateKey;
ALTER TABLE #OldOrgRateEntries ADD TI_RateKey as checksum([{RateEntrySchema.Constants.TI_RateCategory}],[{RateEntrySchema.Constants.TI_Mode}],[{RateEntrySchema.Constants.TI_OriginLRC}],
	[{RateEntrySchema.Constants.TI_DestinationLRC}],[{RateEntrySchema.Constants.TI_ViaLRC}],[{RateEntrySchema.Constants.TI_PlannedLoadLRC}],[{RateEntrySchema.Constants.TI_PlannedDischargeLRC}],
	[{RateEntrySchema.Constants.TI_FirstLoadLRC}],[{RateEntrySchema.Constants.TI_LastDischargeLRC}],[{RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC}],
	[{RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC}],[{RateEntrySchema.Constants.TI_RS_NKServiceLevel_NI}],[{RateEntrySchema.Constants.TI_PL_NKCarrierServiceLevel}],
	[{RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel}],[{RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel}],[{RateEntrySchema.Constants.TI_RH_NKCommodityCode}],[{RateEntrySchema.Constants.TI_FMCTariffID}],
	[{RateEntrySchema.Constants.TI_CartagePickupAddressPostCode}],[{RateEntrySchema.Constants.TI_CartageDeliveryAddressPostCode}],[{RateEntrySchema.Constants.TI_TransitTime}],
	[{RateEntrySchema.Constants.TI_Frequency}],[{RateEntrySchema.Constants.TI_FrequencyUnit}],[{RateEntrySchema.Constants.TI_IsCrossTrade}],[{RateEntrySchema.Constants.TI_TH}],
	[{RateEntrySchema.Constants.TI_OH_TransportProvider}],[{RateEntrySchema.Constants.TI_OH_Supplier}],[{RateEntrySchema.Constants.TI_OH_Consignor}],[{RateEntrySchema.Constants.TI_OH_Consignee}],
	[{RateEntrySchema.Constants.TI_OH_ControllingCustomer}],[{RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride}],
	[{RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride}],[{RateEntrySchema.Constants.TI_RateOrigin}],[{RateEntrySchema.Constants.TI_RateDestination}],
	[{RateEntrySchema.Constants.TI_TZ_OriginZone}],[{RateEntrySchema.Constants.TI_TZ_DestinationZone}],[{RateEntrySchema.Constants.TI_R9_FromSuburb}],[{RateEntrySchema.Constants.TI_R9_ToSuburb}],
	[{RateEntrySchema.Constants.TI_IsTact}],[{RateEntrySchema.Constants.TI_ParentID}],[{RateEntrySchema.Constants.TI_PaymentTerm}],[{RateEntrySchema.Constants.TI_GatewayAgentType}],
	[{RateEntrySchema.Constants.TI_ContractNumber}],[{RateEntrySchema.Constants.TI_AircraftType}],[{RateEntrySchema.Constants.TI_IsNonOperatedReefer}]) Persisted;

SELECT DISTINCT
	oldEntry.{RateEntrySchema.Constants.PK}, oldEntry.TI_RateKey, RateEntry.{RateEntrySchema.Constants.TI_OH_TransportProvider}, RateEntry.{RateEntrySchema.Constants.TI_OH_Supplier},
	RateEntry.{RateEntrySchema.Constants.TI_OH_Consignor}, RateEntry.{RateEntrySchema.Constants.TI_OH_Consignee}, RateEntry.{RateEntrySchema.Constants.TI_OH_ControllingCustomer}
FROM
	#OldOrgRateEntries AS oldEntry
	CROSS APPLY (SELECT {RateEntrySchema.Constants.PK}
		FROM #OldOrgRateEntries
		OUTER APPLY [dbo].GetRateEntryContainerClass({RateEntrySchema.Constants.TI_RC}, {RateEntrySchema.Constants.TI_RateCategory}) AS ExistingContainerClass
		OUTER APPLY [dbo].GetRateEntryContainerClass(oldEntry.{RateEntrySchema.Constants.TI_RC}, oldEntry.{RateEntrySchema.Constants.TI_RateCategory}) AS NewlyAddedContainerClass
		WHERE
			TI_RateKey = oldEntry.TI_RateKey
			AND {RateEntrySchema.Constants.PK} <> oldEntry.{RateEntrySchema.Constants.PK}
			AND
			{RateEntrySchema.Constants.TI_RateCategory}						=	oldEntry.{RateEntrySchema.Constants.TI_RateCategory}						AND
			{RateEntrySchema.Constants.TI_Mode}								=	oldEntry.{RateEntrySchema.Constants.TI_Mode}								AND
			{RateEntrySchema.Constants.TI_OriginLRC}						=	oldEntry.{RateEntrySchema.Constants.TI_OriginLRC}							AND
			{RateEntrySchema.Constants.TI_RateOrigin}						=	oldEntry.{RateEntrySchema.Constants.TI_RateOrigin}							AND
			{RateEntrySchema.Constants.TI_DestinationLRC}					=	oldEntry.{RateEntrySchema.Constants.TI_DestinationLRC}						AND
			{RateEntrySchema.Constants.TI_RateDestination}					=	oldEntry.{RateEntrySchema.Constants.TI_RateDestination}						AND
			{RateEntrySchema.Constants.TI_ViaLRC}							=	oldEntry.{RateEntrySchema.Constants.TI_ViaLRC}								AND
			{RateEntrySchema.Constants.TI_PlannedLoadLRC}					=	oldEntry.{RateEntrySchema.Constants.TI_PlannedLoadLRC}						AND
			{RateEntrySchema.Constants.TI_PlannedDischargeLRC}				=	oldEntry.{RateEntrySchema.Constants.TI_PlannedDischargeLRC}					AND
			{RateEntrySchema.Constants.TI_FirstLoadLRC}						=	oldEntry.{RateEntrySchema.Constants.TI_FirstLoadLRC}						AND
			{RateEntrySchema.Constants.TI_LastDischargeLRC}					=	oldEntry.{RateEntrySchema.Constants.TI_LastDischargeLRC}					AND
			{RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC}			=	oldEntry.{RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC}			AND
			{RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC}		=	oldEntry.{RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC}		AND
			{RateEntrySchema.Constants.TI_RS_NKServiceLevel_NI}				=	oldEntry.{RateEntrySchema.Constants.TI_RS_NKServiceLevel_NI}				AND
			{RateEntrySchema.Constants.TI_PL_NKCarrierServiceLevel}			=	oldEntry.{RateEntrySchema.Constants.TI_PL_NKCarrierServiceLevel}			AND
			{RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel}			=	oldEntry.{RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel}			AND
			{RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel}	=	oldEntry.{RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel}	AND
			{RateEntrySchema.Constants.TI_RH_NKCommodityCode}				=	oldEntry.{RateEntrySchema.Constants.TI_RH_NKCommodityCode}					AND
			{RateEntrySchema.Constants.TI_FMCTariffID}						=	oldEntry.{RateEntrySchema.Constants.TI_FMCTariffID}							AND
			{RateEntrySchema.Constants.TI_CartagePickupAddressPostCode}		=	oldEntry.{RateEntrySchema.Constants.TI_CartagePickupAddressPostCode}		AND
			{RateEntrySchema.Constants.TI_CartageDeliveryAddressPostCode}	=	oldEntry.{RateEntrySchema.Constants.TI_CartageDeliveryAddressPostCode}		AND
			{RateEntrySchema.Constants.TI_TransitTime}						=	oldEntry.{RateEntrySchema.Constants.TI_TransitTime}							AND
			{RateEntrySchema.Constants.TI_Frequency}						=	oldEntry.{RateEntrySchema.Constants.TI_Frequency}							AND
			{RateEntrySchema.Constants.TI_FrequencyUnit}					=	oldEntry.{RateEntrySchema.Constants.TI_FrequencyUnit}						AND
			{RateEntrySchema.Constants.TI_IsCrossTrade}						=	oldEntry.{RateEntrySchema.Constants.TI_IsCrossTrade}						AND
			{RateEntrySchema.Constants.TI_IsTact}							=	oldEntry.{RateEntrySchema.Constants.TI_IsTact}								AND
			{RateEntrySchema.Constants.TI_TH}								=	oldEntry.{RateEntrySchema.Constants.TI_TH}									AND
			{RateEntrySchema.Constants.TI_MatchContainerRateClass}			=	oldEntry.{RateEntrySchema.Constants.TI_MatchContainerRateClass}				AND
			{RateEntrySchema.Constants.TI_PaymentTerm}						=	oldEntry.{RateEntrySchema.Constants.TI_PaymentTerm}							AND
			{RateEntrySchema.Constants.TI_GatewayAgentType}					=	oldEntry.{RateEntrySchema.Constants.TI_GatewayAgentType}					AND
			{RateEntrySchema.Constants.TI_ContractNumber}					=	oldEntry.{RateEntrySchema.Constants.TI_ContractNumber}						AND
			{RateEntrySchema.Constants.TI_AircraftType}						=	oldEntry.{RateEntrySchema.Constants.TI_AircraftType}						AND
			{RateEntrySchema.Constants.TI_IsNonOperatedReefer}				=	oldEntry.{RateEntrySchema.Constants.TI_IsNonOperatedReefer}					AND

--nullables
			({RateEntrySchema.Constants.TI_OH_TransportProvider}				=	oldEntry.{RateEntrySchema.Constants.TI_OH_TransportProvider}				OR {RateEntrySchema.Constants.TI_OH_TransportProvider} is NULL				AND oldEntry.{RateEntrySchema.Constants.TI_OH_TransportProvider} IS NULL)				AND
			({RateEntrySchema.Constants.TI_OH_Supplier}							=	oldEntry.{RateEntrySchema.Constants.TI_OH_Supplier}							OR {RateEntrySchema.Constants.TI_OH_Supplier} IS NULL						AND oldEntry.{RateEntrySchema.Constants.TI_OH_Supplier} IS NULL)						AND
			({RateEntrySchema.Constants.TI_OH_Consignor}						=	oldEntry.{RateEntrySchema.Constants.TI_OH_Consignor}						OR {RateEntrySchema.Constants.TI_OH_Consignor} IS NULL						AND oldEntry.{RateEntrySchema.Constants.TI_OH_Consignor} IS NULL)						AND
			({RateEntrySchema.Constants.TI_OH_Consignee}						=	oldEntry.{RateEntrySchema.Constants.TI_OH_Consignee}						OR {RateEntrySchema.Constants.TI_OH_Consignee} IS NULL						AND oldEntry.{RateEntrySchema.Constants.TI_OH_Consignee} IS NULL)						AND
			({RateEntrySchema.Constants.TI_OH_ControllingCustomer}				=	oldEntry.{RateEntrySchema.Constants.TI_OH_ControllingCustomer}				OR {RateEntrySchema.Constants.TI_OH_ControllingCustomer} IS NULL			AND oldEntry.{RateEntrySchema.Constants.TI_OH_ControllingCustomer} IS NULL)				AND
			({RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride}		=	oldEntry.{RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride}		OR {RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride} IS NULL	AND oldEntry.{RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride} IS NULL)	AND
			({RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride}	=	oldEntry.{RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride}	OR {RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride} IS NULL	AND oldEntry.{RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride} IS NULL)	AND
			({RateEntrySchema.Constants.TI_TZ_OriginZone}						=	oldEntry.{RateEntrySchema.Constants.TI_TZ_OriginZone}						OR {RateEntrySchema.Constants.TI_TZ_OriginZone} IS NULL						AND oldEntry.{RateEntrySchema.Constants.TI_TZ_OriginZone} IS NULL)						AND
			({RateEntrySchema.Constants.TI_TZ_DestinationZone}					=	oldEntry.{RateEntrySchema.Constants.TI_TZ_DestinationZone}					OR {RateEntrySchema.Constants.TI_TZ_DestinationZone} IS NULL				AND oldEntry.{RateEntrySchema.Constants.TI_TZ_DestinationZone} IS NULL)					AND
			({RateEntrySchema.Constants.TI_R9_FromSuburb}						=	oldEntry.{RateEntrySchema.Constants.TI_R9_FromSuburb}						OR {RateEntrySchema.Constants.TI_R9_FromSuburb} IS NULL						AND oldEntry.{RateEntrySchema.Constants.TI_R9_FromSuburb} IS NULL)						AND
			({RateEntrySchema.Constants.TI_R9_ToSuburb}							=	oldEntry.{RateEntrySchema.Constants.TI_R9_ToSuburb}							OR {RateEntrySchema.Constants.TI_R9_ToSuburb} IS NULL						AND oldEntry.{RateEntrySchema.Constants.TI_R9_ToSuburb} IS NULL)						AND
			({RateEntrySchema.Constants.TI_RC}									=	oldEntry.{RateEntrySchema.Constants.TI_RC}									OR {RateEntrySchema.Constants.TI_RC} IS NULL								AND oldEntry.{RateEntrySchema.Constants.TI_RC} IS NULL									OR {RateEntrySchema.Constants.TI_RC} <> oldEntry.{RateEntrySchema.Constants.TI_RC} AND {RateEntrySchema.Constants.TI_MatchContainerRateClass} = 1 AND ExistingContainerClass.ContainerClass = NewlyAddedContainerClass.ContainerClass) AND
			({RateEntrySchema.Constants.TI_ParentID}							=	oldEntry.{RateEntrySchema.Constants.TI_ParentID}							OR {RateEntrySchema.Constants.TI_ParentID} IS NULL							AND oldEntry.{RateEntrySchema.Constants.TI_ParentID} IS NULL)							AND

			(oldEntry.{RateEntrySchema.Constants.TI_RateEndDate} IS NULL OR {RateEntrySchema.Constants.TI_RateStartDate} <= oldEntry.{RateEntrySchema.Constants.TI_RateEndDate})
			AND
			({RateEntrySchema.Constants.TI_RateEndDate} IS NULL OR oldEntry.{RateEntrySchema.Constants.TI_RateStartDate} <= {RateEntrySchema.Constants.TI_RateEndDate})
		) duplicateEntry
		INNER JOIN dbo.RateEntry ON RateEntry.{RateEntrySchema.Constants.PK} = oldEntry.{RateEntrySchema.Constants.PK};

		IF OBJECT_ID('tempdb..#OldOrgRateEntries') IS NOT NULL DROP TABLE #OldOrgRateEntries;
";

			var overlappingRatesByRateKey = new Dictionary<int, List<Tuple<int, Guid>>>();
			using (var cmd = GetCommandOnMainConnection(detectDuplicateSql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@oldPK", SqlDbType.UniqueIdentifier, oldPK);
				cmd.AddParameter("@newPK", SqlDbType.UniqueIdentifier, newPK);

				AddAddressSqlParameters(cmd, addresses);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader[0];
						var rateKey = (int)reader[1];

						int weight = 0;
						void AddWeightForColumnValueMatchingNewOrg(string column)
						{
							Guid.TryParse(reader[column].ToString(), out var guid);
							if (guid == newPK)
							{
								weight++;
							}
						}

						AddWeightForColumnValueMatchingNewOrg(RateEntrySchema.Constants.TI_OH_TransportProvider);
						AddWeightForColumnValueMatchingNewOrg(RateEntrySchema.Constants.TI_OH_Supplier);
						AddWeightForColumnValueMatchingNewOrg(RateEntrySchema.Constants.TI_OH_Consignor);
						AddWeightForColumnValueMatchingNewOrg(RateEntrySchema.Constants.TI_OH_Consignee);
						AddWeightForColumnValueMatchingNewOrg(RateEntrySchema.Constants.TI_OH_ControllingCustomer);

						overlappingRatesByRateKey.GetOrAdd(rateKey, () => new List<Tuple<int, Guid>>()).Add(new Tuple<int, Guid>(weight, pk));
					}
				}
			}

			var entriesToDelete = new List<Guid>();
			foreach (var group in overlappingRatesByRateKey.Values)
			{
				entriesToDelete.AddRange(group.OrderByDescending(x => x.Item1).Skip(1).Select(x => x.Item2));
			}

			if (entriesToDelete.Any())
			{
				LogOverlappingRateEntries(entriesToDelete, oldPK, newPK, shouldUpdateLogParent);
				DeleteOverlappingRateEntries(entriesToDelete);
			}
		}

		static (string oldParam, string newParam) GetNextAddressParamNames(ref int paramIndex)
		{
			const string oldAddressParamPrefix = "@oldAddrPK";
			const string newAddressParamPrefix = "@newAddrPK";

			var suffix = (++paramIndex).ToString(CultureInfo.InvariantCulture);
			return (oldAddressParamPrefix + suffix, newAddressParamPrefix + suffix);
		}

		void LogAndDeleteOverlappingRateEntriesBeforeAndAfter(string selectSql, string updateSql, Guid oldPK, Guid newPK, bool shouldUpdateLogParent, Dictionary<ZGuid, ZGuid> addresses)
		{
			//1. Delete any selected Rate Entries that already overlap before any changes
			LogAndDeleteOverlappingRateEntries(selectSql, true, oldPK, newPK, shouldUpdateLogParent, addresses);

			//2. Delete any Rate Entries that overlap after the Organizations are merged
			var selectAndUpdate = selectSql + ";" + System.Environment.NewLine + updateSql;
			LogAndDeleteOverlappingRateEntries(selectAndUpdate, false, oldPK, newPK, shouldUpdateLogParent, addresses);
		}

		void LogAndDeleteOverlappingRateEntries(string selectSql, bool skipFirstRate, Guid oldPK, Guid newPK, bool shouldUpdateLogParent, Dictionary<ZGuid, ZGuid> addresses)
		{
			var selectOverlappingRateEntries = selectSql + $@"
SELECT
	oldOrgRateEntry.{RateEntrySchema.Constants.PK}, oldOrgRateEntry.TI_RateKey
FROM
	#OldOrgRateEntries AS oldOrgRateEntry
	CROSS APPLY [dbo].[GetOverlappingRateEntries]
	(
		oldOrgRateEntry.{RateEntrySchema.Constants.PK},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RateCategory},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_Mode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OriginLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_DestinationLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_ViaLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_PlannedLoadLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_PlannedDischargeLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_FirstLoadLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_LastDischargeLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RS_NKServiceLevel_NI},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_PL_NKCarrierServiceLevel},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RH_NKCommodityCode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_FMCTariffID},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_CartagePickupAddressPostCode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_CartageDeliveryAddressPostCode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_TransitTime},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_Frequency},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_FrequencyUnit},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_IsCrossTrade},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_IsTact},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_MatchContainerRateClass},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_TH},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OH_TransportProvider},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OH_Supplier},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OH_Consignor},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OH_Consignee},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OH_ControllingCustomer},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RateOrigin},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RateDestination},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_TZ_OriginZone},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_TZ_DestinationZone},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_R9_FromSuburb},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_R9_ToSuburb},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RC},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RCC_ComponentCode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_ContainerUnitSection},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RRC_RepairCode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RMC_Material},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_EstimateType},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_REG_EquipmentGrade},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_MNRGroup},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RateStartDate},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_RateEndDate},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_ParentID},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_PaymentTerm},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_GatewayAgentType},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_ContractNumber},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_AircraftType},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_ShipmentConsolidationStatus},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_HBLDeliveryMode},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_IsNonOperatedReefer},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_YardUnitType},
		oldOrgRateEntry.{RateEntrySchema.Constants.TI_YardUnitLoad}
	);

IF OBJECT_ID('tempdb..#OldOrgRateEntries') IS NOT NULL DROP TABLE #OldOrgRateEntries;";

			var entriesToDelete = new List<Guid>();
			using (var cmd = GetCommandOnMainConnection(selectOverlappingRateEntries)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@oldPK", SqlDbType.UniqueIdentifier, oldPK);
				cmd.AddParameter("@newPK", SqlDbType.UniqueIdentifier, newPK);

				AddAddressSqlParameters(cmd, addresses);

				var overlappingRatesByRateKey = GetOverlappingRatesByRateKey(cmd);

				entriesToDelete = skipFirstRate
					? overlappingRatesByRateKey.Select(x => x.Value.Skip(1)).SelectMany(x => x).ToList()
					: overlappingRatesByRateKey.Values.SelectMany(x => x).ToList();
			}

			if (entriesToDelete.Any())
			{
				LogOverlappingRateEntries(entriesToDelete, oldPK, newPK, shouldUpdateLogParent);
				DeleteOverlappingRateEntries(entriesToDelete);
			}
		}

		void AddAddressSqlParameters(DbCommand cmd, Dictionary<ZGuid, ZGuid> addresses)
		{
			if (addresses != null)
			{
				int paramIndex = 0;
				foreach (var addressPair in addresses)
				{
					(var oldParam, var newParam) = GetNextAddressParamNames(ref paramIndex);
					cmd.AddParameter(oldParam, SqlDbType.UniqueIdentifier, addressPair.Key.ToGuid());
					cmd.AddParameter(newParam, SqlDbType.UniqueIdentifier, addressPair.Value.ToGuid());
				}
			}
		}

		Dictionary<int, HashSet<Guid>> GetOverlappingRatesByRateKey(DbCommand cmd)
		{
			var overlappingRatesByRateKey = new Dictionary<int, HashSet<Guid>>();
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = (Guid)reader[0];
					var rateKey = (int)reader[1];
					overlappingRatesByRateKey.GetOrAdd(rateKey, () => new HashSet<Guid>()).Add(pk);
				}
			}
			return overlappingRatesByRateKey;
		}

		void LogOverlappingRateEntries(List<Guid> entriesToDelete, Guid oldPK, Guid newPK, bool shouldUpdateLogParent)
		{
			var findRatingHeaderCount = @"
SELECT
	TH_PK, COUNT(TH_PK) AS deletedEntriesCount
INTO
	#RateEntriesToLog
FROM
	dbo.RatingHeader INNER JOIN dbo.RateEntry ON TI_TH = TH_PK
WHERE
	TI_PK IN (SELECT value FROM @entriesToDelete)
GROUP BY
	TH_PK;

";

			var updateLogParentSql = shouldUpdateLogParent
				? "UPDATE #RateEntriesToLog SET TH_PK = @newPK WHERE TH_PK = @oldPK;"
				: "";

			var insertLogSql = @"
INSERT INTO dbo.StmALog
(
	SL_PK,
	SL_Parent,
	SL_Table,
	SL_SE_NKEvent,
	SL_Reference,
	SL_EventTime," /* SuppressCodeSmell Reason = Adding a Log with a Parent Table. */ + @"
	SL_GS_NKUser
)
SELECT
	NEWID(),
	TH_PK,
	'RatingHeader',
	'DEL',
	'Merging ' + @oldOrgCode + ' with ' + @newOrgCode + ' has resulted in ' + CONVERT(NVARCHAR, deletedEntriesCount) + ' Rate Entry(s) overlap. ' + @oldOrgCode + ' rates deleted.',
	GETDATE(),
	'E'
FROM
	#RateEntriesToLog;

IF OBJECT_ID('tempdb..#RateEntriesToLog') IS NOT NULL
	DROP TABLE #RateEntriesToLog;
";

			var findAndLogRateEntriesDeleted = findRatingHeaderCount + updateLogParentSql + insertLogSql;
			using (var cmd = GetCommandOnMainConnection(findAndLogRateEntriesDeleted)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddTableValuedParameter("@entriesToDelete", RateEntrySchema.PK, entriesToDelete);
				cmd.AddParameter("@oldOrgCode", SqlDbType.NVarChar, OldOrgHeader.OH_Code.ToString());
				cmd.AddParameter("@newOrgCode", SqlDbType.NVarChar, NewOrgHeader.OH_Code.ToString());
				cmd.AddParameter("@oldPK", SqlDbType.UniqueIdentifier, oldPK);
				cmd.AddParameter("@newPK", SqlDbType.UniqueIdentifier, newPK);
				cmd.ExecuteNonQuery();
			}
		}

		void DeleteOverlappingRateEntries(List<Guid> entriesToDelete)
		{
			var sqlText = "DELETE FROM dbo.RateEntry WHERE TI_PK IN (SELECT value FROM @entriesToDelete);";
			using (var cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddTableValuedParameter("@entriesToDelete", RateEntrySchema.PK, entriesToDelete);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#endregion

		#region One Off Quote Carrier

		void MergeRateOneOffCarrier()
		{
			// Can just delete records with the old org if there would be a duplicate since the record has no other info
			const string sql = @"
delete from dbo.RateOneOffCarrier
where TTC_OH_Carrier = @OldOrgPk
and TTC_TT in (select TTC_TT from dbo.RateOneOffCarrier where TTC_OH_Carrier = @NewOrgPk);

update dbo.RateOneOffCarrier
set TTC_OH_Carrier = @NewOrgPk
where TTC_OH_Carrier = @OldOrgPk
";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddParameter("OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge AR and AP

		void MergeARAP()
		{
			MergeARorAP(OrgCompanyDataSchema.OB_IsDebtor.Name, true);
			MergeARorAP(OrgCompanyDataSchema.OB_IsCreditor.Name, false);
			MoveCollectionNotes();
		}

		const string ARNeededColumns = "OB_IsDebtor, OB_OJ_ARDebtorGroup,OB_ARCategory,OB_ARConsolidatedAccountingCategory,OB_RX_NKARDDefltCurrency,OB_ARCreditRating,OB_ARCreditLimit,OB_AROnCreditHold,OB_ARAccountAndCreditReviewDue,OB_ARWHTApplicable,OB_ARDontShowTaxOnDocs,OB_ARQualityAssured,OB_ARQualityAssuredCheckedDate,OB_AB_ARPayToAccount,OB_ARTreatDisbursementsAsStandardValue,OB_ARReceiptInvoiceAfterPostingDefault,OB_ARCombinedStatementInvoice,OB_ARBuyersConsolInvoicingStyle,OB_ARExternalDebtorCode,OB_ARTransactionCreationRestriction,OB_OCT_ARTaxTemplate";
		const string APNeededColumns = "OB_IsCreditor, OB_OG_APCreditorGroup,OB_APCategory,OB_ARConsolidatedAccountingCategory,OB_RX_NKAPDefltCurrency,OB_AB_APDefaultBankAccount,OB_AC_APDefaultChargeCode,OB_APCreditLimit,OB_APPaymentTerms,OB_APPaymentTermDays,OB_APVATConfig,OB_APWHTApplicable,OB_APPayInvoiceAfterPostingDefault,OB_APQualityAssured,OB_APQualityAssuredCheckedDate,OB_APExternalCreditorCode,OB_APTransactionCreationRestriction,OB_OCT_APTaxTemplate";

		void MergeARorAP(string column, bool isAR)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"select ob_pk from dbo.orgcompanydata where ob_oh = @OldOrganisationPK
				and ob_gc not in (select ob_gc from dbo.orgcompanydata where ob_oh = @NewOrganisationPK) and {0} = 1", column);
			var oldOrganisationParameter = ZSqlParameter.New("@OldOrganisationPK", oldOrganisation.ToGuid(), OrgCompanyDataSchema.OB_OH);
			var newOrganisationParameter = ZSqlParameter.New("@NewOrganisationPK", newOrganisation.ToGuid(), OrgCompanyDataSchema.OB_OH);

			var ob_pksToMove = GetPKsFromQuery(sql, oldOrganisationParameter, newOrganisationParameter);

			sql = string.Format(CultureInfo.InvariantCulture, @"select ob_pk from dbo.orgcompanydata where ob_oh = @OldOrganisationPK
				and ob_gc in (select ob_gc from dbo.orgcompanydata where ob_oh = @NewOrganisationPK and {0} = 0)  and {0} = 1", column);
			var ob_pksToMerge = GetPKsFromQuery(sql, oldOrganisationParameter, newOrganisationParameter);

			if (!ob_pksToMove.IsNullOrEmpty())
			{
				MoveARAPRecords(ob_pksToMove, isAR);
			}

			if (!ob_pksToMerge.IsNullOrEmpty())
			{
				MergeARAPRecords(ob_pksToMerge, isAR);
			}
		}
#if DEBUG
		protected virtual
#endif
 void MoveARAPRecords(Guid[] pks, bool isAR)
		{
			var primarykeys = new Dictionary<Guid, Guid>();
			var fields = isAR ? ARNeededColumns : APNeededColumns;
			foreach (var copiedPk in pks)
			{
				var newPK = Guid.NewGuid();
				primarykeys.Add(copiedPk, newPK);
				var sql = string.Format(CultureInfo.InvariantCulture, @"insert into dbo.orgcompanydata (ob_pk, ob_oh, ob_gc, {0}, ob_systemcreatetimeutc, ob_systemcreateuser, ob_systemlastedittimeutc, ob_systemlastedituser) select @NewPK, @NewOrganisation, ob_gc, {0}, GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser from dbo.orgcompanydata where ob_pk = @CopiedPk
insert into dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Percentage, JCF_Amount, JCF_Number, JCF_Flag)
select NEWID(), JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, @NewOrganisation, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Percentage, JCF_Amount, JCF_Number, JCF_Flag
from dbo.AccJobConfig join dbo.OrgCompanyData on OB_GC = JCF_GC and OB_OH = JCF_ParentId
where OB_PK = @CopiedPk and JCF_ParentTableCode = 'OH' and JCF_Ledger = @LedgerCode
delete dbo.AccJobConfig where JCF_ParentTableCode = 'OH' and JCF_Ledger = @LedgerCode and JCF_ParentId in (select OB_OH from dbo.OrgCompanyData where OB_PK = @CopiedPk)

UPDATE dbo.AccOrgTaxConfiguration SET OTC_OB = @NewPK, OTC_SystemLastEditTimeUtc = GetUtcDate(), OTC_SystemLastEditUser = @CurrentUser FROM dbo.AccOrgTaxConfiguration JOIN dbo.AccTaxConfiguration ON OTC_ETC = ETC_PK WHERE OTC_OB = @CopiedPK AND ETC_Ledger = @LedgerCode",
					fields);

				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@NewPK", SqlDbType.UniqueIdentifier, newPK);
					cmd.AddParameter("@NewOrganisation", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
					cmd.AddParameter("@CopiedPk", SqlDbType.UniqueIdentifier, copiedPk);
					cmd.AddParameter("@LedgerCode", SqlDbType.Char, 2, isAR ? "AR" : "AP");
					cmd.AddParameterBasedOnDbColumn("@CurrentUser", GlbStaff.CurrentUser.GS_Code.ToString(), OrgCompanyDataSchema.OB_SystemCreateUser);
					cmd.ExecuteNonQuery();
				}
			}

			string mainQueryWithParam = string.Format(CultureInfo.InvariantCulture, isAR ? MoveARLinksQuery : MoveAPLinksQuery, "@PK");
			MoveARAPLinks(mainQueryWithParam, pks, primarykeys);
		}

#if DEBUG
		protected virtual
#endif
 void MergeARAPRecords(Guid[] pks, bool isAR)
		{
			string fields = isAR ? ARNeededColumns : APNeededColumns;
			string[] columns = fields.Split(',');
			var innerSelect = new ZStringBuilder();
			string sql;
			foreach (string col in columns)
			{
				string tmp = string.Empty;
				if (col == OrgCompanyDataSchema.OB_ARConsolidatedAccountingCategory.Name)
				{
					tmp = string.Format(CultureInfo.InvariantCulture, "{0} = (select case when ({0} is not null and {0} <> '') then {0} else (select {0} from dbo.orgcompanydata where ob_pk = @CopiedPK) end from dbo.orgcompanydata where ob_pk = ({1}))",
						col, SelectNewPKsFromOld);
				}
				else
				{
					tmp = string.Format(CultureInfo.InvariantCulture, "{0} = (select {0} from dbo.orgcompanydata where ob_pk = @CopiedPK)", col);
				}
				innerSelect.Append(tmp);
			}

			var innerSelectString = innerSelect.ToStringWithDelimiterBetweenAppends(",");

			foreach (var pk in pks)
			{
				sql = string.Format(CultureInfo.InvariantCulture, @"update dbo.orgcompanydata set {0} where ob_pk = ({1})
declare @T table (PK uniqueidentifier)
insert into @T
select T.JCF_PK from
(merge dbo.AccJobConfig as target 
using (select JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Percentage, JCF_Amount, JCF_Number, JCF_Flag
	from dbo.AccJobConfig join dbo.OrgCompanyData on OB_GC = JCF_GC and OB_OH = JCF_ParentId
	where OB_PK = @CopiedPK and JCF_ParentTableCode = 'OH' and JCF_Ledger = @LedgerCode) as source
on (target.JCF_GC = source.JCF_GC and target.JCF_ParentTableCode = 'OH' and target.JCF_ConfigType = source.JCF_ConfigType and target.JCF_Ledger = source.JCF_Ledger and
	target.JCF_JobType = source.JCF_JobType and target.JCF_ServiceDirection = source.JCF_ServiceDirection and target.JCF_TransportMode = source.JCF_TransportMode and 
	target.JCF_Code = source.JCF_Code and target.JCF_ParentId = @NewOrganisationPK)
when matched then
	update set JCF_Code = source.JCF_Code, JCF_Code2 = source.JCF_Code2, JCF_Percentage = source.JCF_Percentage, JCF_Amount = source.JCF_Amount, JCF_Number = source.JCF_Number, JCF_Flag = source.JCF_Flag, JCF_SystemLastEditTimeUtc = GETUTCDATE(), JCF_SystemLastEditUser = @CurrentUser
when not matched then
	insert (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Percentage, JCF_Amount, JCF_Number, JCF_Flag)
		values (NEWID(), source.JCF_ConfigType, source.JCF_GC, source.JCF_Ledger, source.JCF_ParentTableCode, @NewOrganisationPK, source.JCF_JobType, source.JCF_ServiceDirection, source.JCF_TransportMode, source.JCF_Code, source.JCF_Code2, source.JCF_Percentage, source.JCF_Amount, source.JCF_Number, source.JCF_Flag)
output source.JCF_PK) as T
delete from dbo.AccJobConfig where JCF_PK in (select PK from @T)

UPDATE dbo.AccOrgTaxConfiguration SET OTC_OB = ({1}), OTC_SystemLastEditTimeUtc = GetUtcDate(), OTC_SystemLastEditUser = @CurrentUser
				FROM dbo.AccOrgTaxConfiguration JOIN dbo.AccTaxConfiguration ON OTC_ETC = ETC_PK WHERE OTC_OB = @CopiedPK AND ETC_Ledger = @LedgerCode
", innerSelectString, SelectNewPKsFromOld);

				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					try
					{
						cmd.AddParameter("@CopiedPK", SqlDbType.UniqueIdentifier, pk);
						cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
						cmd.AddParameter("@LedgerCode", SqlDbType.Char, 2, isAR ? "AR" : "AP");
						cmd.AddParameter("@CurrentUser", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
						cmd.ExecuteNonQuery();
					}
					catch (SqlException sqlEx)
					{
						var message = string.Format(CultureInfo.InvariantCulture, "ARAP Merge Failed - OB_PK: {0}, JCF_ParentID: {1}", pk, newOrganisation);
						ErrorReporter.ReportOnce("2f321a03-8afd-4e65-b3c2-74ae041514e6", message, sqlEx);
						throw;
					}
				}
			}

			var newOrganisationParameter = ZSqlParameter.New("@NewOrganisationPK", newOrganisation.ToGuid(), OrgCompanyDataSchema.OB_OH);
			var currentUser = ZSqlParameter.New("@CurrentUser", GlbStaff.CurrentUser.GS_Code, OrgInvoiceTypeSchema.PI_SystemCreateUser);
			MoveARAPLinks(isAR ? MoveARLinksQuery : MoveAPLinksQuery, pks, "(" + SelectNewPKsFromOld + ")", newOrganisationParameter, currentUser);
		}

		void MoveCollectionNotes()
		{
			string sql = "select ob_pk from dbo.orgcompanydata where ob_oh = @OldOrganisationPK";
			var oldOrganisationParameter = ZSqlParameter.New("@OldOrganisationPK", oldOrganisation.ToGuid(), OrgCompanyDataSchema.OB_OH);
			var pks = GetPKsFromQuery(sql, oldOrganisationParameter);
			if (!pks.IsNullOrEmpty())
			{
				var newOrganisationParameter = ZSqlParameter.New("@NewOrganisationPK", newOrganisation.ToGuid(), OrgCompanyDataSchema.OB_OH);
				var currentUser = ZSqlParameter.New("@CurrentUser", GlbStaff.CurrentUser.GS_Code, OrgCollectionNoteSchema.PN_SystemCreateUser);
				MoveARAPLinks(MoveCollectionNotesQuery, pks, "(" + SelectNewPKsFromOld + ")", true, newOrganisationParameter, currentUser);
			}
		}

		void MoveARAPLinks(string mainQuery, Guid[] pks, string subQuery, params ZSqlParameter[] sqlParameters)
		{
			MoveARAPLinks(mainQuery, pks, subQuery, false, sqlParameters);
		}

		void MoveARAPLinks(string mainQuery, Guid[] pks, string subQuery, bool dontMoveRollups, params ZSqlParameter[] sqlParameters)
		{
			foreach (var copiedPk in pks)
			{
				string sql = string.Format(CultureInfo.InvariantCulture, mainQuery, subQuery);
				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@CopiedPK", SqlDbType.UniqueIdentifier, copiedPk);
					cmd.AddParameters(sqlParameters);
					cmd.ExecuteNonQuery();
				}

				if (!dontMoveRollups)
				{
					MoveInvoiceRollups(copiedPk, subQuery, sqlParameters);
				}
			}
		}

		void MoveARAPLinks(string mainQuery, Guid[] pks, Dictionary<Guid, Guid> primarykeys)
		{
			var currentUser = ZSqlParameter.New("@CurrentUser", GlbStaff.CurrentUser.GS_Code, OrgInvoiceRollupOrGroupSchema.PG_SystemCreateUser);

			foreach (var copiedPk in pks)
			{
				using (DbCommand cmd = GetCommandOnMainConnection(mainQuery)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@CopiedPK", SqlDbType.UniqueIdentifier, copiedPk);
					cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, primarykeys[copiedPk]);
					cmd.AddParameter(currentUser);
					cmd.ExecuteNonQuery();
				}

				MoveARTerms(copiedPk, primarykeys[copiedPk]);
				var pkParameter = ZSqlParameter.New("@NewPK", primarykeys[copiedPk], OrgInvoiceRollupOrGroupSchema.PG_OB);
				MoveInvoiceRollups(copiedPk, "@NewPK", pkParameter, currentUser);
			}
		}

		void MoveInvoiceRollups(Guid copiedPk, string subQuery, params ZSqlParameter[] sqlParameters)
		{
			string sql = "select pg_pk from dbo.OrgInvoiceRollupOrGroup where PG_OB = @CopiedPk";
			var copiedPKParameter = ZSqlParameter.New("@CopiedPk", copiedPk, OrgInvoiceRollupOrGroupSchema.PG_OB);
			var rollupPKs = GetPKsFromQuery(sql, copiedPKParameter);
			if (!rollupPKs.IsNullOrEmpty())
			{
				foreach (var rollupPKToCopy in rollupPKs)
				{
					sql = string.Format(CultureInfo.InvariantCulture, MoveRollups, subQuery);
					using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameters(sqlParameters);
						cmd.AddParameter("@CopiedPk", SqlDbType.UniqueIdentifier, copiedPk);
						cmd.AddParameter("@RollupPKToCopy", SqlDbType.UniqueIdentifier, rollupPKToCopy);
						cmd.ExecuteNonQuery();
					}
				}

				sql = "delete from dbo.OrgInvoiceRollupOrGroup where PG_OB = @CopiedPk";
				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@CopiedPk", SqlDbType.UniqueIdentifier, copiedPk);
					cmd.ExecuteNonQuery();
				}
			}
		}

		void MoveARTerms(Guid oldOrgCompanyDataPK, Guid newOrgCompanyDataPK)
		{
			var primaryKeys = new Dictionary<Guid, Guid>();
			string sql = "select PY_PK from dbo.OrgARTerms where PY_OB = @OldOrgCompanyDataPK";
			var oldOrgCompanyDataPKParameter = ZSqlParameter.New("@OldOrgCompanyDataPK", oldOrgCompanyDataPK, OrgARTermsSchema.PY_OB);
			var oldARTermsPKs = GetPKsFromQuery(sql, oldOrgCompanyDataPKParameter);
			if (!oldARTermsPKs.IsNullOrEmpty())
			{
				foreach (var oldARTermsPK in oldARTermsPKs)
				{
					var newARTermsPK = Guid.NewGuid();
					primaryKeys.Add(oldARTermsPK, newARTermsPK);

					using (DbCommand cmd = GetCommandOnMainConnection(MoveARTermsQuery)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@NewOrgCompanyDataPK", SqlDbType.UniqueIdentifier, newOrgCompanyDataPK);
						cmd.AddParameter("@NewARTermsPK", SqlDbType.UniqueIdentifier, newARTermsPK);
						cmd.AddParameter("@OldArTermsPK", SqlDbType.UniqueIdentifier, oldARTermsPK);
						cmd.AddParameterBasedOnDbColumn("@CurrentUser", GlbStaff.CurrentUser.GS_Code.ToString(), OrgARTermsSchema.PY_SystemCreateUser);
						cmd.ExecuteNonQuery();
					}
				}

				MoveARTermsCycles(primaryKeys);

				sql = "delete from dbo.OrgARTerms where PY_OB = @OldOrgCompanyDataPK";
				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter(oldOrgCompanyDataPKParameter);
					cmd.ExecuteNonQuery();
				}
			}
		}

		void MoveARTermsCycles(Dictionary<Guid, Guid> primaryKeys)
		{
			foreach (var oldARTermsPK in primaryKeys.Keys)
			{
				using (DbCommand cmd = GetCommandOnMainConnection(MoveARTermsCyclesQuery)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@NewPK", SqlDbType.UniqueIdentifier, primaryKeys[oldARTermsPK]);
					cmd.AddParameter("@OldARTermsPK", SqlDbType.UniqueIdentifier, oldARTermsPK);
					cmd.AddParameterBasedOnDbColumn("@CurrentUser", GlbStaff.CurrentUser.GS_Code.ToString(), OrgARTermsCycleSchema.P5_SystemCreateUser);
					cmd.ExecuteNonQuery();
				}
			}
		}

		const string SelectNewPKsFromOld = @"
					select OB_PK from dbo.orgcompanydata 
						where OB_OH = @NewOrganisationPK
						and OB_GC = 
							(select OB_GC from dbo.orgcompanydata where OB_PK = @CopiedPK)";

		const string MoveRollups = @"insert into dbo.OrgInvoiceRollupOrGroup (
									PG_PK, PG_IsValid, PG_JobType, PG_TransportMode, PG_ServiceDirection, PG_GroupOrSubTotal, PG_GroupOrSubtotalStyle, PG_InvoiceLineDisplayOption, PG_InvoicePostingStyle, PG_OB, PG_SystemCreateTimeUtc, PG_SystemCreateUser, PG_SystemLastEditTimeUtc, PG_SystemLastEditUser)
							select 	NEWID(), PG_IsValid, PG_JobType, PG_TransportMode, PG_ServiceDirection, PG_GroupOrSubTotal, PG_GroupOrSubtotalStyle, PG_InvoiceLineDisplayOption, PG_InvoicePostingStyle, {0}, GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser from dbo.OrgInvoiceRollupOrGroup where PG_OB = @CopiedPk and PG_PK = @RollupPKToCopy
							and not exists
							(
								select * from dbo.OrgInvoiceRollupOrGroup t1
								inner join dbo.OrgInvoiceRollupOrGroup t2 on
								t1.PG_JobType = t2.PG_JobType and
								t1.PG_TransportMode = t2.PG_TransportMode and
								t1.PG_ServiceDirection = t2.PG_ServiceDirection
								where                                 
								t1.PG_PK = @RollupPKToCopy
								and t2.PG_PK <> @RollupPKToCopy
								and t2.PG_OB = {0}
							)";

		const string MoveARTermsQuery = @"
				insert into dbo.OrgARTerms(PY_PK, PY_IsValid, PY_InvoiceClass, PY_InvoiceTerm, PY_InvoiceDays, PY_AgreedPaymentMethod, PY_JobType, PY_Direction, PY_TransportMode, PY_GE_Department, PY_GB_Branch, PY_OB, PY_SystemCreateTimeUtc, PY_SystemCreateUser, PY_SystemLastEditTimeUtc, PY_SystemLastEditUser)
				select @NewARTermsPK, PY_IsValid, PY_InvoiceClass, PY_InvoiceTerm, PY_InvoiceDays, PY_AgreedPaymentMethod, PY_JobType, PY_Direction, PY_TransportMode, PY_GE_Department, PY_GB_Branch, @NewOrgCompanyDataPK, GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser
				from dbo.OrgARTerms where PY_PK = @OldArTermsPK";

		const string MoveARTermsCyclesQuery = @"
				insert into dbo.OrgARTermsCycle (P5_PK, P5_ToDay, P5_PaymentDay, P5_PY, P5_SystemCreateTimeUtc, P5_SystemCreateUser, P5_SystemLastEditTimeUtc, P5_SystemLastEditUser)
				select NEWID(), P5_ToDay, P5_PaymentDay, @NewPK, GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser FROM dbo.OrgARTermsCycle where P5_PY = @OldARTermsPK

				delete from dbo.OrgARTermsCycle where P5_PY = @OldARTermsPK";

		const string MoveARLinksQuery = @"insert into dbo.OrgInvoiceType (
									PI_PK, PI_IsValid, PI_Module, PI_Type, PI_Interval, PI_StartDay, PI_OB, PI_SecondaryType, PI_SystemCreateTimeUtc, PI_SystemCreateUser, PI_SystemLastEditTimeUtc, PI_SystemLastEditUser) 
							select  NEWID(), PI_IsValid, PI_Module, PI_Type, PI_Interval, PI_StartDay, {0}, 'INV', GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser from dbo.OrgInvoiceType where PI_OB = @CopiedPK
						
						insert into dbo.OrgWhsChgAttribGrpBy (
									PX_PK, PX_IsValid, PX_Code, PX_Order, PX_OB, PX_SystemCreateTimeUtc, PX_SystemCreateUser, PX_SystemLastEditTimeUtc, PX_SystemLastEditUser) 
							select  NEWID(), PX_IsValid, PX_Code, PX_Order, {0}, GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser from dbo.OrgWhsChgAttribGrpBy where PX_OB = @CopiedPK

						delete from dbo.OrgInvoiceType where PI_OB = @CopiedPK
						
						delete from dbo.OrgWhsChgAttribGrpBy where PX_OB = @CopiedPK";

		const string MoveAPLinksQuery = @"
						insert into dbo.AccAPAccountDetails (
									A1_PK, A1_RX_NKAccountCurrency, A1_AccountName, A1_BankName, A1_BankAccount, A1_BankBsb, A1_BankSwift, A1_PaymentMethod, A1_IsDefaultAccount, A1_BankBranchName, A1_BankAddress1, A1_BankAddress2, A1_BankAddress3, A1_OB, A1_SystemCreateTimeUtc, A1_SystemCreateUser, A1_SystemLastEditTimeUtc, A1_SystemLastEditUser)
							select  NEWID(), A1_RX_NKAccountCurrency, A1_AccountName, A1_BankName, A1_BankAccount, A1_BankBsb, A1_BankSwift, A1_PaymentMethod, A1_IsDefaultAccount, A1_BankBranchName, A1_BankAddress1, A1_BankAddress2, A1_BankAddress3, {0}, GetUtcDate(), @CurrentUser, GetUtcDate(), @CurrentUser from dbo.AccAPAccountDetails where A1_OB = @CopiedPK
												
						delete from dbo.AccAPAccountDetails where A1_OB = @CopiedPK";

		const string MoveCollectionNotesQuery = @"insert into dbo.OrgCollectionNote (
									PN_PK, PN_OC, PN_CallBackDate, PN_Status, PN_CallDisposition, PN_CallDetailNote, PN_AmountOverdueAtCallTime, PN_TotalOutstandingValueAtCallTime, PN_SystemCreateTimeUtc, PN_SystemCreateUser, PN_OB, PN_SystemLastEditTimeUtc, PN_SystemLastEditUser)
							select  NEWID(), PN_OC, PN_CallBackDate, PN_Status, PN_CallDisposition, PN_CallDetailNote, PN_AmountOverdueAtCallTime, PN_TotalOutstandingValueAtCallTime, PN_SystemCreateTimeUtc, PN_SystemCreateUser, {0}, GetUtcDate(), @CurrentUser from dbo.OrgCollectionNote where PN_OB = @CopiedPK
					delete from dbo.OrgCollectionNote where PN_OB = @CopiedPK";

		Guid[] GetPKsFromQuery(string sql, params ZSqlParameter[] parameters)
		{
			var result = new List<Guid>();
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameters(parameters);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0));
					}
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Merge Org Web URLs

		void MergeOrgWebURLs()
		{
			string sql = "select count(*) from dbo.OrgWebURL where PU_IsPrimary = 1 and PU_OH = @NewOrganisationPK";
			int count = 0;
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				count = Convert.ToInt32(cmd.ExecuteScalar());
			}
			if (count > 0)
			{
				sql = "update dbo.OrgWebURL set PU_IsPrimary = 0 where PU_OH = @OldOrganisationPK";
				using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
			sql = @"update dbo.OrgWebURL
set PU_OH = @NewOrganisationPK 
where PU_PK in (select d.PU_PK
from dbo.OrgWebURL d
left join dbo.OrgWebURL s on s.PU_URL = d.PU_URL and s.PU_OH = @NewOrganisationPK
where d.PU_OH = @OldOrganisationPK and s.PU_PK is null)";
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
			sql = "delete from dbo.OrgWebURL where PU_OH = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeBarcodeRuleSets

		void MergeBarcodeRuleSets()
		{
			var sql = @"
UPDATE
	dbo.BarcodeRule
SET
	BRU_BRS_RuleSet = Duplicates.RuleSetPKToKeep,
	BRU_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	BRU_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.BarcodeRule
	JOIN dbo.BarcodeRuleSet ON BRU_BRS_RuleSet = BRS_PK
	JOIN
	(
		SELECT
			BRS_Module,
			Buyer,
			Supplier,
			BRS_RelatedEntityId,
			CAST(MIN(CAST(BRS_PK as BINARY(16))) as uniqueidentifier) as RuleSetPKToKeep
		FROM
			dbo.BarcodeRuleSet
			CROSS APPLY
			(
				SELECT
					CASE WHEN BRS_OH_Buyer = @OldOrg then @NewOrg else BRS_OH_Buyer end as Buyer,
					CASE WHEN BRS_OH_Supplier = @OldOrg then @NewOrg else BRS_OH_Supplier end as Supplier
			) as BuyerAndSupplier
		WHERE BRS_IsSystem = 0
		GROUP BY
			BRS_Module,
			Buyer,
			Supplier,
			BRS_RelatedEntityId
		HAVING COUNT(*) > 1
	) Duplicates
	ON BarcodeRuleSet.BRS_Module = Duplicates.BRS_Module
		AND ISNULL(CASE WHEN BarcodeRuleSet.BRS_OH_Buyer = @OldOrg then @NewOrg else BarcodeRuleSet.BRS_OH_Buyer end, '00000000-0000-0000-0000-000000000000') 
				= ISNULL(CASE WHEN Duplicates.Buyer = @OldOrg then @NewOrg else Duplicates.Buyer end, '00000000-0000-0000-0000-000000000000')
		AND ISNULL(CASE WHEN BarcodeRuleSet.BRS_OH_Supplier = @OldOrg then @NewOrg else BarcodeRuleSet.BRS_OH_Supplier end, '00000000-0000-0000-0000-000000000000') 
				= ISNULL(CASE WHEN Duplicates.Supplier = @OldOrg then @NewOrg else Duplicates.Supplier end, '00000000-0000-0000-0000-000000000000')
		AND ISNULL(BarcodeRuleSet.BRS_RelatedEntityId, '00000000-0000-0000-0000-000000000000') = ISNULL(Duplicates.BRS_RelatedEntityId, '00000000-0000-0000-0000-000000000000')
WHERE
	BRU_BRS_RuleSet <> Duplicates.RuleSetPKToKeep

UPDATE
	dbo.BarcodeValidationRule
SET
	BVR_BRS_RuleSet = Duplicates.RuleSetPKToKeep,
	BVR_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	BVR_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.BarcodeValidationRule ValidationRule
	JOIN dbo.BarcodeRuleSet ValidationRuleSet ON ValidationRule.BVR_BRS_RuleSet = ValidationRuleSet.BRS_PK
	CROSS APPLY
	(
		SELECT
			ISNULL(CASE WHEN ValidationRuleSet.BRS_OH_Buyer = @OldOrg THEN @NewOrg ELSE ValidationRuleSet.BRS_OH_Buyer END, '00000000-0000-0000-0000-000000000000') AS Buyer,
			ISNULL(CASE WHEN ValidationRuleSet.BRS_OH_Supplier = @OldOrg THEN @NewOrg ELSE ValidationRuleSet.BRS_OH_Supplier END, '00000000-0000-0000-0000-000000000000') AS Supplier,
			ISNULL(ValidationRuleSet.BRS_RelatedEntityId, '00000000-0000-0000-0000-000000000000') RelatedEntity
	) RelationShips
	JOIN
	(
		SELECT
			DuplicateRuleSet.BRS_Module,
			BuyerAndSupplier.Buyer,
			BuyerAndSupplier.Supplier,
			BuyerAndSupplier.RelatedEntity,
			CAST(MIN(CAST(DuplicateRuleSet.BRS_PK AS BINARY(16))) AS uniqueidentifier) AS RuleSetPKToKeep
		FROM
			dbo.BarcodeRuleSet DuplicateRuleSet
			CROSS APPLY
			(
				SELECT
					ISNULL(CASE WHEN DuplicateRuleSet.BRS_OH_Buyer = @OldOrg THEN @NewOrg ELSE DuplicateRuleSet.BRS_OH_Buyer END, '00000000-0000-0000-0000-000000000000') AS Buyer,
					ISNULL(CASE WHEN DuplicateRuleSet.BRS_OH_Supplier = @OldOrg THEN @NewOrg ELSE DuplicateRuleSet.BRS_OH_Supplier END, '00000000-0000-0000-0000-000000000000') AS Supplier,
					ISNULL(DuplicateRuleSet.BRS_RelatedEntityId, '00000000-0000-0000-0000-000000000000') AS RelatedEntity
			) AS BuyerAndSupplier
		WHERE
			DuplicateRuleSet.BRS_IsSystem = 0
			AND (DuplicateRuleSet.BRS_OH_Buyer IN (@OldOrg, @NewOrg) OR DuplicateRuleSet.BRS_OH_Supplier IN (@OldOrg, @NewOrg))
		GROUP BY
			DuplicateRuleSet.BRS_Module,
			BuyerAndSupplier.Buyer,
			BuyerAndSupplier.Supplier,
			BuyerAndSupplier.RelatedEntity
		HAVING COUNT(*) > 1
	) Duplicates
	ON ValidationRuleSet.BRS_Module = Duplicates.BRS_Module
		AND RelationShips.Buyer = Duplicates.Buyer
		AND RelationShips.Supplier = Duplicates.Supplier
		AND RelationShips.RelatedEntity = Duplicates.RelatedEntity
WHERE
	ValidationRule.BVR_BRS_RuleSet <> Duplicates.RuleSetPKToKeep
	AND (ValidationRuleSet.BRS_OH_Buyer IN (@OldOrg, @NewOrg) OR ValidationRuleSet.BRS_OH_Supplier IN (@OldOrg, @NewOrg))

-- Delete all RuleSet with no rules (i.e the duplicates)
DELETE FROM dbo.BarcodeRuleSet
WHERE NOT EXISTS (
	SELECT NULL FROM dbo.BarcodeRule WHERE BRU_BRS_RuleSet = BRS_PK
	UNION
	SELECT NULL FROM dbo.BarcodeValidationRule WHERE BVR_BRS_RuleSet = BRS_PK
)

UPDATE dbo.BarcodeRuleSet SET BRS_OH_Buyer = @NewOrg, BRS_SystemLastEditTimeUtc = @SystemLastEditTimeUtc, BRS_SystemLastEditUser = @SystemLastEditUser WHERE BRS_OH_Buyer = @OldOrg
UPDATE dbo.BarcodeRuleSet SET BRS_OH_Supplier = @NewOrg, BRS_SystemLastEditTimeUtc = @SystemLastEditTimeUtc, BRS_SystemLastEditUser = @SystemLastEditUser WHERE BRS_OH_Supplier = @OldOrg

-- Change all Merged Barcode Rule numbers to be unique
UPDATE
	dbo.BarcodeRule
SET
	BRU_RuleNumber = NewNumber,
	BRU_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	BRU_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.BarcodeRule
	JOIN
	(
		SELECT 
			BRU_PK,
			ROW_NUMBER() OVER (PARTITION BY BRU_BRS_RuleSet ORDER BY BRU_RuleNumber) as NewNumber
		FROM
			dbo.BarcodeRule
	) ReorderedBarcodeRule ON BarcodeRule.BRU_PK = ReorderedBarcodeRule.BRU_PK
WHERE
	BRU_RuleNumber <> NewNumber
";

			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Delete Old Org Ref Org Consortium Pivots

		void DeleteOldOrgRefOrgConsortiumPivots()
		{
			SimpleDelete(RefOrgConsortiumPivotSchema.Constants.TableName, RefOrgConsortiumPivotSchema.RO_OH.Name);
		}

		#endregion

		#region Delete Old Part Relations

		void DeleteOldOrgPartRelations()
		{
			var sqlText = @"WITH DuplicatedRecords 
AS 
(
	SELECT OU_OP, OU_Relationship
	FROM (
		SELECT OU_OP, OU_Relationship, COUNT(1) OVER(PARTITION BY OU_OP, ISNULL(MappedRelationship, OU_Relationship)) CNT
		FROM dbo.OrgPartRelation 
		LEFT JOIN (
			SELECT 'BTH' Both, 'SUP' MappedRelationship UNION
			SELECT 'BTH' Both, 'OWN' MappedRelationship
		) Map ON Both = OU_Relationship
		WHERE OU_OH IN (@OldOrganisationPK, @NewOrganisationPK)
	) A
	WHERE CNT > 1
)
DELETE dbo.OrgPartRelation WHERE OU_PK in (
SELECT OU_PK 
FROM dbo.OrgPartRelation 
INNER JOIN DuplicatedRecords
ON OrgPartRelation.OU_OP = DuplicatedRecords.OU_OP AND OrgPartRelation.OU_Relationship = DuplicatedRecords.OU_Relationship 
WHERE OrgPartRelation.OU_OH = @OldOrganisationPK)";

			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Fix Acc Transaction Headers

		void FixAccTransactionHeaders()
		{
			string sql = @"
UPDATE dbo.AccTransactionHeader
SET AH_TransactionCount = NewTranCount, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = @SystemLastEditUser
FROM dbo.AccTransactionHeader
JOIN (
	-- Renumber transactions with duplicate AH_TransactionNum
	SELECT
		AH_PK,
		ROW_NUMBER() OVER (
			PARTITION BY
				ungroupedheaders.AH_Ledger,
				ungroupedheaders.AH_TransactionType,
				ungroupedheaders.AH_TransactionNum,
				ungroupedheaders.AH_GC
			ORDER BY
				ungroupedheaders.AH_Ledger,
				ungroupedheaders.AH_TransactionType,
				ungroupedheaders.AH_TransactionNum,
				ungroupedheaders.AH_TransactionCount,
				ungroupedheaders.AH_GC
		) AS NewTranCount
	FROM dbo.AccTransactionHeader ungroupedheaders
	JOIN (
		-- Identify transactions with duplicate AH_TransactionNum
		SELECT
			groupedheaders.AH_Ledger,
			groupedheaders.AH_TransactionType,
			groupedheaders.AH_TransactionNum,
			groupedheaders.AH_GC,
			@NewOrg as AH_OH
		FROM dbo.AccTransactionHeader groupedheaders
		WHERE groupedheaders.AH_OH IN (@OldOrg, @NewOrg)
			AND AH_Ledger IN ('AP', 'UA')
			AND AH_TransactionType IN ('INV', 'CRD', 'ADJ')
		GROUP BY
			groupedHeaders.AH_GC,
			groupedheaders.AH_Ledger,
			groupedheaders.AH_TransactionType,
			groupedheaders.AH_TransactionNum,
			groupedheaders.AH_TransactionCount
		HAVING COUNT(*) > 1
	) groupedheaders ON (1=1)
		AND ungroupedheaders.AH_Ledger = groupedheaders.AH_Ledger
		AND ungroupedheaders.AH_TransactionType = groupedheaders.AH_TransactionType
		AND ungroupedheaders.AH_GC = groupedheaders.AH_GC
		AND ungroupedheaders.AH_TransactionNum = groupedheaders.AH_TransactionNum
		AND ungroupedheaders.AH_OH IN (@OldOrg, @NewOrg)
) renumberedheaders ON
	renumberedheaders.AH_PK = AccTransactionHeader.AH_PK
WHERE AH_OH IN (@OldOrg, @NewOrg)
;

UPDATE dbo.AccTransactionHeader
SET AH_OH = @NewOrg, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = @SystemLastEditUser
WHERE AH_OH = @OldOrg
;
";
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Acc Transaction Lines

		void MergeSubAccountTypeOrgnisationInTransactionLine()
		{
			var sql = "UPDATE dbo.AccTransactionLineSubAccount SET AL1_SubClassParentId = @neworg, AL1_SystemLastEditTimeUtc = GETUTCDATE(), AL1_SystemLastEditUser = @SystemLastEditUser WHERE AL1_SubClassParentId IS NOT NULL AND AL1_SubClassParentId = @OldOrg";
			using (DbCommand cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Acc Transaction Header

		void MergeSubAccountTypeOrgnisationInTransactionHeader()
		{
			var sql = "UPDATE dbo.AccTransactionHeaderSubAccount SET AHS_SubClassParentId = @neworg, AHS_SystemLastEditTimeUtc = GETUTCDATE(), AHS_SystemLastEditUser = @SystemLastEditUser WHERE AHS_SubClassParentId IS NOT NULL AND AHS_SubClassParentId = @OldOrg";
			using (DbCommand cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Fix Acc Tax Return Lines

		void FixAccTaxReturnLines()
		{
			string sql = @"
;WITH linesRowNumber AS
(
	SELECT ARL_PK, ROW_NUMBER() OVER (ORDER BY ARL_OrgMergeCounter) AS RowNumber
	FROM dbo.AccTaxReturnLine
	WHERE ARL_OH_Organisation = @OldOrg
)
UPDATE dbo.AccTaxReturnLine SET ARL_OH_Organisation = @NewOrg, ARL_OrgMergeCounter = MaxOrgCounter + RowNumber, ARL_SystemLastEditTimeUtc = GETUTCDATE(), ARL_SystemLastEditUser = @SystemLastEditUser
FROM dbo.AccTaxReturnLine LinesToUpdate
	JOIN
		(
			SELECT ARL_ATR_AccTaxReturn, MAX(ARL_OrgMergeCounter) AS MaxOrgCounter
			FROM dbo.AccTaxReturnLine
			WHERE ARL_OH_Organisation IN (@OldOrg, @NewOrg)
			GROUP BY ARL_ATR_AccTaxReturn
		) LineCounter ON LineCounter.ARL_ATR_AccTaxReturn = LinesToUpdate.ARL_ATR_AccTaxReturn
	JOIN linesRowNumber RowNumber ON RowNumber.ARL_PK = LinesToUpdate.ARL_PK
WHERE LinesToUpdate.ARL_OH_Organisation = @OldOrg
";
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge dbo.CusPermitHeader

		void MergeCusPermitHeader()
		{
			string sql = "UPDATE dbo.CusPermitHeader SET CPH_OH_PermitHolder = @NewOrg WHERE CPH_OH_PermitHolder IS NOT NULL AND CPH_OH_PermitHolder = @OldOrg";
			using (DbCommand cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Orders

		class OrderForMerge
		{
			public OrderForMerge(Guid pk, string number)
			{
				PK = pk;
				OrderNumber = number;
			}

			public Guid PK { get; set; }
			public string OrderNumber { get; set; }
		}

		List<OrderForMerge> GetOrders(ZGuid orgPK)
		{
			List<OrderForMerge> result = new List<OrderForMerge>();
			using (DbCommand cmd = GetCommandOnMainConnection(@"
select JD_PK, JD_OrderNumber 
from dbo.JobOrderHeader 
join dbo.OrgAddress on JD_OA_BuyerAddress = OA_PK
where OA_OH = @OrgPK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						result.Add(new OrderForMerge(rd.GetGuid(0), rd.GetString(1)));
					}
				}
			}
			return result;
		}

		void MergeOrders()
		{
			var factory = GetNewFactory();
			var orders = GetOrders(oldOrganisation);
			var oldValues = new Dictionary<ZGuid, string>();
			foreach (var order in orders)
			{
				oldValues.Add(order.PK, order.OrderNumber);
				order.OrderNumber = GetNewOrderNumber(factory);
			}

			string sql = "update dbo.JobOrderHeader set jd_ordernumber = @NewOrderNumber where jd_pk = @PK"; // NB: No need to update JD_OA_BuyerAddress as it is handled by MergeOrgAddresses
																											 // Why do I need to use Log Factory here?
			var logfactory = GetNewFactory();
			foreach (var order in orders)
			{
				using (var cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					cmd.AddParameterBasedOnDbColumn("@NewOrderNumber", order.OrderNumber, JobOrderHeaderSchema.JD_OrderNumber);
					cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, order.PK);
					cmd.ExecuteNonQuery();
				}
				var parent = logfactory.Load(JobOrderHeaderSchema.Constants.Prefix, order.PK);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				parent.GetLogs().AddNew(Events.EditedARecord, "Order Number was changed from " + oldValues[order.PK] + " by org merge");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				if (mergeOrgHeader != null)
				{
					mergeOrgHeader.MergedOrders.Add(new string[] { mergeOrgHeader.OldOrganisation.OH_Code, oldValues[order.PK], order.OrderNumber });
				}
			}
			logfactory.Save();
		}

		protected virtual string GetNewOrderNumber(BusinessObjectFactory factory)
		{
			return Enterprise.Environment.Env.NumberFountains.OrderNumber.GetNextFormatted(factory);
		}

		#endregion

		#region Merge Warehouse

		void MergeWarehouseLinks()
		{
			DeleteWhsClientParameterByWarehouse();
			MergeVASOrders();
			MergeWhsPickFaces();
			MergeWhsAdHocServiceJobs();
		}

		#region DeleteWhsClientParameterByWarehouse

		void DeleteWhsClientParameterByWarehouse()
		{
			SimpleDelete(WhsClientParameterByWarehouseSchema.Constants.TableName, WhsClientParameterByWarehouseSchema.WY_OH_Client.Name);
		}

		#endregion

		#region Merge Whs Dockets

		void MergeWhsDockets()
		{
			var query = $@"
EXEC dbo.SuspendTrigger 'TG_WhsDocket_PreventClientChangeWhenHasLines';
EXEC dbo.SuspendTrigger 'TG_WhsDocket_PreventClientChangeWhenStartedReceiving';

SELECT
	WD_PK as DocketPK,
	MaxSplitNo,
	ROW_NUMBER() OVER(PARTITION BY WD_DocketType, WD_ExternalReference ORDER BY WD_ExternalReferenceSplit) as SequenceToAddToMaxSplitNo,
	NewDocketPKForReceiveCreatedFromPick
INTO
	#DocketsToUpdate
FROM
	dbo.WhsDocket OldClientDocket
	CROSS APPLY
	(
		SELECT
			MAX(WD_ExternalReferenceSplit) as MaxSplitNo
		FROM
			dbo.WhsDocket NewClientDocket
		WHERE
			NewClientDocket.WD_OH_Client = @NewOrgPk
			AND NewClientDocket.WD_DocketType = OldClientDocket.WD_DocketType
			AND NewClientDocket.WD_ExternalReference = OldClientDocket.WD_ExternalReference
	) NewClientDocket
	OUTER APPLY
	(
		SELECT
			WD_PK AS NewDocketPKForReceiveCreatedFromPick
		FROM
			dbo.WhsDocket NewClientDocket
		WHERE
			NewClientDocket.WD_OH_Client = @NewOrgPk
			AND NewClientDocket.WD_DocketType = 'INW'
			AND OldClientDocket.WD_DocketType = 'INW'
			AND OldClientDocket.WD_WP_ParentPickForReceive IS NOT NULL
			AND OldClientDocket.WD_WP_ParentPickForReceive = NewClientDocket.WD_WP_ParentPickForReceive
	) NewDocketPKForReceiveCreatedFromPick
WHERE
	WD_OH_Client = @OldOrgPk

UPDATE dbo.WhsDocketLine
SET
	WE_WD = NewDocketPKForReceiveCreatedFromPick,
	WE_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	WE_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.WhsDocketLine
	JOIN #DocketsToUpdate ON DocketPK = WE_WD AND NewDocketPKForReceiveCreatedFromPick IS NOT NULL

DELETE dbo.WhsDocket
FROM
	dbo.WhsDocket
	JOIN #DocketsToUpdate ON DocketPK = WD_PK AND NewDocketPKForReceiveCreatedFromPick IS NOT NULL

UPDATE dbo.WhsDocket
SET
	WD_OH_Client = @NewOrgPk,
	WD_ExternalReferenceSplit = CASE WHEN MaxSplitNo IS NULL THEN WD_ExternalReferenceSplit ELSE MaxSplitNo + SequenceToAddToMaxSplitNo END,
	WD_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	WD_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.WhsDocket
	JOIN #DocketsToUpdate ON DocketPK = WD_PK

EXEC dbo.ResumeTrigger 'TG_WhsDocket_PreventClientChangeWhenHasLines';
EXEC dbo.ResumeTrigger 'TG_WhsDocket_PreventClientChangeWhenStartedReceiving';

DROP TABLE #DocketsToUpdate
";

			using (var cmd = GetCommandOnMainConnection(query))
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Whs Serial Numbers

		void MergeWhsSerialNumbers()
		{
			var query = $@"
EXEC dbo.SuspendTrigger 'TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber';

-- In the previous step, we updated the docket's client. By making this change, it should now align the client with the corresponding job.

UPDATE
	WhsSerialNumber
SET
	WSN_OH_Client = @NewOrgPk
WHERE
	WSN_OH_Client = @OldOrgPk;

EXEC dbo.ResumeTrigger 'TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber';
";

			using (var cmd = GetCommandOnMainConnection(query))
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeVASOrders

		void MergeVASOrders()
		{
			var sql = @"
INSERT dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_EventTime, SL_GS_NKUser)
SELECT NEWID(), MergingOrders.WVO_PK, 'WhsVASOrder', 'Old Customer Reference: ' + MergingOrders.WVO_CustomerReferenceNo, GETDATE(), 'E'
FROM
	dbo.WhsVASOrder MergingOrders
	JOIN dbo.WhsVASOrder WholeTable ON MergingOrders.WVO_PK <> WholeTable.WVO_PK
		AND MergingOrders.WVO_CustomerReferenceNo = WholeTable.WVO_CustomerReferenceNo
		AND WholeTable.WVO_OH_Client = @NewOrganisationPK
		WHERE MergingOrders.WVO_OH_Client = @OldOrganisationPK

UPDATE dbo.WhsVASOrder
SET
	WVO_OH_Client = @NewOrganisationPK,
	WVO_CustomerReferenceNo = CASE WHEN WholeTable.WVO_PK IS NULL THEN WhsVASOrder.WVO_CustomerReferenceNo ELSE WhsVASOrder.WVO_JobID END,
	WVO_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	WVO_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.WhsVASOrder
	LEFT JOIN dbo.WhsVASOrder WholeTable ON WhsVASOrder.WVO_PK <> WholeTable.WVO_PK
		AND WhsVASOrder.WVO_CustomerReferenceNo = WholeTable.WVO_CustomerReferenceNo	
		AND WholeTable.WVO_OH_Client = @NewOrganisationPK
		WHERE WhsVASOrder.WVO_OH_Client = @OldOrganisationPK
";

			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeClientPickPackParametersByWhs

		void MergeClientPickPackParametersByWhs()
		{
			var sql = @"
DELETE dbo.WhsClientPickPackParamsByWhs
FROM
	dbo.WhsClientPickPackParamsByWhs
	JOIN dbo.WhsClientPickPackParamsByWhs WholeTable ON
		WhsClientPickPackParamsByWhs.WPP_PK <> WholeTable.WPP_PK
		AND WhsClientPickPackParamsByWhs.WPP_WW_Warehouse = WholeTable.WPP_WW_Warehouse
		AND EXISTS
		(
			SELECT WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel
			INTERSECT
			SELECT WholeTable.WPP_WSH_SalesChannel
		)
		AND WholeTable.WPP_OH_Client IN (@OldOrganisationPK, @NewOrganisationPK)
	JOIN
	(
		SELECT
			WD_WW_Whs,
			WD_WSH_SalesChannel,
			COUNT(DISTINCT CASE WHEN WD_OH_Client = @OldOrganisationPK THEN KP_PK END) as CountOfPackagesOnOld,
			COUNT(DISTINCT CASE WHEN WD_OH_Client = @NewOrganisationPK THEN KP_PK END) as CountOfPackagesOnNew
		FROM
			dbo.WhsDocket
			JOIN dbo.PkgPackageJob ON KJ_ParentID = WD_PK AND WD_DocketType = 'ORD'
			JOIN dbo.PkgPackage ON KP_KJ_ParentPackageJob = KJ_PK
		WHERE
			WD_OH_Client IN (@OldOrganisationPK, @NewOrganisationPK)
		GROUP BY
			WD_WW_Whs,
			WD_WSH_SalesChannel
	) as NumberOfPackages ON
		WD_WW_Whs = WhsClientPickPackParamsByWhs.WPP_WW_Warehouse
		AND EXISTS
		(
			SELECT WD_WSH_SalesChannel
			INTERSECT
			SELECT WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel
		)
WHERE
	WhsClientPickPackParamsByWhs.WPP_OH_Client IN (@OldOrganisationPK, @NewOrganisationPK)
	AND
	(
		(WhsClientPickPackParamsByWhs.WPP_OH_Client = @OldOrganisationPK AND CountOfPackagesOnOld <= CountOfPackagesOnNew)
		OR
		(WhsClientPickPackParamsByWhs.WPP_OH_Client = @NewOrganisationPK AND CountOfPackagesOnNew < CountOfPackagesOnOld)
	)
";

			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeWhsProductParamsByWhsAndClient

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "SQL query doesn't need IFormatting in this case")]
		void MergeWhsProductParamsByWhsAndClient()
		{
			var sql = @"
Delete dbo.WhsProductParamsByWhsAndClient
From
	dbo.WhsProductParamsByWhsAndClient
	Join dbo.WhsProductParamsByWhsAndClient as Duplicates on WhsProductParamsByWhsAndClient.W3_OP = Duplicates.W3_OP
		And WhsProductParamsByWhsAndClient.W3_WW = Duplicates.W3_WW
		And WhsProductParamsByWhsAndClient.W3_PK <> Duplicates.W3_PK
	Where
		WhsProductParamsByWhsAndClient.W3_OH = @OldOrganisationPK
		AND Duplicates.W3_OH = @NewOrganisationPK
";

			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeWhsPickFaces

		void MergeWhsPickFaces()
		{
			var sql = @"
DELETE dbo.WhsPickFace
FROM
	dbo.WhsPickFace
	JOIN dbo.WhsPickFace as Duplicates ON WhsPickFace.WF_OP = Duplicates.WF_OP
		AND WhsPickFace.WF_WL = Duplicates.WF_WL
		AND WhsPickFace.WF_PK <> Duplicates.WF_PK
	WHERE
		WhsPickFace.WF_OH_Client = @OldOrganisationPK
		AND Duplicates.WF_OH_Client = @NewOrganisationPK
";

			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeWhsAdHocServiceJobs

		void MergeWhsAdHocServiceJobs()
		{
			var sql = @"
SELECT 
	W1.WSJ_PK,
	W1.WSJ_CustomerReference
INTO #DuplicateCustomerReferences
FROM
	dbo.WhsAdHocServiceJob W1
	JOIN dbo.WhsAdHocServiceJob W2 ON
		W1.WSJ_CustomerReference = W2.WSJ_CustomerReference
		AND W2.WSJ_OH_Client = @NewOrganisationPK
		AND W1.WSJ_PK<> W2.WSJ_PK
WHERE
	W1.WSJ_OH_Client = @OldOrganisationPK

SELECT
	WSJ_PK,
	WSJ_CustomerReference,
	LEFT(WSJ_CustomerReference, 31) + '_' + RIGHT('000' + RTRIM(CAST((ISNULL(ExistingReference, 0) + 1) AS varchar(3))), 3) NewCustomerReference
INTO #ServiceJobChanges
FROM
	(
		SELECT
			WSJ_PK,
			WSJ_CustomerReference
		FROM
			#DuplicateCustomerReferences
	) Duplicates
CROSS APPLY
	(
		SELECT
			MAX(CAST(RIGHT(WSJ_CustomerReference, 3) AS smallint)) ExistingReference
		FROM
			dbo.WhsAdHocServiceJob
		WHERE
			WhsAdHocServiceJob.WSJ_CustomerReference LIKE LEFT(Duplicates.WSJ_CustomerReference, 31) + '\_[0-9][0-9][0-9]' ESCAPE '\'
	) AS ExistingMaxAppendIndex

UPDATE dbo.WhsAdHocServiceJob
SET
	WSJ_CustomerReference = NewCustomerReference,
	WSJ_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	WSJ_SystemLastEditUser = @SystemLastEditUser
FROM
	dbo.WhsAdHocServiceJob
JOIN #ServiceJobChanges ON WhsAdHocServiceJob.WSJ_PK = #ServiceJobChanges.WSJ_PK

UPDATE dbo.WhsAdHocServiceJob
SET
	WSJ_OH_Client = @NewOrganisationPK,
	WSJ_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	WSJ_SystemLastEditUser = @SystemLastEditUser
WHERE
	WSJ_OH_Client = @OldOrganisationPK

DROP TABLE #DuplicateCustomerReferences
DROP TABLE #ServiceJobChanges";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#endregion

		#region MergeProductStyles

		void MergeProductStyles()
		{
			string sql = "";

			var oldOrgProductStylePKsAndCodes = GetProductStylePksAndCodesForOwner(oldOrganisation);
			var newOrgProductStyleCodesAndPks = GetProductStylePksAndCodesForOwner(newOrganisation);
			if (oldOrgProductStylePKsAndCodes.Any())
			{
				var newOrgProductStyleCodesSet = newOrgProductStyleCodesAndPks.Select(pc => pc.Code).ToHashSet();
				var existingProductStylePKsAndCodes = oldOrgProductStylePKsAndCodes.Where(pc => newOrgProductStyleCodesSet.Contains(pc.Code)).ToArray();

				sql = @"INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_EventTime, SL_GS_NKUser) 
						SELECT inserts.OwnerChangeLogPK, inserts.OldOrgProductStylePK, 'WhsProductStyle', inserts.OwnerChangeMessage, GETDATE(), 'E'  FROM (
							SELECT
								CONVERT(uniqueidentifier,SUBSTRING(stml.Value, 1, @GuidLength)) AS OwnerChangeLogPK,
								CONVERT(uniqueidentifier,SUBSTRING(stml.Value, @GuidLength + 1, @GuidLength)) AS OldOrgProductStylePK,
								SUBSTRING(stml.Value, 2 * @GuidLength + 1 , LEN(stml.Value) - 2 * @GuidLength) AS OwnerChangeMessage
							FROM
								@InsertsTVP AS stml	
						) inserts;
				";

				if (existingProductStylePKsAndCodes.Length > 0)
				{
					sql += @"
							UPDATE
								dbo.WhsProductStyle
							SET
								WST_Code = updates.WST_Code,
								WST_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
								WST_SystemLastEditUser = @SystemLastEditUser
							FROM dbo.WhsProductStyle INNER JOIN (
								SELECT
									CONVERT(uniqueidentifier,SUBSTRING(stml.Value, @GuidLength + 1, @GuidLength)) AS OldOrgProductStylePK,
									SUBSTRING(stml.Value, CHARINDEX(@UpdateCodeSeperator, stml.Value) + 1, LEN(stml.Value) - CHARINDEX(@UpdateCodeSeperator, stml.Value)) AS WST_Code
								FROM
									@UpdatesTVP AS stml	
							) updates 
							ON WhsProductStyle.WST_PK = updates.OldOrgProductStylePK;

							INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_EventTime, SL_GS_NKUser) 
							SELECT updates.CodeChangeLogPK, updates.OldOrgProductStylePK, 'WhsProductStyle', updates.CodeChangeMessage, GETDATE(), 'E'  FROM (
								SELECT
									CONVERT(uniqueidentifier,SUBSTRING(stml.Value, 1, @GuidLength)) AS CodeChangeLogPK,
									CONVERT(uniqueidentifier,SUBSTRING(stml.Value, @GuidLength + 1, @GuidLength)) AS OldOrgProductStylePK,
									SUBSTRING(stml.Value, 2 * @GuidLength + 1, CHARINDEX(@UpdateCodeSeperator, stml.Value) - (2 * @GuidLength + 1)) AS CodeChangeMessage
								FROM
									@UpdatesTVP AS stml	
							) updates;";
				}

				using (var insertsTable = new DataTable())
				using (var updatesTable = new DataTable())
				using (var cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				{
					insertsTable.Locale = CultureInfo.InvariantCulture;
					insertsTable.Columns.Add((NoResString)"Value",
						typeof(string));

					updatesTable.Locale = CultureInfo.InvariantCulture;
					updatesTable.Columns.Add((NoResString)"Value",
						typeof(string));

					for (int i = 0; i < oldOrgProductStylePKsAndCodes.Length; i++)
					{
						string logMessage;

						if (mergeOrgHeader == null)
						{
							logMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Owner has been changed. Old Owner PK was {0}",
								oldOrganisation.ToGuid());
						}
						else
						{
							logMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Owner has been changed. Old Owner PK was {0} and Code was {1}",
								oldOrganisation.ToGuid(), mergeOrgHeader.OldOrganisation.OH_Code);
						}

						insertsTable.Rows.Add(Guid.NewGuid().ToString() + oldOrgProductStylePKsAndCodes[i].PK.ToGuid().ToString() + logMessage);
					}

					cmd.AddParameter("@GuidLength", SqlDbType.Int, Guid.NewGuid().ToString().Length);
					cmd.AddTableValuedParameter("@InsertsTVP", TVPHelper.TVP_nvarchar, insertsTable);
					cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);

					if (existingProductStylePKsAndCodes.Any())
					{
						var updateCodeSeperator = '|';

						foreach (var productStyleAndPk in existingProductStylePKsAndCodes)
						{
							productStyleAndPk.NewCode = FindNewStyleCode(productStyleAndPk, newOrgProductStyleCodesAndPks, oldOrgProductStylePKsAndCodes, 1);
							var logMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Code has been renamed from {0} to {1}",
								productStyleAndPk.Code, productStyleAndPk.NewCode);
							updatesTable.Rows.Add(Guid.NewGuid().ToString() + productStyleAndPk.PK.ToGuid().ToString() + logMessage + updateCodeSeperator.ToString() + productStyleAndPk.NewCode);
						}

						cmd.AddParameter("@UpdateCodeSeperator", SqlDbType.VarChar, updateCodeSeperator);
						cmd.AddTableValuedParameter("@UpdatesTVP", TVPHelper.TVP_nvarchar, updatesTable);
					}

					cmd.ExecuteNonQuery();
				}
			}
		}

		string FindNewStyleCode(ProductStylePKAndCode oldOrgProductStyleCodeAndPK, ProductStylePKAndCode[] newOrgProductStyleCodesAndPks, ProductStylePKAndCode[] oldOrgProductStyleCodesAndPks, int count)
		{
			var length = WhsProductStyleSchema.WST_Code.MaxLength;
			string newStyleCode = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", oldOrgProductStyleCodeAndPK.Code, count); // Concatenated string to generate unique names using c#6

			if (newStyleCode.Length > length)
			{
				newStyleCode = new Random().Next(100000000).ToString(CultureInfo.InvariantCulture);
			}

			if (newOrgProductStyleCodesAndPks.Any(s => s.Code == newStyleCode))
			{
				newStyleCode = FindNewStyleCode(oldOrgProductStyleCodeAndPK, newOrgProductStyleCodesAndPks, oldOrgProductStyleCodesAndPks, ++count);
			}

			if (oldOrgProductStyleCodesAndPks.Any(s => s.NewCode == newStyleCode || (!string.IsNullOrEmpty(s.Code) && s.Code == newStyleCode)))
			{
				newStyleCode = FindNewStyleCode(oldOrgProductStyleCodeAndPK, newOrgProductStyleCodesAndPks, oldOrgProductStyleCodesAndPks, ++count);
			}

			return newStyleCode;
		}

		ProductStylePKAndCode[] GetProductStylePksAndCodesForOwner(ZGuid ownerPK)
		{
			IList<ProductStylePKAndCode> result = new List<ProductStylePKAndCode>();
			var productStylePKAndCodes = (NoResString)"SELECT WST_PK, WST_Code FROM dbo.WhsProductStyle WHERE WST_OH_Owner = @OwnerPK";

			using (DbCommand cmd = GetCommandOnMainConnection(productStylePKAndCodes)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OwnerPK", SqlDbType.UniqueIdentifier, ownerPK.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new ProductStylePKAndCode(reader.GetGuid(0), reader.GetString(1)));
					}
				}
				return result.ToArray();
			}
		}

		class ProductStylePKAndCode
		{
			public ProductStylePKAndCode(ZGuid pk, string code)
			{
				PK = pk;
				Code = code;
			}
			public ZGuid PK { get; }
			public string Code { get; }
			public string NewCode { get; set; }
		}

		#endregion

		#region Simple Delete Template

		void SimpleDelete(string tableName, string fkColumn)
		{
			string sqlText = string.Format("delete from {0} WHERE {1} = @OldOrgPK", tableName, fkColumn);
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrgPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Delete Ref Packs

		void DeleteRefPacks()
		{
			SimpleDelete(RefPacksSchema.Constants.TableName, RefPacksSchema.RP_OH_Supplier.Name);
		}

		#endregion

		#region Delete dbo.OrgAirlineMAWBStockManagement

		void DeleteOrgAirlineMAWBStockManagement()
		{
			SimpleDelete(OrgAirlineMAWBStockManagementSchema.Constants.TableName, OrgAirlineMAWBStockManagementSchema.OHM_OH_Carrier.Name);
		}

		#endregion

		#region Delete Org Product Type

		void DeleteOrgProductType()
		{
			SimpleDelete(OrgProductTypeSchema.Constants.TableName, OrgProductTypeSchema.OPT_OH_Owner.Name);
		}

		#endregion

		#region Delete dbo.CusSeaManSlotOrg

		void DeleteCusSeaManSlotOrg()
		{
			SimpleDelete(CusSeaManSlotOrgSchema.Constants.TableName, CusSeaManSlotOrgSchema.BS_OH_SlotCharterer.Name);
		}

		#endregion

		#region Delete dbo.JobTradeLaneVoyage

		void DeleteJobTradeLaneVoyage()
		{
			SimpleDelete(JobTradeLaneVoyageSchema.Constants.TableName, JobTradeLaneVoyageSchema.NB_OH.Name);
		}

		#endregion

		#region MergeStmMenuDocumentConfig

		void DeleteStmMenuDocumentConfig()
		{
			DataTable oldOrgDocumentConfigs = new DataTable();
			string sql = @"
				delete from dbo.StmMenuDocumentConfigItem where S4_S3 in (select S3_PK from dbo.StmMenuDocumentConfig where S3_OH = @OldOrg)
				delete from dbo.StmMenuDocumentConfig where S3_OH = @OldOrg";
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region UpdateCountryDatasWithApprovedLocation

		void UpdateCountryDatasWithApprovedLocation()
		{
			var sqlSelect = "select OV_PK from dbo.OrgCountryData where OV_OA_ApprovedLocation = @EntryKey";
			var sql = @"
begin try 
	update dbo.OrgCountryData set OV_OA_ApprovedLocation = @EntryValue where OV_PK = @PK
end try
begin catch
	--dont care - everyting is deleted later
end catch";

			foreach (var entry in mergedAddresses)
			{
				var entryKeyParameter = ZSqlParameter.New("@EntryKey", entry.Key.ToGuid(), OrgCountryDataSchema.OV_OA_ApprovedLocation);
				var pks = GetPKsFromQuery(sqlSelect, entryKeyParameter);

				foreach (var pk in pks)
				{
					using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@EntryValue", SqlDbType.UniqueIdentifier, entry.Value.ToGuid());
						cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
						cmd.ExecuteNonQuery();
					}
				}
			}

			sql = "update dbo.OrgCountryData set OV_OA_ApprovedLocation = null where OV_OA_ApprovedLocation in (select oa_pk from dbo.orgaddress where oa_oh = @OldOrganisationPK)";
			using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeOrgServiceLevels

		void MergeOrgServiceLevels()
		{
			BusinessObjectFactory factory = mergeOrgHeader != null ? mergeOrgHeader.Factory : GetNewFactory();
			bool hasExistingServiceLevels = factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(OrgServiceLevel)), new ZQuery(OrgServiceLevelSchema.PM_OH, newOrganisation));
			if (!hasExistingServiceLevels)
			{
				var oldOrganisationParameter = ZSqlParameter.New("@OldOrganisationPK", oldOrganisation.ToGuid(), OrgServiceLevelSchema.PM_OH);
				var pks = GetPKsFromQuery("select PM_PK from dbo.OrgServiceLevel where PM_OH = @OldOrganisationPK", oldOrganisationParameter);
				var sql = @"
begin try 
	update dbo.OrgServiceLevel set PM_OH = @NewOrganisationPK where PM_PK = @PK	
end try
begin catch
	--dont care - everyting is deleted on OrgHeader.Delete()
end catch";
				foreach (var pk in pks)
				{
					using (DbCommand cmd = GetCommandOnMainConnection(sql)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
						cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#endregion

		#region DeleteOrgPatternMatchAddress

		public void DeleteOrgPatternMatchAddress()
		{
			SimpleDelete(OrgPatternMatchAddressSchema.Constants.TableName, OrgPatternMatchAddressSchema.P3_OH_MatchOrg.Name);
		}

		#endregion

		#region DeleteOrgMatchApproval

		public void DeleteOrgMatchApproval()
		{
			SimpleDelete(OrgMatchApprovalSchema.Constants.TableName, OrgMatchApprovalSchema.P2_OH_MatchOrg1.Name);
			SimpleDelete(OrgMatchApprovalSchema.Constants.TableName, OrgMatchApprovalSchema.P2_OH_MatchOrg2.Name);
		}

		#endregion

		#region MoveOrgDocuments

		void MoveOrgDocuments()
		{
			using (DbCommand cmd = GetCommandOnMainConnection("update dbo.OrgDocument set OD_OH_RelatedFilterByParty = @NewOrganisationPK where OD_OH_RelatedFilterByParty = @OldOrganisationPK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		void MergeSuppressedDocuments()
		{
			using (var cmd = GetCommandOnMainConnection("update dbo.OrgDocument set OD_OH_Suppressed = @NewOrganisationPK where OD_OH_Suppressed = @OldOrganisationPK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MoveOrgSubscriptions

		void MoveOrgSubscriptions()
		{
			const string query = @"
;WITH 
	selectNewCategoryAndTypeFromOldOrg AS (
		SELECT [GCS_MediaCategory],[GCS_MediaType] FROM [dbo].[GlbCompanyCampaignSubscription] WHERE [GCS_OH]=@oldOrgPk
			EXCEPT
		SELECT [GCS_MediaCategory],[GCS_MediaType] FROM [dbo].[GlbCompanyCampaignSubscription] WHERE [GCS_OH]=@newOrgPk
	),
	selectPkColumn AS (
		SELECT [GCS_PK],[GCS_OH],[GCS_SystemLastEditUser],[GCS_SystemLastEditTimeUtc] FROM selectNewCategoryAndTypeFromOldOrg AS T1
			INNER JOIN (SELECT [GCS_PK],[GCS_MediaCategory],[GCS_MediaType],[GCS_OH],[GCS_SystemLastEditUser],[GCS_SystemLastEditTimeUtc] FROM [dbo].[GlbCompanyCampaignSubscription] WHERE [GCS_OH]=@oldOrgPk ) AS T2
			ON (T1.[GCS_MediaCategory]=T2.[GCS_MediaCategory] AND T1.[GCS_MediaType]=T2.[GCS_MediaType])
	)
UPDATE selectPkColumn
SET [GCS_OH]=@newOrgPk,[GCS_SystemLastEditUser]=@SystemLastEditUser,[GCS_SystemLastEditTimeUtc]=@SystemLastEditTimeUtc

DELETE FROM [dbo].[GlbCompanyCampaignSubscription] WHERE [GCS_OH]=@oldOrgPk
";
			using (var cmd = GetCommandOnMainConnection(query))
			{
				cmd.AddParameter("oldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("newOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MoveGenAddOnColumn

		void MoveGenAddOnColumn()
		{
			using (DbCommand cmd = GetCommandOnMainConnection(string.Format(
				"update dbo.GenAddOnColumn set XA_Data='{0}', XA_SystemLastEditUser = @SystemLastEditUser, XA_SystemLastEditTimeUtc = @SystemLastEditTimeUtc where XA_Data='{1}' and (XA_Name like 'OH[_]%' or XA_Name like '%[_]OH[_]%' or XA_Name like '%[_]OH')",
				newOrganisation, oldOrganisation)))
			{
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Move Country Data

		class MergeOrgCountryData
		{
			public MergeOrgCountryData(Guid pk, string countryCode, Guid org, Guid address)
			{
				PK = pk;
				CountryCode = countryCode;
				OrgAddressPK = address;
			}

			public Guid PK { get; set; }
			public string CountryCode { get; set; }
			public Guid OrgAddressPK { get; set; }
		}

		List<MergeOrgCountryData> GetOrgCountryData(ZGuid orgPK)
		{
			List<MergeOrgCountryData> result = new List<MergeOrgCountryData>();
			using (DbCommand cmd = GetCommandOnMainConnection("select OV_PK, OV_RN_NKClientCountryRelation, OV_OH_OrgHeader, OV_OA_ApprovedLocation from dbo.OrgCountryData where OV_OH_OrgHeader = @OrgPK"))
			{
				cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						object address = rd.GetValue(3);
						result.Add(new MergeOrgCountryData(rd.GetGuid(0), rd.GetString(1), rd.GetGuid(2), (address == DBNull.Value ? Guid.Empty : rd.GetGuid(3))));
					}
				}
			}
			return result;
		}

		void MoveCountryData()
		{
			List<MergeOrgCountryData> oldData = GetOrgCountryData(oldOrganisation);
			List<MergeOrgCountryData> newData = GetOrgCountryData(newOrganisation);
			foreach (MergeOrgCountryData data in oldData)
			{
				MergeOrgCountryData[] forSameCountry = Array.FindAll(newData.ToArray(), d => d.CountryCode == data.CountryCode);
				bool shouldCopy =
					(
						data.CountryCode == Core.Constants.CountryCodes.UnitedStates ||
						!Array.Exists(forSameCountry, d => d.CountryCode == data.CountryCode)
					) &&
					!Array.Exists(forSameCountry, d => d.OrgAddressPK == data.OrgAddressPK);

				if (shouldCopy)
				{
					using (DbCommand cmd = GetCommandOnMainConnection(@"
	update dbo.OrgCountryData set OV_OH_OrgHeader = @NewOrganisationPK where OV_PK = @DataPK"))
					{
						cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
						cmd.AddParameter("@DataPK", SqlDbType.UniqueIdentifier, data.PK);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		void MoveGlobalChargeCodeMapPivot()
		{
			var pivotPKListToBeDeleted = new List<Guid>();
			var sqlText = @"SELECT YP_PK FROM dbo.AccGlobalChargeCodeMapPivot AS A WHERE YP_OH_LocalClientOverride = @OldOrganisationPK
AND EXISTS
(
	SELECT YP_PK FROM dbo.AccGlobalChargeCodeMapPivot AS B
	WHERE 
	B.YP_OH_LocalClientOverride = @NewOrganisationPK
	AND A.YP_YG = B.YP_YG
	AND A.YP_AC = B.YP_AC
	AND A.YP_TYPE = B.YP_TYPE		
)";

			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						pivotPKListToBeDeleted.Add(rd.GetGuid(0));
					}
				}
			}

			if (pivotPKListToBeDeleted.Count > 0)
			{
				sqlText = "DELETE dbo.AccGlobalChargeCodeMapPivot FROM dbo.AccGlobalChargeCodeMapPivot JOIN @PkList ON Value = YP_PK"; // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects

				using (var dataTable = new DataTable())
				{
					dataTable.Locale = CultureInfo.InvariantCulture;
					dataTable.Columns.Add((NoResString)"Value",
						typeof(Guid));

					foreach (var item in pivotPKListToBeDeleted)
					{
						dataTable.Rows.Add(item);
					}

					using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddTableValuedParameter("@PkList", TVPHelper.TVP_uniqueidentifier, dataTable);
						cmd.ExecuteNonQuery();
					}
				}
			}

			sqlText = @"UPDATE dbo.AccGlobalChargeCodeMapPivot
SET YP_OH_LocalClientOverride = @NewOrganisationPK,
	YP_SystemLastEditTimeUtc = GETUTCDATE(),
	YP_SystemLastEditUser = @SystemLastEditUser
WHERE YP_OH_LocalClientOverride = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeJobComInvoiceHeader

		void MergeJobComInvoiceHeader()
		{
			foreach (KeyValuePair<ZGuid, ZGuid> pair in mergedAddresses)
			{
				using (var cmd = GetCommandOnMainConnection(@"UPDATE dbo.JobComInvoiceHeader
SET JZ_OH_Supplier = @NewOrgPk,
    JZ_OA_SupplierAddress = @NewAddrPk,
    JZ_SystemLastEditTimeUtc = GETUTCDATE(),
    JZ_SystemLastEditUser = @UserCode
WHERE JZ_OH_Supplier = @OldOrgPk AND JZ_OA_SupplierAddress = @OldAddrPk"))
				{
					cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
					cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
					cmd.AddParameterBasedOnDbColumn("@NewAddrPk", pair.Value.ToGuid(), JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress);
					cmd.AddParameterBasedOnDbColumn("@OldAddrPk", pair.Key.ToGuid(), JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress);
					cmd.AddParameterBasedOnDbColumn("@UserCode", GlbStaff.CurrentUser.GS_Code.ToString(), JobComInvoiceHeaderSchema.JZ_SystemLastEditUser);
					cmd.ExecuteNonQuery();
				}
			}

			using (var cmd = GetCommandOnMainConnection(@"UPDATE dbo.JobComInvoiceHeader
SET JZ_OH_Supplier = @NewOrgPk,
    JZ_SystemLastEditTimeUtc = GETUTCDATE(),
    JZ_SystemLastEditUser = @UserCode
WHERE JZ_OH_Supplier = @OldOrgPk"))
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@UserCode", GlbStaff.CurrentUser.GS_Code.ToString(), JobComInvoiceHeaderSchema.JZ_SystemLastEditUser);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeJobDeclaration

		void MergeJobDeclaration()
		{
			foreach (KeyValuePair<ZGuid, ZGuid> pair in mergedAddresses)
			{
				using (var cmd = GetCommandOnMainConnection($@"
UPDATE dbo.JobDeclaration
SET
	JE_OH_Importer = @NewOrgPk,
	JE_OA_ImporterAddress = @NewAddrPk,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JE_OH_Importer = @OldOrgPk AND JE_OA_ImporterAddress = @OldAddrPk;
UPDATE dbo.JobDeclaration
SET
	JE_OH_Supplier = @NewOrgPk,
	JE_OA_SupplierAddress = @NewAddrPk,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JE_OH_Supplier = @OldOrgPk AND JE_OA_SupplierAddress = @OldAddrPk"))
				{
					cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
					cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
					cmd.AddParameter("@NewAddrPk", SqlDbType.UniqueIdentifier, pair.Value.ToGuid());
					cmd.AddParameter("@OldAddrPk", SqlDbType.UniqueIdentifier, pair.Key.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}

			using (var cmd = GetCommandOnMainConnection($@"
UPDATE dbo.JobDeclaration
SET
	JE_OH_Importer = @NewOrgPk,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JE_OH_Importer = @OldOrgPk;
UPDATE dbo.JobDeclaration
SET
	JE_OH_Supplier = @NewOrgPk,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JE_OH_Supplier = @OldOrgPk"))
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeRatingContractNamedAccountPivots

		void MergeRatingContractNamedAccountPivots()
		{
			const string query = @"
;WITH 
	selectParentIDAndParentTableCodeFromOldOrg AS (
		SELECT [RNP_ParentID],[RNP_ParentTableCode] FROM [dbo].[RatingContractNamedAccountPivot] WHERE [RNP_OH_NamedAccount]=@oldOrgPk
			EXCEPT
		SELECT [RNP_ParentID],[RNP_ParentTableCode] FROM [dbo].[RatingContractNamedAccountPivot] WHERE [RNP_OH_NamedAccount]=@newOrgPk
	),
	selectPkColumn AS (
		SELECT [RNP_PK],[RNP_OH_NamedAccount] FROM selectParentIDAndParentTableCodeFromOldOrg AS T1
			INNER JOIN (SELECT [RNP_PK],[RNP_ParentID],[RNP_ParentTableCode],[RNP_OH_NamedAccount] FROM [dbo].[RatingContractNamedAccountPivot] WHERE [RNP_OH_NamedAccount]=@oldOrgPk) AS T2
			ON (T1.[RNP_ParentID]=T2.[RNP_ParentID] AND T1.[RNP_ParentTableCode]=T2.[RNP_ParentTableCode])
	)
UPDATE selectPkColumn
SET [RNP_OH_NamedAccount]=@newOrgPk

DELETE FROM [dbo].[RatingContractNamedAccountPivot] WHERE [RNP_OH_NamedAccount]=@oldOrgPk
";
			using (var cmd = GetCommandOnMainConnection(query))
			{
				cmd.AddParameter("oldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("newOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region MergeAllocationRouteAgentPivots

		void MergeAllocationRouteAgentPivots()
		{
			const string query = @"
;WITH 
	selectAllocationRouteFromOldOrg AS (
		SELECT [ARA_RCA_AllocationLine] FROM [dbo].[AllocationRouteAgentPivot] WHERE [ARA_OH_Agent]=@oldOrgPk
			EXCEPT
		SELECT [ARA_RCA_AllocationLine] FROM [dbo].[AllocationRouteAgentPivot] WHERE [ARA_OH_Agent]=@newOrgPk
	),
	selectPkColumn AS (
		SELECT [ARA_PK],[ARA_OH_Agent] FROM selectAllocationRouteFromOldOrg AS T1
			INNER JOIN (SELECT [ARA_PK],[ARA_RCA_AllocationLine],[ARA_OH_Agent] FROM [dbo].[AllocationRouteAgentPivot] WHERE [ARA_OH_Agent]=@oldOrgPk) AS T2
			ON (T1.[ARA_RCA_AllocationLine]=T2.[ARA_RCA_AllocationLine])
	)
UPDATE selectPkColumn
SET [ARA_OH_Agent]=@newOrgPk

DELETE FROM [dbo].[AllocationRouteAgentPivot] WHERE [ARA_OH_Agent]=@oldOrgPk
";
			using (var cmd = GetCommandOnMainConnection(query))
			{
				cmd.AddParameter("oldOrgPk", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.AddParameter("newOrgPk", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge Org Address Additional Info

		// when merging addresses, move across non duplicate add infos
		void MergeOrgAddressAdditionalInfo()
		{
			DeleteOrgTranslatedAddressAdditionalInfoWhenMerging();

			foreach (KeyValuePair<ZGuid, ZGuid> pair in mergedAddresses)
			{
				var sql = @"
UPDATE A
SET OAI_OA_Address = @NewAddrPK, OAI_IsPrimary = 0
FROM dbo.OrgAddressAdditionalInfo A
WHERE OAI_OA_Address = @OldAddrPk
AND NOT EXISTS (SELECT NULL FROM dbo.OrgAddressAdditionalInfo B WHERE B.OAI_OA_Address = @NewAddrPK AND B.OAI_AdditionalInfo = A.OAI_AdditionalInfo)
";
				using (var cmd = GetCommandOnMainConnection(sql))
				{
					cmd.AddParameter("@NewAddrPk", SqlDbType.UniqueIdentifier, pair.Value.ToGuid());
					cmd.AddParameter("@OldAddrPk", SqlDbType.UniqueIdentifier, pair.Key.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}

		void DeleteOrgTranslatedAddressAdditionalInfoWhenMerging()
		{
			var sql = @"
DELETE A FROM dbo.OrgTranslatedAddressAdditionalInfo A
JOIN dbo.OrgAddressAdditionalInfo ON OAI_PK = OTI_OAI
JOIN @OldAddrPks ON value = OAI_OA_Address
";
			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.AddTableValuedParameter("@OldAddrPks", "dbo.TVP_uniqueidentifier", mergedAddresses.Select(u => u.Key.ToGuid()));
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Merge dbo.OrgCompetitor

		void MergeOrgCompetitors()
		{
			string sqlText = "UPDATE dbo.OrgCompetitor SET OCP_OH_Parent = @NewOrganisationPK WHERE OCP_OH_Parent = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}

			sqlText = "UPDATE dbo.OrgCompetitor SET OCP_OH_Competitor = @NewOrganisationPK WHERE OCP_OH_Competitor = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}

			sqlText = "DELETE FROM dbo.OrgCompetitor WHERE OCP_OH_Parent = @OldOrganisationPK OR OCP_OH_Competitor = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		void MergeOrgRefFacilities()
		{
			string sqlText = @"update dbo.OrgRefFacility
set OFC_OH_Organization = @NewOrganisationPK 
from dbo.OrgRefFacility t1
where OFC_OH_Organization = @OldOrganisationPK
AND
not exists (select 1 from dbo.OrgRefFacility t2 
			where t2.OFC_OH_Organization = @NewOrganisationPK and
			      t2.OFC_RFT_Facility = t1.OFC_RFT_Facility and
				  t2.OFC_OA_PremisesAddress = t1.OFC_OA_PremisesAddress);";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}

			sqlText = "DELETE FROM dbo.OrgRefFacility WHERE OFC_OH_Organization = @OldOrganisationPK";
			using (DbCommand cmd = GetCommandOnMainConnection(sqlText)) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		#region Move OrgGroup Links

		class MergeGlbGroupOrgLink
		{
			public MergeGlbGroupOrgLink(Guid pk, Guid groupPK)
			{
				PK = pk;
				GroupPK = groupPK;
			}

			public Guid PK { get; set; }
			public Guid GroupPK { get; set; }
		}

		List<MergeGlbGroupOrgLink> GetGlbGroupOrgLinks(ZGuid orgPK)
		{
			var result = new List<MergeGlbGroupOrgLink>();
			using (var cmd = GetCommandOnMainConnection((NoResString)"SELECT GOK_PK, GOK_GG_Group FROM dbo.GlbGroupOrgLink WHERE GOK_OH_Org = @OrgPK"))
			{
				cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						result.Add(new MergeGlbGroupOrgLink(rd.GetGuid(0), rd.GetGuid(1)));
					}
				}
			}
			return result;
		}

		void MoveGlbGroupLinks()
		{
			var oldOrgGroupLinks = GetGlbGroupOrgLinks(oldOrganisation);
			var newOrgGroupLinks = GetGlbGroupOrgLinks(newOrganisation);
			foreach (var oldGroupLink in oldOrgGroupLinks)
			{
				if (newOrgGroupLinks.Any(x => x.GroupPK == oldGroupLink.GroupPK))
				{
					//Delete oldGroupLink
					//In contact merge, we'll be transferring all old contact GlbGroupContactLinks over to the new contacts. That'll happen independently of org links
					using (var cmd = GetCommandOnMainConnection(@"
						DELETE FROM dbo.GlbGroupOrgLink WHERE GOK_PK = @OldGroupPK"))
					{
						cmd.AddParameter("@OldGroupPK", SqlDbType.UniqueIdentifier, oldGroupLink.PK);
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					//Update GOK_OH_Org
					using (var cmd = GetCommandOnMainConnection((NoResString)@"
						UPDATE dbo.GlbGroupOrgLink
						SET
							GOK_OH_Org = @NewOrganisationPK,
							GOK_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
							GOK_SystemLastEditUser = @SystemLastEditUser
						WHERE GOK_PK = @OldGroupPK"))
					{
						cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
						cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
						cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
						cmd.AddParameter("@OldGroupPK", SqlDbType.UniqueIdentifier, oldGroupLink.PK);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#endregion

		#region MergeProductionRules

		void MergeProductionRules()
		{
			const string sql = @"
UPDATE 
	dbo.ProductionRule
SET 
	PRL_RuleDefinition = REPLACE(PRL_RuleDefinition, '""' +  CONVERT(VARCHAR(36), @OldOrganisationPK) + '""',  '""' +  CONVERT(VARCHAR(36), @NewOrganisationPK) + '""' ),
	PRL_SystemLastEditTimeUtc = @SystemLastEditTimeUtc,
	PRL_SystemLastEditUser = @SystemLastEditUser
WHERE 
	PRL_RuleDefinition LIKE '%""' + CONVERT(VARCHAR(36), @OldOrganisationPK)+ '""%'
";
			using var cmd = GetCommandOnMainConnection(sql);

			cmd.AddParameter("@NewOrganisationPK", SqlDbType.UniqueIdentifier, newOrganisation.ToGuid());
			cmd.AddParameter("@OldOrganisationPK", SqlDbType.UniqueIdentifier, oldOrganisation.ToGuid());
			cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
			cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, ZDateTime.UtcNow);
			var result = cmd.ExecuteNonQuery();
		}

		#endregion

		#region Implementation

		OrgHeader newOrgHeader;
		OrgHeader NewOrgHeader
		{
			get
			{
				if (newOrgHeader == null)
				{
					if (mergeOrgHeader != null)
					{
						newOrgHeader = mergeOrgHeader.NewOrganisation;
					}
					else
					{
						newOrgHeader = GetNewFactory().Load<OrgHeader>(newOrganisation);
					}
				}
				return newOrgHeader;
			}
		}

		OrgHeader oldOrgHeader;
		OrgHeader OldOrgHeader
		{
			get
			{
				if (oldOrgHeader == null)
				{
					if (mergeOrgHeader != null)
					{
						oldOrgHeader = mergeOrgHeader.OldOrganisation;
					}
					else
					{
						oldOrgHeader = GetNewFactory().Load<OrgHeader>(oldOrganisation);
					}
				}
				return oldOrgHeader;
			}
		}

		DbCommand GetCommandOnMainConnection(string sqlText)
		{
			return GetCommandOnMainConnection(sqlText, Connection);
		}

		static DbCommand GetCommandOnMainConnection(string sqlText, DbConnection connection)
		{
			var cmd = connection.Command(sqlText); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			cmd.CommandTimeout = 900;
			return cmd;
		}

		protected readonly ZGuid oldOrganisation;
		readonly ZGuid newOrganisation;
		readonly MergeOrgAddressCollection mergeAddressCollection;
		protected readonly MergeOrgContactCollection mergeContactsCollection;
		Dictionary<ZGuid, ZGuid> mergedAddresses;
		protected readonly MergeOrgHeader mergeOrgHeader;
		readonly StringCollectionX brandsInitial = new StringCollectionX();
		readonly List<IMergeAction> extraMergeActions = new List<IMergeAction>();

		#endregion
	}
}
