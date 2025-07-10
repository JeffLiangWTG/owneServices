using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	static internal class CollectionTest_Extensions
	{
		internal static bool IsCondition2Met(this ProcessTaskCollection collection, string condition, string value)
		{
			var task = collection.Factory.New<ProcessTask>();
			task.TemplateConditions.TemplateCondition2 = condition;
			task.TemplateConditions.TemplateCondition2Value = value;
			return collection.IsCondition2Met(task);
		}
	}
}
