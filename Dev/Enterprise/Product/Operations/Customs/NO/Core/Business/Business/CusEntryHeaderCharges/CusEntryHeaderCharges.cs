using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public class CusEntryHeaderCharges : Customs.Business.CusEntryHeaderCharges, Integration.Customs.NO.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusEntryHeaderCharges Clone() => (CusEntryHeaderCharges)base.Clone();

		public new CusEntryHeaderChargesValidation Validation => (CusEntryHeaderChargesValidation)base.Validation;

		public new CusEntryHeaderChargesLookups Lookups => (CusEntryHeaderChargesLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation() => new CusEntryHeaderChargesValidation(this);

		protected override Customs.Business.CusEntryHeaderChargesLookups GetNewLookups() => new CusEntryHeaderChargesLookups(this);
	}
}
