using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgCusAccountProvider : MasterFiles.Business.OrgCusAccountProvider
	{
		public OrgCusAccountProvider() : base(Core.Constants.CountryCodes.SouthAfrica)
		{ }
		public override MasterFiles.Business.OrgCusAccountLookups GetNewLookups(OrgCusAccount cusAccount) => new OrgCusAccountLookups(cusAccount);

		public override MasterFiles.Business.OrgCusAccountValidation GetNewValidation(OrgCusAccount cusAccount) => new OrgCusAccountValidation(cusAccount);

		public override void SetDefaultValues(OrgCusAccount cusAccount)
		{
			base.SetDefaultValues(cusAccount);
			cusAccount.CZ_Code = FANCode;
		}

		public const string FANCode = "FAN";
		public const string FANDesc = "Override FAN for Deferment Selection";
	}
}
