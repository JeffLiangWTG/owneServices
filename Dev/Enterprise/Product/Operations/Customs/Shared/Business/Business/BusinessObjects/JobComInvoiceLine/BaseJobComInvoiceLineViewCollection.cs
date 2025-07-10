using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// BusinessObjectCollectionView used on [InvoiceHeader.JobComInvoiceLines] showing a subset from 
	/// the full collection against the JobDeclaration. Will also accept a parent collection from the
	/// InvoiceHeader for the case where there is no JobDeclaration available.
	/// </summary>
	public class BaseJobComInvoiceLineViewCollection : BusinessObjectCollectionView<BaseJobComInvoiceLine>, IExternalFactoryRefreshable, IAllInvoiceLines
	{
		public BaseJobComInvoiceLineViewCollection(BaseJobComInvoiceHeader bizO, InvoiceLineCompleteCollection completeCollection)
			: base(completeCollection)
		{
			InvoiceHeader = bizO;
			JobDeclaration = InvoiceHeader.JobDeclaration;
			Rebuild();
		}

		public BaseJobComInvoiceLineViewCollection(BaseJobComInvoiceHeader invoice, InvoiceLineDependentCollection completeCollection)
			: base(completeCollection)
		{
			InvoiceHeader = invoice;
			JobDeclaration = InvoiceHeader.JobDeclaration;
			Rebuild();
		}

		public BaseJobComInvoiceLine GetByLineNo(ZShort lineNo)
		{
			BaseJobComInvoiceLine result = null;
			foreach (BaseJobComInvoiceLine invoiceLine in this)
			{
				if (invoiceLine.JI_LineNo == lineNo)
				{
					result = invoiceLine;
					break;
				}
			}
			return result;
		}

		public void UpdateProductDetailsOnSupplierBuyerChange()
		{
			if (!IsUpdateProductDetailsOnSupplierBuyerChangeSuspended)
			{
				foreach (BaseJobComInvoiceLine invoiceLine in this.ToArray())
				{
					invoiceLine.UpdateProductDetailsOnSupplierBuyerChange();
				}
			}
		}

		#region Suspend UpdateProductDetailsOnSupplierBuyerChange

		bool IsUpdateProductDetailsOnSupplierBuyerChangeSuspended
		{
			get { return updateProductDetailsOnSupplierBuyerChangeSuspenderIndex > 0; }
		}
		int updateProductDetailsOnSupplierBuyerChangeSuspenderIndex;

		public IDisposable SuspendUpdateProductDetailsOnSupplierBuyerChange()
		{
			return new DisposableAction(() => updateProductDetailsOnSupplierBuyerChangeSuspenderIndex++, () => updateProductDetailsOnSupplierBuyerChangeSuspenderIndex--);
		}

		#endregion

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

		#region Implementation

		protected readonly BaseJobComInvoiceHeader InvoiceHeader;
		protected readonly BaseJobDeclaration JobDeclaration;

		protected const int Seen = 1;
		protected const int NotSeen = -1;

		protected virtual void OnCloning()
		{
		}

		protected virtual void OnClone(ZGuid linePK, ZGuid clonedLinePK)
		{
		}

		protected virtual void OnClonedFinalised()
		{
		}

		/// <summary>
		/// This is not exposed to GUI. Interface importing and unit tests add new elements this way.
		/// If you need to add defaulting logic that is applicable for GUI, see InvoiceLineCollection.cs
		/// </summary>
		/// <param name="child"></param>
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			BaseJobComInvoiceLine invoiceLine = child as BaseJobComInvoiceLine;
			if (invoiceLine != null && InvoiceHeader != null)
			{
				invoiceLine.JI_JZ = InvoiceHeader.PK;
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			BaseJobComInvoiceLine invoiceLine = child as BaseJobComInvoiceLine;
			if (invoiceLine != null && InvoiceHeader != null && !InvoiceHeader.IsDeleted)
			{
				invoiceLine.JI_ClusterKey = InvoiceHeader.JZ_ClusterKey;
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = false;
			if (InvoiceHeader != null)
			{
				result = ((BaseJobComInvoiceLine)element).JI_JZ == InvoiceHeader.PK;
			}
			return result;
		}

		#endregion

		#region IAllInvoiceLines members

		bool IAllInvoiceLines.Contains(BaseJobComInvoiceLine invoiceLine)
		{
			return Contains(invoiceLine);
		}

		#endregion

		#region IExternalFactoryRefreshable Members

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
			foreach (BaseJobComInvoiceLine invoiceLine in this)
			{
				invoiceLine.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
			}
		}

		#endregion
	}
}
