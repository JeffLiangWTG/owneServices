using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationPartyCollection))]
	public class EDICommunicationPartyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(EDICommunicationPartyCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDICommunicationPartyCollection(Factory);
		}
	}
}
