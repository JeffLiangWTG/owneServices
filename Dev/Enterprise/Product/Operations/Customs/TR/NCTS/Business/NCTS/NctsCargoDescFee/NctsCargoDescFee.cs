using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsCargoDescFee : EU.NCTS.Business.NctsCargoDescFee, Integration.Customs.TR.INctsCargoDescFee
	{
		public NctsCargoDescFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsCargoDescFeeValidation Validation => (NctsCargoDescFeeValidation)base.Validation;

		protected override CusInBondFeeValidation GetNewValidation() => new NctsCargoDescFeeValidation(this);

		public new NctsCargoDescFeeLookups Lookups => (NctsCargoDescFeeLookups)base.Lookups;

		protected override CusInBondFeeLookups GetNewLookups() => new NctsCargoDescFeeLookups(this);
	}
}
