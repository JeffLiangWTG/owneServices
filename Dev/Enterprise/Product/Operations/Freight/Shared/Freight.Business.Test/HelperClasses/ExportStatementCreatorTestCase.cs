using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class ExportStatementCreatorTestCase : TestCaseWithFactory
	{
		protected OrgHeader CreateOrganisation(ZString name, ZString address1, ZString eIN)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.MainAddress.OA_Address1 = address1;
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, eIN, USCountry);
			return org;
		}

		protected RefCountry USCountry
		{
			get
			{
				if (fUSCountry == null)
				{
					fUSCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
				}
				return fUSCountry;
			}
		}
		RefCountry fUSCountry;
	}
}
