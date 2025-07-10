using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationPartyConfigCollection))]
	public class EDICommunicationPartyConfigCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(EDICommunicationPartyConfigCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDICommunicationPartyConfigCollection(Factory);
		}
	}
}
