using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaMessageCollection))]
	sealed class AsycudaMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestManifestMessageCollection()
		{
			AssertEquals(typeof(AsycudaMessageCollection), manifestHeader.Messages.GetType());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<AsycudaMessage>();

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaMessageCollection(Factory, manifestHeader);

		protected override Type GetExpectedCollectionType() => typeof(AsycudaMessageCollection);

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();
		}
		AsycudaManifestHeader manifestHeader;
	}
}
