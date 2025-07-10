using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class TypeSafeBill : Customs.Business.Bill
	{
		protected TypeSafeBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusDecHouseBillLookups Lookups
		{
			get { return (CusDecHouseBillLookups)base.Lookups; }
		}

		public new CusDecHouseBillValidation Validation
		{
			get { return (CusDecHouseBillValidation)base.Validation; }
		}

		public new JobDeclaration Declaration
		{
			get { return base.Declaration as JobDeclaration; }
		}
	}
}
