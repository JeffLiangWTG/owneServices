

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusEntryLineFee Clone()
		{
			return (CusEntryLineFee)base.Clone();
		}

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

		public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups()
		{
			return new CusEntryLineFeeLookups(this);
		}

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation()
		{
			return new CusEntryLineFeeValidation(this);
		}

		#endregion

		#endregion
	}
}
