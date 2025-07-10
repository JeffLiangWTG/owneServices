using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public sealed class DocumentZQuery : ZQuery
	{
		public DocumentZQuery(bool includeForms = false)
		{
			var menuTypes = includeForms
				? new[] { Constants.StmMenuItemTypes.Documents, Constants.StmMenuItemTypes.WebReports, Constants.StmMenuItemTypes.Forms }
				: new[] { Constants.StmMenuItemTypes.Documents, Constants.StmMenuItemTypes.WebReports };

			AddToFilter(StmMenuItemSchema.SU_MenuType, menuTypes);
		}

		public DocumentZQuery(BusinessContext businessContext)
			: this(businessContext.ToString())
		{
		}

		public DocumentZQuery(BusinessContext businessContext, string menuName, bool onlySystemDefined = false)
			: this(businessContext.ToString(), menuName, onlySystemDefined)
		{
		}

		public DocumentZQuery(string businessContext, bool includeForms = false)
			: this(includeForms)
		{
			AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);
		}

		public DocumentZQuery(string businessContext, string menuName, bool onlySystemDefined = false)
			: this(businessContext)
		{
			AddToFilter(StmMenuItemSchema.SU_MenuName, menuName);

			if (onlySystemDefined)
			{
				AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			}
		}

		public DocumentZQuery(SchemaColumn schemaColumn, object value)
			: this(schemaColumn, SQLComparisonOperator.Equal, value)
		{
		}

		public DocumentZQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
			: this()
		{
			AddToFilter(schemaColumn, comparisonOperator, value);
		}
	}
}
