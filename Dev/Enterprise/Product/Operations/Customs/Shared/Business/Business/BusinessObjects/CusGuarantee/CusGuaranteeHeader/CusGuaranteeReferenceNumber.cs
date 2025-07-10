using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class CusGuaranteeReferenceNumber : CusCodeData
	{
		public CusGuaranteeReferenceNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public BaseCusGuaranteeHeader CusGuarantee => (BaseCusGuaranteeHeader)base.Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = GuaranteeCusCodeDataTypeList.Codes.GRN;
		}

		[ResourceStringData("96AD80B7-3CCD-43EE-A94F-B8522E8338BB", Caption = "Type")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		protected override CusCodeDataLookups GetNewLookups() => new CusGuaranteeReferenceNumberLookups(this);

		protected internal override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(BaseCusGuaranteeHeader));
	}
}
