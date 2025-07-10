using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccOrgTaxConfigurationTemplateDataHelper
	{
		void UpdateTaxConfigurationsForMultipleOrgnizations(Guid templatePK);
		void UpdateTaxConfigurationsForOrgnization(AccOrgTaxConfigurationCollectionByLedger targetOrgTaxConfigCollection, IEnumerable<AccOrgTaxConfiguration> sourceOrgTaxConfigList);
	}

	public class AccOrgTaxConfigurationTemplateDataHelper : IAccOrgTaxConfigurationTemplateDataHelper
	{
		public void UpdateTaxConfigurationsForMultipleOrgnizations(Guid templatePK)
		{
			using (var cmd = Db.Connection.Command("UpdateTaxConfigurationsForOrgnization"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@AccOrgTaxConfigurationTemplatePK", SqlDbType.UniqueIdentifier, templatePK);
				cmd.AddParameter("@OperationStaff", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.ExecuteNonQuery();
			}
		}

		public void UpdateTaxConfigurationsForOrgnization(AccOrgTaxConfigurationCollectionByLedger targetOrgTaxConfigCollection, IEnumerable<AccOrgTaxConfiguration> sourceOrgTaxConfigList)
		{
			foreach (var oldOrgTaxConfig in targetOrgTaxConfigCollection)
			{
				oldOrgTaxConfig.OTC_IsActive = false;
			}

			foreach (var sourceOrgTaxConfig in sourceOrgTaxConfigList)
			{
				var targetOrgTaxConfig = targetOrgTaxConfigCollection.SingleOrDefault(x => x.OTC_ETC == sourceOrgTaxConfig.OTC_ETC);

				if (targetOrgTaxConfig == null)
				{
					targetOrgTaxConfig = targetOrgTaxConfigCollection.AddNew();
					targetOrgTaxConfig.OTC_ETC = sourceOrgTaxConfig.OTC_ETC;
				}

				targetOrgTaxConfig.CopyEditableColumns(sourceOrgTaxConfig);
			}
		}
	}
}
