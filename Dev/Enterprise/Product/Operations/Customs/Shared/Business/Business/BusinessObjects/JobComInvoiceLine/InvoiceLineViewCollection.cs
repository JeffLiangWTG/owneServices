using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IInvoiceLineViewCollection<out TInvoiceLine> : IBusinessObjectCollection<TInvoiceLine>
		where TInvoiceLine : BaseJobComInvoiceLine
	{
		bool CopyLastLineDetailsToNewLines { get; set; }
		bool HasUnclassifiedLines { get; }

		// Without this, some GUI UTs will fail about Resource strings
		new TInvoiceLine this[int index] { get; }

		void LoadFilteredLines(Predicate<TInvoiceLine> predicate = null);
		void LoadCusEntryLineFetchHintIfNeeded();
		void Rebuild();
	}

	/// <summary>
	/// Contains a Subset of [JobDeclaration.InvoiceLines] that takes into account the "Show Unclassified Lines" flag.
	/// collection shown in the GUI.
	/// </summary>
	public class InvoiceLineViewCollection<TInvoiceLine> : BusinessObjectCollectionView<TInvoiceLine>, IBusinessObjectRelationshipFilterProvider, IInvoiceLineViewCollection<TInvoiceLine>, IBusinessObjectCollectionNotificationsViewerProvider
		where TInvoiceLine : BaseJobComInvoiceLine
	{
		protected class BaseLineComparer : PropertyComparer
		{
			public BaseLineComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{
			}
			#region IComparer Members

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				const int comparedSameValue = 0;
				int result = 0;

				if (x != y)
				{
					var lineX = (TInvoiceLine)x;
					var lineY = (TInvoiceLine)y;
					if (PropertyDescriptor.DisplayName == BaseJobComInvoiceLine.Schema.JI_Calc_Invoice
						|| PropertyDescriptor.DisplayName == BaseJobComInvoiceLine.Schema.JI_LineNo)
					{
						if (lineX.InvoiceHeader != null && lineY.InvoiceHeader != null)
						{
							result = lineX.InvoiceHeader.JZ_InvoiceNumber.CompareTo(lineY.InvoiceHeader.JZ_InvoiceNumber);
						}
						if (result == comparedSameValue)
						{
							result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
						}
						if (result == comparedSameValue)
						{
							result = lineX.PK.CompareTo(lineY.PK);
						}
						result = Direction == ListSortDirection.Ascending ? result : -result;
					}
					else if (PropertyDescriptor.DisplayName == BaseJobComInvoiceLine.Schema.MergedLineNumber)
					{
						ZString xEntryNumber = (lineX.CusEntryLine == null || lineX.CusEntryLine.Header == null) ? ZString.Empty : lineX.CusEntryLine.Header.EntryNumber;
						ZString yEntryNumber = (lineY.CusEntryLine == null || lineY.CusEntryLine.Header == null) ? ZString.Empty : lineY.CusEntryLine.Header.EntryNumber;

						result = xEntryNumber.CompareTo(yEntryNumber);
						if (result == comparedSameValue)
						{
							int xLineNumber = (lineX.CusEntryLine == null) ? 0 : lineX.CusEntryLine.CL_LineNumber;
							int yLineNumber = (lineY.CusEntryLine == null) ? 0 : lineY.CusEntryLine.CL_LineNumber;
							result = xLineNumber.CompareTo(yLineNumber);
							result = Direction == ListSortDirection.Ascending ? result : -result;
						}
					}
					else
					{
						result = base.Compare(x, y);
						if (result == comparedSameValue)
						{
							result = lineX.InvoiceHeader.JZ_InvoiceNumber.CompareTo(lineY.InvoiceHeader.JZ_InvoiceNumber);
						}
						if (result == comparedSameValue)
						{
							result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
						}
					}
				}
				return result;
			}

			#endregion
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return new BaseLineComparer(property, direction);
		}

		public InvoiceLineViewCollection(BaseJobDeclaration jobDeclaration)
			: base(jobDeclaration.InvoiceLines)
		{
			Declaration = jobDeclaration;
			currentLoadView = LoadView.All;
			((IBindingList)this).ListChanged += InvoiceLineViewCollection_ListChanged;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// nope
		}

		#region Load/Unload view elements

		protected internal enum LoadView { All, FilteredOnly }
		protected internal LoadView currentLoadView;

		public void LoadFilteredLines(Predicate<TInvoiceLine> predicate = null)
		{
			currentLoadView = LoadView.FilteredOnly;
			Predicate = predicate;
			Rebuild();
		}

		Predicate<TInvoiceLine> Predicate { get; set; }

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var invoiceLine = (TInvoiceLine)element;

			bool isLinkedToDeclaration = invoiceLine.OverrideParent == Declaration;
			if (!isLinkedToDeclaration)
			{
				var invoiceHeader = invoiceLine.InvoiceHeader;
				//JobComInvoiceHeader.JobDeclaration is loaded with a cached version of JZ_JE and do not want to use it if it is empty
				isLinkedToDeclaration = invoiceHeader != null &&
				(
					(invoiceHeader.JZ_JE == Declaration.PK || invoiceHeader.AdditionalDeclarations.Any(job => job.PK == Declaration.PK))
					|| (!invoiceLine.IsDeleted && !Declaration.IsPersistent && invoiceHeader.PreDeclarationPk.IsValid && invoiceHeader.JZ_JE.IsEmpty && Declaration.PK == invoiceHeader.PreDeclarationPk)
				);
			}

			var result = false;

			if (isLinkedToDeclaration)
			{
				switch (currentLoadView)
				{
					case LoadView.FilteredOnly:
						result = Predicate?.Invoke(invoiceLine) ?? true;
						break;
					default:
						result = true;
						break;
				}
			}

			return result;
		}

		public bool HasUnclassifiedLines
		{
			get
			{
				foreach (TInvoiceLine line in this)
				{
					if (line.JI_Tariff.IsEmpty)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		public IEnumerator<TInvoiceLine> GetEnumerator() => Elements.Cast<TInvoiceLine>().GetEnumerator();

		public bool CopyLastLineDetailsToNewLines { get; set; }

		public void LoadCusEntryLineFetchHintIfNeeded()
		{
			if (!hasLoadedCusEntryLineFetchHint)
			{
				hasLoadedCusEntryLineFetchHint = true;
				foreach (TInvoiceLine invoiceLine in this)
				{
					Factory.AddFetchHint(CusEntryLineSchema.PK, invoiceLine.JI_CL);
				}
			}
		}
		bool hasLoadedCusEntryLineFetchHint;

		#region Implementation

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new FetchStrategies.InvoiceLineViewCollectionFetchStrategy(this);
		}

		protected readonly BaseJobDeclaration Declaration;

		void InvoiceLineViewCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!IsRebuilding && !IsLoading && e != null && e.ListChangedType == ListChangedType.ItemAdded && e.NewIndex < Count && e.NewIndex > -1)
			{
				TInvoiceLine invoiceLine = this[e.NewIndex];
				if (invoiceLine != null && invoiceLine.InvoiceHeader != null)
				{
					invoiceLine.InvoiceHeader.ReApportionLineWeightIfNeeded(!invoiceLine.LinePriceForWeightApportionCalculation.IsEmpty);
				}
			}
		}

		/// <summary>
		/// Consider putting defaulting logic in InvoiceLineCompleteCollection so that other parts of system
		/// that use Declaration.InvoiceLines to add invoice lines instead of this collection can have the logic.
		/// CopyListLineDetailsToNewLinesIfEnabled() is driven by the menu click from the form that uses this collection.
		/// </summary>
		/// <param name="child"></param>
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var line = (TInvoiceLine)child;

			using (line.GetValidationSuspender())
			{
				if (Count > 0)
				{
					TInvoiceLine previousLine = this[Count - 1];

					CopyLastLineDetailsToNewLinesIfEnabled(line, previousLine);
				}
			}
		}

		protected virtual void CopyLastLineDetailsToNewLinesIfEnabled(TInvoiceLine newLine, TInvoiceLine previousLine)
		{
			if (CopyLastLineDetailsToNewLines)
			{
				newLine.CopyPersistentValuesFrom(previousLine, new BusinessObjectCloneArgs(new[] { JobComInvoiceLineSchema.Constants.JI_JZ, JobComInvoiceLineSchema.Constants.JI_LineNo, JobComInvoiceLineSchema.Constants.JI_MatchingKey }));

				if (newLine is IAddInfoChildSupporter newSupporter && previousLine is IAddInfoChildSupporter previousSupporter
					&& newSupporter.AddInfoChild is BusinessObject newAddInfo && previousSupporter.AddInfoChild is BusinessObject previousAddInfo)
				{
					using (newAddInfo.SuspendSettingHasChanges())
					using (newAddInfo.GetValidationSuspender())
					{
						newAddInfo.CopyPersistentValuesFrom(previousAddInfo, new BusinessObjectCloneArgs(new[] { newSupporter.ChildForeignKeyColumn.Name }));
					}
				}
			}
		}

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new InvoiceLineImportSuspender(Declaration);
		}

		protected class InvoiceLineImportSuspender : IDisposable
		{
			public InvoiceLineImportSuspender(BaseJobDeclaration jobDeclaration)
			{
				if (jobDeclaration != null)
				{
					invoiceSuspenders = new DisposableList(jobDeclaration.Invoices.Select(invoice => invoice.GetLineNumberRenumberingSuspender()).Union(new[] { jobDeclaration.SuspendWeightApportionment() }));
					declaration = jobDeclaration;
					existingInvoiceLines = new HashSet<TInvoiceLine>(declaration.InvoiceLines.Cast<TInvoiceLine>());
				}
			}

			readonly HashSet<TInvoiceLine> existingInvoiceLines;
			readonly DisposableList invoiceSuspenders;
			readonly BaseJobDeclaration declaration;

			[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Code analysis doesnt like the ?. syntax on the dispose calls")]
			public void Dispose()
			{
				if (declaration != null)
				{
					DisposeCore(declaration);
				}
			}

			protected virtual void DisposeCore(BaseJobDeclaration declaration)
			{
				invoiceSuspenders?.Dispose();
				var weightApportionmentEnabled = declaration.WeightApportionmentEnabled;
				foreach (var invoice in declaration.Invoices)
				{
					var needToReNumber = false;
					var needToReApportion = weightApportionmentEnabled && !invoice.JZ_Weight.IsEmpty;
					var containNewLine = !needToReApportion;
					foreach (var line in invoice.InvoiceLines.Cast<TInvoiceLine>())
					{
						if (!needToReNumber)
						{
							needToReNumber = line.JI_LineNo == ZShort.Zero;
						}

						if (!containNewLine)
						{
							containNewLine = !existingInvoiceLines.Contains(line);
						}

						if (needToReNumber && containNewLine)
						{
							break;
						}
					}

					if (needToReNumber)
					{
						invoice.InvoiceLineLineNumberGenerator.ReCalculateAll();
					}

					if (containNewLine && needToReApportion)
					{
						invoice.ReApportionLineWeightIfNeeded(true);
					}
				}
			}
		}

		#endregion

		#region IBusinessObjectRelationshipFilterProvider Members
		ZQuery IBusinessObjectRelationshipFilterProvider.GetFilter(Type relatedBusinesObjectType) => GetFilterForRelatedBusinessObject(relatedBusinesObjectType);

		protected virtual ZQuery GetFilterForRelatedBusinessObject(Type relatedBusinesObjectType)
		{
			ZQuery result = null;
			if (typeof(CommonJobComInvoiceHeader) == relatedBusinesObjectType)
			{
				result = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, Declaration.PK);
				result.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
				result.FetchOnlyFromLocalCache = !Declaration.IsInDatabase;
			}
			return result;
		}
		#endregion

		#region IBusinessObjectCollectionNotificationsViewerProvider Members

		List<(ZString FieldName, ResourceStringData Caption, ZInt ColumnWidth)> IBusinessObjectCollectionNotificationsViewerProvider.HumanReadableColumns
		{
			get
			{
				var list = new List<(ZString FieldName, ResourceStringData Caption, ZInt ColumnWidth)>();
				list.Add((BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, Res.GetData("InvoiceLineViewCollection|33e474b8-06c3-4c7c-85c0-35c64056500c", "Inv. No.", "Invoice Number"), 200));
				list.Add((BaseJobComInvoiceLine.Schema.JI_LineNo, Res.GetData("InvoiceLineViewCollection|09a92ab0-8006-4737-a5a7-67c3ee1dbd93", "LNO", "Inv. Line#", "Invoice Line #"), 100));
				return list;
			}
		}

		#endregion
	}
}

