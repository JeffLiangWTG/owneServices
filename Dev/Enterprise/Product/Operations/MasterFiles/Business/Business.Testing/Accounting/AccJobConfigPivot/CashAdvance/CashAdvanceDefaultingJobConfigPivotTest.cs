using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CashAdvanceDefaultingJobConfigPivotTest : AccJobConfigPivotTest
	{
		public void TestJobConfiguration()
		{
			var configuration = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			configuration.CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			configuration.CAC_GC = Env.CurrentCompanyPK;
			configuration.CAC_Ledger = LedgerTypes.AccountsReceivable;
			configuration.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			configuration.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.CAC_TransportMode = Core.Constants.TransportModes.Sea;
			configuration.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			var pivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			pivot.JCT_Code = ChargeCodeGroupList.Codes.Freight;
			pivot.JCT_JCF_JobConfig = configuration.PK;

			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedPivot = reloadFactory.Load<CashAdvanceDefaultingChargeGroup>(pivot.PK);
			AssertNotNull("Pivot is linked to a AccJobConfig record", reloadedPivot.JobConfiguration);
			AssertEquals("Pivot is linked to correct AccJobConfig record", configuration.PK, reloadedPivot.JobConfiguration.PK);
		}
	}
}
