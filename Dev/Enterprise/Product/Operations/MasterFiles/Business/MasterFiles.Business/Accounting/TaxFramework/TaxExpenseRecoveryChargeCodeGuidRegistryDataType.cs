using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public class TaxExpenseRecoveryChargeCodeGuidRegistryDataType : GuidRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue != Guid.Empty)
			{
				var factory = new BusinessObjectFactory();
				var collection = new DynamicBusinessObjectCollection(factory);
				var parameters = new ZSqlParameterCollection();
				parameters.Add(ZSqlParameter.New("@ChargeCodePK", proposedValue, AccChargeCodeSchema.PK));
				parameters.Add(ZSqlParameter.New("@CompanyPK", companyPK, AccChargeCodeSchema.AC_GC));

				var sqlText = $@"
SELECT DISTINCT ETC_TaxSystemCode
FROM
	dbo.AccTaxOverrideGroupChargeCodePivot
INNER JOIN dbo.AccTaxOverrideGroup ON ACP_AX_TaxOverrideGroup = AX_PK
INNER JOIN dbo.AccChargeCode ON ACP_AC_ChargeCode = AC_PK
INNER JOIN dbo.AccTaxOverrideGroupTaxConfigurationPivot ON AX_PK = AXP_AX_TaxOverrideGroup
INNER JOIN dbo.AccTaxConfiguration ON AXP_ETC_TaxConfiguration = ETC_PK
WHERE 
	AC_PK = @ChargeCodePK AND AC_GC = @CompanyPK
	AND ETC_ParentID IN
		(
			SELECT
				@CompanyPK
			UNION ALL
			SELECT
				GB_PK
			FROM
				dbo.GlbBranch
			WHERE
				GB_GC = @CompanyPK
				AND GB_IsActive = 1
		)";

				collection.Load(sqlText, parameters);

				if (collection.Count > 0)
				{
					for (int i = 0; i < collection.Count; i++)
					{
						var taxSystem = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystem((ZString)collection[i][AccTaxConfigurationSchema.Constants.ETC_TaxSystemCode], factory);

						if (taxSystem.TaxSuperType == TaxSuperTypeList.TurnoverTax.Code && !taxSystem.IncludeInInvoceTotal)
						{
							throw new RegistryValidationException(Res.GetString("d9ed7478-2219-4c21-84ea-2aac31bbdc8b", "You cannot set this charge code because this charge code already attached to one of the Tax Override Group for TRX EXPENSE Tax Configuration."));
						}
					}
				}
			}
		}
	}
}
