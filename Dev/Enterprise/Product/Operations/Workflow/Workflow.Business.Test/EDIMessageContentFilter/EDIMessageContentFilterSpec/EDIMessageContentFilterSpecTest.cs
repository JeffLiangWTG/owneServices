using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterSpec))]
	class EDIMessageContentFilterSpecTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIMessageContentFilter>().UniversalEvent;

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return nameof(EDIMessageContentFilterSpec.FilterType);
				yield return nameof(EDIMessageContentFilterSpec.Lines);
				yield return nameof(EDIMessageContentFilterSpec.Documents);
			}
		}
	}
}
