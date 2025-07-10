using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class DeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		public DeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			var parent = Parent;
			var info = parent.OrganisationPKInfo;

			if (parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && declaration.IsImport)
			{
				if (parent.Organisation != null)
				{
					var yfksNo = parent.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.SupplierCode, Core.Constants.CountryCodes.Turkey);
					if (yfksNo.IsEmpty)
					{
						info.AddMessageError(Res.GetString("12609FE3-CA77-45E3-BF07-D6C0720D7791", "Please provide CSC type (Customs Supplier Code) from TR Customs."));
					}
				}
			}
		}
	}
}
