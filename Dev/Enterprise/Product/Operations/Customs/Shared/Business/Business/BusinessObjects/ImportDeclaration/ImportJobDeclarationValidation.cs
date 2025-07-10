using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class ImportJobDeclarationValidation : ZValidation
	{
		public ImportJobDeclarationValidation(ImportJobDeclaration parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateDeclarationPK();
			ValidateCountryCode();
		}

		public override Type AutoValidationType
		{
			get { return typeof(ImportJobDeclarationValidation); }
		}

		public void ValidateDeclarationPK()
		{
			ValidateCalculatedProperty(parent.DeclarationPKInfo);
		}

		protected void CheckDeclarationPK()
		{
			if (!parent.DeclarationPK.IsValid)
			{
				parent.DeclarationPKInfo.AddError(Res.GetString("8a73fdf1-7b33-418b-9e42-fc84d3473453", "Please select a valid declaration."));
			}
			else
			{
				BaseJobDeclaration declaration = parent.Factory.Load<BaseJobDeclaration>(parent.DeclarationPK);
				if (declaration != null && !declaration.JE_JS.IsEmpty)
				{
					parent.DeclarationPKInfo.AddError(Res.GetString("50032219-f58c-47ab-8b0a-486b811d86ca", "This Declaration is attached to a shipment. Import by using the brokerage tab on the shipment, or 'Create Declaration from Shipment' menu option"));
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(parent.DeclarationPKInfo, parent.Lookups.DeclarationList);
				}
			}
		}

		public void ValidateCountryCode()
		{
			ValidateCalculatedProperty(parent.CountryCodeInfo);
		}

		protected void CheckCountryCode()
		{
			if (parent.CountryCode.IsEmpty)
			{
				parent.CountryCodeInfo.AddError(Res.GetString("9eeddf25-f3e5-4157-8d1c-a6087d36bda0", "Please select a valid country code."));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(parent.CountryCodeInfo, parent.Lookups.CountryList);

				if (!parent.CountryCodeInfo.HasNotifications())
				{
					if (parent.CountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					{
						parent.CountryCodeInfo.AddError(Res.GetString("b5ccde3c-0f27-4322-b7c4-ad0a1607ba23", "Please use the 'Copy' menu instead of importing the declaration for this country."));
					}
				}
			}
		}

		readonly ImportJobDeclaration parent;
	}
}
