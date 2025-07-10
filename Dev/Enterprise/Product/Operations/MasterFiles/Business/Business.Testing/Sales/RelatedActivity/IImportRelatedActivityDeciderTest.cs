using System;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class IImportRelatedActivityDeciderTest<T> : TestCase
			where T : IImportRelatedActivityDecider
	{
		public void TestImportRelatedActivityPromptUserDeciderAttribute()
		{
			var attribute = typeof(T).GetAttribute<ImportRelatedActivityPromptUserDeciderAttribute>();
			if (attribute != null)
			{
				var type = Type.GetType(attribute.TypeName);
				AssertNotNull("Could not find type: " + attribute.TypeName, type);
				Assert(type.FullName + " must implement " + typeof(T).FullName, typeof(T).IsAssignableFrom(type));
			}
			else
			{
				Assert("This decider does not have a prompt user option", true);
			}
		}
	}
}
