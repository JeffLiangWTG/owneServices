using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test.BusinessObjects.TelPreDriveChecklistEntry
{
	[TestedType(typeof(TelPreDriveChecklistEntryCollection))]
	public class TelPreDriveChecklistEntryCollectionTests : ActiveBusinessObjectCollectionTestCase<TelPreDriveChecklistEntryCollection>
	{
		protected override TelPreDriveChecklistEntryCollection GetCollectionToTest()
		{
			return new TelPreDriveChecklistEntryCollection(Factory, new ZQuery());
		}
	}
}
