using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CALicenceNumber : CusCodeData, ICusDocument
	{
		public CALicenceNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CALicenceNumber;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobDeclaration)); }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CALicenceNumberValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Controlling Agency Licence"; }
		}

		#region ICusDocument Members

		ZString ICusDocument.LicenceNumber
		{
			get { return CY_Data; }
		}

		#endregion
	}
}
