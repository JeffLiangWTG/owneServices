using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaContainerWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new AsycudaContainerWrapper(null);
		}

		public void TestMessageSequence()
		{
			Assert(wrappedContainer.MessageSequence.IsEmpty);
			wrappedContainer.MessageSequence = 2;
			AssertEquals(2, wrappedContainer.MessageSequence);
		}

		public void TestContainerNumber()
		{
			Assert(wrappedContainer.ContainerNumber.IsEmpty);
			container.ACN_ContainerNumber = "HKFU0029385";
			AssertEquals("HKFU0029385", wrappedContainer.ContainerNumber);
		}

		public void TestContainerMode()
		{
			Assert(wrappedContainer.ContainerMode.IsEmpty);
		}

		public void TestStatus()
		{
			Assert(wrappedContainer.Status.IsEmpty);
			container.ACN_EmptyFullIndicator = ContainerStatusList.Codes.C7;
			AssertEquals(ContainerStatusList.Codes.C7, wrappedContainer.Status);
		}

		public void TestSize()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR);
			container.ACN_AMA_Manifest = header.PK;
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOEquipmentSizeTypeCode = "15";
			Factory.Save();
			Assert(wrappedContainer.Size.IsEmpty);
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertEquals("15", wrappedContainer.Size);
		}

		public void TestStowPosition()
		{
			Assert(wrappedContainer.StowPosition.IsEmpty);
			container.ACN_StowageLocation = "ABC";
			AssertEquals("ABC", wrappedContainer.StowPosition);
		}

		public void TestSealNumbers()
		{
			AssertEquals(Enumerable.Empty<ZString>(), wrappedContainer.SealNumbers);
			container.ACN_Seal1 = "4356";
			container.ACN_Seal2 = "9876";
			AssertEquals(1, wrappedContainer.SealNumbers.Count());
			AssertEquals("4356", wrappedContainer.SealNumbers.First());
		}

		public void TestIsPallet()
		{
			Assert(!wrappedContainer.IsPallet);
		}

		public void TestSealingParty()
		{
			Assert(wrappedContainer.SealingParty.IsEmpty);
		}

		public void TestAttachedEquipmentCode()
		{
			Assert(wrappedContainer.AttachedEquipmentCode.IsEmpty);
		}

		public void TestRelatedPackages()
		{
			AssertEquals(Enumerable.Empty<ZGuid>(), wrappedContainer.RelatedPackages);
		}

		public void TestStuffingLocation()
		{
			Assert(wrappedContainer.StuffingLocation.IsEmpty);
		}

		public void TestPK()
		{
			Assert(wrappedContainer.PK.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<AsycudaContainer>();
			wrappedContainer = new AsycudaContainerWrapper(container);
		}

		AsycudaContainer container;
		ITransportEquipment wrappedContainer;
	}
}
