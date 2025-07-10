using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class RestrictedCode : Customs.Business.CusCodeData, Integration.Customs.US.IRestrictedCode
	{
		public RestrictedCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(RestrictedCodeLookups.RestrictedCodeList))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		#region Implementation

		public new RestrictedCodeLookups Lookups
		{
			get { return (RestrictedCodeLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new RestrictedCodeLookups(this);
		}

		public new RestrictedCodeValidation Validation
		{
			get { return (RestrictedCodeValidation)base.Validation; }
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new RestrictedCodeValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.IORBusinessRules;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (CY_Data.IsEmpty || Parent == null || Parent.IsDeleted)
			{
				Delete();
			}
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(OrgCountryData)); }
		}

		#endregion
	}
}
