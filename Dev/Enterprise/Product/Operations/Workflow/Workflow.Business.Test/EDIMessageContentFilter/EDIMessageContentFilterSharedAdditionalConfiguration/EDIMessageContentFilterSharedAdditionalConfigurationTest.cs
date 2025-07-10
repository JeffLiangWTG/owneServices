using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterSharedAdditionalConfiguration))]
	class EDIMessageContentFilterSharedAdditionalConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIMessageContentFilter>().Config;
		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return nameof(EDIMessageContentFilterSharedAdditionalConfiguration.ExcludeEmptyElements);
			}
		}
	}
}
