//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccOrgTaxConfigurationTemplateValidation
//
//    This class should be used for overriding validation in AutoAccOrgTaxConfigurationTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Text.RegularExpressions;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using Enterprise.ZArchitecture.Schema;

	public class AccOrgTaxConfigurationTemplateValidation : AutoAccOrgTaxConfigurationTemplateValidation
	{
		public AccOrgTaxConfigurationTemplateValidation(AutoAccOrgTaxConfigurationTemplate parent) : base(parent)
		{
		}

		protected override void CheckOCT_Code()
		{
			base.CheckOCT_Code();
			MandatoryValidation.CheckEntered(Parent.OCT_CodeInfo);
			ValidateUniqueForCompany(Parent.OCT_CodeInfo);

			if (!Parent.OCT_CodeInfo.HasErrors())
			{
				var alphaNumericRegex = new Regex(@"^[a-z0-9]+\s*$", RegexOptions.IgnoreCase);
				if (!alphaNumericRegex.IsMatch(Parent.OCT_Code))
				{
					Parent.OCT_CodeInfo.AddError(Res.GetString("52C3AA53-D6BB-41EF-A3B3-D5EE5A1E2566", "{0} is not a valid Template Code.", Parent.OCT_Code));
				}
			}
		}

		protected override void CheckOCT_Description()
		{
			base.CheckOCT_Description();
			MandatoryValidation.CheckEntered(Parent.OCT_DescriptionInfo);
			ValidateUniqueForCompany(Parent.OCT_DescriptionInfo);
		}

		void ValidateUniqueForCompany(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Notifications.HasErrors())
			{
				var schemaColumn = (AccOrgTaxConfigurationTemplateSchema.Instance as ITableSchema)?.GetSchemaColumn(propertyInfo.Name);
				if (schemaColumn != null)
				{
					var filter = new ZQuery(AccOrgTaxConfigurationTemplateSchema.OCT_GC_Company, Parent.OCT_GC_Company);
					filter.AddToFilter(JoinCondition.And, schemaColumn, SQLComparisonOperator.Equal, propertyInfo.Value);
					filter.AddToFilter(JoinCondition.And, AccOrgTaxConfigurationTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

					if (Parent.Factory.Exists(typeof(AccOrgTaxConfigurationTemplate), filter))
					{
						propertyInfo.AddError(Res.GetString("AE802816-7449-4DF9-8966-BDD70B78BF31", "A Tax Configuration Template with {0} \"{1}\" already exists in the current login company.", propertyInfo.HumanReadableName, propertyInfo.Value));
					}
				}
			}
		}
	}
}
