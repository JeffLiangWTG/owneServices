using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVAirWayBillValidator : USAirWayBillValidator
	{
		public CusUSLVAirWayBillValidator(CusUSLVClearance clearance)
			: base(null)
		{
			this.clearance = clearance;
		}

		readonly CusUSLVClearance clearance;

		protected override bool IsImportDeclarationWithNonNumericMasterBillPrefix => !clearance.MasterBillCarrierHasNumericBillPrefix;
	}
}
