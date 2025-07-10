using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class BulkDetentionHeaderValidation : AutoBulkDetentionHeaderValidation
	{
		public BulkDetentionHeaderValidation(AutoBulkDetentionHeader parent)
			: base(parent) { }

		protected override void CheckDetentionType()
		{
			base.CheckDetentionType();
			ListValidation.ErrorIfInvalidCode(Parent.DetentionTypeInfo, Parent.Lookups.DetentionTypes);
		}

		protected override void CheckPrincipalPK()
		{
			base.CheckPrincipalPK();
			ListValidation.ErrorIfInvalidPK(Parent.PrincipalPKInfo, Parent.Lookups.Principals);
		}

		protected override void CheckClientPK()
		{
			base.CheckClientPK();
			ListValidation.ErrorIfInvalidPK(Parent.ClientPKInfo, Parent.Lookups.Clients);
		}

		protected override void CheckCountryCode()
		{
			base.CheckCountryCode();
			ListValidation.ErrorIfInvalidCode(Parent.CountryCodeInfo, Parent.Lookups.Countries);
		}

		#region Implementation

		new BulkDetentionHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkDetentionHeader)base.Parent; }
		}

		#endregion
	}
}


