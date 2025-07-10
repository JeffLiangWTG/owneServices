using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelPreDriveChecklistTemplateEntryCollection))]
	class TelPreDriveChecklistTemplateEntryCollectionTests : ActiveBusinessObjectCollectionTestCase<TelPreDriveChecklistTemplateEntryCollection>
	{
		protected override TelPreDriveChecklistTemplateEntryCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<TelPreDriveChecklistTemplateHeader>();
			return new TelPreDriveChecklistTemplateEntryCollection(Factory, header);
		}
	}
}
