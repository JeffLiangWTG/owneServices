using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DummyWithDtbBookingJobInvoicingSupporter : DummyJobHeaderParentJobInvoicingSupporter
	{
		public override ZDateTime ATA
		{
			get { return new ZDateTime(ZDateTime.Now.Year, 5, 15); }
		}

		public override ZDateTime ATD
		{
			get { return new ZDateTime(ZDateTime.Now.Year, 5, 16); }
		}

		public override ZDecimal ActualChargeable
		{
			get { return 100m; }
		}

		public override ZString ActualChargeableUnit
		{
			get { return "M3"; }
		}

		public override ZDecimal ActualLoadingMeters
		{
			get { return 1m; }
		}

		public override ZDecimal ActualVolume
		{
			get { return 2m; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return "M3"; }
		}

		public override ZDecimal ActualWeight
		{
			get { return 3m; }
		}

		public override ZString ActualWeightUnit
		{
			get { return "KG"; }
		}

		public override SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.DtbBookingConsolidation; }
		}

		public override OrgHeader Broker
		{
			get { return null; }
		}

		public override OrgHeader Consignee
		{
			get { return null; }
		}

		public override OrgHeader Consignor
		{
			get { return null; }
		}

		public override ZDecimal ConsolExchangeRate
		{
			get { return 4m; }
		}

		public override RefCurrency ConsolRateCurrency
		{
			get { return null; }
		}

		public override ZString ConsolType
		{
			get { return "ConsolType"; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Consol; }
		}

		public override int ContainerCount
		{
			get { return 5; }
		}

		public override ZString ContainerMode
		{
			get { return "ContainerMode"; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return false; }
		}

		public override ZString DefaultChargeGroup
		{
			get { return "DefaultChargeGroup"; }
		}

		public override OrgHeader GetDefaultDebtor(AccChargeCode code, JobHeader job, ZString relatedJobNumber) => null;

		public override RefUNLOCO Destination
		{
			get { return null; }
		}

		public override ZDateTime ETA
		{
			get { return new ZDateTime(ZDateTime.Now.Year, 5, 17); }
		}

		public override ZDateTime ETD
		{
			get { return new ZDateTime(ZDateTime.Now.Year, 5, 18); }
		}

		public override SecurityCheckpoint EditSecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsolidationEdit; }
		}

		public override bool EditSecurityLock
		{
			get { return false; }
		}

		public override ZString EditSecurityMessage
		{
			get { return "EditSecurityMessage"; }
		}

		public override ZDateTime GetCustomsClearanceDate()
		{
			return new ZDateTime(ZDateTime.Now.Year, 5, 19);
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return null;
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			return new ZDateTime(ZDateTime.Now.Year, 5, 20);
		}

		public override string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
		{
			return "ReasonNotToAllowChangeOnCostDetails";
		}

		public override string GetReasonNotToAllowPosting()
		{
			return "ReasonNotToAllowPosting";
		}

		public override ZString HouseBillNumber
		{
			get { return "HouseBillNumber"; }
		}

		public override bool IsDirectShipment
		{
			get { return true; }
		}

		public override bool IsDomestic
		{
			get { return true; }
		}

		public override bool IsExport
		{
			get { return true; }
		}

		public override bool IsImport
		{
			get { return false; }
		}

		public override bool IsPlugInReadOnly
		{
			get { return false; }
		}

		public override JobHeader Job
		{
			get { return job; }
		}

		public void SetJob(JobHeader job)
		{
			this.job = job;
		}
		JobHeader job;

		public override string JobNumber => "Default";

		public override SecurityCheckpoint JobInvoicingSecurity
		{
			get { return Env.Security.MaintainShipmentJobInvoicing; }
		}

		public override ZString MasterBillNumber
		{
			get { return "MasterBillNumber"; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return null; }
		}

		public override RefUNLOCO Origin
		{
			get { return null; }
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get { return ZGuid.Empty; }
		}

		public override void PostedStateChanged()
		{
		}

		public override OrgHeader ReceivingAgent
		{
			get { return null; }
		}

		public override OrgHeader SendingAgent
		{
			get { return null; }
		}

		public override ZString ShipmentNumberOfColoadMaster
		{
			get { return "ShipmentNumberOfColoadMaster"; }
		}

		public override bool IncludeInConsolCosting(bool includeRelatedShipments)
		{
			return true;
		}

		public override ZDecimal TEUCount
		{
			get { return 6m; }
		}

		public override RefUNLOCO GetTranshipmentPort(CostSell costOrSell) => null;

		public override ZString TransportMode
		{
			get { return "TransportMode"; }
		}
	}
}
