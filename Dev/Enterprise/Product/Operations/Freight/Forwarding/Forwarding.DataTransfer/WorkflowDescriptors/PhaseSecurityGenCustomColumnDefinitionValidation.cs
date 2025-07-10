using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class PhaseSecurityGenCustomColumnDefinitionValidation : AutoGenCustomColumnDefinitionValidation
	{
		public PhaseSecurityGenCustomColumnDefinitionValidation(GenCustomColumnDefinition parent)
			: base(parent)
		{
		}

		protected override void CheckXC_Name()
		{
			base.CheckXC_Name();

			if (!Parent.XC_Name.IsEmpty && Parent.XC_ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix)
			{
				ProcessTaskTemplate processTaskTemplate = Parent.Factory.Load<ProcessTaskTemplate>(Parent.XC_ParentID);
				if (processTaskTemplate != null && !processTaskTemplate.P0_ProcessType.IsEmpty)
				{
					var columnsSubQuery = new ZDBOnlySubQuery(typeof(GenCustomColumnDefinition), GenCustomColumnDefinitionSchema.XC_ParentID);
					columnsSubQuery.AddToFilter(GenCustomColumnDefinitionSchema.XC_Name, SQLComparisonOperator.Equal, Parent.XC_Name);

					var templatesQuery = new ZDBOnlyQuery(typeof(ProcessTaskTemplate));
					templatesQuery.AddToFilter(ProcessTaskTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.XC_ParentID);
					templatesQuery.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, processTaskTemplate.P0_ProcessType);
					templatesQuery.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);
					templatesQuery.AddSubQuery(columnsSubQuery, JoinCondition.And);

					if (Parent.Factory.LoadTop1<ProcessTaskTemplate>(templatesQuery) != null)
					{
						Parent.XC_NameInfo.AddWarning(Res.GetString("df11fd51-703c-45ee-a957-d40ec35ad23f",
	@"There is a Custom Field definition in other '{0}' Workflow Template with same Name which may create confusion with phase control.", processTaskTemplate.P0_ProcessType));
					}
				}
			}
		}
	}
}

