using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class SlaughterDate : CusCodeData
	{
		public SlaughterDate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new SlaughterDateValidation(this);
		}

		public new SlaughterDateValidation Validation => (SlaughterDateValidation)base.Validation;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.SlaughterDates;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.SlaughterDate|CY_Date", Caption = "Date", FullDescription = "The date of slaughter for animal products.")]
		public override ZDateTime CY_Date { get => base.CY_Date; set => base.CY_Date = value; }
	}
}
