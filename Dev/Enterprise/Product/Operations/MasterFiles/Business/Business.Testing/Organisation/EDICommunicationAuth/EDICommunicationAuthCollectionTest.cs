using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationAuthCollection))]
	public class EDICommunicationAuthCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(EDICommunicationAuthCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDICommunicationAuthCollection(Factory);
		}
	}
}
