using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderActiveCollection : ActiveBusinessObjectCollection<BaseJobComInvoiceHeader>, IExternalFactoryRefreshable, IInvoiceHeaderActiveCollection
	{
		#region Constructors

		/// <summary>
		/// For JobDeclaration.Invoices which has all invoices whose JZ_JE points to the passed declaration
		/// </summary>
		/// <param name="declaration"></param>
		public InvoiceHeaderActiveCollection(BaseJobDeclaration declaration)
			: this(declaration, true)
		{
		}

		public InvoiceHeaderActiveCollection(BaseJobDeclaration declaration, bool allowNew)
			: this(declaration, allowNew, Array.Empty<object>(), delegate
			{ return true; }, JobComInvoiceHeaderSchema.JZ_JE)
		{
		}

		/// <summary>
		/// For GroupInvoice.JobComInvoiceHeaders or GroupInvoice.AllJobComInvoiceHeaders
		/// </summary>
		/// <param name="isDirectRelationship">true for GroupInvoice.JobComInvoiceHeaders which has JZ_JZ_GroupInvoiceFK pointing to GroupInvoice.PK, false for GroupInvoice.AllJobComInvoiceHeaders which has all grandchildren and great grandchildren etc of GroupInvoice</param>
		public InvoiceHeaderActiveCollection(BaseJobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: this(
			isDirectRelationship ? groupInvoice : groupInvoice.JobDeclaration,
			false,
			isDirectRelationship ? Array.Empty<object>() : new object[] { groupInvoice },
			delegate(BaseJobComInvoiceHeader invoice)
			{ return isDirectRelationship || groupInvoice.IsThisInvoicePartOfThisGroup(invoice); },
			isDirectRelationship ? JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK : JobComInvoiceHeaderSchema.JZ_JE)
		{
		}

		public InvoiceHeaderActiveCollection(Bill bill)
			: this(bill, false, Array.Empty<object>(), delegate
			{ return true; }, JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill)
		{
		}

		InvoiceHeaderActiveCollection(IDeclarationProvider master, bool allowNew, object[] collectionState, DoesInvoiceMatchFilter matchesFilterDelegate, SchemaGuidColumn fkColumnToMaster)
			: base(master.Factory, (BusinessObject)master, new ZQuery(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false), fkColumnToMaster)
		{
			this.declaration = master.Declaration;
			if (declaration == null)
			{
				ErrorReporter.ReportOnce("declaration should not be null");
			}
			this.allowNew = allowNew;
			this.collectionState = collectionState;
			this.matchesFilterDelegate = matchesFilterDelegate;
			((IBindingList)this).ListChanged += new ListChangedEventHandler(InvoiceHeaderActiveCollection_ListChanged);
		}

		public readonly BaseJobDeclaration declaration;

		public InvoiceHeaderActiveCollection(BaseJobDeclaration declaration, string customsGenPivotType)
			: this(declaration, typeof(BaseJobComInvoiceHeader), customsGenPivotType)
		{
		}

		protected InvoiceHeaderActiveCollection(IDeclarationProvider master, Type elementType, string customsGenPivotType)
			: base(master.Declaration.Factory, GetMultipleDeclarationsInvoiceRelationship(master.Declaration, elementType, customsGenPivotType))
		{
			this.declaration = master.Declaration;
			if (declaration == null)
			{
				ErrorReporter.ReportOnce("declaration should not be null");
			}
			this.allowNew = true;
			this.collectionState = Array.Empty<object>();
			this.matchesFilterDelegate = delegate
			{ return true; };
			((IBindingList)this).ListChanged += new ListChangedEventHandler(InvoiceHeaderActiveCollection_ListChanged);
			((MultipleCollectionRelationship)Relationship).BeforeCollectionCountChange += InvoiceHeaderActiveCollection_BeforeCollectionCountChange;
		}

		#endregion

		#region Methods

		public bool HasInvoicesWithValuationDateOverride
		{
			get
			{
				if (hasInvoicesWithValuationDateOverrideCached == null)
				{
					hasInvoicesWithValuationDateOverrideCached = new CachedProperty<ZBool>(Factory, GetHasInvoicesWithValuationDateOverride);
				}
				return hasInvoicesWithValuationDateOverrideCached.Value;
			}
		}
		CachedProperty<ZBool> hasInvoicesWithValuationDateOverrideCached;

		ZBool GetHasInvoicesWithValuationDateOverride()
		{
			bool result = false;
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				if (invoice.JZ_ValuationDateOverride.IsValid)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public ZDateTime EffectiveValuationDateForParentGroup
		{
			get
			{
				if (effectiveValuationDateCached == null)
				{
					effectiveValuationDateCached = new CachedProperty<ZDateTime>(Factory, GetEffectiveValuationDateForParentGroup);
				}
				return effectiveValuationDateCached.Value;
			}
		}
		CachedProperty<ZDateTime> effectiveValuationDateCached;

		ZDateTime GetEffectiveValuationDateForParentGroup()
		{
			List<ZDateTime> result = new List<ZDateTime>();
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				if (!result.Contains(invoice.EffectiveValuationDate))
				{
					result.Add(invoice.EffectiveValuationDate);
				}
			}

			if (result.Count == 1)
			{
				return result[0];
			}
			else
			{
				return declaration != null ? declaration.DateOfValuation : ZDateTime.Empty;
			}
		}

		public virtual bool HasInvoicesWithRecommendedChargeInGroupCharges(ICustomsChargeCode chargeCode)
		{
			bool result = false;
			if (chargeCode != null)
			{
				foreach (BaseJobComInvoiceHeader invoice in this)
				{
					var incoTermAndCharngeFactory = invoice.IncoTermAndChargeFactory;
					var incoTerm = invoice.IncoTerm;
					result = incoTermAndCharngeFactory.IsThisChargeRecommendedForThisIncoTerm(incoTerm, chargeCode.Code)
						&& incoTermAndCharngeFactory.IsThisChargeRecommendedForThisInvoice(invoice, incoTerm, chargeCode.Code)
						&& !incoTermAndCharngeFactory.CanThisIncoTermHaveThisCharge(incoTerm, chargeCode);
					if (result)
					{
						break;
					}
				}
			}
			return result;
		}

		public ZDateTime ValuationDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (Count > 0 && DoAllInvoicesHaveTheSameValuationDate)
				{
					result = this[0].EffectiveValuationDate;
				}
				else if (declaration != null)
				{
					result = declaration.DateOfValuation;
				}

				return result;
			}
		}

		public bool DoAllInvoicesHaveTheSameValuationDate
		{
			get
			{
				if (doAllInvoicesHaveTheSameValuationDateCached == null)
				{
					doAllInvoicesHaveTheSameValuationDateCached = new CachedProperty<bool>(Factory, GetDoAllInvoicesHaveTheSameValuationDate);
				}
				return doAllInvoicesHaveTheSameValuationDateCached.Value;
			}
		}

		bool GetDoAllInvoicesHaveTheSameValuationDate()
		{
			bool sameValuationDates = true;
			ZDateTime firstValuationDate = ZDateTime.Empty;
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				if (firstValuationDate.IsEmpty)
				{
					firstValuationDate = invoice.EffectiveValuationDate;
				}
				else
				{
					if (invoice.EffectiveValuationDate != firstValuationDate)
					{
						sameValuationDates = false;
						break;
					}
				}
			}
			return sameValuationDates;
		}

		CachedProperty<bool> doAllInvoicesHaveTheSameValuationDateCached;

		public string[] InvoiceCurrencies
		{
			get
			{
				if (invoiceCurrenciesCached == null)
				{
					invoiceCurrenciesCached = new CachedProperty<string[]>(Factory, GetInvoiceCurrencies);
				}

				return invoiceCurrenciesCached.Value;
			}
		}

		string[] GetInvoiceCurrencies()
		{
			List<string> result = new List<string>();
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				if (invoice.Invoice_Currency != null && !result.Contains(invoice.Invoice_Currency.RX_Code))
				{
					result.Add(invoice.Invoice_Currency.RX_Code);
				}
			}
			return result.ToArray();
		}
		CachedProperty<string[]> invoiceCurrenciesCached;

		public ZString[] InvoiceIncoterms
		{
			get
			{
				if (invoiceIncotermsCached == null)
				{
					invoiceIncotermsCached = new CachedProperty<ZString[]>(Factory, GetInvoiceIncoterms);
				}
				return invoiceIncotermsCached.Value;
			}
		}
		CachedProperty<ZString[]> invoiceIncotermsCached;

		ZString[] GetInvoiceIncoterms()
		{
			var result = new List<ZString>();
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				var incoTerm = invoice.IncoTerm;
				if (!incoTerm.IsEmpty && !result.Contains(incoTerm))
				{
					result.Add(incoTerm);
				}
			}
			return result.ToArray();
		}

		public void SetExchangeRate()
		{
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				invoice.SetExchangeRateIfNotUserEntered();
			}
		}

		public bool HasApportionedCharges
		{
			get
			{
				bool result = false;
				foreach (BaseJobComInvoiceHeader invoice in this)
				{
					result = invoice.GroupCharges.Count > 0;
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		public void SetNeedToGetNewChargeIncoTermFactory()
		{
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				invoice.NeedToGetNewIncoTermAndChargeFactory = true;
				invoice.Charges.MarkAsNeedingValidation();
				invoice.GroupCharges.MarkAsNeedingValidation();
			}
		}

		public Money TotalInvoiceLinesAmount
		{
			get
			{
				Money result = Money.Empty;
				foreach (BaseJobComInvoiceHeader invoiceHeader in this)
				{
					ZDecimal totalLinePrice = invoiceHeader.JZ_Calc_LinesEntered;
					if (!totalLinePrice.IsEmpty)
					{
						result = invoiceHeader.CurrencyConverter.Add(result, new Money(totalLinePrice, invoiceHeader.Invoice_Currency));
					}
				}
				return result;
			}
		}

		public ZDecimal TotalNoOfPacks
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (BaseJobComInvoiceHeader invoiceHeader in this)
				{
					result += invoiceHeader.JZ_NoOfPacks;
				}
				return result;
			}
		}

		#region AreChargesBalancedForInvoices

		public bool AreChargesBalancedForInvoices(out string detailedErrorMessage)
		{
			detailedErrorMessage = string.Empty;

			bool result = true;

			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				var lineChargesAggregated = new LineChargesAggregator().GetTotal(invoice, new BaseJobComInvoiceLine.LineComparer());

				result = AreChargesBalancedForOneInvoice(invoice, lineChargesAggregated, out detailedErrorMessage);
				if (!result)
				{
					break;
				}
			}

			if (result)
			{
				result = AreChargesBalancedForGroupInvoices(out detailedErrorMessage);
			}

			return result;
		}

		bool AreChargesBalancedForGroupInvoices(out string detailedErrorMessage)
		{
			bool result = true;

			detailedErrorMessage = string.Empty;

			foreach (BaseJobComInvoiceGroupHeader groupInvoice in declaration.AllGroupHeaders)
			{
				result = AreChargesBalancedForOneGroupInvoice(groupInvoice, out detailedErrorMessage);

				if (!result)
				{
					break;
				}
			}

			return result;
		}

		bool AreChargesBalancedForOneGroupInvoice(BaseJobComInvoiceGroupHeader groupInvoice, out string detailedErrorMessage)
		{
			bool result = true;
			detailedErrorMessage = string.Empty;

			List<ApportionChargeKey> chargeKeys = new List<ApportionChargeKey>();

			foreach (ApportionChargeKey chargeKey in GetChargeCodeToAggregate(new TypedEnumerable<JobComInvCharge>(groupInvoice.Charges)))
			{
				if (!chargeKeys.Exists(x => x.Equals(chargeKey)))
				{
					chargeKeys.Add(chargeKey);
				}
			}

			foreach (ApportionChargeKey chargeKey in chargeKeys)
			{
				Money total = Money.Empty;

				foreach (BaseJobComInvoiceHeader invoice in groupInvoice.AllJobComInvoiceHeaders)
				{
					total = groupInvoice.CurrencyConverter.Add(total, invoice.Charges.GetCharge(chargeKey));
					total = groupInvoice.CurrencyConverter.Add(total, invoice.GroupCharges.GetCharge(chargeKey));
				}

				Money groupChargeAmount = groupInvoice.CurrencyConverter.ConvertExact(groupInvoice.Charges.GetCharge(chargeKey), groupInvoice.LocalCurrency);
				Money totalAmount = groupInvoice.CurrencyConverter.ConvertExact(total, groupInvoice.LocalCurrency);

				if (totalAmount.Amount.Round(2) != groupChargeAmount.Amount.Round(2))
				{
					result = false;
					detailedErrorMessage = Res.GetString("fd2676fa-6833-4bd3-9d95-89ba51638ad7", "({0}) Group Inv. Amount:{1}, Total Inv. Amount:{2}", chargeKey.ChargeKey.ChargeCode + " at Group Invoice " + groupInvoice.JZ_InvoiceNumber, groupChargeAmount.ToString(), totalAmount.ToString());

					break;
				}
			}

			return result;
		}

		bool AreChargesBalancedForOneInvoice(BaseJobComInvoiceHeader invoice, Dictionary<ApportionChargeKey, Money> aggregated, out string detailedErrorMessage)
		{
			bool result = true;//if there are no charges, then no problem!
			detailedErrorMessage = string.Empty;

			List<ApportionChargeKey> chargeKeys = new List<ApportionChargeKey>(aggregated.Keys);

			foreach (ApportionChargeKey chargeKey in GetChargeCodeToAggregate(new TypedEnumerable<JobComInvCharge>(invoice.Charges)))
			{
				if (!chargeKeys.Exists(x => x.Equals(chargeKey)))
				{
					chargeKeys.Add(chargeKey);
				}
			}

			foreach (ApportionChargeKey chargeKey in GetChargeCodeToAggregate(new TypedEnumerable<JobComInvCharge>(invoice.GroupCharges)))
			{
				if (!chargeKeys.Exists(x => x.Equals(chargeKey)))
				{
					chargeKeys.Add(chargeKey);
				}
			}

			foreach (ApportionChargeKey chargeKey1 in chargeKeys)
			{
				Money invoiceLineAmounts;

				if (!aggregated.TryGetValue(chargeKey1, out invoiceLineAmounts))
				{
					invoiceLineAmounts = Money.Empty;
				}

				Money invoiceAmount = invoice.Charges.GetCharge(chargeKey1);
				invoiceAmount = invoice.CurrencyConverter.Add(invoiceAmount, invoice.GroupCharges.GetCharge(chargeKey1));

				Money invoiceLinesAggregatedAmountInLocalCurrency = invoice.CurrencyConverter.ConvertExact(invoiceLineAmounts, invoice.LocalCurrency);
				Money invoiceLevelChargeAmountInLocalCurrency = invoice.CurrencyConverter.ConvertExact(invoiceAmount, invoice.LocalCurrency);

				if (invoiceLinesAggregatedAmountInLocalCurrency.Amount.Round(2) != invoiceLevelChargeAmountInLocalCurrency.Amount.Round(2))
				{
					result = false;

					detailedErrorMessage = Res.GetString("cda846ff-596a-4dfd-8352-9e4659505f84", "({0}) Inv. Amount:{1}, Total Line Amount:{2}", chargeKey1.ChargeKey.ChargeCode + " at Invoice " + invoice.JZ_InvoiceNumber, invoiceLevelChargeAmountInLocalCurrency.ToString(), invoiceLinesAggregatedAmountInLocalCurrency.ToString());
					break;
				}
			}

			return result;
		}

		IEnumerable<ApportionChargeKey> GetChargeCodeToAggregate(IEnumerable<JobComInvCharge> charges)
		{
			foreach (JobComInvCharge charge in charges)
			{
				if (charge.J7_Amount > 0m && charge.Currency != null && !charge.J7_IsSystem)
				{
					yield return charge.ApportionChargeKey;
				}
			}
		}

		#endregion

		public BaseJobComInvoiceHeader EarliestInvoice
		{
			get
			{
				BaseJobComInvoiceHeader result = null;
				ZDateTime resultDate = ZDateTime.Empty;
				foreach (BaseJobComInvoiceHeader invoiceHeader in this)
				{
					ZDateTime dateToUse = invoiceHeader.DeclarationDate;
					if (!dateToUse.IsValid)
					{
						dateToUse = invoiceHeader.JZ_InvoiceDate;
					}

					if (dateToUse.IsValid && (result == null || dateToUse < resultDate))
					{
						result = invoiceHeader;
						resultDate = dateToUse;
					}
				}
				return result;
			}
		}

		public BaseJobComInvoiceHeader LatestInvoice
		{
			get
			{
				BaseJobComInvoiceHeader result = null;
				ZDateTime resultDate = ZDateTime.Empty;
				foreach (BaseJobComInvoiceHeader invoiceHeader in this)
				{
					ZDateTime dateToUse = invoiceHeader.DeclarationDate;
					if (!dateToUse.IsValid)
					{
						dateToUse = invoiceHeader.JZ_InvoiceDate;
					}

					if (dateToUse.IsValid && (result == null || dateToUse > resultDate))
					{
						result = invoiceHeader;
						resultDate = dateToUse;
					}
				}
				return result;
			}
		}

		internal void SetMergingInProgress(bool started)
		{
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				invoice.SetMergingInProgress(started);
			}
		}

		public BaseJobComInvoiceHeader Find(ZShort sequenceNumber)
		{
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				if (invoice.JZ_InvoiceDisplaySequence == sequenceNumber)
				{
					return invoice;
				}
			}
			return null;
		}

		#endregion

		#region Additional Invoices Support

		public bool SupportAdditionalInvoices
		{
			get
			{
				return Relationship is MultipleCollectionRelationship;
			}
		}

		public void RefreshAdditionalInvoices()
		{
			if (SupportAdditionalInvoices)
			{
				foreach (BaseJobComInvoiceHeader invoice in this)
				{
					if (IsAdditionalInvoice(invoice))
					{
						invoice.InvoiceLines.CountChanged -= InvoiceLines_CountChanged;
					}
				}
				var relationship = ((MultipleCollectionRelationship)Relationship);
				relationship.SupportAdditionalRelationship = declaration.SupportAdditionalInvoices;
				relationship.Refresh();
			}
		}

		public void RemoveAll()
		{
			foreach (var invoice in this.ToArray())
			{
				RemoveFromRelationship(invoice);
			}
		}

		void InvoiceHeaderActiveCollection_BeforeCollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			var invoice = (BaseJobComInvoiceHeader)e.BizObject;
			if (e.ItemRemoved && IsAdditionalInvoice(invoice))
			{
				invoice.InvoiceLines.CountChanged -= InvoiceLines_CountChanged;
				foreach (var line in invoice.InvoiceLines)
				{
					declaration.InvoiceLines.Remove(line);
				}
			}
		}

		public bool IsAdditionalInvoice(BaseJobComInvoiceHeader invoice)
		{
			return SupportAdditionalInvoices && ((MultipleCollectionRelationship)Relationship).MatchesAdditionalRelationshipFilter(invoice);
		}

		protected override void OnLoadedIntoCollectionCore(BaseJobComInvoiceHeader loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			if (IsAdditionalInvoice(loadedObject))
			{
				loadedObject.InvoiceLines.CountChanged += InvoiceLines_CountChanged;
			}
		}

		void InvoiceLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				declaration.InvoiceLines.Add(e.BizObject);
			}
			if (e.ItemRemoved)
			{
				declaration.InvoiceLines.Remove(e.BizObject);
			}
		}

		#endregion

		#region SetOverrideDeclaration

		public void SetOverrideDeclaration(BaseJobDeclaration declaration)
		{
			if (declaration == null || declaration.SupportAdditionalInvoices)
			{
				foreach (BaseJobComInvoiceHeader header in this)
				{
					header.OverrideParent = declaration;
				}
			}
		}

		#endregion

		#region Implementation

		void InvoiceHeaderActiveCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory);
		}

		readonly bool allowNew;

		readonly object[] collectionState;

		readonly DoesInvoiceMatchFilter matchesFilterDelegate;
		delegate bool DoesInvoiceMatchFilter(BaseJobComInvoiceHeader invoice);

		protected override bool AllowNew
		{
			get { return allowNew; }
		}

		protected override object[] GetCollectionState()
		{
			return collectionState;
		}

		protected override bool MatchesFilterCore(BaseJobComInvoiceHeader element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && matchesFilterDelegate(element);
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new FetchStrategies.InvoiceHeaderActiveCollectionFetchStrategy(this);
		}

		#endregion

		#region ActiveBusinessObjectCollection Overrides

		/// <summary>
		/// What is required happens in Architecture level in relationship objects.
		/// </summary>
		protected sealed override void SetRelationshipDefaultsForElementCore(BaseJobComInvoiceHeader newElement, bool throwIfRelationshipNotSupported)
		{
			var existingInvoiceIsBeingAttachedToDeclaration = newElement.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired;
			using (newElement.SuspendSettingHasChanges())
			using (newElement.GetValidationSuspender())
			{
				IDisposable suspender = declaration != null ? declaration.SuspendMarkApportionmentDirty() : null;

				if (declaration != null)
				{
					newElement.JZ_JE = declaration.PK;
				}

				base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);

				if (suspender != null)
				{
					suspender.Dispose();
				}
			}
			if (existingInvoiceIsBeingAttachedToDeclaration)
			{
				newElement.HasChanges = true;
			}
		}

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
		{
			IDisposable suspender = declaration != null ? declaration.SuspendMarkApportionmentDirty() : null;

			base.SetDefaultsForNewElementCore(newElement);

			GetDefaultSetterForInvoiceHeader(newElement, declaration).DefaultForNewElement();

			if (suspender != null)
			{
				suspender.Dispose();
			}
		}

		protected virtual DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader(newElement, declaration);
		}

		public override void Delete(BaseJobComInvoiceHeader businessObject)
		{
			if (!IsAdditionalInvoice(businessObject))
			{
				base.Delete(businessObject);
			}
		}

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new InvoiceHeaderImportSuspender(declaration);
		}

		#endregion

		#region IExternalFactoryRefreshable Members

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		bool fExternalFactoryRefreshEnabled;

		public bool ExternalFactoryRefreshEnabled
		{
			get { return fExternalFactoryRefreshEnabled; }
			set
			{
				if (fExternalFactoryRefreshEnabled != value)
				{
					fExternalFactoryRefreshEnabled = value;
					OnExternalFactoryRefreshEnabledChanged();
				}
			}
		}

		protected virtual void OnExternalFactoryRefreshEnabledChanged()
		{
			foreach (BaseJobComInvoiceHeader invoiceHeader in this)
			{
				invoiceHeader.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
			}
		}

		protected override void OnAdded(BaseJobComInvoiceHeader businessObject)
		{
			base.OnAdded(businessObject);
			if (businessObject?.JobDeclaration is BaseJobDeclaration declaration)
			{
				declaration.ReApportionInvoiceWeightIfNeeded(!businessObject.JZ_InvoiceAmount.IsEmpty);
			}
		}

		#endregion

		#region GetMultipleDeclarationsInvoiceRelationship

		static ICollectionRelationship GetMultipleDeclarationsInvoiceRelationship(BaseJobDeclaration declaration, Type elementType, string customsGenPivotType)
		{
			var genPivotType = GenPivotTypeDecider.GetType(customsGenPivotType)
				?? throw new ArgumentException("customsGenPivotType is invalid.");

			ZQuery filter = new ZQuery(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);

			var dependentRelationship = new DependentRelationship(declaration, elementType, filter, JobComInvoiceHeaderSchema.JZ_JE);
			var manyToManyRelationship = new ManyToManyRelationshipWithRelationType(declaration, elementType, genPivotType, filter, GenPivotSchema.XX_Relation2ID, GenPivotSchema.XX_Relation1ID, customsGenPivotType);
			var genPivotQuery = new ZQuery();
			genPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, customsGenPivotType);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);

			manyToManyRelationship.AdditionalDivotFilter = genPivotQuery;
			return new MultipleCollectionRelationship(elementType, dependentRelationship, manyToManyRelationship, declaration.SupportAdditionalInvoices);
		}

		#endregion

		#region IInvoiceHeaderActiveCollection Members

		Shared.IBaseJobComInvoiceHeader IInvoiceHeaderActiveCollection.AddNew()
		{
			return AddNew();
		}

		#endregion
	}
}
