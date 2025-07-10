using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UserDefinedValues]
	[DeferTriggerAndRunBeforeCommit(OrgPartRelationValidationHelper.TG_OrgPartRelation_EnsureOwnerOfProductWithSameBarcodeIsUnique, OrgPartRelationValidationHelper.WhsCheckOwnerAndBarcodeOfProductAreUnique, OrgPartRelationSchema.Constants.OU_OP, typeof(IWhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgPartRelation_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(OrgPartRelationValidationHelper.TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ, OrgPartRelationValidationHelper.UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart, OrgPartRelationSchema.Constants.OU_OP, typeof(IUpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate), ValueToRunStoredProcWith = ValueVersion.Both)]
	public class OrgPartRelation : AutoOrgPartRelation, IOrgPartRelation, ICustomFieldProvider, IPartProvider, ISupportDataImporting, IWorkflowProvider
	{
		public new class Schema : AutoOrgPartRelation.Schema
		{
			public const string OU_Organisation = "OU_Organisation";
		}

		public static OrgPartRelation New(BusinessObjectFactory factory)
		{
			return factory.New<OrgPartRelation>();
		}

		public OrgPartRelation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(OU_UnitsPerClientUQ), ConcurrencyPolicy.Ignore);
		}

		bool fIsImportingData;

		#region New Properties

		public bool MatchesImporter(ZGuid importerPK)
		{
			return OU_OH == importerPK && IsOwner;
		}

		public bool MatchesSupplier(ZGuid supplierPK)
		{
			return OU_OH == supplierPK && IsSupplier;
		}

		public bool MatchesClassificationOrganisation(ZGuid classificationOrganisationPK)
		{
			return OU_OH == classificationOrganisationPK && IsClassificationOrganisation;
		}

		public static bool IsOwnerType(ZString relationship)
		{
			return relationship == OrgPartRelation.RelationshipTypes.Owner || relationship == OrgPartRelation.RelationshipTypes.Both;
		}

		/// <summary>
		/// Owner or both
		/// </summary>
		public bool IsOwner
		{
			get { return OU_Relationship == RelationshipTypes.Owner || IsBoth; }
		}

		/// <summary>
		/// Supplier or both
		/// </summary>
		public bool IsSupplier
		{
			get { return OU_Relationship == RelationshipTypes.Supplier || IsBoth; }
		}

		public bool IsBoth
		{
			get { return OU_Relationship == RelationshipTypes.Both; }
		}

		public bool IsClassificationOrganisation => OU_Relationship == RelationshipTypes.ClassificationOrganization;

		public bool NeedCheckDuplicateBarCode => IsOwner || IsClassificationOrganisation;

		public ZString CodeAndRelationship
		{
			get { return Organisation != null ? Organisation.OH_Code + "/" + OU_Relationship : ""; }
		}

		#endregion

		#region CusSupImpClassOverride

		public bool HasRelatedPartPivots
		{
			get { return Factory.LoadTop1<Enterprise.Integration.Customs.IBaseCusClassPartPivot>(RelatedPivotsQuery) != null; }
		}

		public string GetCountryCodesWithRelatedCusClassPartPivot()
		{
			List<string> result = new List<string>();

			BusinessObject[] pivots = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.IBaseCusClassPartPivot>(RelatedPivotsQuery);

			foreach (BusinessObject pivot in pivots)
			{
				ZString countryCode = (ZString)pivot[CusClassPartPivotSchema.CI_RN_NKCountry];

				if (!result.Contains(countryCode))
				{
					result.Add(countryCode);
				}
			}

			return new ZStringBuilder(result).ToStringWithDelimiterBetweenAppends(",");
		}

		public void DeleteRelatedCusClassPartPivots()
		{
			BusinessObject[] overrides = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.IBaseCusClassPartPivot>(RelatedPivotsQuery);
			foreach (BusinessObject one in overrides)
			{
				one.Delete();
			}
		}

		ZQuery RelatedPivotsQuery
		{
			get
			{
				var query = new ZQuery(CusClassPartPivotSchema.CI_OH, OU_OH);
				query.AddToFilter(CusClassPartPivotSchema.CI_OP, OU_OP);
				return query;
			}
		}

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			if (!IsDeleted)
			{
				DeleteRelatedCusClassPartPivots();
				WorkflowItems.RemoveAndDeleteAll();
				base.Delete();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OU_Relationship = RelationshipTypes.Owner;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase || OU_ClientUQInfo.HasChanges)
			{
				MaintainOU_UnitsPerClientUQForDatabaseConstraint();
			}

			void MaintainOU_UnitsPerClientUQForDatabaseConstraint()
			{
				if (OU_ClientUQ.IsEmpty)
				{
					OU_UnitsPerClientUQ = 0; // Clear before the trigger, to ensure the constraint is satisfied.
				}
				else
				{
					var part = SupplierPart;

					if (part != null)
					{
						// Calculate in memory so we have an "original value" in case a subsequent save occurs, to ensure the above clearing can post clearing it.
						// Database trigger will usually override this and are considered the source of truth however.
						var conversion = new ConversionsToSKUTable(part, allowFractionalConversions: true)[OU_ClientUQ];
						OU_UnitsPerClientUQ = conversion?.QtySKU > 0m ? 1m / conversion.QtySKU : 0m;
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		public OrgHeader Organisation
		{
			get { return Header; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Fetch(this);
		}

		class Fetch : EnterpriseBusinessObjectFetchStrategy
		{
			public Fetch(EnterpriseBusinessObject bizO)
				: base(bizO)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(OrgHeader.Schema.TableName, ((OrgPartRelation)BusinessObject).OU_OH);
			}
		}

		public OrgHeaderCollection OrgHeaderCollection
		{
			get
			{
				if (fOrgHeaderCollection == null)
				{
					fOrgHeaderCollection = new OrgHeaderCollection(Factory);
				}
				return fOrgHeaderCollection;
			}
		}
		OrgHeaderCollection fOrgHeaderCollection;

		#endregion

		#region Error Strings

		public static string CannotDeleteExistingTransactionsError
		{
			get { return Res.GetString("dcaaf74b-b8bc-41cd-be1c-99c26dae54a6", "There are existing transactions for this Product in the Warehouse module. This relationship cannot be deleted."); }
		}

		public static string CannotChangeReferenceBecauseOfExistingTransactionsError
		{
			get { return Res.GetString("b111978b-372b-a6ad-c7ee-b500e68a15aa", "Cannot change this relationship because there are existing transactions for the original organization."); }
		}

		public static string DuplicateRelationshipError
		{
			get { return Res.GetString("07cc02a8-f5ec-43e4-a0f9-af38be8cf380", "Duplicate organization relationship - please remove this entry (or the duplicate entry)"); }
		}

		public static string NoClientUQConverterError
		{
			get { return Res.GetString("a1d30643-2584-438c-93d5-27c78c898595", "There is no unit conversion from Stock Keeping Unit to Client UQ. This means the Client Quantities on reports and web tracker may be incorrect. If the conversion is 1.0 then ignore this warning, otherwise enter a unit conversion on the Unit Conversions Tab from Stock Keeping Unit to Client UQ"); }
		}

		public static string WarehouseConsigneeRelationshipWarning
		{
			get { return Res.GetString("5c1b5966-e0da-46dc-a1d9-1300e68a15c6", "Warehouse Consignee MUST only be selected and used for Warehouse Products.\r\nIt MUST NOT be used for Customs Products.\r\nMismatching of Product Codes on the Declaration Lines form may result if WCN is used for any Customs Product Codes."); }
		}

		public static string RFAttributeConfirmError
		{
			get { return Res.GetString("ba0ff83c-e0af-4ee1-b792-a7d8e22720ad", "This attribute has not been selected for use"); }
		}

		public static string RFAttributeConfirm_AttributeNeutralError
		{
			get { return Res.GetString("e111978b-0001-4175-a5f6-c7c08d9a5984", "The Pick Mode is Attribute Neutral. A Serial Number Attribute must be used as Confirmation."); }
		}

		public static string RFAttributeConfirm_ReleaseCapturedAttributeError
		{
			get { return Res.GetString("b69724ab-1052-4a28-ae8a-f611d9a07eec", "This attribute is Release Captured."); }
		}

		public static string SameOrgAsOwnerAndSupplier
		{
			get { return Res.GetString("b2089c10-b792-494f-9fc7-003037ce304d", "The same organization has been entered as both an Owner and a Supplier. Remove this organization, and change the SUP record to the BTH code"); }
		}

		public static string PickModeError
		{
			get { return Res.GetString("da54f337-2ca0-48f9-ab53-eadec0b125c3", "Attribute Neutral Pick Mode can only be specified for products that use the Serial Number Attribute."); }
		}

		#endregion

		#region Property Overrides

		#region OU_UsePartAttrib

		#region OU_UsePartAttrib1

		public override ZBool OU_UsePartAttrib1
		{
			get { return base.OU_UsePartAttrib1; }
			set
			{
				base.OU_UsePartAttrib1 = value;
				OU_IsPartAttrib1ReleaseCaptured &= OU_UsePartAttrib1;
				SetJulianBatchNumberDefaults(OrgPartRelationSchema.OU_UsePartAttrib1, attributeNumber: 1);
			}
		}

		#endregion

		#region OU_UsePartAttrib2

		public override ZBool OU_UsePartAttrib2
		{
			get { return base.OU_UsePartAttrib2; }
			set
			{
				base.OU_UsePartAttrib2 = value;
				OU_IsPartAttrib2ReleaseCaptured &= OU_UsePartAttrib2;
				SetJulianBatchNumberDefaults(OrgPartRelationSchema.OU_UsePartAttrib2, attributeNumber: 2);
			}
		}

		#endregion

		#region OU_UsePartAttrib3

		public override ZBool OU_UsePartAttrib3
		{
			get { return base.OU_UsePartAttrib3; }
			set
			{
				base.OU_UsePartAttrib3 = value;
				OU_IsPartAttrib3ReleaseCaptured &= OU_UsePartAttrib3;
				SetJulianBatchNumberDefaults(OrgPartRelationSchema.OU_UsePartAttrib3, attributeNumber: 3);
			}
		}

		#endregion

		#region OU_UseSerialNumber

		[ReadOnlyMember(nameof(OU_UseSerialNumber_ReadOnly))]
		public override ZBool OU_UseSerialNumber
		{
			get { return base.OU_UseSerialNumber; }
			set
			{
				base.OU_UseSerialNumber = value;
				OU_IsSerialNumberReleaseCaptured &= OU_UseSerialNumber;
				SetRFAttributeConfirm();
			}
		}

		#endregion

		#region SetJulianBatchNumberDefaults

		void SetJulianBatchNumberDefaults(SchemaBoolColumn usePartAttributeColumn, int attributeNumber)
		{
			var organisation = Organisation;
			if ((ZBool)this[usePartAttributeColumn] && organisation != null && organisation.PartAttributeManager.IsPartAttributeAJulianBatchNumber(attributeNumber))
			{
				OU_UsePackingDate = true;
				OU_UseExpiryDate = true;
				OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			}
		}

		#endregion

		#endregion

		[ReadOnlyMember(nameof(OU_IsPartAttrib1ReleaseCaptured_ReadOnly))]
		public override ZBool OU_IsPartAttrib1ReleaseCaptured
		{
			get { return base.OU_IsPartAttrib1ReleaseCaptured; }
			set
			{
				base.OU_IsPartAttrib1ReleaseCaptured = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOU_RFAttributeConfirm();
					Validation.ValidateOU_CompletePalletPicking();
				}
			}
		}

		[ReadOnlyMember(nameof(OU_IsPartAttrib2ReleaseCaptured_ReadOnly))]
		public override ZBool OU_IsPartAttrib2ReleaseCaptured
		{
			get { return base.OU_IsPartAttrib2ReleaseCaptured; }
			set
			{
				base.OU_IsPartAttrib2ReleaseCaptured = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOU_RFAttributeConfirm();
					Validation.ValidateOU_CompletePalletPicking();
				}
			}
		}

		[ReadOnlyMember(nameof(OU_IsPartAttrib3ReleaseCaptured_ReadOnly))]
		public override ZBool OU_IsPartAttrib3ReleaseCaptured
		{
			get { return base.OU_IsPartAttrib3ReleaseCaptured; }
			set
			{
				base.OU_IsPartAttrib3ReleaseCaptured = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOU_RFAttributeConfirm();
					Validation.ValidateOU_CompletePalletPicking();
				}
			}
		}

		[ReadOnlyMember(nameof(OU_IsSerialNumberReleaseCaptured_ReadOnly))]
		public override ZBool OU_IsSerialNumberReleaseCaptured
		{
			get { return base.OU_IsSerialNumberReleaseCaptured; }
			set
			{
				base.OU_IsSerialNumberReleaseCaptured = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOU_RFAttributeConfirm();
					Validation.ValidateOU_CompletePalletPicking();
				}
			}
		}

		public ZString OU_Organisation
		{
			get { return Header == null ? ZString.Empty : Header.OH_FullNameTruncated; }
		}

		public ZPropertyInfo OU_OrganisationInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OU_Organisation); }
		}

		public event System.ComponentModel.CancelEventHandler OnOU_OHChanging;

		public override ZGuid OU_OP
		{
			get { return base.OU_OP; }
			set
			{
				if (value == ZGuid.Empty)
				{
					LogRemovalOfRelationshipOnPart();
				}
				base.OU_OP = value;
			}
		}

		#region OU_OH

		[ActionField(ReadOnly = true)]
		[List("OrgHeaderCollection")]
		public override ZGuid OU_OH
		{
			get { return base.OU_OH; }
			set
			{
				bool hasChanged = base.OU_OH != value;
				if (hasChanged)
				{
					System.ComponentModel.CancelEventArgs args = new System.ComponentModel.CancelEventArgs(false);
					if (OnOU_OHChanging != null)
					{
						OnOU_OHChanging(this, args);
					}

					if (!args.Cancel)
					{
						base.OU_OH = value;
					}
					else
					{
						OU_OHInfo.RefreshBinding();
					}

					var header = Header;
					if (header != null && header.CountryData.OV_MakePartsBothImportAndExport)
					{
						OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
					}

					if (value.IsValid)
					{
						ApplyDefaultRoyaltyFromOrganisation(value);

						var orgMiscServ = header?.MiscServ;
						if (orgMiscServ != null)
						{
							OU_PickMode = orgMiscServ.OM_WhsDefaultWarehousePickMode;
							OU_RollUpAttributesOnDocuments = orgMiscServ.OM_WhsDefaultWarehouseRollUp;
						}
					}

					var supplierPart = SupplierPart;
					if (supplierPart != null)
					{
						supplierPart.RelatedOrganisations.MarkAsNeedingValidation();
						supplierPart.BillOfMaterials.MarkAsNeedingValidation();
						LogChangeOfOrgOfRelationshipOnPart(supplierPart);
					}
				}
			}
		}

		void LogRemovalOfRelationshipOnPart()
		{
			var supplierPart = SupplierPart;
			var org = Header;
			if (org != null && supplierPart != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				supplierPart.Logs.AddNew(Events.EditedARecord, string.Format("Relationship {0} to party '{1}' removed", OU_Relationship, org.OH_Code), ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void LogChangeOfOrgOfRelationshipOnPart(OrgSupplierPart supplierPart)
		{
			Argument.NotNull(supplierPart, "OrgSupplierPart supplierPart");
			var org = Header;
			if (org != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				supplierPart.Logs.AddNew(Events.EditedARecord, string.Format("Party {0} became '{1}'", OU_Relationship, org.OH_Code), ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void LogChangeOfTypeOfRelationshipOnPart(OrgSupplierPart supplierPart)
		{
			Argument.NotNull(supplierPart, "OrgSupplierPart supplierPart");
			var org = Header;
			if (org != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				supplierPart.Logs.AddNew(Events.EditedARecord, string.Format("'{0}' became party {1}", org.OH_Code, OU_Relationship), ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void ApplyDefaultRoyaltyFromOrganisation(ZGuid newOrganisation)
		{
			OrgHeader newOrgHeader;
			RoyaltyRetriever royaltyRetriever = new RoyaltyRetriever();

			OrgSupplierPart supplierPart;

			if (!IsDeleted
				&& (newOrgHeader = Factory.Load<OrgHeader>(newOrganisation)) != null
				&& (supplierPart = SupplierPart) != null
				&& supplierPart.RelatedOrganisations != null
				&& royaltyRetriever.FindRoyalty(supplierPart.RelatedOrganisations, newOrgHeader))
			{
				OU_RoyaltyPercent = royaltyRetriever.RoyaltyPercentage;
			}
		}

		#endregion

		public bool HasCurrentStockIncludingInTransit()
		{
			return HasTransactionsIncludingInTransit(OU_OH, true);
		}

		internal bool HasTransactionsIncludingInTransit(ZGuid clientPK, bool currentStockOrInTransitOnly = false)
		{
			bool result = false;

			if (IsOwnerType((ZString)OU_RelationshipInfo.OriginalValue)) // Only Owner/Both relationships are relevant to Warehouse
			{
				var supplierPart = SupplierPart;
				if (clientPK.IsValid && supplierPart != null)
				{
					var query = new ZDBOnlyQuery(typeof(IWhsDocket));
					query.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPK);

					var subQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsDocketLineSchema.WE_WD);
					subQuery.AddToFilter(WhsDocketLineSchema.WE_OP, supplierPart.PK);
					if (currentStockOrInTransitOnly)
					{
						subQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
					}

					query.AddSubQuery(subQuery, JoinCondition.And);

					result = Factory.LoadTop1<IWhsDocket>(query) != null;
				}
			}

			return result;
		}

		[List("RelationshipTypeList")]
		public override ZString OU_Relationship
		{
			get { return base.OU_Relationship; }
			set
			{
				var oldValue = base.OU_Relationship;
				base.OU_Relationship = value;
				if (value == RelationshipTypes.Supplier)
				{
					OU_LandedCostMarginPercent1 = 0m;
					OU_LandedCostMarginPercent2 = 0m;
					OU_LandedCostMarginPercent3 = 0m;
				}
				OrgSupplierPart supplierPart = SupplierPart;
				if (supplierPart != null)
				{
					supplierPart.MarkAsNeedingValidation();
					supplierPart.BillOfMaterials.MarkAsNeedingValidation();
					if (oldValue != value)
					{
						LogChangeOfTypeOfRelationshipOnPart(supplierPart);
					}
				}

				// tested in OrgPartRelationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateOU_WCG_CartonGroup();
				}
			}
		}

		#region OU_RFAttributeConfirm

		[List("Lookups.RFAttributeConfirmList")]
		public override ZString OU_RFAttributeConfirm
		{
			get { return base.OU_RFAttributeConfirm; }
			set { base.OU_RFAttributeConfirm = value; }
		}

		#endregion

		#region OU_PickMode

		[List("Lookups.PickModeList")]
		public override ZString OU_PickMode
		{
			get { return base.OU_PickMode; }
			set
			{
				base.OU_PickMode = value;
				SetRFAttributeConfirm();
			}
		}

		void SetRFAttributeConfirm()
		{
			if (OU_PickMode == WhsPickMode.Codes.AttributeNeutral)
			{
				if (OU_UseSerialNumber)
				{
					OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
				}
			}
			else
			{
				OU_RollUpAttributesOnDocuments = false;
			}
		}

		#endregion

		#region OU_PreventReceivingOvers

		public override ZBool OU_PreventReceivingOvers
		{
			get { return base.OU_PreventReceivingOvers; }
			set
			{
				base.OU_PreventReceivingOvers = value;
				if (!OU_PreventReceivingOvers)
				{
					OU_ReceiveOverageTolerancePercent = ZShort.Zero;
				}
			}
		}

		#endregion

		#region OU_ReceiveOverageTolerancePercent

		[ReadOnlyMember(nameof(OU_ReceiveOverageTolerancePercent_ReadOnly))]
		public override ZShort OU_ReceiveOverageTolerancePercent
		{
			get { return base.OU_ReceiveOverageTolerancePercent; }
			set
			{
				base.OU_ReceiveOverageTolerancePercent = value;
			}
		}

		#endregion

		[ActionField(ReadOnly = true)]
		public override ZBool OU_FormLayoutController
		{
			get { return base.OU_FormLayoutController; }
			set
			{
				OrgSupplierPart supplierPart = SupplierPart;
				if (value)
				{
					if (supplierPart != null)
					{
						foreach (OrgPartRelation relation in supplierPart.RelatedOrganisations)
						{
							if (relation.OU_FormLayoutController)
							{
								relation.OU_FormLayoutController = false;
							}
						}
					}
				}
				base.OU_FormLayoutController = value;
				if (supplierPart != null)
				{
					supplierPart.MarkAsNeedingValidation();
				}
			}
		}

		[List("Lookups.UQ_List")]
		public override ZString OU_ClientUQ
		{
			get => base.OU_ClientUQ;
			set => base.OU_ClientUQ = value;
		}

		[List("Lookups.RoyaltyCurrencies")]
		public override ZString OU_RX_NKRoyaltyCurrency
		{
			get => base.OU_RX_NKRoyaltyCurrency;
			set => base.OU_RX_NKRoyaltyCurrency = value;
		}

		#region OU_JulianBatchNoFormat

		[List("Lookups.JulianBatchNumberFormatsList")]
		public override ZString OU_JulianBatchNoFormat
		{
			get { return base.OU_JulianBatchNoFormat; }
			set { base.OU_JulianBatchNoFormat = value; }
		}

		#endregion

		#region OU_OPC_Category

		[ResourceStringData("OrgPartRelation|OPC_CategoryCode", ShortCaption = "Cat. Code", Caption = "Category Code")]
		public override ZGuid OU_OPC_Category
		{
			get
			{
				return base.OU_OPC_Category;
			}
			set
			{
				base.OU_OPC_Category = value;
			}
		}

		public ZString CategoryCode => Category?.OPC_CategoryCode ?? ZString.Empty;

		#endregion

		#region OU_WCG_CartonGroup

		[List("Lookups.CartonGroups")]
		public override ZGuid OU_WCG_CartonGroup
		{
			get { return base.OU_WCG_CartonGroup; }
			set { base.OU_WCG_CartonGroup = value; }
		}

		#endregion

		#region OU_WHC_DefaultInventoryHoldCode

		[List("Lookups.HoldCodes")]
		[ResourceStringData("OrgPartRelation|WHC_DefaultInventoryHoldCode", ShortCaption = "Hold Code", Caption = "Default Hold Code")]
		public override ZGuid OU_WHC_DefaultInventoryHoldCode
		{
			get => base.OU_WHC_DefaultInventoryHoldCode;
			set => base.OU_WHC_DefaultInventoryHoldCode = value;
		}

		#endregion

		#region ReadOnlyProperties

		bool OU_IsPartAttrib1ReleaseCaptured_ReadOnly => !OU_UsePartAttrib1;

		bool OU_IsPartAttrib2ReleaseCaptured_ReadOnly => !OU_UsePartAttrib2;

		bool OU_IsPartAttrib3ReleaseCaptured_ReadOnly => !OU_UsePartAttrib3;

		bool OU_IsSerialNumberReleaseCaptured_ReadOnly => !OU_UseSerialNumber;

		protected bool OU_LandedCostMarginPercent1_ReadOnly => OU_Relationship == RelationshipTypes.Supplier;

		protected bool OU_LandedCostMarginPercent2_ReadOnly => OU_Relationship == RelationshipTypes.Supplier;

		protected bool OU_LandedCostMarginPercent3_ReadOnly => OU_Relationship == RelationshipTypes.Supplier;

		protected bool OU_UsePartAttrib1_ReadOnly
			=> !Organisation?.PartAttributeManager.IsPartAttributeUsedByOrganisation(1) ?? true;

		protected bool OU_UsePartAttrib2_ReadOnly
			=> !Organisation?.PartAttributeManager.IsPartAttributeUsedByOrganisation(2) ?? true;

		protected bool OU_UsePartAttrib3_ReadOnly
			=> !Organisation?.PartAttributeManager.IsPartAttributeUsedByOrganisation(3) ?? true;

		protected bool OU_UseExpiryDate_ReadOnly
			=> !Organisation?.PartAttributeManager.IsExpiryDateUsedByOrganisation ?? true;

		protected bool OU_UsePackingDate_ReadOnly
			=> !Organisation?.PartAttributeManager.IsPackingDateUsedByOrganisation ?? true;

		protected bool OU_UseSerialNumber_ReadOnly
			=> !Organisation?.PartAttributeManager.IsSerialNumberUsedByOrganisation ?? true;

		protected bool OU_RollUpAttributesOnDocuments_ReadOnly
		{
			get
			{
				var organisation = this.Organisation;
				return organisation == null || !organisation.PartAttributeManager.IsAttributeNeutralUsedByProduct(SupplierPart);
			}
		}

		protected bool OU_ReceiveOverageTolerancePercent_ReadOnly => !OU_PreventReceivingOvers;

		#endregion

		#endregion

		#region List Properties

		public static class RelationshipTypes
		{
			public const string Both = "BTH";
			public const string ClassificationOrganization = "CLS";
			public const string Owner = "OWN";
			public const string Supplier = "SUP";
			public const string WarehouseConsignee = "WCN";
		}

		public CodeDescriptionPairList RelationshipTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(RelationshipTypes.Both, Res.GetString("4c5d1b33-bcee-4398-a589-e8fcb2cd38f4", "Both Owner(Importer) and Supplier(Exporter)"));
				result.AddPair(RelationshipTypes.ClassificationOrganization, Res.GetString("1c26dc55-1cd9-4906-a051-0291d1bb04d5", "Classification Organization"));
				result.AddPair(RelationshipTypes.Owner, Res.GetString("cb891568-a183-4c69-8f19-488e28dc6eaf", "Owner (Importer)"));
				result.AddPair(RelationshipTypes.Supplier, Res.GetString("25e3b27d-9014-49e3-a325-36326943d350", "Supplier (Exporter)"));
				result.AddPair(RelationshipTypes.WarehouseConsignee, Res.GetString("abde2e09-753c-4a4c-9334-ad62a115f711", "Warehouse Consignee"));
				return result;
			}
		}

		#endregion

		#region Method Overrides

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			OrgPartRelation partRelation = (OrgPartRelation)Factory.New(GetType());

			partRelation.OU_Hi = OU_Hi;
			partRelation.OU_LandedCostMarginPercent1 = OU_LandedCostMarginPercent1;
			partRelation.OU_LandedCostMarginPercent2 = OU_LandedCostMarginPercent2;
			partRelation.OU_LandedCostMarginPercent3 = OU_LandedCostMarginPercent3;
			partRelation.OU_LocalPartNumber = OU_LocalPartNumber;
			partRelation.OU_OH = OU_OH;
			partRelation.OU_PickMode = OU_PickMode; // It will override default pick mode (in set OU_OH)
			partRelation.OU_Relationship = OU_Relationship;
			partRelation.OU_Ti = OU_Ti;
			partRelation.OU_UseExpiryDate = OU_UseExpiryDate;
			partRelation.OU_UsePackingDate = OU_UsePackingDate;
			partRelation.OU_UsePartAttrib1 = OU_UsePartAttrib1;
			partRelation.OU_UsePartAttrib2 = OU_UsePartAttrib2;
			partRelation.OU_UsePartAttrib3 = OU_UsePartAttrib3;
			partRelation.OU_UseSerialNumber = OU_UseSerialNumber;

			return partRelation;
		}

		#endregion

		#endregion

		#region ICustomFieldProvider Implementation

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region IPartProvider Implementation

		OrgSupplierPart IPartProvider.Part
		{
			get { return SupplierPart; }
		}

		#endregion

		#region ISupportDataImporting Implementation

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region IWorkflowProvider Implementation

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new OrgPartRelationProcessTasksCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, OU_OH, ZGuid.Empty);
			return result;
		}

		public ZString WorkflowType
		{
			get { return WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode; }
		}

		ProcessTaskCollection workflowItems;

		#endregion

		#region ICanDelete Implementation

		public override bool CanDelete
		{
			get
			{
				var canDelete = true;
				if (IsInDatabase && !IsDeleted && SupplierPart != null && !SupplierPart.IsDeleted)
				{
					canDelete = HasDuplicateOwners(this) || (!HasTransactionsIncludingInTransit((ZGuid)OU_OHInfo.OriginalValue) && !HasAsnLineOnUnfinalisedReceive);
				}
				return canDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("dcaaf74b-b8bc-41cd-be1c-99c26dae54a6", "There are existing transactions for this Product in the Warehouse module. This relationship cannot be deleted.");

		#endregion

		#region HasDuplicateOwners

		bool HasDuplicateOwners(OrgPartRelation elementToDelete)
		{
			var result = false;

			foreach (var businessObject in SupplierPart.RelatedOrganisations)
			{
				var relation = (OrgPartRelation)businessObject;
				if (relation.Organisation != null &&
					relation.PK != elementToDelete.PK &&
					relation.OU_OH == elementToDelete.OU_OH &&
					(relation.OU_Relationship == RelationshipTypes.Owner || relation.OU_Relationship == RelationshipTypes.Both))
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region HasAsnLineOnUnfinalisedReceive

		public bool HasAsnLineOnUnfinalisedReceive
		{
			get
			{
				var subQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsAsnLineSchema.WN_WD);

				subQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, OU_OHInfo.OriginalValue);
				subQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, new[] { "FIN", "CAN" });
				subQuery.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");

				var query = new ZDBOnlyQuery(typeof(IWhsAsnLine));

				query.AddToFilter(WhsAsnLineSchema.WN_OP, OU_OP);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return Factory.ExistsInDatabase(WhsAsnLineSchema.Constants.TableName, query);
			}
		}

		#endregion
	}
}
