#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	public class DummyLandedCostHeader : DummyBusinessObject, ILandedCostHeader
	{
		public DummyLandedCostHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ILandedCostHeader Members

		public IComparer LineComparerExposed;
		public IComparer LineComparer
		{
			get { return LineComparerExposed; }
		}

		public bool HasMultiInvoicesExposed;
		public bool HasMultiInvoices
		{
			get { return HasMultiInvoicesExposed; }
		}

		public ZString LandedCostTypeExposed;
		ZString ILandedCostHeader.LandedCostType
		{
			get { return LandedCostTypeExposed; }
		}

		public ZString JobNumberExposed;
		ZString ILandedCostHeader.JobNumber
		{
			get { return JobNumberExposed; }
		}

		public ZDate DateOfEntryExposed;
		ZDate ILandedCostHeader.DateOfEntry
		{
			get { return DateOfEntryExposed; }
		}

		public ILandedCostDistributeTo[] CandidatesToDistributeCostToExposed;
		IEnumerable<ILandedCostDistributeTo> ILandedCostHeader.CandidatesToDistributeCostTo
		{
			get { return CandidatesToDistributeCostToExposed; }
		}

		public ILandedCostChargeHolder[] ChargeHoldersExposed;
		IEnumerable<ILandedCostChargeHolder> ILandedCostHeader.ChargeHolders
		{
			get { return ChargeHoldersExposed; }
		}

		public IUltimateDistributee[] UltimateDistributeesExposed;
		public IEnumerable<IUltimateDistributee> UltimateDistributees
		{
			get { return UltimateDistributeesExposed; }
		}

		public ILandedCostExchangeRateHolder[] ExchangeRateHoldersExposed;
		public IEnumerable<ILandedCostExchangeRateHolder> ExchangeRateHolders
		{
			get { return ExchangeRateHoldersExposed; }
		}

		public bool IsJobInLCRunnableStateExposed;
		bool ILandedCostHeader.IsJobInLCRunnableState
		{
			get { return IsJobInLCRunnableStateExposed; }
		}

		public string TableCodeExposed;
		public string TableCode
		{
			get { return TableCodeExposed; }
		}

		ZGuid ILandedCostHeader.PK
		{
			get { return PK; }
		}

		ZString ILandedCostHeader.TableCode
		{
			get { return TableCodeExposed ?? DummyBizoSchema.Constants.Prefix; }
		}

		public bool IsLCSupportedExposed;
		bool ILandedCostHeader.IsLCSupported
		{
			get { return IsLCSupportedExposed; }
		}

		public string MessageShownWhenLCIsNotSupportedExposed = "I don't like cheese.";
		string ILandedCostHeader.MessageShownWhenLCIsNotSupported
		{
			get { return MessageShownWhenLCIsNotSupportedExposed; }
		}

		public OrgHeader ConsigneeExposed;
		public OrgHeader Consignee
		{
			get { return ConsigneeExposed; }
		}

		public ZString UniqueReferenceNumberExposed;
		public ZString UniqueReferenceNumber
		{
			get { return UniqueReferenceNumberExposed; }
		}

		public DutyTaxEntryFee TotalDutyTaxEntryFeeItemsExposed;
		public DutyTaxEntryFee TotalDutyTaxEntryFeeItems
		{
			get { return TotalDutyTaxEntryFeeItemsExposed; }
		}

		public virtual void DoStuffBeforeRunningLCDistribution()
		{
		}

		public bool IsAirExposed;
		public bool IsAir
		{
			get { return IsAirExposed; }
		}

		public ZGuid CompanyPK
		{
			get { return companyPK.IsEmpty ? GlbCompany.CurrentCompany.PK : companyPK; }
			set { companyPK = value; }
		}
		ZGuid companyPK;

		IHaveRequiredDocuments ILandedCostHeader.RequiredDocumentsProvider
		{
			get { return null; }
		}

		Type ILandedCostHeader.DocsAndCartageType
		{
			get { return typeof(DummyLandedCostHeader); }
		}

		Type ILandedCostHeader.DocsAndCartageParentType
		{
			get { return typeof(DummyLandedCostHeader); }
		}

		public event EventHandler OnLCSupportedChanged
		{
			add
			{
				Z0_BoolInfo.ValueChanged += value;
			}
			remove
			{
				Z0_BoolInfo.ValueChanged -= value;
			}
		}

		event EventHandler ILandedCostHeader.OnExchangeRateHolderDeleted
		{
			add { }
			remove { }
		}

		#endregion

		#region ILandedCostHeader Members

		public ZBool SupportsNoCostApportionmentItem { get; set; }

		public ZDecimal DefaultEstimatedDutyPercentExposed;
		ZDecimal ILandedCostHeader.DefaultEstimatedDutyPercent
		{
			get { return DefaultEstimatedDutyPercentExposed; }
		}

		public Dictionary<ZString, ZDecimal> DefaultExchangeRatesExposed;
		public Dictionary<ZString, ZDecimal> GetDefaultExchangeRates()
		{
			return DefaultExchangeRatesExposed;
		}

		#endregion
	}
}
#endif
