namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class CheckDefaultNamingViolation
	{
		public const string Query = @"
select o.name as ObjectName,t.name as TableName,o.type_desc as ObjectDescription 
from sys.tables t 
join (
select o.is_system_named,o.name,o.type_desc,o.parent_object_id
from sys.default_constraints o
union all 
select o.is_system_named,o.name,o.type_desc,o.parent_object_id
from sys.check_constraints o
) o on o.parent_object_id = t.object_id
and o.is_system_named = 1";
	}
}
