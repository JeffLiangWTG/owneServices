using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class OfficeCode : EuOfficeCode
	{
		public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		public override bool CanDelete => false;

		protected override ZZRefCusCodeListCombined OfficeCore => CY_Data.IsEmpty ? null : ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Data, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today);
	}
}
