using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	[CodeProperty(OrgSalesProductCustomColumnDefinition.Schema.XC_Name), DescriptionProperty(OrgSalesProductCustomColumnDefinition.Schema.XC_Name)]
	public class OrgSalesProductCustomColumnDefinition : GenCustomColumnDefinition
	{
		public OrgSalesProductCustomColumnDefinition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XC_ParentTableCode = OrgSalesProductSchema.Constants.Prefix;
		}

		#endregion

		#region Validation

		protected override GenCustomColumnDefinitionValidation GetNewValidation()
		{
			return new OrgSalesProductCustomColumnDefinitionValidation(this);
		}

		#endregion

		#region Lookups

		protected override GenCustomColumnDefinitionLookups GetNewLookups()
		{
			return new OrgSalesProductCustomColumnDefinitionLookups(this);
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			XC_ParentTableCode = OrgSalesProductSchema.Constants.Prefix;

			if (XC_ParentID.IsEmpty)
			{
				var parent = GetOrCreateSalesProduct("TST");
				XC_ParentID = parent.PK;
			}
		}

		OrgSalesProduct GetOrCreateSalesProduct(string productCode)
		{
			var result = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<OrgSalesProduct>();
				result.MP_Code = productCode;
			}

			return result;
		}

#endif
		#endregion
	}
}
