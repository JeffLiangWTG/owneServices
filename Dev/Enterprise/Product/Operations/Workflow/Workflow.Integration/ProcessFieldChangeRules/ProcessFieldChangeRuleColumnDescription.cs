using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public static class ProcessFieldChangeRuleColumnDescription
	{
		public static ZString DefaultDescription(SchemaColumn fieldColumn)
		{
			var columnDescription = DataBoundResourceStrings.GetColumnDescriptiveName(fieldColumn.TableName, fieldColumn.Name);
			return columnDescription == fieldColumn.TableName + "|" + fieldColumn.Name ? fieldColumn.Name : columnDescription;
		}
	}
}
