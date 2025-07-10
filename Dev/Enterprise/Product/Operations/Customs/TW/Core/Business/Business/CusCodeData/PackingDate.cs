using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class PackingDate : CusCodeData
	{
		public PackingDate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new PackingDateValidation(this);
		}

		public new PackingDateValidation Validation => (PackingDateValidation)base.Validation;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.PackingDates;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.PackingDate|CY_Date", Caption = "Date", FullDescription = "The date of packing for animal products.")]
		public override ZDateTime CY_Date { get => base.CY_Date; set => base.CY_Date = value; }
	}
}
