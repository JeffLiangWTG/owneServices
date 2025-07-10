using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Collection used on the Declaration that has a Complete Set of Invoice Lines for the Declaration.
	/// </summary>
	public class InvoiceLineCompleteCollection : BusinessObjectCollection<BaseJobComInvoiceLine>
	{
		public InvoiceLineCompleteCollection(BaseJobDeclaration jobDeclaration)
			: base(jobDeclaration.Factory)
		{
			JobDeclaration = jobDeclaration;
			CountChanged += InvoiceLineCompleteCollection_CountChanged;
			needsAdditionalLinkBetweenInvoiceLineAndEntryLine = JobDeclaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine;
		}
		readonly bool needsAdditionalLinkBetweenInvoiceLineAndEntryLine;

		public override void Load()
		{
			if (needsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				var query = new BaseJobComInvoiceHeader.Loader(Factory).GetQuery(JobDeclaration);
				var invoices = JobDeclaration.LoadInvoicesFromQuery(query);
				var supportAdditionalDeclarations = invoices.OfType<BaseJobComInvoiceHeader>().FirstOrDefault()?.SupportAdditionalDeclarations ?? false;
				if (supportAdditionalDeclarations)
				{
					foreach (var header in invoices)
					{
						Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, header.PK);
					}
				}
				Factory.AddFetchHint(CusUnderbondDecSchema.BU_ClusterKey, JobDeclaration.JE_ClusterKey);
			}
			using (JobDeclaration.SuspendInvoiceLineDataInitialization())
			{
				base.Load();
			}

			InitialiseInvoiceLineDataIfNeeded();
		}

		protected override void OnLoaded()
		{
			using (JobDeclaration.SuspendInvoiceLineDataInitialization())
			{
				base.OnLoaded();
			}

			InitialiseInvoiceLineDataIfNeeded();
		}

		void InitialiseInvoiceLineDataIfNeeded()
		{
			if (!hasInitialisedInvoiceLineData && JobDeclaration.ShouldInitialiseInvoiceLineData)
			{
				hasInitialisedInvoiceLineData = true;
				var invoiceLineListWithFetchHint = new List<BaseJobComInvoiceLine>();
				foreach (BaseJobComInvoiceLine invoiceLine in this)
				{
					invoiceLine.AddAddFetchHintForInitialiseData();
					invoiceLineListWithFetchHint.Add(invoiceLine);
				}
				invoiceLineListWithFetchHint.ForEach(x => x.InitialiseData(false));
				invoiceLineListWithFetchHint.ForEach(x => x.RefreshPartDetailsIfNeeded());
			}
		}
		bool hasInitialisedInvoiceLineData;

		public IDisposable SuspendInvoiceLineListChanged()
		{
			return SuspendListChanged();
		}

		public bool HasApportionedCharges
		{
			get
			{
				bool result = false;
				foreach (BaseJobComInvoiceLine invoiceLine in this)
				{
					result = invoiceLine.ApportionedCharges.Count > 0;
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		public OrgHeader GetCreateProductImporter(ProductRelationDefaultOption option, DeclarationForProductCreationHelper productCreationHelper, BaseJobComInvoiceLine invoiceLine)
		{
			OrgHeader result = null;
			if (option != ProductRelationDefaultOption.OptionForSupplier)
			{
				if (productCreationHelper.Importer != null)
				{
					result = productCreationHelper.Importer;
				}
				else if (invoiceLine != null)
				{
					result = invoiceLine.Importer;
				}
			}
			return result;
		}

		public OrgHeader GetCreateProductSupplier(ProductRelationDefaultOption option, DeclarationForProductCreationHelper productCreationHelper, BaseJobComInvoiceLine invoiceLine)
		{
			OrgHeader result = null;
			if (option != ProductRelationDefaultOption.OptionForImporter)
			{
				if (productCreationHelper.Supplier != null)
				{
					result = productCreationHelper.Supplier;
				}
				else if (invoiceLine != null)
				{
					result = invoiceLine.Supplier;
				}
			}
			return result;
		}

		public void AddNewPartClassifications()
		{
			Factory.ClearQueryCache(CusClassPartPivotSchema.Constants.TableName);

			var declaration = JobDeclaration;
			var linesHavingUnmatchedProductClassifications = this.Cast<BaseJobComInvoiceLine>().Where(invoiceLine => declaration.IsUnmatchedProductClassification(invoiceLine)).ToArray();
			ReloadPivotsAndChildren(linesHavingUnmatchedProductClassifications.Select(x => x.Part).Distinct().ToList());

			foreach (var invoiceLine in linesHavingUnmatchedProductClassifications)
			{
				if (declaration.IsUnmatchedProductClassification(invoiceLine)) // see if it still has an unmatched product in case it was added by a previous line
				{
					invoiceLine.Lookups.PartsList.AddPivotWithAdditionalLineDetails(invoiceLine.Part, invoiceLine);
				}
			}
		}

		void ReloadPivotsAndChildren(List<OrgSupplierPart> parts)
		{
			foreach (var part in parts)
			{
				part.PivotsForBinding.Load();
				foreach (BaseCusClassPartPivot pivot in part.PivotsForBinding)
				{
					pivot.Children?.Load();
				}
			}
		}

		public List<string> AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption option = ProductRelationDefaultOption.None, DeclarationForProductCreationHelper productCreationHelper = null)
		{
			Factory.ClearQueryCache(OrgSupplierPartSchema.Constants.TableName);
			Factory.ClearQueryCache(OrgPartRelationSchema.Constants.TableName);

			var hasAdditionalOrgHeader = false;
			var partNums = new HashSet<ZString>();
			var duplicateProducts = new HashSet<string>();

			var declaration = JobDeclaration;
			var collection = (productCreationHelper?.InvoiceHeader as BaseJobComInvoiceHeader)?.InvoiceLines.ToArray() ?? this.ToArray();
			var linesHavingNewProductPart = collection.Cast<BaseJobComInvoiceLine>().Where(invoiceLine => declaration.IsNotPersistentActiveProduct(invoiceLine)).ToArray();
			foreach (var invoiceLine in linesHavingNewProductPart)
			{
				if (invoiceLine.JI_OP.IsEmpty && partNums.Contains(invoiceLine.JI_PartNo))
				{
					invoiceLine.PartSyncManager.Refresh();
				}
				else if (declaration.IsNotPersistentActiveProduct(invoiceLine)) //see if it is still a new product - happens in the even that the part was added by a previous line (on this invoice header)
				{
					OrgHeader importer = null;
					OrgHeader supplier = null;
					if (option != ProductRelationDefaultOption.None && productCreationHelper != null)
					{
						supplier = GetCreateProductSupplier(option, productCreationHelper, invoiceLine);
						importer = GetCreateProductImporter(option, productCreationHelper, invoiceLine);
						hasAdditionalOrgHeader = true;
					}
					else
					{
						supplier = invoiceLine.Supplier;
						importer = invoiceLine.Importer;
					}

					if (IsDuplicateProduct(invoiceLine.JI_PartNo, supplier, importer, out var duplicateDescription))
					{
						duplicateProducts.Add(duplicateDescription);
					}
					else
					{
						var part = new MasterFiles.Business.OrgSupplierPart.Loader(Factory).Load(invoiceLine.JI_PartNo, importer, supplier, allowInactive: true) as OrgSupplierPart;
						if (part?.OP_IsActive ?? false) //See if the part already exists - happens in the event that the part was added by a previous invoice header (in this declaration).
						{
							invoiceLine.PartSyncManager.Refresh();
							continue;
						}
						else if (part == null || !DoesInactivePartMatchInvoiceLineDetail(part, supplier, importer, invoiceLine))
						{
							part = (OrgSupplierPart)invoiceLine.Lookups.PartsList.AdditionalAddNewByOrgHeader(supplier, importer, hasAdditionalOrgHeader);
							part.OP_PartNum = invoiceLine.JI_PartNo;
						}
						else if (!part.OP_IsActive)
						{
							part.OP_IsActive = true;
							part.ClearDataAndDeleteChildren();
							invoiceLine.Lookups.PartsList.UpdatePart(part, supplier, importer, hasAdditionalOrgHeader);
						}
						partNums.Add(invoiceLine.JI_PartNo);
						invoiceLine.JI_OP = part.PK; //Should not call invoiceLine.PartSyncManager.Refresh() as it will delete existing (and valid) data
					}
				}
			}

			return duplicateProducts.ToList();
		}

		bool IsDuplicateProduct(ZString partNum, OrgHeader supplier, OrgHeader importer, out string duplicateDescription)
		{
			var partParties = new List<RelatedPartyWithCode>();
			if (supplier != null)
			{
				partParties.Add(new RelatedPartyWithCode(OrgPartRelation.RelationshipTypes.Supplier, supplier.OH_Code));
			}

			if (importer != null)
			{
				partParties.Add(new RelatedPartyWithCode(OrgPartRelation.RelationshipTypes.Owner, importer.OH_Code));
			}

			var duplicateDetector = new DuplicateProductDetectorNoBizO(ZGuid.NewZGuid(), partNum, true, partParties, null);
			duplicateDetector.Validate();
			if (duplicateDetector.HasError)
			{
				duplicateDescription = Res.GetString(
					"{698EFA89-0059-459F-8BDF-0937E39DEB5D}",
					"{0} (Owner = {1}, Supplier = {2})",
					partNum,
					duplicateDetector.OwnerCodeFromLastErrorOrWarning.Trim(),
					duplicateDetector.SupplierCodeFromLastErrorOrWarning.Trim()
				);
				return true;
			}

			duplicateDescription = null;
			return false;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool DoesInactivePartMatchInvoiceLineDetail(OrgSupplierPart inactivePart, OrgHeader supplier, OrgHeader importer, BaseJobComInvoiceLine invoiceLine)
		{
			var result = !inactivePart.OP_IsActive;
			if (result)
			{
				var supplierToMatch = supplier == null ? ZGuid.Empty : supplier.PK;
				var importerToMatch = importer == null ? ZGuid.Empty : importer.PK;
				var supplierPKs = new List<ZGuid>();
				var buyerPKs = new List<ZGuid>();
				foreach (OrgPartRelation partRelation in inactivePart.RelatedOrganisations)
				{
					var partSupplier = ZGuid.Empty;
					var partOwner = ZGuid.Empty;
					switch (partRelation.OU_Relationship)
					{
						case OrgPartRelation.RelationshipTypes.Both:
							partSupplier = partRelation.OU_OH;
							partOwner = partSupplier;
							AddToList(supplierPKs, partSupplier);
							AddToList(buyerPKs, partOwner);
							break;
						case OrgPartRelation.RelationshipTypes.Supplier:
							partSupplier = partRelation.OU_OH;
							AddToList(supplierPKs, partSupplier);
							break;
						case OrgPartRelation.RelationshipTypes.Owner:
							partOwner = partRelation.OU_OH;
							AddToList(buyerPKs, partOwner);
							break;
					}
					if (buyerPKs.Count > 1 || supplierPKs.Count > 1
						|| (!partOwner.IsEmpty && partOwner != (importerToMatch.IsEmpty ? (partOwner == partSupplier ? partOwner : ZGuid.Empty) : importerToMatch))
						|| (!partSupplier.IsEmpty && partSupplier != (supplierToMatch.IsEmpty ? (partOwner == partSupplier ? partSupplier : ZGuid.Empty) : supplierToMatch)))
					{
						result = false;
						break;
					}
				}

				if (result && (
					(((supplierToMatch.IsEmpty && supplierPKs.Count != 0) || (importerToMatch.IsEmpty && buyerPKs.Count != 0)) && (supplierPKs.Count != 1 || buyerPKs.Count != 1 || supplierPKs[0] != buyerPKs[0]))
					|| (!supplierToMatch.IsEmpty && (supplierPKs.Count != 1 || supplierPKs[0] != supplierToMatch))
					|| (!importerToMatch.IsEmpty && (buyerPKs.Count != 1 || buyerPKs[0] != importerToMatch))))
				{
					result = false;
				}
				if (result)
				{
					result = inactivePart.DoesPartMatchPivotForInactiveCheck(invoiceLine);
				}
			}
			return result;
		}

		void AddToList(List<ZGuid> list, ZGuid pk)
		{
			if (!pk.IsEmpty && !list.Contains(pk))
			{
				list.Add(pk);
			}
		}

		#region BOM Proceesing

		public void ExpandAllBOMProductLines()
		{
			foreach (BaseJobComInvoiceLine line in this.ToArray())
			{
				ExpandOneBOMProductLine(line);
			}
		}

		public void ExpandOneBOMProductLine(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null && invoiceLine.IsBOMParentLine && !invoiceLine.IsBOMLineExpanded)
			{
				parentLineNumber = invoiceLine.JI_LineNo;
				ExpandBOMProductLineInternal(invoiceLine);
			}
		}
		ZShort parentLineNumber;

		void ExpandBOMProductLineInternal(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.Part != null)
			{
				foreach (OrgPartBOM orgPartBOM in invoiceLine.Part.BillOfMaterials)
				{
					if (orgPartBOM.Component != null)
					{
						BaseJobComInvoiceLine newLine = this.AddNew();
						parentLineNumber++;
						newLine.JI_LineNo = parentLineNumber;
						newLine.JI_PartNo = orgPartBOM.Component.OP_PartNum;
						newLine.JI_InvoiceUQ = orgPartBOM.OE_F3_NKPackType;
						ZDecimal quantity = invoiceLine.JI_InvoiceQuantity;
						if (invoiceLine.JI_InvoiceUQ != invoiceLine.Part.OP_StockKeepingUnit)
						{
							quantity = invoiceLine.UnitConverter.Convert(quantity, invoiceLine.ConvertedInvoiceUnit, invoiceLine.Part.OP_StockKeepingUnit);
						}
						quantity = orgPartBOM.OE_ComponentQty * quantity;
						newLine.JI_InvoiceQuantity = quantity;
						newLine.BOMParentLine = invoiceLine;
						if (newLine.IsBOMParentLine)
						{
							ExpandBOMProductLineInternal(newLine);
						}
					}
				}
			}
			invoiceLine.IsBOMLineExpanded = true;
		}

		public void CollapseAllBOMProductLines()
		{
			foreach (BaseJobComInvoiceLine line in this.ToArray())
			{
				CollapseOneBOMProductLine(line);
			}
		}

		public void CollapseOneBOMProductLine(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null && !invoiceLine.IsDeleted)
			{
				if (invoiceLine.IsBOMLineExpanded)
				{
					foreach (BaseJobComInvoiceLine line in this.ToArray())
					{
						if (line.BOMParentLine == invoiceLine)
						{
							if (line.IsBOMLineExpanded)
							{
								CollapseOneBOMProductLine(line);
							}
							line.Delete();
						}
					}
					invoiceLine.IsBOMLineExpanded = false;
				}
			}
		}

		#endregion

		public void LoadStmNoteFetchHintIfNeeded()
		{
			if (!hasLoadedStmNoteFetchHint)
			{
				hasLoadedStmNoteFetchHint = true;
				foreach (BaseJobComInvoiceLine invoiceLine in this)
				{
					Factory.AddFetchHint(StmNoteSchema.ST_ParentID, invoiceLine.PK);
				}
			}
		}
		bool hasLoadedStmNoteFetchHint;

		public void AddAdditionalEntryLineLinksFetchHintIfNeeded()
		{
			if (!hasAddAdditionalEntryLineLinksFetchHint)
			{
				hasAddAdditionalEntryLineLinksFetchHint = true;
				foreach (BaseJobComInvoiceLine invoiceLine in this)
				{
					Factory.AddFetchHint(CusUnderbondDecSchema.BU_JI, invoiceLine.PK);
				}
			}
		}
		bool hasAddAdditionalEntryLineLinksFetchHint;

		#region Implementation

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new FetchStrategies.InvoiceLineCompleteCollectionFetchStrategy(this);
		}

		protected readonly BaseJobDeclaration JobDeclaration;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			result.AddToFilter(InvoiceLineFilterForDeclaration.GetFilter(JobDeclaration));

			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			BaseJobComInvoiceLine invoiceLine = (BaseJobComInvoiceLine)child;

			using (invoiceLine.GetValidationSuspender())
			using (invoiceLine.SuspendSettingHasChanges())
			{
				if (Count == 0)
				{
					SetDefaultForFirstInvoiceLine(invoiceLine);
				}
				else if (Count > 0)
				{
					BaseJobComInvoiceLine previousLine = this[Count - 1];

					SetDefaultFromPreviousLine(previousLine, invoiceLine);
				}
				SetDefaultForCommonInvoiceLine(invoiceLine);
			}
		}

		protected virtual void SetDefaultForCommonInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
		}

		void AutoAllocateContainerToInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			if (ShouldAutoAllocateContainerToInvoiceLine && JobDeclaration.IsPersistent && JobDeclaration.CusContainers.Count == 1 && invoiceLine.IsContainerisedMode &&
				CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.GetValueWithoutFallback(JobDeclaration.RegistryCompanyPK, Guid.Empty, Guid.Empty))
			{
				var container = JobDeclaration.CusContainers[0];
				invoiceLine.ToggleLinkageWithContainer(container, true);
			}
		}

		protected virtual bool ShouldAutoAllocateContainerToInvoiceLine => !JobDeclaration.SupportsChcPivotBetweenInvoiceLineAndPacking;

		void AutoAllocatePackageToInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			if (ShouldAutoAllocatePackageToInvoiceLine(invoiceLine) && JobDeclaration.IsPersistent && JobDeclaration.Packages.Count == 1 &&
				CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.GetValueWithoutFallback(JobDeclaration.RegistryCompanyPK, Guid.Empty, Guid.Empty))
			{
				var package = JobDeclaration.Packages[0];
				invoiceLine.ToggleLinkageWithPackage(package, true);
			}
		}

		protected virtual bool ShouldAutoAllocatePackageToInvoiceLine(BaseJobComInvoiceLine invoiceLine) => JobDeclaration.SupportsChcPivotBetweenInvoiceLineAndPacking;

		protected virtual void SetDefaultForFirstInvoiceLine(BaseJobComInvoiceLine firstInvoiceLine)
		{
			if (JobDeclaration.Invoices.Count > 0)
			{
				firstInvoiceLine.JI_JZ = JobDeclaration.Invoices[0].PK;
			}

			var entryInstructionProvider = JobDeclaration.CustomsEntryInstructionProvider;
			var entryInstructions = entryInstructionProvider == null ? null : entryInstructionProvider.CustomsEntryInstructions;
			if (entryInstructions != null && entryInstructions.Count == 1)
			{
				var firstCEI = entryInstructions[0];
				if (firstCEI.IsPersistent)
				{   // No need to set JI_CEI on a non-persistent CEI, i.e. on the standalone invoice form
					firstInvoiceLine.JI_CEI = firstCEI.PK;
				}
			}
			string weightUQ = Env.Registry.PackageWeightUnit;
			firstInvoiceLine.JI_WeightUQ = weightUQ;
			firstInvoiceLine.JI_NetWeightUQ = weightUQ;
			firstInvoiceLine.JI_VolumeUQ = Env.Registry.PackageVolumeUnit;
		}

		protected virtual void SetDefaultFromPreviousLine(BaseJobComInvoiceLine previousLine, BaseJobComInvoiceLine currentInvoiceLine)
		{
			currentInvoiceLine.JI_JZ = previousLine.JI_JZ;
			currentInvoiceLine.JI_CEI = previousLine.JI_CEI;
			if (JobDeclaration.ShouldCopyProcedureFromPreviousInvoiceLine)
			{
				currentInvoiceLine.JI_Procedure = previousLine.JI_Procedure;
			}
			if (previousLine.EntryInstruction != null && !previousLine.EntryInstruction.IsPersistent)
			{
				currentInvoiceLine.JI_CEI = ZGuid.Empty;
			}
			currentInvoiceLine.JI_WeightUQ = previousLine.JI_WeightUQ;
			currentInvoiceLine.JI_NetWeightUQ = previousLine.JI_NetWeightUQ;
			currentInvoiceLine.JI_VolumeUQ = previousLine.JI_VolumeUQ;

			ZString previousOrderNumbers = previousLine.JI_OrderNumber;
			if (!previousOrderNumbers.IsEmpty)
			{
				ZString[] splitOrderNumber = previousLine.JI_OrderNumber.Split('-');
				ZQuery filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, splitOrderNumber[0]);

				if (new List<Order>(JobDeclaration.AttachedOrders.Find(filter)).Count == 0)
				{
					currentInvoiceLine.JI_OrderNumber = previousLine.JI_OrderNumber.Right(currentInvoiceLine.JI_OrderNumberInfo.MaxLength);
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (JobDeclaration != null)
			{
				JobDeclaration.MarkApportionmentDirty();
			}

			if (bizO is BaseJobComInvoiceLine invoiceLine && invoiceLine.InvoiceHeader is BaseJobComInvoiceHeader invoice)
			{
				invoice.InvalidateJZ_Calc_LinesEnteredCache();
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (!IsLoading && bizOAdded is BaseJobComInvoiceLine invoiceLine && invoiceLine.InvoiceHeader is BaseJobComInvoiceHeader invoice)
			{
				AutoAllocateContainerToInvoiceLine(invoiceLine);
				AutoAllocatePackageToInvoiceLine(invoiceLine);
				invoice.ReApportionLineWeightIfNeeded(true);
				invoice.InvalidateJZ_Calc_LinesEnteredCache();
			}
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);
			hasLoadedStmNoteFetchHint = false;
			hasAddAdditionalEntryLineLinksFetchHint = false;
		}

		#endregion

		#region ByEntryLine ILookup

		void InvoiceLineCompleteCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			byEntryLine = null;
			if (e.BizObject is BaseJobComInvoiceLine invoiceLine)
			{
				var additionalEntryLineLinks = invoiceLine.AdditionalEntryLineLinks;
				if (e.ItemAdded)
				{
					invoiceLine.JI_CLInfo.ValueChanged -= ResetByEntryLine_ValueChanged;
					invoiceLine.JI_CLInfo.ValueChanged += ResetByEntryLine_ValueChanged;
					additionalEntryLineLinks.CollectionCountChange -= AdditionalEntryLineLinks_CountChanged;
					additionalEntryLineLinks.CollectionCountChange += AdditionalEntryLineLinks_CountChanged;
					additionalEntryLineLinks.CountChanged -= AdditionalEntryLineLinksOnCountChanged;
					additionalEntryLineLinks.CountChanged += AdditionalEntryLineLinksOnCountChanged;
					additionalEntryLineLinks.ForEach(link =>
					{
						link.BU_CLInfo.ValueChanged -= ResetByEntryLine_ValueChanged;
						link.BU_CLInfo.ValueChanged += ResetByEntryLine_ValueChanged;
					});
				}
				else if (e.ItemRemoved)
				{
					invoiceLine.JI_CLInfo.ValueChanged -= ResetByEntryLine_ValueChanged;
					additionalEntryLineLinks.CollectionCountChange -= AdditionalEntryLineLinks_CountChanged;
					additionalEntryLineLinks.CountChanged -= AdditionalEntryLineLinksOnCountChanged;
					additionalEntryLineLinks.ForEach(link => link.BU_CLInfo.ValueChanged -= ResetByEntryLine_ValueChanged);
				}
			}
		}

		void AdditionalEntryLineLinksOnCountChanged(object sender, EventArgs e)
		{
			byEntryLine = null;
		}

		void ResetByEntryLine_ValueChanged(object sender, EventArgs e)
		{
			byEntryLine = null;
		}

		void AdditionalEntryLineLinks_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			byEntryLine = null;
			if (e.ItemAdded)
			{
				var bizObj = (AdditionalInvoiceLineEntryLineLink)e.BizObject;
				bizObj.BU_CLInfo.ValueChanged -= ResetByEntryLine_ValueChanged;
				bizObj.BU_CLInfo.ValueChanged += ResetByEntryLine_ValueChanged;
			}
			else if (e.ItemRemoved)
			{
				((AdditionalInvoiceLineEntryLineLink)e.BizObject).BU_CLInfo.ValueChanged -= ResetByEntryLine_ValueChanged;
			}
		}

		public ILookup<ZGuid, BaseJobComInvoiceLine> ByEntryLine => byEntryLine ?? (byEntryLine = this.Cast<BaseJobComInvoiceLine>()
			.SelectMany(invoiceLine =>
			{
				IEnumerable<(ZGuid, BaseJobComInvoiceLine)> result = new (ZGuid, BaseJobComInvoiceLine)[] { (invoiceLine.JI_CL, invoiceLine) };
				if (needsAdditionalLinkBetweenInvoiceLineAndEntryLine)
				{
					result = result.Union(invoiceLine.AdditionalEntryLineLinks.Select(link => (link.BU_CL, invoiceLine)));
				}
				return result;
			})
			.ToLookup(tuple => tuple.Item1, tuple => tuple.Item2));

		ILookup<ZGuid, BaseJobComInvoiceLine> byEntryLine;

		#endregion

		#region SetOverrideDeclaration

		public void SetOverrideDeclaration(BaseJobDeclaration declaration)
		{
			if (declaration == null || declaration.SupportAdditionalInvoices)
			{
				foreach (BaseJobComInvoiceLine line in this)
				{
					line.OverrideParent = declaration;
					if (declaration == null)
					{
						line.RefreshDeclaration();
					}
				}
			}
		}

		#endregion
	}
}
