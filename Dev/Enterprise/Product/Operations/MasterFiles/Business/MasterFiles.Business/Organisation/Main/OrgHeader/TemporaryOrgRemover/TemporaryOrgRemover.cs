using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.TemporaryOrgRemover
{
	public class Remover : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Tables that are INDEPENDENT from dbo.OrgHeader

		public static readonly string[] IndependentTables = new string[]
		{
			AccDraftInvoiceHeaderSchema.Constants.TableName,
			AccQueryClaimSchema.Constants.TableName,
			AccTransactionHeaderSchema.Constants.TableName,
			AccTransactionLinesSchema.Constants.TableName,
			AccPaymentApprovalSchema.Constants.TableName,
			AccReceivedChequeSchema.Constants.TableName,
			AccCashAdvanceRequestHeaderSchema.Constants.TableName,
			GlbBranchSchema.Constants.TableName,
			GlbCompanySchema.Constants.TableName,
			GlbPortDeliveryTimeSchema.Constants.TableName,
			JobChargeSchema.Constants.TableName,
			JobHeaderSchema.Constants.TableName,
			JobRequiredDocumentSchema.Constants.TableName,
			JobServiceSchema.Constants.TableName,
			RefCarrierConsortiumSchema.Constants.TableName,
			RefOrgConsortiumPivotSchema.Constants.TableName,
			RefVesselSchema.Constants.TableName,
			RefPacksSchema.Constants.TableName,
			OrgMatchApprovalSchema.Constants.TableName,
			OrgAgentRelationshipSchema.Constants.TableName,
			OrgColdCallRegisterSchema.Constants.TableName,
			OrgDocumentSchema.Constants.TableName,
			OrgMatchApprovalSchema.Constants.TableName,
			OrgPartRelationSchema.Constants.TableName,
			OrgPatternMatchAddressSchema.Constants.TableName,
			OrgProfitShareDetailsSchema.Constants.TableName,
			ProcessTaskTemplateSchema.Constants.TableName,
			ProcessTaskNotificationSchema.Constants.TableName,
			OrgBrandOrRelatedNameSchema.Constants.TableName,
			OrgCustomLabelsSchema.Constants.TableName,
			OrgOpportunitySchema.Constants.TableName,
			OrgCompetitorSchema.Constants.TableName,
			OrgCommissionAgreementSchema.Constants.TableName,
			OrgCommissionAgreementRecipientSchema.Constants.TableName,
			OrgPatternMatchOverrideSchema.Constants.TableName,
			OrgSalesSchema.Constants.TableName,
			OrgTradeDetailSchema.Constants.TableName,
			OrgTradeProspectSchema.Constants.TableName,
			OrgTradePeriodSchema.Constants.TableName,
			OrgTradeValueSchema.Constants.TableName,
			OrgSalesCallSchema.Constants.TableName,
			OrgSecuritySchema.Constants.TableName,
			OrgStaffAssignmentsSchema.Constants.TableName,
			OrgSupBuyLinkTrnModeSchema.Constants.TableName,
			OrgLandedCostingPrefsSchema.Constants.TableName, OrgLandedCostingPrefsSchema.O9_OH.Name,
			PatternMatchingAddressSchema.Constants.TableName, PatternMatchingAddressSchema.PMA_OH.Name,
			PatternMatchingDomainSchema.Constants.TableName, PatternMatchingDomainSchema.PMD_OH.Name,
			PatternMatchingEmailSchema.Constants.TableName, PatternMatchingEmailSchema.PME_OH.Name,
			PatternMatchingNameSchema.Constants.TableName, PatternMatchingNameSchema.PMN_OH.Name,
			PatternMatchingPhoneSchema.Constants.TableName, PatternMatchingPhoneSchema.PMP_OH.Name,
			PatternMatchingRegCodeSchema.Constants.TableName, PatternMatchingRegCodeSchema.PMR_OH.Name,
			StmMenuDocumentConfigSchema.Constants.TableName,
			RefZoneHeaderSchema.Constants.TableName,
			RefContainerStockSchema.Constants.TableName,
			RefEquipmentSchema.Constants.TableName,
			RefExchangeRateSchema.Constants.TableName,
			AccComplianceDocumentHeaderSchema.Constants.TableName,
			AccChargeCodeUniversalCodeMappingSchema.Constants.TableName
		};

		#endregion

		#region Tables that are DEPENDENT from dbo.OrgHeader and OrgAddress

		// Please change TestDependentFields as well
		public static readonly string[,] DependentTables = new string[,]
		{
			{ OrgAddressSchema.Constants.TableName, OrgAddressSchema.OA_OH.Name },
			{ OrgAirlineMAWBStockManagementSchema.Constants.TableName, OrgAirlineMAWBStockManagementSchema.OHM_OH_Carrier.Name },
			{ OrgCompanyDataSchema.Constants.TableName, OrgCompanyDataSchema.OB_OH.Name },
			{ OrgContactSchema.Constants.TableName, OrgContactSchema.OC_OH.Name },
			{ OrgCountryDataSchema.Constants.TableName, OrgCountryDataSchema.OV_OH_OrgHeader.Name },
			{ OrgCusCodeSchema.Constants.TableName, OrgCusCodeSchema.OK_OH.Name },
			{ OrgCusAccountSchema.Constants.TableName, OrgCusAccountSchema.CZ_OH.Name },
			{ OrgCustomLabelsSchema.Constants.TableName, OrgCustomLabelsSchema.OT_OH.Name },
			{ OrgMiscServSchema.Constants.TableName, OrgMiscServSchema.OM_OH.Name },
			{ OrgPatternMatchSchema.Constants.TableName, OrgPatternMatchSchema.OS_OH.Name },
			{ OrgRateTariffLevelSchema.Constants.TableName, OrgRateTariffLevelSchema.P7_OH.Name },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, OrgSupplierBuyerLinkSchema.OL_OH_Buyer.Name },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, OrgSupplierBuyerLinkSchema.OL_OH_Supplier.Name },
			{ OrgServiceLevelSchema.Constants.TableName, OrgServiceLevelSchema.PM_OH.Name },
			{ OrgAppointedAgentPortsSchema.Constants.TableName, OrgAppointedAgentPortsSchema.O5_OH.Name },
			{ OrgContainerDetentionSchema.Constants.TableName, OrgContainerDetentionSchema.PD_OH_Carrier.Name },
			{ OrgContainerDetentionSchema.Constants.TableName, OrgContainerDetentionSchema.PD_OH_Client.Name },
			{ OrgRelatedPartySchema.Constants.TableName, OrgRelatedPartySchema.PR_OH_Parent.Name },
			{ OrgRateCommodityDefaultingRuleSchema.Constants.TableName, OrgRateCommodityDefaultingRuleSchema.ORC_OH.Name },
			{ OrgContainerDetentionSchema.Constants.TableName, OrgContainerDetentionSchema.PD_OH_Client.Name },
			{ EDICommunicationsModeSchema.Constants.TableName, EDICommunicationsModeSchema.EK_OH_MessageVAN.Name },
			{ AccClientInvoiceOrderSchema.Constants.TableName, AccClientInvoiceOrderSchema.AI_OH_Client.Name },
			{ OrgWebURLSchema.Constants.TableName, OrgWebURLSchema.PU_OH.Name },
			{ OrgPatternMatchSchema.Constants.TableName, OrgPatternMatchSchema.OS_OA.Name },
			{ OrgPatternMatchOverrideSchema.Constants.TableName, OrgPatternMatchOverrideSchema.OO_OH.Name },
			{ OrgPatternMatchOverrideSchema.Constants.TableName, OrgPatternMatchOverrideSchema.OO_LocalGuid.Name },
			{ OrgAddressCapabilitySchema.Constants.TableName, OrgAddressCapabilitySchema.PZ_OA.Name },
			{ OrgCountryDataSchema.Constants.TableName, OrgCountryDataSchema.OV_OA_ApprovedLocation.Name },
			{ OrgRateFeeChargeLevelSchema.Constants.TableName, OrgRateFeeChargeLevelSchema.ORF_OH.Name },
			{ OrgCarrierAccountSchema.Constants.TableName, OrgCarrierAccountSchema.OAN_OH_Carrier.Name },
			{ OrgCarrierNamedAccountSchema.Constants.TableName, OrgCarrierNamedAccountSchema.ONA_OH_Carrier.Name },
			{ OrgCarrierNamedAccountSchema.Constants.TableName, OrgCarrierNamedAccountSchema.ONA_OH_Organization.Name },
			{ OrgWhsClientAccountAssociationSchema.Constants.TableName, OrgWhsClientAccountAssociationSchema.OWC_OH_Client.Name },
			{ PatternMatchingAddressSchema.Constants.TableName, PatternMatchingAddressSchema.PMA_OH.Name },
			{ PatternMatchingEmailSchema.Constants.TableName, PatternMatchingEmailSchema.PME_OH.Name },
			{ PatternMatchingDomainSchema.Constants.TableName, PatternMatchingDomainSchema.PMD_OH.Name },
			{ PatternMatchingNameSchema.Constants.TableName, PatternMatchingNameSchema.PMN_OH.Name },
			{ PatternMatchingPhoneSchema.Constants.TableName, PatternMatchingPhoneSchema.PMP_OH.Name },
			{ PatternMatchingRegCodeSchema.Constants.TableName, PatternMatchingRegCodeSchema.PMR_OH.Name },
			{ StmNumberRangeMatchingDetailSchema.Constants.TableName, StmNumberRangeMatchingDetailSchema.NRM_OH_Client.Name },
			{ JobDocumentDeliverySchema.Constants.TableName, JobDocumentDeliverySchema.JDC_OH.Name },
			{ AccChargeCreditorOverrideSchema.Constants.TableName, AccChargeCreditorOverrideSchema.ACC_OH_Creditor.Name },
			{ OrgAirlineBranchAccountSchema.Constants.TableName, OrgAirlineBranchAccountSchema.OAA_OH_Carrier.Name },
			{ AccChargeCodeCarrierIataMappingSchema.Constants.TableName, AccChargeCodeCarrierIataMappingSchema.ACI_OH_Carrier.Name },
			{ RatingDocumentsChargeOrderSchema.Constants.TableName, RatingDocumentsChargeOrderSchema.RCO_OH_Client.Name },
			{ OrgRefFacilitySchema.Constants.TableName, OrgRefFacilitySchema.OFC_OH_Organization.Name },
			{ GlbGroupOrgLinkSchema.Constants.TableName, GlbGroupOrgLinkSchema.GOK_OH_Org.Name },
			{ ExternalRequestSchema.Constants.TableName, ExternalRequestSchema.REQ_OH_AssignedOrganization.Name },
			{ ExternalRequestSchema.Constants.TableName, ExternalRequestSchema.REQ_OH_ReviewerOrganization.Name },
		};

		#endregion

		public Remover()
			: base(new BusinessObjectFactory())
		{
		}

		public delegate bool ProgressChangedEventHandler(int percentComplete);
		public event ProgressChangedEventHandler ProgressChanged;

		#region DeleteTemporaryOrgs

		public void DeleteTemporaryOrgs()
		{
			int totalOrgCount = Organisations.Count;
			for (int currentOrg = 0; currentOrg < totalOrgCount; currentOrg++)
			{
				if (Organisations[currentOrg].IncludeInDelete)
				{
					DeleteOrganisation(Organisations[currentOrg]);
				}

				if (!OnProgressChanged(currentOrg * 100 / totalOrgCount))
				{
					break;
				}
			}
		}

		public void BulkDeleteTemporaryOrgs()
		{
			int totalOrgCount = Organisations.Count;
			for (int currentOrg = 0; currentOrg < totalOrgCount; currentOrg++)
			{
				DeleteOrganisation(Organisations[currentOrg]);

				if (!OnProgressChanged(currentOrg * 100 / totalOrgCount))
				{
					break;
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		void DeleteOrganisation(TemporaryOrg orgToDelete)
		{
			using (var transactionManager = WorkConnection.BeginTransactionWithManager())
			{
				try
				{
					var sql = FormattableString.Invariant($@"
						DELETE {OrgSupplierBuyerLinkSchema.Constants.SqlSchemaName}.{OrgSupplierBuyerLinkSchema.Constants.TableName}
						WHERE {OrgSupplierBuyerLinkSchema.Constants.OL_OH_Buyer} = @pk
							OR {OrgSupplierBuyerLinkSchema.Constants.OL_OH_Supplier} = @pk;

						DELETE {OrgInvoiceRollupOrGroupSchema.Constants.SqlSchemaName}.{OrgInvoiceRollupOrGroupSchema.Constants.TableName}
						WHERE {OrgInvoiceRollupOrGroupSchema.Constants.PG_OB}
							IN (
								SELECT {OrgCompanyDataSchema.Constants.PK}
								FROM {OrgCompanyDataSchema.Constants.SqlSchemaName}.{OrgCompanyDataSchema.Constants.TableName}
								JOIN {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
								ON {OrgCompanyDataSchema.Constants.OB_OH} = {OrgHeaderSchema.PK.Name}
								WHERE {OrgHeaderSchema.Constants.OH_Code} = @orgCode);

						DELETE {OrgCusCodeSchema.Constants.SqlSchemaName}.{OrgCusCodeSchema.Constants.TableName}
						WHERE {OrgCusCodeSchema.Constants.OK_OH} = @pk;

						DELETE {OrgCusAccountSchema.Constants.SqlSchemaName}.{OrgCusAccountSchema.Constants.TableName}
						WHERE {OrgCusAccountSchema.Constants.CZ_OH} = @pk;

						DELETE {OrgPatternMatchOverrideSchema.Constants.SqlSchemaName}.{OrgPatternMatchOverrideSchema.Constants.TableName}
						WHERE {OrgPatternMatchOverrideSchema.Constants.OO_Relationship} = '{Core.Constants.OrgPatternMatchOverrideRelationships.Organisation}'
							AND	({OrgPatternMatchOverrideSchema.Constants.OO_OH} = @pk
								OR {OrgPatternMatchOverrideSchema.Constants.OO_LocalGuid} = @pk);

						DELETE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
						WHERE {OrgContactSchema.Constants.OC_OH} = @pk;

						DELETE {OrgRelatedPartySchema.Constants.SqlSchemaName}.{OrgRelatedPartySchema.Constants.TableName}
						WHERE {OrgRelatedPartySchema.Constants.PR_OH_Parent} = @pk;

						DELETE {PatternMatchingAddressSchema.Constants.SqlSchemaName}.{PatternMatchingAddressSchema.Constants.TableName}
						WHERE {PatternMatchingAddressSchema.Constants.PMA_OH} = @pk;

						DELETE {PatternMatchingEmailSchema.Constants.SqlSchemaName}.{PatternMatchingEmailSchema.Constants.TableName}
						WHERE {PatternMatchingEmailSchema.Constants.PME_OH} = @pk;

						DELETE {PatternMatchingDomainSchema.Constants.SqlSchemaName}.{PatternMatchingDomainSchema.Constants.TableName}
						WHERE {PatternMatchingDomainSchema.Constants.PMD_OH} = @pk;

						DELETE {PatternMatchingNameSchema.Constants.SqlSchemaName}.{PatternMatchingNameSchema.Constants.TableName}
						WHERE {PatternMatchingNameSchema.Constants.PMN_OH} = @pk;

						DELETE {PatternMatchingPhoneSchema.Constants.SqlSchemaName}.{PatternMatchingPhoneSchema.Constants.TableName}
						WHERE {PatternMatchingPhoneSchema.Constants.PMP_OH} = @pk;

						DELETE {PatternMatchingRegCodeSchema.Constants.SqlSchemaName}.{PatternMatchingRegCodeSchema.Constants.TableName}
						WHERE {PatternMatchingRegCodeSchema.Constants.PMR_OH} = @pk;

						Delete {AccChargeCreditorOverrideSchema.Constants.SqlSchemaName}.{AccChargeCreditorOverrideSchema.Constants.TableName}
						WHERE {AccChargeCreditorOverrideSchema.Constants.ACC_OH_Creditor} = @pk;

						DELETE {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
						WHERE {OrgHeaderSchema.Constants.OH_Code} = @orgCode;");

					var command = WorkConnection.Command(sql);
					command.AddParameter("@pk", OrgHeaderSchema.PK.SqlDbType, OrgHeaderSchema.PK.MaxLength, orgToDelete.OrgPK.ToGuid());
					command.AddParameter("@orgCode", OrgHeaderSchema.OH_Code.SqlDbType, OrgHeaderSchema.OH_Code.MaxLength, orgToDelete.Code.ToString());
					command.ExecuteNonQuery();

					transactionManager.CommitTransaction();
					++OrgsDeletedCount;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					transactionManager.RollbackTransaction();
					AddOrgToFailedList(e.Message, orgToDelete);
					AnyOrgsFailedToDelete = true;
				}
			}
		}

		#endregion

		bool OnProgressChanged(int percentComplete)
		{
			if (ProgressChanged != null)
			{
				return ProgressChanged(percentComplete);
			}

			return true;
		}

		void AddOrgToFailedList(string exceptionMessage, TemporaryOrg orgToDelete)
		{
			string[] parentAndChild = MetaData.GetParentAndChildTablesFromFK(WorkConnection, MetaData.GetFKNameFromErrorMessage(exceptionMessage));
			orgToDelete.ChildTableForFailedDelete = DataBoundResourceStrings.GetTableDescriptiveName(parentAndChild[1]);
			FailedOrganisations.Add(orgToDelete);
		}

		public bool AnyOrgsFailedToDelete;
		public TemporaryOrgCollection FailedOrganisations
		{
			get
			{
				if (failedOrganisations == null)
				{
					failedOrganisations = new TemporaryOrgCollection(Factory);
				}

				return failedOrganisations;
			}
		}
		TemporaryOrgCollection failedOrganisations;

		#endregion

		#region Organisations

		public virtual TemporaryOrgCollection Organisations
		{
			get
			{
				if (organisations == null)
				{
					organisations = new TemporaryOrgCollection(Factory);
					organisations.Load();
				}

				return organisations;
			}
		}

		TemporaryOrgCollection organisations;

		public int OrgsDeletedCount { get; private set; }

		#endregion

		#region DependentFields

		static internal string DependentFields
		{
			get
			{
				string result = "";

				for (int i = 0; i < DependentTables.GetLength(0); i++)
				{
					result += DependentTables[i, 1];
					if (i < DependentTables.GetLength(0) - 1)
					{
						result += ", ";
					}
				}

				return result;
			}
		}

		#endregion

		DbConnection WorkConnection
		{
			get { return Db.Connection; }  // Temp Org Remover has to bypass BO layer in order to be fast.
		}

		// Used in BusinessObjectTestCase using Reflection
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in BusinessObjectTestCase using Reflection")]
		static bool IsTableInTestData(string tableName, ArrayList fields)
		{
			if (((IList)Remover.IndependentTables).Contains(tableName))
			{
				return true;
			}

			for (int i = 0; i < Remover.DependentTables.GetLength(0); i++)
			{
				if (Remover.DependentTables[i, 0] == tableName && fields.Contains(Remover.DependentTables[i, 1]))
				{
					return true;
				}
			}

			return false;
		}
	}
}
