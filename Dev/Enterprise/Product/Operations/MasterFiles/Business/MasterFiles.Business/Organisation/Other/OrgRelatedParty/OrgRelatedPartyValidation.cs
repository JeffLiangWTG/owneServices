using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using IOrgImpAddInfo = Enterprise.Integration.Customs.CA.IOrgImpAddInfo;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRelatedPartyValidation : AutoOrgRelatedPartyValidation
	{
		public OrgRelatedPartyValidation(AutoOrgRelatedParty parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePartyTypeDescription();
			ValidateCompanyLevel();
		}

		#region ValidateIsInCollectionAlready

		void ValidateIsInCollectionAlready(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && !AllowMultipleRelatedPartyTypeList.Contains(Parent.PR_PartyType))
			{
				var query = new ZQuery(OrgRelatedPartySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, Parent.PR_OH_Parent);
				query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, Parent.PR_PartyType);
				query.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, Parent.PR_FreightDirection);
				query.AddToFilter(OrgRelatedPartySchema.PR_FreightTransportMode, Parent.PR_FreightTransportMode);
				query.AddToFilter(OrgRelatedPartySchema.PR_FreightContainerMode, Parent.PR_FreightContainerMode);
				query.AddToFilter(OrgRelatedPartySchema.PR_Location, Parent.PR_Location);

				var isICS = Parent.PR_PartyType == RelatedPartyTypeList.Codes.SelfFilerForICS2;
				if (isICS)
				{
					query.AddToFilter(OrgRelatedPartySchema.PR_RN_NKImporterCountry, Parent.PR_RN_NKImporterCountry);
				}

				var isFCW = Parent.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
				if (isFCW)
				{
					query.AddToFilter(OrgRelatedPartySchema.PR_RN_NKImporterCountry, Parent.PR_RN_NKImporterCountry);
				}

				if (Parent.Parent != null && !Parent.Parent.IsInDatabase)
				{
					query.FetchOnlyFromLocalCache = true;
				}

				var isNFP = Parent.PR_PartyType == RelatedPartyTypeList.Codes.NotifyParty;

				if (!isNFP)
				{
					if (Parent.Factory.Load<OrgRelatedParty>(query).Any(relatedParty => relatedParty.PR_GC == Parent.PR_GC && relatedParty.PR_OA == Parent.PR_OA)) // Empty ZGuid doesn't work well ZQuery with local data
					{
						if (isICS)
						{
							propertyInfo.AddError(Res.GetString("086932D8-1079-4F25-BD35-CE363AB59134",
								"Only one Self-Filer is allowed for the same Transport Mode, Location and Import Country/Region fields combination."));
						}
						else if (isFCW)
						{
							propertyInfo.AddError(Res.GetString("2165c274-c6bc-495e-97b8-55903422357f",
								"There is already a party with the same type, address, direction, mode, company level, UNLOCO and import country. You can only specify a single related organization for this combination."));
						}
						else
						{
							propertyInfo.AddError(Res.GetString("0dad3522-21c2-4ee6-9ab7-6097385f1303",
								"There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination."));
						}
					}
				}
				else
				{
					if (Parent.Factory.Load<OrgRelatedParty>(query).Any(relatedParty => relatedParty.PR_GC == Parent.PR_GC)) // Empty ZGuid doesn't work well ZQuery with local data
					{
						propertyInfo.AddError(Res.GetString("b1d35fc2-d51b-4df1-9c26-48fccd1decbe",
								"When Party Type is NFP, 'direction, transport mode and container mode' must be unique. You can only specify a single related organization for this combination."));
					}
				}
			}
		}

		void ValidateIsRelatedPartyInCollectionAlready(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && AllowMultipleRelatedPartyTypeList.Contains(Parent.PR_PartyType))
			{
				var query = new ZQuery(OrgRelatedPartySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, Parent.PR_OH_Parent);
				query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, Parent.PR_PartyType);
				query.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, Parent.PR_FreightDirection);
				query.AddToFilter(OrgRelatedPartySchema.PR_FreightTransportMode, Parent.PR_FreightTransportMode);
				query.AddToFilter(OrgRelatedPartySchema.PR_FreightContainerMode, Parent.PR_FreightContainerMode);
				query.AddToFilter(OrgRelatedPartySchema.PR_Location, Parent.PR_Location);
				query.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, Parent.PR_OH_RelatedParty);

				if (Parent.Parent != null && !Parent.Parent.IsInDatabase)
				{
					query.FetchOnlyFromLocalCache = true;
				}

				if (Parent.Factory.Load<OrgRelatedParty>(query).Any(relatedParty => relatedParty.PR_GC == Parent.PR_GC && relatedParty.PR_OA == Parent.PR_OA)) // Empty ZGuid doesn't work well ZQuery with local data
				{
					propertyInfo.AddError(Res.GetString("1af55f07-1ca0-4222-b12f-d5ec4edc85e3",
						"There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination."));
				}
			}
		}

		readonly ZString[] AllowMultipleRelatedPartyTypeList = new ZString[]
		{
			RelatedPartyTypeList.Codes.CSAApprovedVendor,
			RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee,
			RelatedPartyTypeList.Codes.ServiceProvider,
			RelatedPartyTypeList.Codes.Manufacturer
		};

		#endregion

		#region CheckCompanyLevel

		public void ValidateCompanyLevel()
		{
			ValidateCalculatedProperty(RelatedPartyRecord.CompanyLevelInfo);
		}

		protected virtual void CheckCompanyLevel()
		{
			if (RelatedPartyRecord.IsCompanySpecific && RelatedPartyRecord.PR_GC.IsEmpty)
			{
				RelatedPartyRecord.CompanyLevelInfo.AddError(Res.GetString("acb5ddb8-fd9d-4545-a26b-ee113b63b51b", "The Company Level must be 'COM' for this type of party."));
			}

			if (RelatedPartyRecord.IsEnterpriseLevelOnly && !RelatedPartyRecord.PR_GC.IsEmpty)
			{
				RelatedPartyRecord.CompanyLevelInfo.AddError(Res.GetString("045fde88-2c2b-4880-a9fa-36c20c2d4d69", "The Company Level must be 'ENT' for this type of party."));
			}

			ValidateIsInCollectionAlready(RelatedPartyRecord.CompanyLevelInfo);
		}

		#endregion

		#region CheckPR_OA

		protected override void CheckPR_OA()
		{
			base.CheckPR_OA();
			var relatedParty = RelatedPartyRecord;
			if (relatedParty.PR_PartyType == RelatedPartyTypeList.Codes.LocalTransport && relatedParty.PR_OA.IsEmpty)
			{
				relatedParty.PR_OAInfo.AddWarning(Res.GetString("1372122b-9369-4142-9a87-2676497a7b59", "Please enter the appropriate 'For Address'."));
			}
			else if (relatedParty.PR_PartyType == RelatedPartyTypeList.Codes.Warehouse && relatedParty.PR_OA.IsEmpty)
			{
				relatedParty.PR_OAInfo.AddError(Res.GetString("2af433fc-8174-4056-9cc3-dcded7b0e147", "Enter a Warehouse Address."));
			}
			else if (relatedParty.PR_PartyType == RelatedPartyTypeList.Codes.NationalDistributionCentre && relatedParty.PR_OA.IsEmpty)
			{
				relatedParty.PR_OAInfo.AddError(Res.GetString("b57f0049-f3e9-4fdf-845f-24b655082577", "Enter a 'For Address' which National Distribution Center is related to."));
			}
		}

		#endregion

		#region CheckPartyTypeDescription

		public void ValidatePartyTypeDescription()
		{
			ValidateCalculatedProperty(RelatedPartyRecord.PartyTypeDescriptionInfo);
		}

		protected void CheckPartyTypeDescription()
		{
			if (RelatedPartyRecord.PR_PartyType.IsEmpty)
			{
				RelatedPartyRecord.PartyTypeDescriptionInfo.AddError(Res.GetString("addca9da-7f8b-4b97-b45f-34a5bdc76977", "Please enter a valid value."));
			}
			else
			{
				RelatedPartyRecord.PartyTypeDescriptionInfo.AddAllNotificationsFrom(RelatedPartyRecord.PR_PartyTypeInfo);
			}
		}

		#endregion

		#region PR_FreightTransportMode

		protected override void CheckPR_FreightTransportMode()
		{
			base.CheckPR_FreightTransportMode();
			if (RelatedPartyRecord.ShouldHaveMode && RelatedPartyRecord.PR_FreightTransportMode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(RelatedPartyRecord.PR_FreightTransportModeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(RelatedPartyRecord.PR_FreightTransportModeInfo);
			}
			ValidateIsInCollectionAlready(RelatedPartyRecord.PR_FreightTransportModeInfo);
		}

		#endregion

		#region PR_GC

		protected override void CheckPR_GC()
		{
			base.CheckPR_GC();

			if (RelatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor)
			{
				ValidatePR_OH_RelatedParty();
			}
		}

		#endregion

		#region PR_FreightContainerMode

		protected override void CheckPR_FreightContainerMode()
		{
			base.CheckPR_FreightContainerMode();
			ListValidation.ErrorIfInvalidCode(RelatedPartyRecord.PR_FreightContainerModeInfo);

			if (RelatedPartyRecord.PR_PartyType != RelatedPartyTypeList.Codes.SelfFilerForICS2)
			{
				ValidateIsInCollectionAlready(RelatedPartyRecord.PR_FreightContainerModeInfo);
			}
		}

		#endregion

		#region CheckPR_PartyType

		protected override void CheckPR_PartyType()
		{
			base.CheckPR_PartyType();
			MandatoryValidation.CheckEntered(RelatedPartyRecord.PR_PartyTypeInfo);
			if (!RelatedPartyRecord.PR_PartyTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(RelatedPartyRecord.PR_PartyTypeInfo);
			}
			ValidateIsInCollectionAlready(RelatedPartyRecord.PR_PartyTypeInfo);
			CheckOnlyOneWarehouseRelatedPartyExists();
			CheckParentIsAProductChild();

			if ((!RelatedPartyRecord.IsInDatabase || RelatedPartyRecord.HasChanges) && !RelatedPartyRecord.IsAlrightToAddOrDelete)
			{
				RelatedPartyRecord.PR_PartyTypeInfo.AddError(Res.GetString("66d8e183-2b7e-4e36-8cd3-95fb6d563b15", "New Related Party of this type is not allowed based on your current security rights."));
			}

			var relatedOrg = RelatedPartyRecord.RelatedParty;
			if (relatedOrg != null && !relatedOrg.OH_IsBroker && RelatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.CustomsAgentBroker)
			{
				RelatedPartyRecord.PR_PartyTypeInfo.AddError(Res.GetString("4FB5CD9A-BE3E-4074-BD02-324DFACF7CDB", "CAB type cannot be chosen for non broker organizations"));
			}

			var parentOrg = RelatedPartyRecord.ParentOrg;
			var impAddInfo = parentOrg != null ? parentOrg.GetCountryData(Core.Constants.CountryCodes.Canada).ImpAddInfo as IOrgImpAddInfo : null;
			if (impAddInfo != null && !impAddInfo.IsCSAApprovedImporter && (RelatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee || RelatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.CSAApprovedVendor))
			{
				RelatedPartyRecord.PR_PartyTypeInfo.AddError(Res.GetString("08f9cfeb-d7d4-4ef4-ab8e-d400da384f94", "CAU or CAV type cannot be chosen for non CSA Approved Importer organizations. \r\nPlease check the 'CSA Approved Importer' at 'Organization->Details->Config->Canada->CSA'."));
			}

			if (parentOrg != null && !RelatedPartyRecord.PR_PartyTypeInfo.HasErrors() && (!RelatedPartyRecord.IsInDatabase || RelatedPartyRecord.HasChanges))
			{
				if (parentOrg.ConsignorRelatedParties.Contains(RelatedPartyRecord) && !parentOrg.ConsignorRelatedParties.IsThisPartOfTheCollectionInternal(RelatedPartyRecord))
				{
					RelatedPartyRecord.PR_PartyTypeInfo.AddError(Res.GetString("4ff1e780-deb2-405f-96e5-fdd714fb1449", "The selected Party Type cannot be added as a Consignor Related Party."));
				}
				else if (parentOrg.ConsigneeRelatedParties.Contains(RelatedPartyRecord) && !parentOrg.ConsigneeRelatedParties.IsThisPartOfTheCollectionInternal(RelatedPartyRecord))
				{
					RelatedPartyRecord.PR_PartyTypeInfo.AddError(Res.GetString("ad68847e-9cea-4f8c-b4ad-b4c206e98e1f", "The selected Party Type cannot be added as a Consignee Related Party."));
				}
			}
		}

		void CheckParentIsAProductChild()
		{
			if (!RelatedPartyRecord.PR_PartyTypeInfo.HasErrors() && Parent.PR_PartyType == RelatedPartyTypeList.Codes.ProductRelationship)
			{
				if (HasAProductParent(Parent.PR_OH_Parent))
				{
					Parent.PR_PartyTypeInfo.AddError(Res.GetString("d54b24d1-7a7d-4fb9-9ad4-87eb966311f7", "{0} has a Product Parent, it cannot have Product children", RelatedPartyRecord.ParentOrg.OH_Code));
				}
			}
		}

		void CheckOnlyOneWarehouseRelatedPartyExists()
		{
			if (!RelatedPartyRecord.PR_PartyTypeInfo.HasErrors() && Parent.PR_PartyType == RelatedPartyTypeList.Codes.Warehouse)
			{
				var query = new ZQuery();
				query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, Parent.PR_OH_Parent);
				query.AddToFilter(OrgRelatedPartySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.Warehouse);

				bool anotherWarehouseRelatedPartyExists = Parent.Factory.LoadTop1<OrgRelatedParty>(query) != null;
				if (anotherWarehouseRelatedPartyExists)
				{
					Parent.PR_PartyTypeInfo.AddError(Res.GetString("98ee8328-214e-4f7b-a5e5-cdbbdf02842b", "There can only be one Warehouse Related Party."));
				}
			}
		}

		#endregion

		#region CheckPR_OH_RelatedParty

		protected override void CheckPR_OH_RelatedParty()
		{
			base.CheckPR_OH_RelatedParty();
			var relatedPartyRecord = RelatedPartyRecord;
			var parentOrg = relatedPartyRecord.ParentOrg;
			var relatedOrg = relatedPartyRecord.Factory.Load<OrgHeader>(relatedPartyRecord.PR_OH_RelatedParty);

			if (relatedOrg != null && relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ProductRelationship)
			{
				if (AnyRelatedPartyIsAProductParent())
				{
					relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("65adff33-3e3b-4a67-ba1b-b40c3ce8c1ba", "Related Part is a Product Parent, please select an organization that is not already a Parent"));
				}
				else if (HasAProductParent(relatedOrg.PK))
				{
					relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("df7365e2-cb99-456d-b971-000aae47d3ef", "Organization already has a Product Parent, please select an organization that does not have a product parent"));
				}

				bool AnyRelatedPartyIsAProductParent()
				{
					if (relatedOrg.PK == relatedPartyRecord.PR_OH_Parent)
					{
						return true;
					}

					var query = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.Equal, relatedOrg.PK);
					query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, SQLComparisonOperator.Equal, RelatedPartyTypeList.Codes.ProductRelationship);
					return relatedPartyRecord.Factory.LoadTop1<OrgRelatedParty>(query) != null;
				}
			}

			if (relatedOrg != null && !relatedOrg.OH_IsBroker && relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.CustomsAgentBroker)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(
					Res.GetString("b56298d9-da98-416f-aebd-7e0a32687419", "Party Type: NON Broker Organizations cannot be selected for CAB Type"));
			}
			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.NationalDistributionCentre
				&& relatedOrg != null && !(relatedOrg.OH_IsMiscFreightServices
				&& relatedOrg.OH_IsDistributionCentre))
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("07d58cad-649d-43ef-8132-c0898d0bacbd", "Enter a Distribution Center."));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.AccountingVATGSTGroup &&
				relatedOrg != null && relatedOrg.ClosestPort != null && relatedPartyRecord.ParentOrg != null && relatedPartyRecord.ParentOrg.ClosestPort != null &&
				relatedPartyRecord.ParentOrg.ClosestPort.RL_RN_NKCountryCode != relatedOrg.ClosestPort.RL_RN_NKCountryCode)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(
					Res.GetString("c37fb372-e827-4ec5-be71-a86e95fba8f8", "The AR/AP Organization must have a UNLOCO from the SAME country/region as the Related Party"));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.AccountingVATGSTGroup &&
				relatedPartyRecord.PR_OH_RelatedParty.IsValid && !OrgRelatedParty.IsOrgProxy(relatedPartyRecord.PR_OH_RelatedParty))
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(
					Res.GetString("3c8606fa-d756-4f34-ac11-94e1dd7d12d7", "The Related Party must be an Organization Proxy of the current Login Company"));
			}

			if (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value
				&& relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ControllingAgent
				&& relatedOrg != null
				&& !(relatedOrg.OH_IsControllingAgent && relatedOrg.OH_IsForwarder))
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(
					Res.GetString("1e6ebccf-967f-4615-8a61-2f26660e5cf2", "Only an organization flagged as Controlling Agent and Forwarder/Agent can be used as a Controlling Agent."));
			}

			if (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ControllingCustomer
				&& relatedOrg != null
				&& !relatedOrg.OH_IsControllingCustomer)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("a2ee4928-8430-4b88-95bf-dba77fa2993f", "Only an organization flagged as Controlling Customer can be used as a Controlling Customer."));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo
				&& relatedOrg != null
				&& !relatedOrg.OH_IsDebtor)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("5585067F-C817-4A4A-A398-A834C2CB24D4", "Only an organization flagged as Receivable can be used as an {0}.", RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo
				&& relatedOrg != null
				&& !relatedOrg.OH_IsDebtor)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("11E3C33A-9DEE-4990-9F95-13E9F8BA2CD4", "Only an organization flagged as Receivable can be used as an {0}.", RelatedPartyTypeList.Descriptions.InvoiceCustomsJobsTo));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceFreightJobsTo
				&& relatedOrg != null
				&& !relatedOrg.OH_IsDebtor)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("EE2C7112-5C15-4E26-8F1B-4315D9846F7E", "Only an organization flagged as Receivable can be used as an {0}.", RelatedPartyTypeList.Descriptions.InvoiceFreightJobsTo));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.LocalTransport
				&& relatedOrg != null
				&& (!relatedOrg.OH_IsShippingProvider || !relatedOrg.OH_IsLocalTransport))
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("3CCCC301-08B6-484D-9240-48DDBC2FE462", "Only an organization set up as a Carrier and flagged as Road Transport can be used as a {0}.", RelatedPartyTypeList.Descriptions.LocalTransport));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor
				&& relatedPartyRecord.CompanyLevel == CompanyLevelList.Codes.COM
				&& relatedOrg != null
				&& !relatedOrg.OH_IsCreditor)
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("7AED9709-6F5B-4C2F-9180-49A98F792187", "Only an organization flagged as Payable can be used as an {0}.", RelatedPartyTypeList.Descriptions.ServiceProviderCreditor));
			}

			if (relatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ControllingAgent
				&& (!relatedPartyRecord.IsInDatabase || relatedPartyRecord.PR_PartyTypeInfo.HasChanges || relatedPartyRecord.PR_OH_RelatedPartyInfo.HasChanges)
				&& parentOrg != null
				&& !parentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity
				&& parentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity
				&& !OrgRelatedParty.IsOrgProxy(relatedPartyRecord.PR_OH_RelatedParty))
			{
				relatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("d5355167-1beb-4d2e-aa45-dba122f69947", "You can only set an Organization Proxy of the current Login Company as a Controlling Agent, based on your current security rights."));
			}

			if (RelatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ManagementGrouping && RelatedPartyRecord.RelatedParty == RelatedPartyRecord.Parent)
			{
				RelatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(Res.GetString("608771B5-0B39-420A-A0CB-947129A47776", "You cannot have this organization be a party of itself for this type of party."));
			}
			else if (RelatedPartyRecord.PR_PartyType == RelatedPartyTypeList.Codes.ManagementGrouping && RelatedPartyRecord.PR_GC.IsEmpty && RelatedPartyRecord.HasChanges)
			{
				var relatedParty = RelatedPartyRecord.RelatedParty;
				if (parentOrg != null && relatedParty != null)
				{
					var parentValidationResult = parentOrg.RelatedManagementParentRelations.CheckIsValidOrganisation(relatedParty, true);
					if (!parentValidationResult.IsValid)
					{
						RelatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(GetManagementGroupingErrorMessage(parentValidationResult));
					}
					else
					{
						var subsidiaryValidationResult = relatedParty.RelatedManagementSubsidiaryRelations.CheckIsValidOrganisation(parentOrg, false);
						if (!subsidiaryValidationResult.IsValid)
						{
							RelatedPartyRecord.PR_OH_RelatedPartyInfo.AddError(GetManagementGroupingErrorMessage(subsidiaryValidationResult));
						}
					}
				}
			}
			if (relatedOrg != null)
			{
				ValidateIsRelatedPartyInCollectionAlready(RelatedPartyRecord.PR_OH_RelatedPartyInfo);
			}
		}

		bool HasAProductParent(ZGuid orgPK)
		{
			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, SQLComparisonOperator.Equal, orgPK);
			query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, SQLComparisonOperator.Equal, RelatedPartyTypeList.Codes.ProductRelationship);
			query.AddToFilter(OrgRelatedPartySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return Parent.Factory.LoadTop1<OrgRelatedParty>(query) != null;
		}

		static string GetManagementGroupingErrorMessage(RelationValidationResult validationResult)
		{
			return Res.GetString("2b99e255-9bd7-4e7a-9541-8a19886ce215", "{0} Error: {1}", RelatedPartyTypeList.Descriptions.ManagementGrouping, validationResult.Reason);
		}

		#endregion

		#region CheckPR_FreightDirection

		protected override void CheckPR_FreightDirection()
		{
			base.CheckPR_FreightDirection();
			if (RelatedPartyRecord.ShouldHaveDirection
				&& RelatedPartyRecord.PR_PartyType != RelatedPartyTypeList.Codes.ServiceProvider
				&& RelatedPartyRecord.PR_PartyType != RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo)
			{
				MandatoryValidation.CheckEntered(RelatedPartyRecord.PR_FreightDirectionInfo);
			}
			if (!RelatedPartyRecord.ShouldCalculateDirection)
			{
				ListValidation.ErrorIfInvalidCode(RelatedPartyRecord.PR_FreightDirectionInfo);
			}

			if (RelatedPartyRecord.PR_PartyType != RelatedPartyTypeList.Codes.SelfFilerForICS2)
			{
				ValidateIsInCollectionAlready(RelatedPartyRecord.PR_FreightDirectionInfo);
			}
		}

		#endregion

		#region CheckPR_Location

		protected override void CheckPR_Location()
		{
			base.CheckPR_Location();
			ListValidation.ErrorIfInvalidCode(Parent.PR_LocationInfo);
			ValidateIsInCollectionAlready(RelatedPartyRecord.PR_LocationInfo);
		}

		#endregion

		#region CheckPR_RN_NKImporterCountry

		protected override void CheckPR_RN_NKImporterCountry()
		{
			base.CheckPR_RN_NKImporterCountry();

			if (!Parent.PR_RN_NKImporterCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.PR_RN_NKImporterCountryInfo);
			}

			ValidateIsInCollectionAlready(RelatedPartyRecord.PR_RN_NKImporterCountryInfo);
		}

		#endregion

		#region Implementation

		OrgRelatedParty RelatedPartyRecord
		{
			get { return (OrgRelatedParty)base.Parent; }
		}

		#endregion
	}
}
