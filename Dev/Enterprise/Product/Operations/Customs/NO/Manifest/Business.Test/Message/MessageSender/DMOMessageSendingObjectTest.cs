using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOMessageSendingObject))]
sealed class DMOMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestCustomsLevel_Attributes() => CombineAssertions(() =>
		AssertEntity<DMOMessageSendingObject>()
			.HasProperty(x => x.CustomsLevel)
			.WithCaption("Customs Level"));

	public void TestBillNumber_Attributes() => CombineAssertions(() =>
		AssertEntity<DMOMessageSendingObject>()
			.HasProperty(x => x.BillNumber)
			.WithCaption("Bill Number"));

	public void TestRepresentative_Attributes() => CombineAssertions(() =>
		AssertEntity<DMOMessageSendingObject>()
			.HasProperty(x => x.Representative)
			.WithCaption("Representative"));

	public void TestConsignee_Attributes() => CombineAssertions(() =>
		AssertEntity<DMOMessageSendingObject>()
			.HasProperty(x => x.Consignee)
			.WithCaption("Consignee"));

	protected override BusinessObject GetNewBusinessObject()
	{
		return new DMOManifestHeaderMessageSendingObject(Factory.New<AsycudaManifestHeader>());
	}
}
