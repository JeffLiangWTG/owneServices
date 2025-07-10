using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterUniversalShipmentSpec))]
	class EDIMessageContentFilterUniversalShipmentSpecTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIMessageContentFilter>().UniversalShipment;
		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return nameof(EDIMessageContentFilterUniversalShipmentSpec.FilterType);
				yield return nameof(EDIMessageContentFilterUniversalShipmentSpec.Lines);
				yield return nameof(EDIMessageContentFilterUniversalShipmentSpec.Documents);
				yield return nameof(EDIMessageContentFilterUniversalShipmentSpec.AdditionalConfiguration);
			}
		}
	}
}
