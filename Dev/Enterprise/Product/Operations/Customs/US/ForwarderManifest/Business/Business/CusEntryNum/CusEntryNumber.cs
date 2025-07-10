using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class CusEntryNumber : Common.CusEntryNumber
	{
		public CusEntryNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CE_EntryIsSystemGenerated = false;
			CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
		}

		public new CusEntryNumValidation Validation => (CusEntryNumValidation)base.Validation;

		protected override Common.CusEntryNumValidation GetNewValidation() => new CusEntryNumValidation(this);
	}
}
