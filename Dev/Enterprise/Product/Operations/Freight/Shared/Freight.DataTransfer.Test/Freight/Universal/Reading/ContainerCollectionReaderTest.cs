using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ContainerCollectionReader<>))]
	sealed class ContainerCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";
			container1.JC_SealNum = "SEAL";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBB";

			var containerDataObject1 = CreateDataObject(container1.JC_ContainerNum);
			containerDataObject1.Seal = "ANGRYSEAL";
			var containerDataObject2 = CreateDataObject("ZZZ");
			containerDataObject2.Seal = "ZUMBA";

			var logger = new TestErrorLogger();
			var reader = new ContainerCollectionReader<CommonContainer>(new DataObjectList<Container>
			{
				containerDataObject1, containerDataObject2
			},
			logger, new UniversalObjectFactory(), consol.Containers);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AAA|ANGRYSEAL", "ZZZ|ZUMBA"
			},
			FormatContainers(consol));
		}

		public void TestRemoveFromCollection_WhenContainerHasLinkedNonEditableSupplierBooking_ThenThrowDataObjectReadFailureException()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AAA";
				var supplierBooking = Factory.BOFactory.New<IJobSupplierBooking>();
				supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Planned;
				container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

				var reader = new ContainerCollectionReader<CommonContainer>(new DataObjectList<Container> { }, new TestErrorLogger(), new UniversalObjectFactory(), consol.Containers);

				const string expectMessage = "Container AAA is already allocated to an active Supplier Booking in status PLN or CNV. The Supplier Booking must be canceled in order to edit the container count or type on this container.";
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectMessage, reader.ReadIntoCollection);
			});
		}

		[ExpectNoExceptions]
		public void TestRemoveFromCollection_WhenContainerHasLinkedShipmentsWithDeclarations_UnlinkCusContainerAndConsol()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "BBB";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ContainerType = "CNP";

			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var cusContainer = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = container.PK;

			var reader = new ContainerCollectionReader<CommonContainer>(new DataObjectList<Container> { }, new TestErrorLogger(), new UniversalObjectFactory(), consol.Containers);
			reader.ReadIntoCollection();

			AssertEquals(ZGuid.Empty, container.JC_JK);
			AssertEquals(ZGuid.Empty, container.JC_OH_CFSClient);
			Assert(container.HasLinkedShipmentsWithDeclarations);
		}

		public void TestRemoveFromCollection_WhenContainerCanNotBeDeletedAndContainerNumberIsEmpty_ThenFallBackToContainerTypeAndContainerCount()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew(typeof(NotAbleToDeleteContainer));
				container.JC_ContainerCount = 2;

				var refContainer = Factory.New<RefContainer>();
				refContainer.RC_ContainerType = "CNP";

				container.JC_RC = refContainer.PK;

				var supplierBooking = Factory.BOFactory.New<IJobSupplierBooking>();
				supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Planned;
				container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

				var reader = new ContainerCollectionReader<CommonContainer>(new DataObjectList<Container> { }, new TestErrorLogger(), new UniversalObjectFactory(), consol.Containers);

				const string expectMessage = "Container CNP x 2 is already allocated to an active Supplier Booking in status PLN or CNV. The Supplier Booking must be canceled in order to edit the container count or type on this container.";
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectMessage, reader.ReadIntoCollection);
			});
		}

		public void TestRemoveFromCollection_WhenContainerCanNotBeDeletedAndReasonIsNotCoveredInReader_ThenFallBackToGeneralMessage()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew(typeof(NotAbleToDeleteContainer));
			container.JC_ContainerNum = "AAA";

			var reader = new ContainerCollectionReader<CommonContainer>(new DataObjectList<Container> { }, new TestErrorLogger(), new UniversalObjectFactory(), consol.Containers);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Container AAA may not be deleted: This Container has blablabla.", reader.ReadIntoCollection);
		}

		class NotAbleToDeleteContainer : CommonContainer
		{
			public NotAbleToDeleteContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool CanDelete => false;

			public override MultilingualString ReasonForNotAbleToDelete => (NoResString)"This Container has blablabla.";
		}

		#region Implementation

		string[] FormatContainers(CommonConsol consol)
		{
			return consol.Containers.Cast<CommonContainer>()
				.Select(c => string.Format("{0}|{1}", c.JC_ContainerNum, c.JC_SealNum))
				.ToArray();
		}

		Container CreateDataObject(string containerNumber)
		{
			var dataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ContainerNumber = containerNumber;
			return dataObject;
		}

		#endregion
	}
}
