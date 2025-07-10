using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public abstract class ShipmentBasicRegistrationControlJobTestBase : TestCaseWithFactory
	{
		protected virtual void TestAutoCreateRegistryIsOn(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment) { }
		protected virtual void TestJobDeleted(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment) { }
		protected virtual void TestJobRecreated(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment) { }
		protected virtual void TestAutoCreateRegistryIsOff(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment) { }
		protected virtual void TestJobExistsAndAutoCreateRegistryIsOff(ShipmentFormWithBasicRegistration form, ForwardingShipment shipment) { }

		[RequiresSTA]
		public void TestJobScenario()
		{
			var shipment = new BusinessObjectFactory().NewWithValidTestData<ForwardingShipment>();
			AssertNull(shipment.ShipmentJobHeader);

			shipment.JS_RL_NKOrigin = "AUMEL";
			var mockAccounting1 = GetMockAccounting();

			mockAccounting1.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(true);
			Assert(mockAccounting1.Object.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()));

			using (ObjectFactory.Substitute(mockAccounting1.Object))
			{
				using (var form = new ShipmentFormWithBasicRegistration(shipment))
				{
					form.Show();

					TestAutoCreateRegistryIsOn(form, shipment);

					shipment.ShipmentJobHeader.Delete();
					TestJobDeleted(form, shipment);

					new JobHeader.Loader(shipment).TryCreate();
					TestJobRecreated(form, shipment);
				}
			}

			shipment.ShipmentJobHeader.Delete();

			var mockAccounting2 = GetMockAccounting();

			mockAccounting2.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			Assert(!mockAccounting2.Object.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>()));

			using (ObjectFactory.Substitute(mockAccounting2.Object))
			{
				using (var form = new ShipmentFormWithBasicRegistration(shipment))
				{
					form.Show();

					TestAutoCreateRegistryIsOff(form, shipment);
				}

				var job = new JobHeader.Loader(shipment).TryCreate();
				Factory.Save();

				AssertNotNull(shipment.ShipmentJobHeader);
				AssertEquals(job.PK, shipment.ShipmentJobHeader.PK);

				using (var form = new ShipmentFormWithBasicRegistration(shipment))
				{
					form.Show();

					TestJobExistsAndAutoCreateRegistryIsOff(form, shipment);
				}
			}
		}

		[RequiresSTA]
		public void TestScenarioWhenJobIsDeletedFromJobManagmentModule()
		{
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_RL_NKOrigin = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			shipment2.JS_IsForwardRegistered = true;
			using (var form = new ShipmentFormWithBasicRegistration(shipment2))
			{
				form.Show();
				Factory.Save();

				AssertNotNull(shipment2.ShipmentJobHeader);
				var newFactory = new BusinessObjectFactory();
				var relaodedJob = newFactory.Load<JobHeader>(shipment2.ShipmentJobHeader.PK);
				relaodedJob.MarkAsInactive();
				newFactory.Save();

				AssertEquals("Job is cancelled", true, relaodedJob.IsCancelled);
			}
		}

		public static Mock<IAccounting> GetMockAccounting()
		{
			var mockAccounting = new Mock<IAccounting>();

			mockAccounting.Setup(m => m.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation).Returns("TESTLOCATION");
			mockAccounting.Setup(m => m.APAccountGroup).Returns(Guid.NewGuid());
			mockAccounting.Setup(m => m.ARAccountGroup).Returns(Guid.NewGuid());

			return mockAccounting;
		}
	}
}
