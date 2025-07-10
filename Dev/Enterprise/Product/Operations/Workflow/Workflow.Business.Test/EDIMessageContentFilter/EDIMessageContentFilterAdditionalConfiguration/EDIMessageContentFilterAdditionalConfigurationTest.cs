using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterAdditionalConfiguration))]
	class EDIMessageContentFilterAdditionalConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIMessageContentFilter>().UniversalShipment.AdditionalConfiguration;
		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return nameof(EDIMessageContentFilterAdditionalConfiguration.PrimaryDataSource);
			}
		}
	}
}
