using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.CusReconBase
{
	public class CusReconDeclaration : AutoCusReconDeclaration, Integration.Customs.IBaseCusReconDeclaration
	{
		public CusReconDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusReconDeclarationTypeDecider TypeDecider = new CusReconDeclarationTypeDecider();

		public ZString CountryCode => Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateDataModelIfNeeded();
		}

		protected virtual void PopulateDataModelIfNeeded()
		{
			if (!IsInDatabase)
			{
				CRD_DataModel = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);
			}
		}
	}
}
