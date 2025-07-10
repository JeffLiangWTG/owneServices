using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class LicenceAndPermit : Customs.Business.CusCodeData, IACELicenceAndPermit
	{
		public LicenceAndPermit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new LicenceAndPermitLookup Lookups
		{
			get { return (LicenceAndPermitLookup)base.Lookups; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Permits/Licenses"; }
		}

		[MaxLength(10)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value.ToUpper(); }
		}

		#region IACELicenceAndPermit Members

		ZString IACELicenceAndPermit.LicenseCertificatePermitTypeCode
		{
			get { return CY_Code; }
		}

		ZString IACELicenceAndPermit.LicenseNumberCertificateNumberPermitNumber
		{
			get { return CY_Data; }
		}

		#endregion

		#region Implementation

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new LicenceAndPermitLookup(this);
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new LicenceAndPermitValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CY_Type = CusCodeDataTypeList.Codes.LicenceAndPermit;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}

		#endregion
	}
}
