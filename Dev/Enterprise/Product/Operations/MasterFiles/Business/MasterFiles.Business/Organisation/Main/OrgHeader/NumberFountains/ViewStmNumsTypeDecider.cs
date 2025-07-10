using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ViewStmNumsTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(new ZString(row[ViewStmNumsSchema.SN_Name.Name]), factory) : null;
		}

		public Type GetTypeForLoad(IColumnIndexer row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(row.GetValue(ViewStmNumsSchema.SN_Name), factory) : null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		public Type GetTypeForLoad(ZString name, BusinessObjectFactory factory)
		{
			Type result = null;

			if (name.StartsWith(OrganisationViewStmNums.Schema.SN_NamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				result = typeof(OrganisationViewStmNums);
			}
			else if (name.StartsWith(CustomsNumberViewStmNums.Schema.SN_NamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				result = typeof(CustomsNumberViewStmNums);
			}
			else if (name.StartsWith(StaffViewStmNums.Schema.SN_NamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				result = typeof(StaffViewStmNums);
			}

			return result;
		}
	}
}
