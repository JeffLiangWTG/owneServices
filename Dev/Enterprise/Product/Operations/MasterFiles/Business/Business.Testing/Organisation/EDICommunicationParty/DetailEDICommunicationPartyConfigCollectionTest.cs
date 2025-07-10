using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DetailEDICommunicationPartyConfigCollection))]
	public class DetailEDICommunicationPartyConfigCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(DetailEDICommunicationPartyConfigCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var master = Factory.New<EDICommunicationParty>();
			return new DetailEDICommunicationPartyConfigCollection(master);
		}
	}
}
