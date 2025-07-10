using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationJobInvoicingSupporter : JobInvoicingSupporter, IJobInvoicingSupporterWithChargeableFactorSource
	{
		public DtbBookingConsolidationJobInvoicingSupporter(DtbBookingConsolidationJobInvoicingPlugIn parent, IJobInvoicingSupporter parentSupporter, bool shouldUseParentSupporter = false)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			Argument.NotNull(parentSupporter, "parentSupporter");

			Parent = parent;
			ParentSupporter = parentSupporter;
			ShouldUseParentSupporter = shouldUseParentSupporter;
		}

#if DEBUG
		public
#endif
		readonly DtbBookingConsolidationJobInvoicingPlugIn Parent;
#if DEBUG
		public
#endif
		readonly IJobInvoicingSupporter ParentSupporter;
		readonly bool ShouldUseParentSupporter;

		public override ZDecimal ActualChargeable
		{
			get
			{
				if (ShouldUseParentSupporter)
				{
					return ParentSupporter.ActualChargeable;
				}
				return Parent.Bookings.Aggregate<DtbBooking, ZDecimal>(0, (current, booking) => (ZDecimal)(current + booking.KM_Chargeable));
			}
		}

		public override ZString ActualChargeableUnit
		{
			get { return ParentSupporter.ActualChargeableUnit; }
		}

		public override ZDecimal ActualWeight
		{
			get
			{
				if (ShouldUseParentSupporter)
				{
					return ParentSupporter.ActualWeight;
				}
				ZDecimal result = 0m;
				foreach (var booking in Parent.Bookings)
				{
					foreach (DtbBookingPackage_PackageView package in booking.Packages_PackageView)
					{
						var weight = package.WeightFromInstructions;
						result += Constants.Weight.Convert(weight.Amount, weight.Unit, ((IJobInvoicingSupporter)this).ActualWeightUnit);
					}
				}
				return result;
			}
		}

		public override ZString ActualWeightUnit
		{
			get { return ParentSupporter.ActualWeightUnit; }
		}

		public override int ContainerCount
		{
			get
			{
				if (ShouldUseParentSupporter)
				{
					return ParentSupporter.ContainerCount;
				}
				int result = 0;
				foreach (var booking in Parent.Bookings)
				{
					foreach (DtbBookingPackage_PackageView package in booking.Packages_PackageView)
					{
						if (package.Package.IsContainer)
						{
							result += package.QuantityFromInstructions;
						}
					}
				}
				return result;
			}
		}

		public override ZDecimal TEUCount
		{
			get
			{
				if (ShouldUseParentSupporter)
				{
					return ParentSupporter.TEUCount;
				}
				ZDecimal result = 0m;
				foreach (var booking in Parent.Bookings)
				{
					foreach (DtbBookingPackage_PackageView package in booking.Packages_PackageView)
					{
						if (package.Package.IsContainer && package.Package.Container.ContainerType != null)
						{
							result += package.QuantityFromInstructions * package.Package.Container.ContainerType.RC_TEU;
						}
					}
				}
				return result;
			}
		}

		public override int OuterPackTotal
		{
			get
			{
				if (ShouldUseParentSupporter)
				{
					return ParentSupporter.OuterPackTotal;
				}
				return base.OuterPackTotal;
			}
		}

		ChargeableFactorSource IJobInvoicingSupporterWithChargeableFactorSource.ChargeableFactorSource => ChargeableFactorSource.TransportBooking;

		public override ZDateTime ATA
		{
			get { return ParentSupporter.ATA; }
		}

		public override ZDateTime ATD
		{
			get { return ParentSupporter.ATD; }
		}

		public override ZDecimal ActualLoadingMeters
		{
			get { return ParentSupporter.ActualLoadingMeters; }
		}

		public override ZDecimal ActualVolume
		{
			get { return ParentSupporter.ActualVolume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return ParentSupporter.ActualVolumeUnit; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return ParentSupporter.AuditSecurity;
		}

		public override OrgHeader Broker
		{
			get { return ParentSupporter.Broker; }
		}

		public override OrgHeader Consignee
		{
			get { return ParentSupporter.Consignee; }
		}

		public override OrgHeader Consignor
		{
			get { return ParentSupporter.Consignor; }
		}

		public override ZDecimal ConsolExchangeRate
		{
			get { return ParentSupporter.ConsolExchangeRate; }
		}

		public override RefCurrency ConsolRateCurrency
		{
			get { return ParentSupporter.ConsolRateCurrency; }
		}

		public override ZString ConsolType
		{
			get { return ParentSupporter.ConsolType; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return ParentSupporter.ConsumerType; }
		}

		public override ZString ContainerMode
		{
			get { return ParentSupporter.ContainerMode; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return ParentSupporter.CreateAccountingJobOnSavingOfOperationsJob; }
		}

		public override ZString DefaultChargeGroup
		{
			get { return ParentSupporter.DefaultChargeGroup; }
		}

		public override OrgHeader GetDefaultDebtor(AccChargeCode code, JobHeader job, ZString relatedJobNumber) => ParentSupporter.GetDefaultDebtor(code, job, relatedJobNumber);

		public override RefUNLOCO Destination
		{
			get { return ParentSupporter.Destination; }
		}

		public override ZDateTime ETA
		{
			get { return ParentSupporter.ETA; }
		}

		public override ZDateTime ETD
		{
			get { return ParentSupporter.ETD; }
		}

		protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return ParentSupporter.EditSecurityCheckpoint;
		}

		public override bool EditSecurityLock
		{
			get { return ParentSupporter.EditSecurityLock; }
		}

		public override ZString EditSecurityMessage
		{
			get { return ParentSupporter.EditSecurityMessage; }
		}

		public override ZDateTime GetCustomsClearanceDate()
		{
			return ParentSupporter.GetCustomsClearanceDate();
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return ParentSupporter.GetDefaultCreditor(defaultCreditorSetting);
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			return ParentSupporter.GetOperationsSignificantDate(significantDateCode);
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return GetOperationsSignificantDate(significantDateCode);
		}

		public override string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
		{
			return ParentSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCodePK);
		}

		public override string GetReasonNotToAllowPosting()
		{
			return ParentSupporter.GetReasonNotToAllowPosting();
		}

		public override ZString HouseBillNumber
		{
			get { return ParentSupporter.HouseBillNumber; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return ParentSupporter.PaymentTerm; }
		}

		public override bool IsDirectShipment
		{
			get { return ParentSupporter.IsDirectShipment; }
		}

		public override bool IsDomestic
		{
			get { return ParentSupporter.IsDomestic; }
		}

		public override bool IsExport
		{
			get { return ParentSupporter.IsExport; }
		}

		public override bool IsImport
		{
			get { return ParentSupporter.IsImport; }
		}

		public override bool IsPlugInReadOnly
		{
			get { return ParentSupporter.IsPlugInReadOnly; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return ParentSupporter.JobInvoicingSecurity;
		}

		public override ZString MasterBillNumber
		{
			get { return ParentSupporter.MasterBillNumber; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return ParentSupporter.OperationsBranch; }
		}

		public override RefUNLOCO Origin
		{
			get { return ParentSupporter.Origin; }
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get { return ParentSupporter.OverriddenDepartmentPK; }
		}

		public override void PostedStateChanged()
		{
			ParentSupporter.PostedStateChanged();
		}

		public override OrgHeader ReceivingAgent
		{
			get { return ParentSupporter.ReceivingAgent; }
		}

		public override OrgHeader SendingAgent
		{
			get { return ParentSupporter.SendingAgent; }
		}

		public override ZString ShipmentNumberOfColoadMaster
		{
			get { return ParentSupporter.ShipmentNumberOfColoadMaster; }
		}

		public override RefUNLOCO GetTranshipmentPort(CostSell costOrSell)
		{
			return ParentSupporter.GetTranshipmentPort(costOrSell);
		}

		public override ZString TransportMode
		{
			get { return ParentSupporter.TransportMode; }
		}

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return true;    //ALL jobs on the consolidated TB should be used for apportionment 
		}

		public override SecurityCheckpoint ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
