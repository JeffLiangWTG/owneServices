using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IJobComInvoiceGroupHeaderCollection<out TInvoiceGroupHeader> : IBusinessObjectCollection<TInvoiceGroupHeader>, IExternalFactoryRefreshable
		where TInvoiceGroupHeader : BaseJobComInvoiceGroupHeader
	{
		new TInvoiceGroupHeader this[int index] { get; }
		GroupHeaderCollection CollectionToFilter { get; }
		void SetNeedToGetNewChargeIncoTermFactory();
		bool IsMarkAsNeedingValidationSuspended { get; set; }
		void SetOverrideDeclaration(BaseJobDeclaration declaration);
		bool IsAdditionalGroupHeader(BusinessObject groupHeader);
	}

	public class BaseJobComInvoiceGroupHeaderCollection<TInvoiceGroupHeader> : BusinessObjectCollectionView<TInvoiceGroupHeader>, IJobComInvoiceGroupHeaderCollection<TInvoiceGroupHeader>
		where TInvoiceGroupHeader : BaseJobComInvoiceGroupHeader
	{
		public BaseJobComInvoiceGroupHeaderCollection(IInvoiceParent bizO)
			: base(bizO.Declaration.AllGroupHeaders)
		{
			this.Master = bizO;
			this.MasterAsBizObj = (BusinessObject)bizO;
			Rebuild();
		}

		protected readonly IInvoiceParent Master;

		protected readonly BusinessObject MasterAsBizObj;

		public new GroupHeaderCollection CollectionToFilter => (GroupHeaderCollection)base.CollectionToFilter;

		public IEnumerator<TInvoiceGroupHeader> GetEnumerator() => Elements.Cast<TInvoiceGroupHeader>().GetEnumerator();

		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(TInvoiceGroupHeader);

		public void SetNeedToGetNewChargeIncoTermFactory()
		{
			foreach (var groupHeader in this)
			{
				groupHeader.NeedToGetNewIncoTermAndChargeFactory = true;
				groupHeader.Charges.MarkAsNeedingValidation();
			}
		}

		#region Implementation

		protected override void RebuildOnConstruction()
		{
			//Master is not set at this point
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = IsAdditionalGroupHeader(element);
			if (!result)
			{
				var child = (TInvoiceGroupHeader)element;
				result = new ZGuid(child[Master.ForeignKeyInInvoiceToParent.Name]) == MasterAsBizObj.PK;

				if (result && Master.ForeignKeyInInvoiceToParent == JobComInvoiceHeaderSchema.JZ_JE)
				{
					result = child.JZ_JZ_GroupInvoiceFK.IsEmpty;
				}
			}

			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (!IsAdditionalGroupHeader(child))
			{
				IDisposable childValidationSuspender = null;

				if (IsMarkAsNeedingValidationSuspended)
				{
					childValidationSuspender = child.SuspendMarkingAsNeedingValidation();
				}

				base.SetCollectionRelationships(child);
				child[Master.ForeignKeyInInvoiceToParent.Name] = MasterAsBizObj.PK;
				var declaration = Master.Declaration;
				if (declaration != null)
				{
					((TInvoiceGroupHeader)child).JZ_JE = declaration.PK;
					((TInvoiceGroupHeader)child).JZ_ClusterKey = declaration.JE_ClusterKey;
				}

				if (childValidationSuspender != null)
				{
					childValidationSuspender.Dispose();
				}
			}
			else
			{
				base.SetCollectionRelationships(child);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!IsAdditionalGroupHeader(bizOAdded))
			{
				if (!bizOAdded.IsInDatabase && (Master is BaseJobDeclaration declaration))
				{
					var duplicateBizObjs = ((IEnumerable<TInvoiceGroupHeader>)this)
						.Where(p => p != declaration.TopGroupInvoice && !IsAdditionalGroupHeader(p)).ToArray();
					if (duplicateBizObjs.Length > 0)
					{
						ErrorReporter.ReportOnce("Duplicate Top Group Header", "JobDeclaration.JobComInvoiceGroupHeaders.AddNew() is a likely cause. A top group invoice is created when declaration is created and JobDeclaration.JobComInvoiceGroupHeaders is expected to have only one element in it always.");
					}
				}
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			if (!IsAdditionalGroupHeader(bizO))
			{
				CommonJobComInvoiceHeader groupHeader = bizO as CommonJobComInvoiceHeader;
				if (groupHeader != null)
				{
					groupHeader.HiddenOriginalParentGuid = groupHeader.JZ_JE;
				}
			}
			base.OnRemoving(bizO);
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
			foreach (var groupInvoiceHeader in this)
			{
				groupInvoiceHeader.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
			}
		}

		#endregion

		#region Suspend Validation For Created BO

		public bool IsMarkAsNeedingValidationSuspended
		{
			get { return fIsMarkAsNeedingValidationSuspended; }
			set { fIsMarkAsNeedingValidationSuspended = value; }
		}
		bool fIsMarkAsNeedingValidationSuspended;

		#endregion

		#region Additional Invoices Support

		public void SetOverrideDeclaration(BaseJobDeclaration declaration)
		{
			if (declaration == null || declaration.SupportAdditionalInvoices)
			{
				foreach (var header in this)
				{
					header.OverrideParent = declaration;
				}
			}
		}

		public bool IsAdditionalGroupHeader(BusinessObject groupHeader)
		{
			return Master is BaseJobDeclaration && CollectionToFilter.IsAdditionalGroupHeader(groupHeader);
		}

		protected class AdditionalGroupHeaderComparer : IComparer
		{
			public AdditionalGroupHeaderComparer(BaseJobDeclaration declaration)
			{
				jobDeclaration = declaration;
			}

			readonly BaseJobDeclaration jobDeclaration;

			public int Compare(object x, object y)
			{
				int result = 0;

				if (x != y)
				{
					var groupHeaderX = (TInvoiceGroupHeader)x;
					var groupHeaderY = (TInvoiceGroupHeader)y;

					if (groupHeaderX != null && groupHeaderY != null)
					{
						if (groupHeaderX.JZ_JE == jobDeclaration.PK && groupHeaderY.JZ_JE != jobDeclaration.PK)
						{
							result = -1;
						}
						else if (groupHeaderY.JZ_JE == jobDeclaration.PK && groupHeaderX.JZ_JE != jobDeclaration.PK)
						{
							result = 1;
						}
					}
				}

				return result;
			}
		}

		#endregion

		protected override void RebuildCore()
		{
			base.RebuildCore();
			if (Master is BaseJobDeclaration)
			{
				this.Sort(new AdditionalGroupHeaderComparer((BaseJobDeclaration)Master));
			}
		}
	}
}
