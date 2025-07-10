using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationPartyLookups.EDICommunicationPartyOrgContactCollection))]
	sealed class EDICommunicationPartyOrgContactCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDICommunicationPartyLookups.EDICommunicationPartyOrgContactCollection(Factory);
		}
	}
}
