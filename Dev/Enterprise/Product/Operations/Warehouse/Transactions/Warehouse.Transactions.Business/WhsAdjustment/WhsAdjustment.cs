using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[GlowDataDefinition("IWhsAdjustment")]
	[UniversalDataContext(DataContextType.WarehouseAdjustment)]
	public sealed class WhsAdjustment : WhsDocket,
		ICreateDocketLineFromInventory,
		IWhsAdjustment,
		IRatingSupporter,
		IDocumentSupportable,
		IEDocsProvider,
		IPickedStockAdjuster,
		ICriticalChangesVersionID,
		INumberFountainConsumer
	{
		#region Constructors

		public WhsAdjustment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_DocketStatus), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_FinalisedDate), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Customs Stuff

		public override bool IsCustomsTransaction => WD_DocketSubType.EqualsIgnoringCase(AdjustmentType.Codes.Customs);

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WD_DocketType = DocketType.Codes.Adjustment;
			WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			WD_ExternalReference = "ADJUSTMENT";
			UserEnteredFinalisedDate = ZDateTime.Now;
		}

		protected override bool IsUniqueExternalReferenceCreatedOnSaveCore
		{
			get { return true; }
		}

		protected override void OnClientChanged()
		{
			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsAdjustment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);

			base.OnClientChanged();
		}

		protected override void OnWarehouseChanged()
		{
			if (Warehouse != null)
			{
				SetDefaultLocationsForBondWarehouse();
			}

			if (!IsInDatabase && WD_ArrivalDate.IsEmpty)
			{
				WD_ArrivalDate = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			}

			if (IsNewOwnershipAdjustmentParent)
			{
				ChildAdjustment.WD_WW_Whs = WD_WW_Whs;
			}

			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsAdjustment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);

			base.OnWarehouseChanged();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return WD_DocketID.IsEmpty ? Res.GetString("e09bc7fa-83dd-4855-84dc-ea934c055572", "Warehouse Adjustment") : Res.GetString("e8625868-308d-4d59-8e07-44dd3fe62944", "Warehouse Adjustment {0}", WD_DocketID); }
		}

		#endregion

		#region Related Entities

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel => null;

		#endregion

		#region Lines

		[ChildEditable(true)]
		public new WhsAdjustmentLineCollection Lines
		{
			get { return (WhsAdjustmentLineCollection)base.Lines; }
		}

		protected override WhsDocketLineCollection GetNewDocketLineCollection()
		{
			return new WhsAdjustmentLineCollection(this, new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, ZBool.True));
		}

		#endregion

		#region NoteContextsForRelatedNotes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.D;

				return result;
			}
		}

		#endregion

		#region ChildAdjustment

		public WhsAdjustment ChildAdjustment
		{
			get { return Factory.Load<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, PK)).SingleOrDefault(); }
		}

		#endregion

		#region TransportCo

		protected override OrgHeader TransportCoCore => null;

		protected override int TransportCoNameOrPKMaxLength => 0;

		#endregion

		#endregion

		#region Validation

		public new WhsAdjustmentValidation Validation
		{
			get { return (WhsAdjustmentValidation)base.Validation; }
		}

		protected override WhsDocketValidation GetNewValidation()
		{
			return new WhsAdjustmentValidation(this);
		}

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			CommittingLinesHelper.UncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended(Lines.Cast<WhsAdjustmentLine>());
			base.RunPreSaveValidationCore();
		}

		protected override IDisposable GetValidationDataSuspender()
			=> new DisposableList(GetSuspenders());

		IEnumerable<IDisposable> GetSuspenders()
		{
			yield return base.GetValidationDataSuspender();
			if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				yield return UseAdjustmentValidationCache();
			}
		}

		IDisposable UseAdjustmentValidationCache()
		{
			return new DisposableAction(
				() => { serialNumberUniquenessCheckerCache = new SerialNumberUniquenessChecker(this); },
				() => { serialNumberUniquenessCheckerCache = null; });
		}

		#region SerialNumberUniquenessCheckerCache

		internal bool IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot) => SerialNumberUniquenessCheckerCache.IsSerialNumberAlreadyInUse(pivot);

		internal SerialNumberUniquenessChecker SerialNumberUniquenessCheckerCache => serialNumberUniquenessCheckerCache ?? new SerialNumberUniquenessChecker();
		SerialNumberUniquenessChecker serialNumberUniquenessCheckerCache;

		#endregion

		#region IsPalletInTransit

		public Func<ZString, bool> IsPalletInTransit
		{
			get { return isPalletInTransit ?? (isPalletInTransit = CreateIsPalletInTransitDelegate()); }
		}

		Func<ZString, bool> CreateIsPalletInTransitDelegate()
		{
			ICollection<ZString> inTransitDocketLinesWithPalletID = null;

			Func<ZString, bool> result = (palletID) =>
			{
				if (inTransitDocketLinesWithPalletID == null || !((IBusinessObjectInternals)this).IsInPreSaveValidation)
				{
					inTransitDocketLinesWithPalletID = GetInTransitPalletIDsOnThisAdjustment();
				}

				return inTransitDocketLinesWithPalletID.Contains(palletID.ToUpper());
			};

			return result;
		}

		ICollection<ZString> GetInTransitPalletIDsOnThisAdjustment()
		{
			var inTransitPalletIDs = new HashSet<ZString>();
			if (WD_WW_Whs.IsValid)
			{
				var query = new ZDBOnlyQuery(typeof(WhsDocketLine));
				query.AddToFilter(WhsDocketLineSchema.WE_OriginalInventoryStatus, WhsInventoryView.InTransitStatusCodeList);
				query.AddToFilter(WhsDocketLineSchema.WE_PalletID, Lines.Select(l => l.WE_PalletID).Distinct());

				var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
				docketSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, WD_WW_Whs);
				query.AddSubQuery(docketSubQuery, JoinCondition.And);

				var palletIDs = Factory.Load<WhsDocketLine>(query).Select(l => l.WE_PalletID.ToUpper()).Distinct();
				inTransitPalletIDs = new HashSet<ZString>(palletIDs);
			}

			return inTransitPalletIDs;
		}

		Func<ZString, bool> isPalletInTransit;

		#endregion

		#region CheckIfPalletIDExistsInAnotherLocationInThisWarehouse

		public Func<WhsAdjustmentLine, ZString> CheckIfPalletIDExistsInAnotherLocationInThisWarehouse
		{
			get
			{
				return checkIfPalletIDExistsInAnotherLocationInThisWarehouse ?? (checkIfPalletIDExistsInAnotherLocationInThisWarehouse = CreateCheckIfPalletIDExistsInAnotherLocationInThisWarehouseDelegate());
			}
		}

		Func<WhsAdjustmentLine, ZString> CreateCheckIfPalletIDExistsInAnotherLocationInThisWarehouseDelegate()
		{
			IEnumerable<WhsInventoryView> inventoriesWithPalletID = null;

			Func<WhsAdjustmentLine, ZString> result = (adjustmentLine) =>
			{
				var locationString = "";

				if (inventoriesWithPalletID == null || !((IBusinessObjectInternals)this).IsInPreSaveValidation)
				{
					inventoriesWithPalletID = GetInventoriesWithPalletIDsOnThisAdjustment();
				}

				var inventoryCollection = inventoriesWithPalletID.Where(i => i.WI_PalletID.EqualsIgnoringCase(adjustmentLine.WE_PalletID) && i.WI_WL != adjustmentLine.WE_WL && i.WI_WE_InDocketLine != adjustmentLine.PK);
				foreach (var inventory in inventoryCollection)
				{
					if ((inventory.HasStockIncludingNotYetFinalised) && !adjustmentLine.IsInventoryAdjustedOutOnSiblings(inventory) && inventory.WI_WW_Whs.Equals(adjustmentLine.WarehousePK))
					{
						locationString = inventory.Location.ToLocationString();
						break;
					}
				}

				return locationString;
			};

			return result;
		}

		IEnumerable<WhsInventoryView> GetInventoriesWithPalletIDsOnThisAdjustment()
		{
			var query = new ZQuery(WhsInventoryViewSchema.WI_WL, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, Lines.Select(l => l.WE_PalletID).Distinct());
			query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
			return Factory.Load<WhsInventoryView>(query);
		}

		Func<WhsAdjustmentLine, ZString> checkIfPalletIDExistsInAnotherLocationInThisWarehouse;

		#endregion

		CommittingLinesAdjustmentHelper CommittingLinesHelper
		{
			get { return committingLinesHelper ?? (committingLinesHelper = new CommittingLinesAdjustmentHelper(this)); }
		}

		CommittingLinesAdjustmentHelper committingLinesHelper;

		#endregion

		#endregion

		#region CustomizableNumber

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehouseDocketID;

		#endregion

		#region Lookups

		protected override WhsDocketLookups GetNewLookups()
		{
			return new WhsAdjustmentLookups(this);
		}

		#endregion

		#region Properties

		#region Description

		protected override ZString DescriptionCore
		{
			get { return Res.GetString("0bf21865-1dc9-4eb5-b1bc-15e3a3f53c9a", "Adjustment"); }
		}

		#endregion

		#region AdjustmentTypeDescription

		public ZString AdjustmentTypeDescription => Lookups.SubTypes.GetDescriptionFromCode(WD_DocketSubType);

		#endregion

		#region ShouldUpdateWeightAndVolumeOnTheFlyCore

		protected override bool ShouldUpdateWeightAndVolumeOnTheFlyCore
		{
			get { return false; }
		}

		#endregion

		#region OwnershipAdjustedClient

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[RelatedBusinessObject("OwnershipAdjustedClient")]
		[ResourceStringData("7f9f6b74-20e0-4aba-b79c-7dc6751d26a9", Caption = "New Client")]
		public ZGuid OwnershipAdjustedClientPK
		{
			get
			{
				var childAdjustment = ChildAdjustment;
				return childAdjustment != null ? childAdjustment.WD_OH_Client : ZGuid.Empty;
			}
			set
			{
				var childAdjustment = ChildAdjustment;
				if (childAdjustment != null)
				{
					childAdjustment.WD_OH_Client = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateOwnershipAdjustedClientPK();
					}
				}

				OwnershipAdjustedClientPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OwnershipAdjustedClientPKInfo
		{
			get { return GetZPropertyInfo(nameof(OwnershipAdjustedClientPK)); }
		}

		public OrgHeader OwnershipAdjustedClient
		{
			get
			{
				var childAdjustment = ChildAdjustment;
				return childAdjustment != null ? Factory.Load<OrgHeader>(childAdjustment.WD_OH_Client) : null;
			}
		}

		public bool IsNewOwnershipAdjustmentParent
		{
			get { return WD_DocketSubType == AdjustmentType.Codes.OwnershipAdjustment && WD_WD_ParentDocket.IsEmpty; }
		}

		public bool IsNewOwnershipAdjustmentChild
		{
			get { return (WD_DocketSubType == AdjustmentType.Codes.OwnershipAdjustment && !WD_WD_ParentDocket.IsEmpty); }
		}

		#endregion

		#region CanCreateInventoryCore

		protected override bool CanCreateInventoryCore
		{
			get { return true; }
		}

		#endregion

		#region ManualFinaliseReadonly

		public bool ManualFinaliseReadonly
		{
			get { return IsNewOwnershipAdjustmentChild || IsFinalised; }
		}

		#endregion

		#region Readonly

		protected override bool StandardReadOnly
		{
			get { return base.StandardReadOnly || IsNewOwnershipAdjustmentChild; }
		}

		protected override bool NonStandardReadOnly1
		{
			get { return base.NonStandardReadOnly1 || IsNewOwnershipAdjustmentChild; }
		}

		#endregion

		#region WD_FinalisedDate

		public override ZDateTimeOffset WD_FinalisedDate
		{
			get => base.WD_FinalisedDate;
			set
			{
				base.WD_FinalisedDate = value;
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsAdjustment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_CriticalChangesVersionID), WD_FinalisedDate.IsValid ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Ignore);
			}
		}

		#endregion

		#endregion

		#region AttemptDodgyBondedFinalise

		/// <summary>
		/// Normally you cannot adjust out stock that was adjusted in on the same Adjustment, you need to commit
		/// inventory that is currently in the Warehouse. Old Bonded code relies on being able to Adjust out
		/// stock Adjusted in on the same Adjustment and this method is used to temporarily allow this shitty behaviour.
		/// Remove this code when the Old Bonded Logic is removed.
		/// </summary>
		internal IDisposable AttemptDodgyBondedFinalise()
		{
			return new SemaphoreManager(DodgyBondedFinaliseSemaphore);
		}

		internal bool IsAttemptingDodgyBondedFinalise
		{
			get { return DodgyBondedFinaliseSemaphore.IsSuspended; }
		}

		Semaphore DodgyBondedFinaliseSemaphore
		{
			get { return dodgyBondedFinaliseSemaphore ?? (dodgyBondedFinaliseSemaphore = new Semaphore()); }
		}

		Semaphore dodgyBondedFinaliseSemaphore;

		#endregion

		#region Finalisation

		protected override bool FinaliseDocketCore()
		{
			AdjustStock();

			var successfulFinalise = !HasErrors;
			if (!successfulFinalise)
			{
				RollbackAdjustmentLines();
			}
			else if (IsNewOwnershipAdjustmentParent)
			{
				var childAdjustment = FinaliseNewOwnershipAdjustment();
				if (!childAdjustment.IsFinalised)
				{
					successfulFinalise = false;
					RollbackAdjustmentLines();
					AddRowError(WhsErrorTypes.CannotFinaliseChildAdjustment.Message);
				}
			}
			if (successfulFinalise)
			{
				var distinctLocations = IEnumerableExtensions.DistinctBy(Lines, dl => dl.WE_WL).Where(l => l.WE_WL.IsValid).Select(l => l.Location);
				distinctLocations.ForEach(l => l.OnStockOnHandChanged());
				var affectLinesFlag = AdjustmentHasPositiveIncreaseAndDecreaseChangesThatAffectAdjustmentLineLocationCapacity(distinctLocations);
				Lines.Cast<WhsAdjustmentLine>().ForEach(l => l.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity = affectLinesFlag);
			}

			return successfulFinalise;
		}

		bool AdjustmentHasPositiveIncreaseAndDecreaseChangesThatAffectAdjustmentLineLocationCapacity(IEnumerable<WhsLocation> distinctLocations)
		{
			var validLocationPKs = distinctLocations.Where(l => l.HasCapacityLimitation).Select(l => l.PK);
			var locationPKs = new HashSet<ZGuid>(validLocationPKs);
			var validlocationsWithPositiveAdjustment = Lines.Where(l => l.WE_TransactionQuantity > 0 && locationPKs.Contains(l.WE_WL)).Select(l => l.WE_WL);
			var validLocationsWithNegativeAdjustment = Lines.Where(l => l.WE_TransactionQuantity < 0 && locationPKs.Contains(l.WE_WL)).Select(l => l.WE_WL);

			return validlocationsWithPositiveAdjustment.Intersect(validLocationsWithNegativeAdjustment).Any();
		}

		#region AdjustStock

		void AdjustStock()
		{
			var sortedLines = Lines.ToArray<WhsAdjustmentLine>();
			if (!IsCustomsTransaction) // old bonded logic is sorting lines they own way. #warning Remove this when Old Logic is Removed.
			{
				Array.Sort(sortedLines, (x, y) => x.WE_TransactionQuantity.CompareTo(y.WE_TransactionQuantity)); // put stock removal lines before stock adding lines.
			}

			int firstAdjustOutLineIndexForOldBondedFinalise = IsAttemptingDodgyBondedFinalise ? Array.FindIndex(sortedLines, l => l.WE_TransactionQuantity < 0m) : -1;
			if (firstAdjustOutLineIndexForOldBondedFinalise >= 0)
			{
				DodgyBondedAdjustLines(sortedLines, firstAdjustOutLineIndexForOldBondedFinalise);
			}
			else
			{
				AdjustLines(sortedLines);
			}
		}

		#region DodgyBondedAdjustLines

		/// <summary>
		/// Old Bonded code needs to be able to Adjust out stock for stock that was Adjusted in on the same Job.
		/// Normally this is not allowed, as you need to commit Inventory when Adjusting out to inventory currently
		/// in the Warehouse. If we are attempting to do the Old Bonded Finalisation, we will finalise all the
		/// Adjustment Ins first and then run the committment logic via RunPreSaveValidation() and then finalise
		/// all the Adjustment Outs. Remove this code when Old Bonded Logic is removed.
		/// </summary>
		void DodgyBondedAdjustLines(WhsAdjustmentLine[] sortedLines, int firstAdjustOutLineIndex)
		{
			var adjustInLines = new WhsAdjustmentLine[firstAdjustOutLineIndex];
			var adjustOutAdjustmentLines = new WhsAdjustmentLine[sortedLines.Length - firstAdjustOutLineIndex];
			Array.Copy(sortedLines, 0, adjustInLines, 0, firstAdjustOutLineIndex);
			Array.Copy(sortedLines, firstAdjustOutLineIndex, adjustOutAdjustmentLines, 0, sortedLines.Length - firstAdjustOutLineIndex);

			AdjustLines(adjustInLines);
			Array.ForEach(adjustOutAdjustmentLines, l => l.RunPreSaveValidation());
			AdjustLines(adjustOutAdjustmentLines);
		}

		#endregion

		#region AdjustLines

		void AdjustLines(WhsAdjustmentLine[] linesToAdjust)
		{
			foreach (var line in linesToAdjust)
			{
				AdjustLine(line);
				if (IsCustomsTransaction && !line.WE_BondedEntryKey.IsEmpty)
				{
					CalculateCustomsData(line);
				}
			}
		}

		#region AdjustLine

		void AdjustLine(WhsAdjustmentLine line)
		{
			if (line.IsAdjustmentIn)
			{
				Increase(line);
			}
			else
			{
				Decrease(line);
			}
		}

		void Increase(WhsAdjustmentLine line)
		{
			if (line.WE_AdjustmentArrivalDate.IsEmpty)
			{
				line.WE_AdjustmentArrivalDate = Warehouse.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today);
			}

			line.IncreaseStockInLocation(line.PK, line.WE_TransactionQuantity);
		}

		static void Decrease(WhsAdjustmentLine line)
		{
			line.DecreaseStockInLocation(line.WE_TransactionQuantityInfo);
		}

		#endregion

		#region CalculateCustomsData

		void CalculateCustomsData(WhsAdjustmentLine line)
		{
			var parser = new EntryLineCodeParser(line.WE_BondedEntryKey);
			line.CustomsData.WB_EntryKey = parser.EntryNumber;
			line.CustomsData.WB_EntryLineNo = parser.LineNumber;

			WhsBondedCalculator.SetCustomsData(line);
		}

		#endregion

		#endregion

		#endregion

		#region RollbackAdjustmentLines

		void RollbackAdjustmentLines()
		{
			foreach (var relation in Lines.SelectMany(l => l.SupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().Where(r => !r.IsInDatabase)).ToList())
			{
				relation.Delete();
			}

			foreach (var paramsByWhsAndClient in Lines.SelectMany(l => l.Product.ParamsByWhsAndClient.Where(r => !r.IsInDatabase)).ToList())
			{
				paramsByWhsAndClient.Delete();
			}

			ReloadAnyChangedInventory();

			if (IsNewOwnershipAdjustmentChild)
			{
				Lines.DeleteAll();
			}
			else
			{
				foreach (WhsAdjustmentLine line in Lines)
				{
					RollbackFinaliseChanges(line);
				}
			}
		}

		void ReloadAnyChangedInventory()
		{
			foreach (WhsAdjustmentLine line in Lines)
			{
				line.ReloadChangedInventoryOnRollback();
			}
		}

		static void RollbackFinaliseChanges(WhsAdjustmentLine line)
		{
			line.AddErrorsFromCreatedInventory();
			line.Inventory.RemoveAndDeleteAll();
			line.WE_StockOnHand = 0m;
		}

		#endregion

		#region FinaliseNewOwnershipAdjustment

		WhsAdjustment FinaliseNewOwnershipAdjustment()
		{
			var childAdjustment = ChildAdjustment;
			CopyAdjustmentLines(childAdjustment);
			childAdjustment.FinaliseDocketWithoutUserConfirmation();

			return childAdjustment;
		}

		void CopyAdjustmentLines(WhsAdjustment newAdjustment)
		{
			var oldClient = Client;
			var newClient = newAdjustment.Client;
			var productMapping = new Dictionary<ZGuid, ZGuid>();
			var productCodes = IEnumerableExtensions.DistinctBy(Lines, dl => dl.WE_OP).Select(l => l.SupplierPart.OP_PartNum).ToArray();
			var partsWithSameCodeForNewOwner = FindProductsWithSameProductCodeAndNewOwner(Factory, newClient, productCodes).ToDictionary(p => (string)p.OP_PartNum, StringComparer.OrdinalIgnoreCase);

			foreach (WhsAdjustmentLine line in Lines)
			{
				var newLine = line.Clone<WhsAdjustmentLine>();
				newLine.WE_TransactionQuantity = Math.Abs(line.WE_TransactionQuantity);

				CreateOrgPartRelationIfNotExist(newLine, oldClient, newClient, productMapping, partsWithSameCodeForNewOwner);
				CreateMaximumShelfLifeTimeIfNotExist(line.Product, oldClient, newClient, WD_WW_Whs);
				UpdatePartAttributeValue(newLine, line, oldClient, newClient);
				newAdjustment.Lines.Add(newLine);
			}
		}

		static OrgSupplierPart[] FindProductsWithSameProductCodeAndNewOwner(BusinessObjectFactory factory, OrgHeader newClient, ZString[] productCodes)
		{
			var relationQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationQuery.AddToFilter(OrgPartRelationSchema.OU_OH, newClient.PK);
			relationQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });

			var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCodes);
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
			productQuery.AddSubQuery(relationQuery, JoinCondition.And);

			return factory.Load<OrgSupplierPart>(productQuery);
		}

		static void CreateOrgPartRelationIfNotExist(WhsAdjustmentLine newLine, OrgHeader oldClient, OrgHeader newClient, Dictionary<ZGuid, ZGuid> productMapping, Dictionary<string, OrgSupplierPart> partsWithSameCodeForNewOwner)
		{
			if (!productMapping.TryGetValue(newLine.WE_OP, out var mappedProduct))
			{
				OrgSupplierPart productForNewOwner = null;

				var part = newLine.SupplierPart;
				var productRelationships = part.RelatedOrganisations;
				var relation = productRelationships.FindByOrganisationAndRelationship(newClient, OrgPartRelation.RelationshipTypes.Owner);
				if (relation == null && !partsWithSameCodeForNewOwner.TryGetValue(part.OP_PartNum, out productForNewOwner))
				{
					relation = CreateNewRelationship(oldClient, newClient, productRelationships);
				}

				productMapping[newLine.WE_OP] = mappedProduct = productForNewOwner?.PK ?? relation.OU_OP;
			}

			if (newLine.WE_OP != mappedProduct)
			{
				newLine.WE_OP = mappedProduct;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		static OrgPartRelation CreateNewRelationship(OrgHeader oldClient, OrgHeader newClient, OrgPartRelationCollection productRelationships)
		{
			var oldClientRelation = productRelationships.FindByOrganisationAndRelationship(oldClient, OrgPartRelation.RelationshipTypes.Owner);
			var relation = productRelationships.AddNew();
			relation.OU_OH = newClient.PK;
			relation.OU_OP = oldClientRelation.OU_OP;
			relation.OU_Relationship = oldClientRelation.OU_Relationship;
			relation.OU_UseExpiryDate = oldClientRelation.OU_UseExpiryDate;
			relation.OU_UsePackingDate = oldClientRelation.OU_UsePackingDate;
			relation.OU_UseSerialNumber = oldClientRelation.OU_UseSerialNumber;

			var attributeNumbers = new List<PartAttributeNumber>();
			var oldClientsMiscServ = oldClient.MiscServ;
			var newClientsMiscServ = newClient.MiscServ;
			var matchingAttributeNumberForAttrbuteOne = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientsMiscServ.OM_IMPartAttrib1Type, oldClientsMiscServ.OM_IMPartAttrib1Name, newClientsMiscServ, attributeNumbers);
			var numberForAttrbuteTwo = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientsMiscServ.OM_IMPartAttrib2Type, oldClientsMiscServ.OM_IMPartAttrib2Name, newClientsMiscServ, attributeNumbers);
			var numberForAttrbuteThree = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientsMiscServ.OM_IMPartAttrib3Type, oldClientsMiscServ.OM_IMPartAttrib3Name, newClientsMiscServ, attributeNumbers);
			PartAttributeUpdateHelper.SetPartAttributeUsage(relation, matchingAttributeNumberForAttrbuteOne, oldClientRelation.OU_UsePartAttrib1);
			PartAttributeUpdateHelper.SetPartAttributeUsage(relation, numberForAttrbuteTwo, oldClientRelation.OU_UsePartAttrib2);
			PartAttributeUpdateHelper.SetPartAttributeUsage(relation, numberForAttrbuteThree, oldClientRelation.OU_UsePartAttrib3);
			UpdateDateFormats(oldClientRelation, relation);

			return relation;
		}

		void CreateMaximumShelfLifeTimeIfNotExist(WhsProduct product, OrgHeader oldClient, OrgHeader newClient, ZGuid warehousePK)
		{
			var oldClientProductParams = product.ParamsByWhsAndClient.FindWhsProductParamsByWhsAndClient(oldClient.OH_Code, warehousePK);
			if (oldClientProductParams != null && product.IsAJulianBatchNumberAttributeUsed(oldClient))
			{
				var newClientProductParams = product.ParamsByWhsAndClient.FindWhsProductParamsByWhsAndClient(newClient.OH_Code, warehousePK);
				if (oldClientProductParams.W3_MaximumShelfLife > 0 && newClientProductParams == null)
				{
					newClientProductParams = product.ParamsByWhsAndClient.AddNew();
					newClientProductParams.W3_OH = newClient.PK;
					newClientProductParams.W3_WW = warehousePK;
					newClientProductParams.W3_OP = product.Parent.PK;
					newClientProductParams.W3_MaximumShelfLife = oldClientProductParams.W3_MaximumShelfLife;
				}
			}
		}

		static void UpdateDateFormats(OrgPartRelation oldClientRelation, OrgPartRelation newClientRelation)
		{
			newClientRelation.OU_PackingDateFormatString = oldClientRelation.OU_PackingDateFormatString;
			newClientRelation.OU_ExpiryDateFormatString = oldClientRelation.OU_ExpiryDateFormatString;
			newClientRelation.OU_JulianBatchNoFormat = oldClientRelation.OU_JulianBatchNoFormat;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		static void UpdatePartAttributeValue(WhsAdjustmentLine newLine, WhsAdjustmentLine oldLine, OrgHeader oldClient, OrgHeader newClient)
		{
			var oldClientsMiscServ = oldClient.MiscServ;
			var newClientsMiscServ = newClient.MiscServ;
			var attributeNumbers = new List<PartAttributeNumber>();
			var matchingAttributeNumberForAttrbuteOne = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientsMiscServ.OM_IMPartAttrib1Type, oldClientsMiscServ.OM_IMPartAttrib1Name, newClientsMiscServ, attributeNumbers);
			var matchingAttributeNumberForAttrbuteTwo = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientsMiscServ.OM_IMPartAttrib2Type, oldClientsMiscServ.OM_IMPartAttrib2Name, newClientsMiscServ, attributeNumbers);
			var matchingAttributeNumberForAttrbuteThree = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientsMiscServ.OM_IMPartAttrib3Type, oldClientsMiscServ.OM_IMPartAttrib3Name, newClientsMiscServ, attributeNumbers);
			PartAttributeUpdateHelper.SetPartAttributeValue(newLine, matchingAttributeNumberForAttrbuteOne, oldLine.WE_PartAttrib1);
			PartAttributeUpdateHelper.SetPartAttributeValue(newLine, matchingAttributeNumberForAttrbuteTwo, oldLine.WE_PartAttrib2);
			PartAttributeUpdateHelper.SetPartAttributeValue(newLine, matchingAttributeNumberForAttrbuteThree, oldLine.WE_PartAttrib3);
		}

		#endregion

		#region OnFinaliseSucceeded

		protected override void OnFinaliseSucceeded()
		{
			base.OnFinaliseSucceeded();

			var currentUser = GlbStaff.CurrentUser.GS_Code;

			foreach (WhsAdjustmentLine line in Lines.Where(l => l.WE_TransactionQuantity < 0m)) // adjustment outs
			{
				foreach (var pickLine in line.PickLines)
				{
					pickLine.WZ_GS_NKAssignedTo = currentUser;
					pickLine.WZ_PickedDateTime = WD_FinalisedDate;
				}
			}
		}

		#endregion

		#region RunPreFinaliseValidationCore

		protected override bool RunPreFinaliseValidationCore()
		{
			foreach (WhsAdjustmentLine line in Lines)
			{
				RunPreFinaliseLineValidation(line);
			}

			CheckIfOwnershipAdjustmentDoesNotClashWithBarcodes();

			return base.RunPreFinaliseValidationCore();
		}

		void CheckIfOwnershipAdjustmentDoesNotClashWithBarcodes()
		{
			var newClientForOwnershipChange = IsNewOwnershipAdjustmentParent ? OwnershipAdjustedClient : null;
			if (newClientForOwnershipChange != null)
			{
				var partNumbers = IEnumerableExtensions.DistinctBy(Lines, dl => dl.WE_OP).Select(dl => dl.SupplierPart.OP_PartNum).ToArray();

				var relationQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				relationQuery.AddToFilter(OrgPartRelationSchema.OU_OH, newClientForOwnershipChange.PK);
				relationQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });

				var productQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartBarcodeSchema.PH_OP);
				productQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
				productQuery.AddSubQuery(relationQuery, JoinCondition.And);

				var barcodeQuery = new ZDBOnlyQuery(typeof(OrgSupplierPartBarcode));
				barcodeQuery.AddToFilter(OrgSupplierPartBarcodeSchema.PH_Barcode, partNumbers);
				barcodeQuery.AddSubQuery(productQuery, JoinCondition.And);

				var duplicateBarcodes = Factory.Load<OrgSupplierPartBarcode>(barcodeQuery);

				foreach (var duplicateBarcode in duplicateBarcodes)
				{
					AddRowError(Res.GetString("a800fb70-4b83-44fc-9c17-3a730a63e384", "Product Code '{0}' is used as a Barcode for Owner '{1}' on Product '{2}'. Unable to process Ownership change.", duplicateBarcode.PH_Barcode, newClientForOwnershipChange.OH_Code, duplicateBarcode.SupplierPart.OP_PartNum));
				}
			}
		}

		void RunPreFinaliseLineValidation(WhsAdjustmentLine line)
		{
			// a unreproducable bug is allowing null locations to slip through
			// so double check locations entered here. CM
			line.Validation.ValidateWE_WL();
		}

		protected override ZGuid FinaliseConfirmationDialogIdentifier
		{
			get { return new ZGuid("7b424a1a-43c1-403d-99cf-969d0c56babd"); }
		}

		protected override string FinaliseConfirmationMessage
		{
			get { return Res.GetString("2bf2442d-d314-43d7-a0dd-d9d5153ecc47", "Finalizing this Adjustment will update the inventory.\r\nOn finalization, this Adjustment will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\nDo you wish to Finalize this Adjustment?"); }
		}

		#endregion

		protected override bool AddFetchHintsForInventory
		{
			get { return true; }
		}

		#region UserEnteredFinalisedDate

		public ZDateTime UserEnteredFinalisedDate
		{
			get { return fUserEnteredFinalisedDate; }
			set { SetNonPersistentPropertyValue(UserEnteredFinalisedDateInfo, ref fUserEnteredFinalisedDate, value); }
		}
		ZDateTime fUserEnteredFinalisedDate;

		public ZPropertyInfo UserEnteredFinalisedDateInfo
		{
			get { return GetZPropertyInfo(nameof(UserEnteredFinalisedDate)); }
		}

		#endregion

		#endregion

		#region SetDefaultLocationsForBondWarehouse

		void SetDefaultLocationsForBondWarehouse()
		{
			if (Warehouse.IsWarehouseBondEnabled)
			{
				var defaultPK = Warehouse?.DefaultLocation?.PK ?? ZGuid.Empty;
				foreach (WhsAdjustmentLine line in Lines)
				{
					line.WE_WL = defaultPK;
				}
			}
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		protected override bool RequiresCreditCheck
		{
			get { return false; }
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, "WAD");
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsAdjustmentDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IWorkflowProvider Members

		protected override ZString GetWorkflowType()
		{
			return WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;
		}

		protected override WhsDocketProcessTasksCollection GetNewProcessTasksCollection()
		{
			return new WhsAdjustmentProcessTasksCollection(this);
		}

		#endregion

		#region IRelatedJobs Members

		protected override ControllerID JobControllerId
		{
			get { return ControllerIDs.WhsAdjustment; }
		}

		#endregion

		#region Related Jobs Core

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var result = base.GetRelatedJobsCore();
			if (IsNewOwnershipAdjustmentParent || IsNewOwnershipAdjustmentChild)
			{
				result.AddRange(GetRelatedAdjustments());
			}
			return result;
		}

		IEnumerable<IRelatedJob> GetRelatedAdjustments()
		{
			return Factory.Load<WhsAdjustment>(GetRelatedAdjustmentsFilter());
		}

		ZQuery GetRelatedAdjustmentsFilter()
		{
			var condition = new ZQuery();
			condition.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, PK);
			condition.AddToFilter(JoinCondition.Or, WhsDocketSchema.PK, WD_WD_ParentDocket);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
			query.AddToFilter(condition);
			return query;
		}

		#endregion

		#region ISendEmailSource Members

		protected override string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.WarehouseAdjustment;
		}

		#endregion

		#region IWhsLogEvents Members

		protected override string GetDocketEventReferenceParameterType
		{
			get { return Constants.EventReferenceParameterTypes.Adjustment; }
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new WhsAdjustmentRatingAdaptersProvider(this); }
		}

		#endregion

		#region IJobInvoicingPlugin Members

		protected override WhsDocketInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsAdjustmentInvoicingSupporter(this);
		}

		#endregion

		#region IPickedStockAdjuster members

		void IPickedStockAdjuster.AdjustOutInventory(WhsInventoryView inventory, ZGuid originalInventoryPK, ZDecimal quantity)
		{
			Argument.NotNull(inventory, nameof(inventory));
			if (quantity <= 0)
			{
				throw new ArgumentException("Quantity must be positive.");
			}

			if (quantity > inventory.WI_TotalUnits)
			{
				throw new ArgumentException("Cannot record lost stock more than inventory TotalUnits.");
			}

			if (inventory.WI_OH_Client != WD_OH_Client)
			{
				throw new ArgumentException("Client for inventory is different.");
			}

			if (inventory.WI_WW_Whs != WD_WW_Whs)
			{
				throw new ArgumentException("Warehouse for inventory is different.");
			}

			if (!originalInventoryPK.IsValid)
			{
				throw new ArgumentException("Original Inventory must be a valid Guid.");
			}

			var line = Lines.AddNew();
			line.WE_OP = inventory.WI_OP;
			line.WE_F3_NKPackType = line.ProductUQ;
			line.WE_TransactionQuantity = -quantity; // negative quantity as this stock is lost
			line.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.DamagedStock;
			line.WE_OriginalInventoryStatus = inventory.WI_InventoryStatus;
			line.WE_PartAttrib1 = inventory.WI_PartAttrib1;
			line.WE_PartAttrib2 = inventory.WI_PartAttrib2;
			line.WE_PartAttrib3 = inventory.WI_PartAttrib3;
			line.WE_PackingDate = inventory.WI_PackingDate;
			line.WE_ExpiryDate = inventory.WI_ExpiryDate;
			line.WE_SerialNumber = inventory.WI_SerialNumber;
			line.WE_PalletID = inventory.WI_PalletID;
			line.WE_WL = inventory.WI_WL;

			var pickLine = line.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			pickLine.WZ_Units = quantity;
			if (inventory.PK != originalInventoryPK)
			{
				pickLine.WZ_WE_OriginalPickedInventoryLine = originalInventoryPK;
			}
		}

		void IPickedStockAdjuster.LinkToDocket(WhsPickableDocket docket)
		{
			Argument.NotNull(docket, nameof(docket));
			WD_WD_ParentDocket = docket.PK;
		}

		ActionResult IPickedStockAdjuster.PrepareForSaving()
		{
			FinaliseDocketWithoutUserConfirmation();

			return HasErrors ? ActionResult.Failure(this.GetErrors().ToMessageListString()) : ActionResult.Success();
		}

		#endregion

		#region ICriticalChangesVersionID members

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WD_CriticalChangesVersionID = value;
			get => WD_CriticalChangesVersionID;
		}

		bool ICriticalChangesVersionID.IsImmutableStatus => IsFinalised;

		#endregion

		#region AdjustmentLineFromInventory

		AdjustmentLineFromInventoryHelper AdjustmentLineFromInventoryHelper
		{
			get { return adjustmentLineFromInventoryHelper ?? (adjustmentLineFromInventoryHelper = new AdjustmentLineFromInventoryHelper(NotificationSubscriber, this)); }
		}
		AdjustmentLineFromInventoryHelper adjustmentLineFromInventoryHelper;

		public WhsDocketLine CreateDocketLineFromInventory(WhsInventoryView inventoryList)
		{
			return AdjustmentLineFromInventoryHelper.CreateDocketLineFromInventory(this.Lines, inventoryList);
		}

		public void AcceptInventoryLinesFromSearchGrid(BusinessObject[] inventoryList)
		{
			AdjustmentLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(this.Lines, inventoryList);
		}

		#endregion
	}

	#region Document Supporter

	public class WhsAdjustmentDocumentSupporter : DocumentSupporter
	{
		public WhsAdjustmentDocumentSupporter(WhsAdjustment adjustment)
			: base(adjustment)
		{
		}

		protected WhsAdjustment Adjustment
		{
			get { return (WhsAdjustment)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsAdjustment; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsAdjustmentCustomiseDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Core.Constants.DataContext.WhsAdjustment,
				Core.Constants.DataContext.GenericFreightJob
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			switch (dataContext)
			{
				case Constants.DataContext.WhsAdjustment:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.WhsAdjustment, Adjustment) };
					break;

				case Constants.DataContext.GenericFreightJob:
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Adjustment);
					break;
			}

			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact result = new OrgHeaderContact(Adjustment.Client, null);
			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}

	#endregion
}
