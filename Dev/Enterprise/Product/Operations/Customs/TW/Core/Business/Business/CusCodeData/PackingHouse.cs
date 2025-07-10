using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class PackingHouse : CusCodeData
	{
		public PackingHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		public ZZRefCusCodeListCombined CusCode => TWRefCusCodeListLoader.GetPackingHouse(Factory, CY_Code, ZDateTime.Now);

		public override ZString Description => CusCode?.ZZD_Description ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.TW.Business.PackingHouse|CY_Code", Caption = "Code", FullDescription = "The code of packing house for plant products.")]
		[RelatedBusinessObject(nameof(CusCode))]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.PackingHouses;
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new PackingHouseValidation(this);
		}

		public new PackingHouseValidation Validation => (PackingHouseValidation)base.Validation;

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public new PackingHouseLookups Lookups => (PackingHouseLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new PackingHouseLookups(this);
		}
	}
}
