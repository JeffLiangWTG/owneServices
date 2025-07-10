using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = CargoWise.EventReference.Constants;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobDocsAndCartageTest : BaseFreightTest
	{
		public void TestActionFieldDateFormats()
		{
			var values = new[]
			{
				new { Name = JobDocsAndCartageSchema.Constants.JP_EstimatedPickup, Format = ZDateTimePickerFormat.Long },
				new { Name = JobDocsAndCartageSchema.Constants.JP_PickupRequiredBy, Format = ZDateTimePickerFormat.Long },
				new { Name = JobDocsAndCartageSchema.Constants.JP_PickupCartageAdvised, Format = ZDateTimePickerFormat.Long },
				new { Name = JobDocsAndCartageSchema.Constants.JP_PickupCartageCompleted, Format = ZDateTimePickerFormat.Long },

				new { Name = JobDocsAndCartageSchema.Constants.JP_EstimatedDelivery, Format = ZDateTimePickerFormat.Long },
				new { Name = JobDocsAndCartageSchema.Constants.JP_DeliveryRequiredBy, Format = ZDateTimePickerFormat.Long },
				new { Name = JobDocsAndCartageSchema.Constants.JP_DeliveryCartageAdvised, Format = ZDateTimePickerFormat.Long },
				new { Name = JobDocsAndCartageSchema.Constants.JP_DeliveryCartageCompleted, Format = ZDateTimePickerFormat.Long },
			};

			CombineAssertions(delegate
			{
				foreach (var value in values)
				{
					PropertyInfo info = typeof(JobDocsAndCartage).GetProperty(value.Name);
					ActionFieldAttribute att = info == null ? null : ActionFieldAttribute.Get(info);

					AssertEquals(value.Name, value.Format, att == null ? null : att.DateTimeFormat);
				}
			});
		}

		public void TestJP_EstimatedPickup()
		{
			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment1.Consols.Add(consol1);
			shipment2.Consols.Add(consol2);
			CommonContainer container1a = consol1.Containers.AddNew();
			CommonContainer container1b = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();

			PackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 2;
			packLine1a.JL_ActualWeight = 20.2m;
			packLine1a.JL_ActualVolume = 2.2m;
			packLine1a.JL_JS = shipment1.PK;
			container1a.PackLines.Add(packLine1a);

			PackLine packLine1b = shipment1.OuterPackLines.AddNew();
			packLine1b.JL_PackageCount = 2;
			packLine1b.JL_ActualWeight = 20.2m;
			packLine1b.JL_ActualVolume = 2.2m;
			packLine1b.JL_JS = shipment1.PK;
			container1b.PackLines.Add(packLine1b);

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals(ZDateTime.Empty, container1a.JC_DepartureEstimatedPickup);
			AssertEquals(ZDateTime.Empty, container1b.JC_DepartureEstimatedPickup);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureEstimatedPickup);
			AssertEquals(ZDateTime.Empty, shipment1.DocsAndCartage.JP_EstimatedPickup);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_EstimatedPickup);

			ZDateTime currentDateTime = ZDateTime.Now;

			shipment1.DocsAndCartage.JP_EstimatedPickup = currentDateTime;

			AssertEquals(currentDateTime, container1a.JC_DepartureEstimatedPickup);
			AssertEquals(currentDateTime, container1b.JC_DepartureEstimatedPickup);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureEstimatedPickup);
			AssertEquals(currentDateTime, shipment1.DocsAndCartage.JP_EstimatedPickup);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_EstimatedPickup);
		}

		public void TestJP_EstimatedPickup_ShouldNotCreateConfirmIfMinutesEqual()
		{
			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_ActualWeight = 20.2m;
			packLine1.JL_ActualVolume = 2.2m;
			packLine1.JL_JS = shipment1.PK;
			shipment1.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2024, 6, 6, 2, 10, 3);
			AssertEquals(1, shipment1.PickupConfirms.Count);
			Factory.Save();

			var shipment2 = new BusinessObjectFactory().Load<CommonShipment>(shipment1.PK);
			AssertEquals("Precondition", new ZDateTime(2024, 6, 6, 2, 10, 0), shipment2.DocsAndCartage.JP_EstimatedPickup);

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			shipment2.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2024, 6, 6, 2, 10, 3);
			AssertEquals(1, shipment2.PickupConfirms.Count);
		}

		public void TestJP_EstimatedDelivery()
		{
			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment1.Consols.Add(consol1);
			shipment2.Consols.Add(consol2);
			CommonContainer container1a = consol1.Containers.AddNew();
			CommonContainer container1b = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();

			PackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 2;
			packLine1a.JL_ActualWeight = 20.2m;
			packLine1a.JL_ActualVolume = 2.2m;
			packLine1a.JL_JS = shipment1.PK;
			container1a.PackLines.Add(packLine1a);

			PackLine packLine1b = shipment1.OuterPackLines.AddNew();
			packLine1b.JL_PackageCount = 2;
			packLine1b.JL_ActualWeight = 20.2m;
			packLine1b.JL_ActualVolume = 2.2m;
			packLine1b.JL_JS = shipment1.PK;
			container1b.PackLines.Add(packLine1b);

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals(ZDateTime.Empty, container1a.JC_ArrivalEstimatedDelivery);
			AssertEquals(ZDateTime.Empty, container1b.JC_ArrivalEstimatedDelivery);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalEstimatedDelivery);
			AssertEquals(ZDateTime.Empty, shipment1.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_EstimatedDelivery);

			ZDateTime currentDateTime = ZDateTime.Now;

			shipment1.DocsAndCartage.JP_EstimatedDelivery = currentDateTime;

			AssertEquals(currentDateTime, container1a.JC_ArrivalEstimatedDelivery);
			AssertEquals(currentDateTime, container1b.JC_ArrivalEstimatedDelivery);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalEstimatedDelivery);
			AssertEquals(currentDateTime, shipment1.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestJP_EstimatedDelivery_ShouldNotCreateConfirmIfMinutesEqual()
		{
			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_ActualWeight = 20.2m;
			packLine1.JL_ActualVolume = 2.2m;
			packLine1.JL_JS = shipment1.PK;
			shipment1.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2024, 6, 6, 2, 10, 3);
			AssertEquals(1, shipment1.DeliveryConfirms.Count);
			Factory.Save();

			var shipment2 = new BusinessObjectFactory().Load<CommonShipment>(shipment1.PK);
			AssertEquals("Precondition", new ZDateTime(2024, 6, 6, 2, 10, 0), shipment2.DocsAndCartage.JP_EstimatedDelivery);

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			shipment2.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2024, 6, 6, 2, 10, 3);
			AssertEquals(1, shipment2.DeliveryConfirms.Count);
		}

		public void TestDefaultContainersPickupOrDeliveryDateFromParent()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			AssertDefaultContainersPickupOrDeliveryDateFromParent(JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment), container);

			CommonContainer declarationContainer = Factory.New<CommonContainer>();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			BusinessObject cusContainer = ((BusinessObjectCollection)declaration["CusContainers"]).AddNew();
			cusContainer[CusContainerSchema.Constants.CO_JC] = declarationContainer.PK;

			AssertDefaultContainersPickupOrDeliveryDateFromParent(JobDocsAndCartage.New((IDocsAndCartageParent)declaration), declarationContainer);
		}

		void AssertDefaultContainersPickupOrDeliveryDateFromParent(JobDocsAndCartage docsAndCartage, CommonContainer container)
		{
			docsAndCartage.JP_EstimatedDelivery = new ZDateTime(2012, 02, 01);
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalEstimatedDelivery);

			docsAndCartage.JP_EstimatedDelivery = ZDateTime.Invalid;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalEstimatedDelivery);

			docsAndCartage.JP_EstimatedDelivery = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalEstimatedDelivery);

			docsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2012, 02, 01);
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalCartageAdvised);

			docsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Invalid;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalCartageAdvised);

			docsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalCartageAdvised);

			docsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2012, 02, 01);
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalCartageComplete);

			docsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Invalid;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalCartageComplete);

			docsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_ArrivalCartageComplete);

			docsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2012, 02, 01);
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureCartageAdvised);

			docsAndCartage.JP_PickupCartageAdvised = ZDateTime.Invalid;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureCartageAdvised);

			docsAndCartage.JP_PickupCartageAdvised = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureCartageAdvised);

			docsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2012, 02, 01);
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureCartageComplete);

			docsAndCartage.JP_PickupCartageCompleted = ZDateTime.Invalid;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureCartageComplete);

			docsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureCartageComplete);

			docsAndCartage.JP_EstimatedPickup = new ZDateTime(2012, 02, 01);
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureEstimatedPickup);

			docsAndCartage.JP_EstimatedPickup = ZDateTime.Invalid;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureEstimatedPickup);

			docsAndCartage.JP_EstimatedPickup = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2012, 02, 01), container.JC_DepartureEstimatedPickup);
		}

		public void TestJP_PickupCartageAdvised()
		{
			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment1.Consols.Add(consol1);
			shipment2.Consols.Add(consol2);
			CommonContainer container1a = consol1.Containers.AddNew();
			CommonContainer container1b = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();

			PackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 2;
			packLine1a.JL_ActualWeight = 20.2m;
			packLine1a.JL_ActualVolume = 2.2m;
			packLine1a.JL_JS = shipment1.PK;
			container1a.PackLines.Add(packLine1a);

			PackLine packLine1b = shipment1.OuterPackLines.AddNew();
			packLine1b.JL_PackageCount = 2;
			packLine1b.JL_ActualWeight = 20.2m;
			packLine1b.JL_ActualVolume = 2.2m;
			packLine1b.JL_JS = shipment1.PK;
			container1b.PackLines.Add(packLine1b);

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals(ZDateTime.Empty, container1a.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, container1b.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment1.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_PickupCartageAdvised);

			ZDateTime currentDateTime = ZDateTime.Now;

			shipment1.DocsAndCartage.JP_PickupCartageAdvised = currentDateTime;

			AssertEquals(currentDateTime, container1a.JC_DepartureCartageAdvised);
			AssertEquals(currentDateTime, container1b.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureCartageAdvised);
			AssertEquals(currentDateTime, shipment1.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestJP_DeliveryCartageAdvised()
		{
			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment1.Consols.Add(consol1);
			shipment2.Consols.Add(consol2);
			CommonContainer container1a = consol1.Containers.AddNew();
			CommonContainer container1b = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();

			PackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 2;
			packLine1a.JL_ActualWeight = 20.2m;
			packLine1a.JL_ActualVolume = 2.2m;
			packLine1a.JL_JS = shipment1.PK;
			container1a.PackLines.Add(packLine1a);

			PackLine packLine1b = shipment1.OuterPackLines.AddNew();
			packLine1b.JL_PackageCount = 2;
			packLine1b.JL_ActualWeight = 20.2m;
			packLine1b.JL_ActualVolume = 2.2m;
			packLine1b.JL_JS = shipment1.PK;
			container1b.PackLines.Add(packLine1b);

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals(ZDateTime.Empty, container1a.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, container1b.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment1.DocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_DeliveryCartageAdvised);

			ZDateTime currentDateTime = ZDateTime.Now;

			shipment1.DocsAndCartage.JP_DeliveryCartageAdvised = currentDateTime;

			AssertEquals(currentDateTime, container1a.JC_ArrivalCartageAdvised);
			AssertEquals(currentDateTime, container1b.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalCartageAdvised);
			AssertEquals(currentDateTime, shipment1.DocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestTemporarilySetTransportBookingEventReference()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			AssertEquals("Preconditon: No BKQ Log exists yet", false, shipment.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.BookingRequestedCode));

			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Today;

			var expectedLogs = shipment.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == Events.BookingRequestedCode
					&& l.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out var type)
					&& type == Core.Constants.EventReferenceParameterTypes.DeliveryTransport);

			AssertEquals("Only 1 log should have been found", 1, expectedLogs.Count());
			AssertEquals("Should not contain a booking reference", "|TYP=Delivery Transport", expectedLogs.FirstOrDefault().SL_Reference);

			using (shipment.DocsAndCartage.TemporarilySetTransportBookingEventReference("TB00001001"))
			{
				shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Today;
			}

			expectedLogs = shipment.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == Events.BookingRequestedCode
					&& l.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out var type)
					&& type == Core.Constants.EventReferenceParameterTypes.PickupTransport);

			AssertEquals("Only 1 log should have been found", 1, expectedLogs.Count());
			AssertEquals("Should contain a booking reference", "TB00001001|TYP=Pickup Transport", expectedLogs.FirstOrDefault().SL_Reference);
		}

		public void TestJP_PickupCartageCompleted()
		{
			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment1.Consols.Add(consol1);
			shipment2.Consols.Add(consol2);
			CommonContainer container1a = consol1.Containers.AddNew();
			CommonContainer container1b = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();

			PackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 2;
			packLine1a.JL_ActualWeight = 20.2m;
			packLine1a.JL_ActualVolume = 2.2m;
			packLine1a.JL_JS = shipment1.PK;
			container1a.PackLines.Add(packLine1a);

			PackLine packLine1b = shipment1.OuterPackLines.AddNew();
			packLine1b.JL_PackageCount = 2;
			packLine1b.JL_ActualWeight = 20.2m;
			packLine1b.JL_ActualVolume = 2.2m;
			packLine1b.JL_JS = shipment1.PK;
			container1b.PackLines.Add(packLine1b);

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals(ZDateTime.Empty, container1a.JC_DepartureCartageComplete);
			AssertEquals(ZDateTime.Empty, container1b.JC_DepartureCartageComplete);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureCartageComplete);
			AssertEquals(ZDateTime.Empty, shipment1.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_PickupCartageCompleted);

			ZDateTime currentDateTime = ZDateTime.Now;

			shipment1.DocsAndCartage.JP_PickupCartageCompleted = currentDateTime;

			AssertEquals(currentDateTime, container1a.JC_DepartureCartageComplete);
			AssertEquals(currentDateTime, container1b.JC_DepartureCartageComplete);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureCartageComplete);
			AssertEquals(currentDateTime, shipment1.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_PickupCartageCompleted);
		}

		public void TestJP_DeliveryCartageCompleted()
		{
			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment1.Consols.Add(consol1);
			shipment2.Consols.Add(consol2);
			CommonContainer container1a = consol1.Containers.AddNew();
			CommonContainer container1b = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();

			PackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 2;
			packLine1a.JL_ActualWeight = 20.2m;
			packLine1a.JL_ActualVolume = 2.2m;
			packLine1a.JL_JS = shipment1.PK;
			container1a.PackLines.Add(packLine1a);

			PackLine packLine1b = shipment1.OuterPackLines.AddNew();
			packLine1b.JL_PackageCount = 2;
			packLine1b.JL_ActualWeight = 20.2m;
			packLine1b.JL_ActualVolume = 2.2m;
			packLine1b.JL_JS = shipment1.PK;
			container1b.PackLines.Add(packLine1b);

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment2.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals(ZDateTime.Empty, container1a.JC_ArrivalCartageComplete);
			AssertEquals(ZDateTime.Empty, container1b.JC_ArrivalCartageComplete);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalCartageComplete);
			AssertEquals(ZDateTime.Empty, shipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_DeliveryCartageCompleted);

			ZDateTime currentDateTime = ZDateTime.Now;

			shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = currentDateTime;

			AssertEquals(currentDateTime, container1a.JC_ArrivalCartageComplete);
			AssertEquals(currentDateTime, container1b.JC_ArrivalCartageComplete);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalCartageComplete);
			AssertEquals(currentDateTime, shipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment2.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		public void TestContainerDefaultsForPickup()
		{
			CheckContainerDefaultsForShipmentAndDepartureConsoleContainerModes(ContainerModes.LCL, ContainerModes.FCL, false);
			CheckContainerDefaultsForShipmentAndDepartureConsoleContainerModes(ContainerModes.LCL, ContainerModes.LCL, true);
			CheckContainerDefaultsForShipmentAndDepartureConsoleContainerModes(ContainerModes.FCL, ContainerModes.FCL, true);
			CheckContainerDefaultsForShipmentAndDepartureConsoleContainerModes(ContainerModes.FCL, ContainerModes.FCL, true);
		}

		public void TestContainerDefaultsForDelivery()
		{
			CheckContainerDefaultsForShipmentAndArrivalConsoleContainerModes(ContainerModes.LCL, ContainerModes.FCL, false);
			CheckContainerDefaultsForShipmentAndArrivalConsoleContainerModes(ContainerModes.LCL, ContainerModes.LCL, true);
			CheckContainerDefaultsForShipmentAndArrivalConsoleContainerModes(ContainerModes.FCL, ContainerModes.FCL, true);
			CheckContainerDefaultsForShipmentAndArrivalConsoleContainerModes(ContainerModes.FCL, ContainerModes.FCL, true);
		}

		void CheckContainerDefaultsForShipmentAndDepartureConsoleContainerModes(string packingMode, string consolMode, bool shouldDefault)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			shipment.Consols.Add(consol);
			var container = consol.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 20.2m;
			packLine.JL_ActualVolume = 2.2m;
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			shipment.JS_PackingMode = packingMode;
			shipment.ArrivalConsol.JK_ConsolMode = consolMode;
			var currentDateTime = ZDateTime.Now;

			shipment.DocsAndCartage.JP_EstimatedPickup = currentDateTime;
			shipment.DocsAndCartage.JP_PickupCartageAdvised = currentDateTime;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = currentDateTime;

			AssertEquals(currentDateTime, shipment.DocsAndCartage.JP_EstimatedPickup);
			AssertEquals(currentDateTime, shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(currentDateTime, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			if (shouldDefault)
			{
				AssertEquals(currentDateTime, container.JC_DepartureEstimatedPickup);
				AssertEquals(currentDateTime, container.JC_DepartureCartageAdvised);
				AssertEquals(currentDateTime, container.JC_DepartureCartageComplete);
			}
			else
			{
				AssertEquals(ZDateTime.Empty, container.JC_DepartureEstimatedPickup);
				AssertEquals(ZDateTime.Empty, container.JC_DepartureCartageAdvised);
				AssertEquals(ZDateTime.Empty, container.JC_DepartureCartageComplete);
			}
		}

		void CheckContainerDefaultsForShipmentAndArrivalConsoleContainerModes(string packingMode, string consolMode, bool shouldDefault)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			shipment.Consols.Add(consol);
			var container = consol.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 20.2m;
			packLine.JL_ActualVolume = 2.2m;
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			shipment.JS_PackingMode = packingMode;
			shipment.ArrivalConsol.JK_ConsolMode = consolMode;
			var currentDateTime = ZDateTime.Now;

			shipment.DocsAndCartage.JP_EstimatedDelivery = currentDateTime;
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = currentDateTime;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = currentDateTime;

			AssertEquals(currentDateTime, shipment.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals(currentDateTime, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals(currentDateTime, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			if (shouldDefault)
			{
				AssertEquals(currentDateTime, container.JC_ArrivalEstimatedDelivery);
				AssertEquals(currentDateTime, container.JC_ArrivalCartageAdvised);
				AssertEquals(currentDateTime, container.JC_ArrivalCartageComplete);
			}
			else
			{
				AssertEquals(ZDateTime.Empty, container.JC_ArrivalEstimatedDelivery);
				AssertEquals(ZDateTime.Empty, container.JC_ArrivalCartageAdvised);
				AssertEquals(ZDateTime.Empty, container.JC_ArrivalCartageComplete);
			}
		}

		public void TestDeliveryCartageCoPKSetter()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsLocalTransport = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var shipment = Factory.New<CommonShipment>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = carrier.PK;

			AssertEquals("Setting JP_OA_DeliveryCartageCoAddr OrgPK", carrier.PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr_ZAddress.OrgPK);
			AssertEquals("Setting JP_OA_DeliveryCartageCoAddr PK", carrier.MainAddress.PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestPickupCartageCoPKSetter()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsLocalTransport = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var shipment = Factory.New<CommonShipment>();
			shipment.DocsAndCartage.PickupCartageCoPK = carrier.PK;

			AssertEquals("Setting JP_OA_DeliveryCartageCoAddr OrgPK", carrier.PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK);
			AssertEquals("Setting JP_OA_DeliveryCartageCoAddr PK", carrier.MainAddress.PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);
		}

		#region Confirmations

		public void TestJP_EstimatedPickup_Confirms()
		{
			TestConfirmDateBasedOnRegistry(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned);
		}

		public void TestJP_PickupRequiredBy_Confirms()
		{
			TestConfirmDateBasedOnRegistry(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy);
		}

		public void TestJP_PickupCartageCompleted_Confirms()
		{
			TestConfirmDate(ConfirmTimesSyncHelper.ConfirmType.Pickup);
		}

		public void TestJP_EstimatedDelivery_Confirms()
		{
			TestConfirmDateBasedOnRegistry(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned);
		}

		public void TestJP_DeliveryRequiredBy_Confirms()
		{
			TestConfirmDateBasedOnRegistry(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy);
		}

		public void TestJP_DeliveryCartageCompleted_Confirms()
		{
			TestConfirmDate(ConfirmTimesSyncHelper.ConfirmType.Delivery);
		}

		void TestConfirmDateBasedOnRegistry(ConfirmTimesSyncHelper.ConfirmType confirmType, ConfirmTimesSyncHelper.ConfirmDateType confirmDateType)
		{
			var cartageDateFieldName = ConfirmTimesSyncHelper.GetCartageDateFieldName(confirmType, confirmDateType);
			var confirmDateFieldName = ConfirmTimesSyncHelper.GetConfirmDateFieldName(confirmDateType);

			var today = ZDateTime.Today;
			FreightConfigurationRegistry.Instance.AutoCreateLooseConfirmations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment1 = Factory.New<CommonShipment>();
			var confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? shipment1.PickupConfirms : shipment1.DeliveryConfirms;
			AssertEquals(0, confirms.Count);

			var packline = shipment1.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_ActualWeight = 100;
			packline.JL_ActualVolume = 200;
			AssertEquals(0, confirms.Count);

			shipment1.DocsAndCartage[cartageDateFieldName] = today;
			AssertEquals(1, confirms.Count);
			AssertEquals(today, confirms[0][confirmDateFieldName]);

			var shipment2 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? shipment2.PickupConfirms : shipment2.DeliveryConfirms;
			AssertEquals(0, confirms.Count);

			packline = shipment2.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_ActualWeight = 100;
			packline.JL_ActualVolume = 200;
			AssertEquals(0, confirms.Count);

			shipment2.DocsAndCartage[cartageDateFieldName] = today;
			AssertEquals(0, confirms.Count);

			FreightConfigurationRegistry.Instance.AutoCreateLooseConfirmations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipment2.DocsAndCartage[cartageDateFieldName] = today.AddDays(1);
			AssertEquals(today.AddDays(1), confirms[0][confirmDateFieldName]);
		}

		void TestConfirmDate(ConfirmTimesSyncHelper.ConfirmType confirmType)
		{
			var cartageDateFieldName = ConfirmTimesSyncHelper.GetCartageDateFieldName(confirmType, ConfirmTimesSyncHelper.ConfirmDateType.Actual);
			var confirmDateFieldName = ConfirmTimesSyncHelper.GetConfirmDateFieldName(ConfirmTimesSyncHelper.ConfirmDateType.Actual);

			var today = ZDateTime.Today;

			var shipment1 = Factory.New<CommonShipment>();
			var confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? shipment1.PickupConfirms : shipment1.DeliveryConfirms;
			AssertEquals(0, confirms.Count);

			var packline = shipment1.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_ActualWeight = 100;
			packline.JL_ActualVolume = 200;
			AssertEquals(0, confirms.Count);

			shipment1.DocsAndCartage[cartageDateFieldName] = today;
			AssertEquals(1, confirms.Count);
			AssertEquals(today, confirms[0][confirmDateFieldName]);

			var shipment2 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? shipment2.PickupConfirms : shipment2.DeliveryConfirms;
			AssertEquals(0, confirms.Count);

			packline = shipment2.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_ActualWeight = 100;
			packline.JL_ActualVolume = 200;
			AssertEquals(0, confirms.Count);

			shipment2.DocsAndCartage[cartageDateFieldName] = today.AddDays(1);
			AssertEquals(today.AddDays(1), confirms[0][confirmDateFieldName]);
		}

		#endregion

		public void TestDeletionStackTrace()
		{
			var cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			AssertNull(cartage.DeleteCallStack);

			cartage.Delete();
			AssertNotNull(cartage.DeleteCallStack);
		}

		public void TestJobDocsAndCartageWhenSwitchedFromAParent()
		{
			BusinessObject dec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			JobDocsAndCartage decCartage = JobDocsAndCartage.New((IDocsAndCartageParent)dec);
			Factory.Save();
			dec[JobDeclarationSchema.Constants.JE_JS] = Factory.New<CommonShipment>().PK;
			Assert("Should now be deleted", decCartage.IsDeleted);
		}

		public void TestOrderItemsSortOrder()
		{
			JobDocsAndCartage cartage = Shipment.DocsAndCartage;
			OrderItem item1 = cartage.OrderItems.AddNew();
			OrderItem item2 = cartage.OrderItems.AddNew();
			OrderItem item3 = cartage.OrderItems.AddNew();
			OrderItem item4 = cartage.OrderItems.AddNew();

			item1.JT_OrderReference = "1";
			item2.JT_OrderReference = "2";
			item3.JT_OrderReference = "3";
			item4.JT_OrderReference = "4";

			Factory.Save();

			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			CommonShipment retrievedShipment = retrievingFactory.Load<CommonShipment>(Shipment.PK);
			OrderItemCollection retrievedOrderItems = retrievedShipment.DocsAndCartage.OrderItems;
			AssertEquals("1", retrievedOrderItems[0].JT_OrderReference);
			AssertEquals("2", retrievedOrderItems[1].JT_OrderReference);
			AssertEquals("3", retrievedOrderItems[2].JT_OrderReference);
			AssertEquals("4", retrievedOrderItems[3].JT_OrderReference);
		}

		#region Parent Tests

		public void TestParentIsJobClearedForwardingShipment_DoesNotResetStatusOnEDoc()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			Factory.Save();

			var docsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.JobCleared, shipment.JS_ScreeningStatus);

			docsAndCartage.JP_EstimatedPickup = DateTime.UtcNow;
			docsAndCartage.JP_PickupCartageCompleted = DateTime.UtcNow;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.JobCleared, shipment.JS_ScreeningStatus);

			docsAndCartage.JP_EstimatedDelivery = DateTime.UtcNow;
			docsAndCartage.JP_DeliveryCartageCompleted = DateTime.UtcNow;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.JobCleared, shipment.JS_ScreeningStatus);
		}

		public void TestParentLoadsCorrectObject_WithJustShipment()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(Shipment);
			AssertEquals("Shipment's DocsAndCartage is wrong one.", Shipment.DocsAndCartage.PK, cartage.PK);
		}

		public void TestParentLoadsCorrectObject_WithJustDeclaration()
		{
			IDocsAndCartageParent declaration = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			JobDocsAndCartage cartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent((IShipmentWithDocsAndCartage)declaration);
			AssertEquals("Declaration's DocsAndCartage is wrong one.", declaration.RequiredDocumentsProvider.PK, cartage.PK);
		}

		public void TestParentLoadsCorrectObject_WithDeclarationAndShipment()
		{
			Declaration[JobDeclarationSchema.JE_JS.Name] = Shipment.PK;

			JobDocsAndCartage cartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent((IShipmentWithDocsAndCartage)Declaration);
			AssertEquals("Declaration's DocsAndCartage is not the Shipment's.", ((IDocsAndCartageParent)Declaration).RequiredDocumentsProvider.PK, cartage.PK);
			AssertEquals("Shipment's DocsAndCartage is wrong one.", Shipment.DocsAndCartage.PK, cartage.PK);
		}

		#endregion

		public void TestJobDocAndCartageDoNotCreateDirty()
		{
			AssertEquals("Can not be dirty after creation", false, Shipment.DocsAndCartage.HasChanges);
		}

		#region TestShouldNotValidateOrdersHere

		public void TestShouldNotValidateOrdersHere()
		{
			OverseasConsignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;

			Shipment.JS_IsForwardRegistered = true;
			Shipment.ConsigneePK = OverseasConsignee.PK;

			AssertNotNull("Hit to Lazy Load", Shipment.DocsAndCartage);

			Shipment.RunPreSaveValidation();
			AssertNoNotifications("Should have no notifications", Shipment.DocsAndCartage.JP_OrderItemsAsStringInfo);
		}

		#endregion

		#region Business Object Overrides

		public void TestClone()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			cartage.JP_DeliveryCartageAdvised = ZDateTime.Now;
			cartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			cartage.JP_PickupCartageCompleted = ZDateTime.Now;

			JobDocsAndCartage clonedCartage = cartage.Clone(new MockJobDocsAndCartageParent(Factory));
			AssertEquals("Copied values correctly", cartage.JP_DeliveryCartageAdvised, clonedCartage.JP_DeliveryCartageAdvised);
			AssertEquals("Copied values cleared for actual dates", ZDateTime.Empty, clonedCartage.JP_DeliveryCartageCompleted);
			AssertEquals("Copied values cleared for actual dates", ZDateTime.Empty, clonedCartage.JP_PickupCartageCompleted);
		}

		public void TestCopyPersistentValuesFrom()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			cartage.JP_DeliveryCartageAdvised = ZDateTime.Now;
			cartage.JP_OrderItemsAsString = "splaty,bah";
			cartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			cartage.JP_PickupCartageCompleted = ZDateTime.Now;
			cartage.JP_ExportStatement = "LOW";

			JobDocsAndCartage clonedCartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			clonedCartage.CopyPersistentValuesFrom(cartage);
			AssertEquals("Copied values correctly", cartage.JP_DeliveryCartageAdvised, clonedCartage.JP_DeliveryCartageAdvised);
			AssertEquals("Copied values correctly", cartage.JP_OrderItemsAsString, clonedCartage.JP_OrderItemsAsString);
			AssertEquals("Copied values correctly", "LOW", clonedCartage.JP_ExportStatement);
			AssertEquals("Copied values cleared for actual dates", ZDateTime.Empty, clonedCartage.JP_DeliveryCartageCompleted);
			AssertEquals("Copied values cleared for actual dates", ZDateTime.Empty, clonedCartage.JP_PickupCartageCompleted);

			clonedCartage = JobDocsAndCartage.New((IDocsAndCartageParent)Declaration);
			clonedCartage.CopyPersistentValuesFrom(cartage);

			AssertEquals("Values are not copied for declaration", ZString.Empty, clonedCartage.JP_ExportStatement);
		}

		#endregion

		#region New Properties

		#region JP_StorageTimeUnits

		public void TestJP_StorageTimeUnits()
		{
			MockJobDocsAndCartageParent parent = new MockJobDocsAndCartageParent(Factory);
			JobDocsAndCartage cartage = JobDocsAndCartage.New(parent);

			parent.TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AIR should be in hours", "Hours", cartage.JP_StorageTimeUnits);
			parent.TransportMode = Core.Constants.TransportModes.Sea;

			parent.ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("LCL Sea should be in days", "Days", cartage.JP_StorageTimeUnits);

			parent.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("FCL Rail should be in days", "Days", cartage.JP_StorageTimeUnits);
		}

		#endregion

		#region TestJP_OrderItemsAsString

		public void TestJP_OrderItemsAsString()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));

			cartage.JP_OrderItemsAsString = "123, 456";
			AssertEquals("What goes in should come out", "123,456", cartage.JP_OrderItemsAsString);
			cartage.JP_OrderItemsAsString = ",123,,456,";
			AssertEquals("What goes in should come out", "123,456", cartage.JP_OrderItemsAsString);
			cartage.JP_OrderItemsAsString = "123 456";
			AssertEquals("What goes in should come out", "123,456", cartage.JP_OrderItemsAsString);
			cartage.JP_OrderItemsAsString = "   ";
			AssertEquals("What goes in should come out", "", cartage.JP_OrderItemsAsString);
			cartage.JP_OrderItemsAsString = ",,,,,";
			AssertEquals("What goes in should come out", "", cartage.JP_OrderItemsAsString);

			// expect no exception for max length of JT_OrderReference
			cartage.JP_OrderItemsAsString = "1234567890123456789012345678901234567890123456789012345678901234567890,,,,,";
			AssertEquals("1234567890123456789012345678901234567890123456789012345678901234567890,,,,,", cartage.JP_OrderItemsAsString);
		}

		public void TestJP_OrderItemsAsString_AddNew()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			cartage.OrderItems.AddNew().JT_OrderReference = "1";
			cartage.OrderItems.AddNew().JT_OrderReference = "2";
			AssertEquals("1,2", cartage.JP_OrderItemsAsString);

			cartage.OrderItems.AddNew().JT_OrderReference = "3";
			AssertEquals("1,2,3", cartage.JP_OrderItemsAsString);
		}

		public void TestChangingOrderItems_RefreshesBindingOnOrderItemsAsString()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			cartage.JP_OrderItemsAsString = "123, 456";
			bool jP_OrderItemsAsStringInfo_ValueChangedCalled = false;
			cartage.JP_OrderItemsAsStringInfo.ValueChanged += delegate
			{
				jP_OrderItemsAsStringInfo_ValueChangedCalled = true;
			};

			cartage.OrderItems.AddNew();
			AssertEquals("JP_OrderItemsAsStringInfo.ValueChanged called", true, jP_OrderItemsAsStringInfo_ValueChangedCalled);
			jP_OrderItemsAsStringInfo_ValueChangedCalled = false;
			cartage.OrderItems[0].JT_OrderReference = "xxx";
			AssertEquals("JP_OrderItemsAsStringInfo.ValueChanged called", true, jP_OrderItemsAsStringInfo_ValueChangedCalled);
		}

		#endregion

		#endregion

		#region Events

		public void TestEvents_EstimatedPickup()
		{
			var pickupAddress = Factory.New<OrgAddress>();
			pickupAddress.OA_Address1 = "An address in Denver with UTC -6 time zone";
			pickupAddress.OA_Code = "380 WORLD WAY";
			pickupAddress.OA_RL_NKRelatedPortCode = "USGOH";
			Shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;

			var eventTime = new ZDateTime(2024, 05, 24, 01, 19, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_EstimatedPickup = eventTime;
			AssertEquals("Correct event", true, Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_IsEstimate);
			AssertEquals("PCF event's time wrong", eventTime, Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTime);
			AssertEquals("PCF event's UTC time should based on 'Pickup From' address", eventTime.AddHours(6), Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTimeUtc);

			pickupAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKOrigin = "USLAX";
			eventTime = new ZDateTime(2024, 05, 24, 01, 30, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_EstimatedPickup = eventTime;
			AssertEquals("PCF event's UTC time should based on shipment origin when 'Pickup From' address is empty", eventTime.AddHours(7), Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTimeUtc);

			pickupAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKOrigin = "";
			eventTime = new ZDateTime(2024, 05, 24, 01, 40, 00);
			Shipment.DocsAndCartage.JP_EstimatedPickup = eventTime;
			AssertEquals("PCF event's UTC time should based on branch when 'Pickup From' address and shipment origin are both empty", eventTime.ToUniversalBranchTime(), Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTimeUtc);
		}

		public void TestEvents_EstimatedDelivery()
		{
			var deliveryAddress = Factory.New<OrgAddress>();
			deliveryAddress.OA_Address1 = "An address in Los Angeles with UTC -7 time zone";
			deliveryAddress.OA_Code = "1111 ABC Street";
			deliveryAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			var eventTime = new ZDateTime(2024, 05, 24, 01, 19, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_EstimatedDelivery = eventTime;
			AssertEquals("Correct event", true, Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_IsEstimate);
			AssertEquals("DCF event's time wrong", eventTime, Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTime);
			AssertEquals("DCF event's UTC time should based on 'Delivery To' address", eventTime.AddHours(7), Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTimeUtc);

			deliveryAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKDestination = "USGOH";
			eventTime = new ZDateTime(2024, 05, 24, 01, 30, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_EstimatedDelivery = eventTime;
			AssertEquals("DCF event's UTC time should based on shipment destination when 'Delivery To' address is empty", eventTime.AddHours(6), Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTimeUtc);

			deliveryAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKDestination = "";
			eventTime = new ZDateTime(2024, 05, 24, 01, 40, 00);
			Shipment.DocsAndCartage.JP_EstimatedDelivery = eventTime;
			AssertEquals("DCF event's UTC time should based on branch when 'Delivery To' address is empty", eventTime.ToUniversalBranchTime(), Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTimeUtc);
		}

		public void TestEvents_PickupCartageAdvised()
		{
			Shipment.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2000, 1, 1);
			AssertEquals("Correct event", false, Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageAdvised).SL_IsEstimate);
		}

		public void TestEvents_DeliveryCartageAdvised()
		{
			Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2000, 1, 1);
			AssertEquals("Correct event", false, Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageAdvised).SL_IsEstimate);
		}

		public void TestEvents_PickupCartageComplete()
		{
			var pickupAddress = Factory.New<OrgAddress>();
			pickupAddress.OA_Address1 = "An address in Denver with UTC -6 time zone";
			pickupAddress.OA_Code = "380 WORLD WAY";
			pickupAddress.OA_RL_NKRelatedPortCode = "USGOH";
			Shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;

			var eventTime = new ZDateTime(2024, 05, 24, 01, 19, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = eventTime;
			AssertEquals("Correct event", false, Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_IsEstimate);
			AssertEquals("PCF event's time wrong", eventTime, Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTime);
			AssertEquals("PCF event's UTC time should based on 'Pickup From' address", eventTime.AddHours(6), Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTimeUtc);

			pickupAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKOrigin = "USLAX";
			eventTime = new ZDateTime(2024, 05, 24, 01, 30, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = eventTime;
			AssertEquals("PCF event's UTC time should based on shipment origin when 'Pickup From' address is empty", eventTime.AddHours(7), Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTimeUtc);

			pickupAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKOrigin = "";
			eventTime = new ZDateTime(2024, 05, 24, 01, 40, 00);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = eventTime;
			AssertEquals("PCF event's UTC time should based on branch when 'Pickup From' address and shipment origin are both empty", eventTime.ToUniversalBranchTime(), Shipment.Logs.MostRecentLogByEventTime(Events.PickupCartageCompleteFinalised).SL_EventTimeUtc);
		}

		public void TestEvents_DeliveryCartageComplete()
		{
			var deliveryAddress = Factory.New<OrgAddress>();
			deliveryAddress.OA_Address1 = "An address in Los Angeles with UTC -7 time zone";
			deliveryAddress.OA_Code = "1111 ABC Street";
			deliveryAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			var eventTime = new ZDateTime(2024, 05, 24, 01, 19, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = eventTime;
			AssertEquals("Correct event", false, Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_IsEstimate);
			AssertEquals("DCF event's time wrong", eventTime, Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTime);
			AssertEquals("DCF event's UTC time should based on 'Delivery To' address", eventTime.AddHours(7), Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTimeUtc);

			deliveryAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKDestination = "USGOH";
			eventTime = new ZDateTime(2024, 05, 24, 01, 30, 00, DateTimeKind.Utc);
			Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = eventTime;
			AssertEquals("DCF event's UTC time should based on shipment destination when 'Delivery To' address is empty", eventTime.AddHours(6), Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTimeUtc);

			deliveryAddress.OA_RL_NKRelatedPortCode = "";
			Shipment.JS_RL_NKDestination = "";
			eventTime = new ZDateTime(2024, 05, 24, 01, 40, 00);
			Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = eventTime;
			AssertEquals("DCF event's UTC time should based on branch when 'Delivery To' address is empty", eventTime.ToUniversalBranchTime(), Shipment.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_EventTimeUtc);
		}

		#endregion

		#region Testing Services

		#region TestServicesCannotBeAddedOrCancelledByUser

		public void TestServicesCannotBeAddedOrCancelledByUser()
		{
			Event[] eventsToExclude =
			{
				AutoEvents.ServiceRequested,
				AutoEvents.ServiceCompleted,
				AutoEvents.QuarantineRequired,
				AutoEvents.QuarantineComplete,
				AutoEvents.CustomsImpedimentReceived,
				AutoEvents.CustomsCleared,
				AutoEvents.ExportCustomsCleared
			};

			foreach (Event item in eventsToExclude)
			{
				AssertEquals(item.Code, false, Shipment.Logs.EventsThatCannotBeAdded.Contains(item));
				AssertEquals(item.Code, false, Shipment.Logs.EventsThatCannotBeCancelled.Contains(item));
			}
		}

		#endregion

		#endregion

		[ExpectNoExceptions]
		public void TestSavingCartageWithInvalidAddress()
		{
			var jdc = JobDocsAndCartage.New(Shipment);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODE";
			org.OH_IsActive = false;
			org.Addresses.RemoveAndDeleteAll();
			jdc.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK;
			Factory.Save();
		}

		public void TestJP_StorageTimeUnitsInfoNoException()
		{
			JobDocsAndCartage docsAndCartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			AssertNotNull("JP_StorageTimeUnitsInfo should not blow up J D&C", docsAndCartage.JP_StorageTimeUnitsInfo);
		}

		public void TestIncidentI00028325_DeletingJobDocsAndCartageShouldDeleteJobDocuments()
		{
			JobDocsAndCartage docsAndCartage = Shipment.DocsAndCartage;
			JobRequiredDocument miscDoc = docsAndCartage.RequiredDocuments.AddNew();
			miscDoc.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Shipment.Delete();
			Assert("DocsAndCartage is deleted", docsAndCartage.IsDeleted);
			Assert("Job Document is deleted", miscDoc.IsDeleted);
		}

		public void TestFCLAvailability()
		{
			var transportParent = (ITransportParent)Declaration;
			var leg0 = transportParent.Transports.AddNew();

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_IsActive, true));

			leg0.JW_Vessel = vessel.RV_FK;
			leg0.JW_VoyageFlight = "38";
			leg0.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg0.JW_RL_NKLoadPort = "USCHI";
			leg0.JW_RL_NKDiscPort = "AUSYD";
			leg0.JW_ETD = new ZDateTime(2011, 12, 20);
			leg0.JW_ETA = new ZDateTime(2012, 1, 2);
			leg0.JW_TerminalAvailabilityDate = new ZDateTime(2012, 1, 3);
			leg0.JW_LegOrder = 0;

			var leg1 = transportParent.Transports.AddNew();
			leg1.JW_Vessel = vessel.RV_FK;
			leg1.JW_VoyageFlight = "38";
			leg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "AUADL";
			leg1.JW_ETD = new ZDateTime(2012, 1, 2);
			leg1.JW_ETA = new ZDateTime(2012, 1, 12);
			leg1.JW_TerminalAvailabilityDate = new ZDateTime(2012, 1, 13);
			leg1.JW_LegOrder = 1;

			var leg2 = transportParent.Transports.AddNew();
			leg2.JW_Vessel = vessel.RV_FK;
			leg2.JW_VoyageFlight = "38";
			leg2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg2.JW_RL_NKLoadPort = "AUSYD";
			leg2.JW_RL_NKDiscPort = "AUMEL";
			leg2.JW_ETD = new ZDateTime(2012, 1, 12);
			leg2.JW_ETA = new ZDateTime(2012, 1, 14);
			leg2.JW_TerminalAvailabilityDate = new ZDateTime(2012, 1, 15);
			leg2.JW_LegOrder = 2;

			leg1.JW_IsLinked = true;
			leg2.JW_IsLinked = true;
			Assert("PreCondition", leg1.JW_IsLinked);
			Assert("PreCondition", leg2.JW_IsLinked);

			Factory.Save();

			var docAndCartage = JobDocsAndCartage.New((IDocsAndCartageParent)Declaration);

			AssertEquals(new ZDateTime(2012, 1, 15), docAndCartage.JP_FCLAvailable);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenSetJP_FCLDeliveryAndJP_LCLAirStorageRelatedProperties()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.Consols.Add(consol);

			var container = consol.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			var carrierMDDPenalty = container.DeliveryPenalties.AddNew();
			carrierMDDPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			carrierMDDPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;

			shipment.DocsAndCartage.JP_FCLDeliveryDetentionCharge = 1;
			shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 1;
			shipment.DocsAndCartage.JP_FCLDeliveryDetentionFreeDays = 1;
			shipment.DocsAndCartage.JP_LCLAirStorageCharge = 1;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 1;
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenSetJP_FCLPickupRelatedProperties()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.Consols.Add(consol);

			var container = consol.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			var carrierMDDPenalty = container.PickupPenalties.AddNew();
			carrierMDDPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			carrierMDDPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;

			shipment.DocsAndCartage.JP_FCLPickupDetentionCharge = 1;
			shipment.DocsAndCartage.JP_FCLPickupDetentionDays = 1;
			shipment.DocsAndCartage.JP_FCLPickupDetentionFreeDays = 1;
		}

		#region Available / Storage

		#region From Sailing

		public void TestJP_FCLAvailable_ForShipment_ComesFromSailing()
		{
			TestAvailableOrStorage_ForShipment_ComesFromSailing(JobConsolTransportSchema.JW_TerminalAvailabilityDate, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestJP_FCLStorageCommences_ForShipment_ComesFromSailing()
		{
			TestAvailableOrStorage_ForShipment_ComesFromSailing(JobConsolTransportSchema.JW_TerminalStorageDate, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestJP_LCLAvailable_ForShipment_ComesFromSailing()
		{
			TestAvailableOrStorage_ForShipment_ComesFromSailing(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestJP_LCLStorageCommences_ForShipment_ComesFromSailing()
		{
			TestAvailableOrStorage_ForShipment_ComesFromSailing(JobConsolTransportSchema.JW_DepotStorageDate, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestAvailableOrStorage_ForShipment_ComesFromSailing(SchemaDateTimeColumn transportDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[0];
			CommonShipment shipment = Consol.Shipments.AddNew();
			JobDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
			Transport arrivalTransport = Consol.Transports.MostInterestingTransport;
			arrivalTransport.JW_JX = arrivalSailing.PK;

			ZDateTime newValue = new ZDateTime(2010, 1, 2);

			if (transportDateProperty.TableName == JobConsolTransportSchema.Constants.TableName)
			{
				arrivalTransport[transportDateProperty] = newValue;
			}
			else
			{
				throw new InvalidOperationException("unknown table: " + transportDateProperty.TableName);
			}

			AssertEquals(docsAndCartageDateProperty.Name + " comes from the sailing schedule", newValue, docsAndCartage[docsAndCartageDateProperty]);
		}

		public void TestJP_FCLAvailable_ForStandAloneDeclaration_ComesFromSailing()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromSailing(JobConsolTransportSchema.JW_TerminalAvailabilityDate, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestJP_FCLStorageCommences_ForStandAloneDeclaration_ComesFromSailing()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromSailing(JobConsolTransportSchema.JW_TerminalStorageDate, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestJP_LCLAvailable_ForStandAloneDeclaration_ComesFromSailing()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromSailing(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestJP_LCLStorageCommences_ForStandAloneDeclaration_ComesFromSailing()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromSailing(JobConsolTransportSchema.JW_DepotStorageDate, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromSailing(SchemaDateTimeColumn transportDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[0];
			JobDocsAndCartage docsAndCartage = (JobDocsAndCartage)Declaration["DocsAndCartage"];
			Declaration[JobDeclarationSchema.JE_TransportMode] = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration[JobDeclarationSchema.JE_VesselName] = SeaVoyage.JV_RV_NKVessel;
			Declaration[JobDeclarationSchema.JE_VoyageFlightNo] = SeaVoyage.JV_VoyageFlight;
			Declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = arrivalSailing.Destination.JB_RL_NKPortOfDischarge;
			Declaration[JobDeclarationSchema.JE_OH_ShippingLine] = SeaVoyage.JV_OH_Line;

			var transportParent = (ITransportParent)Declaration;
			var transport = transportParent.Transports[0];
			transport.JW_RL_NKLoadPort = arrivalSailing.Origin.JA_RL_NKPortOfLoading;
			transport.JW_IsLinked = true;

			ZDateTime newValue = new ZDateTime(2010, 1, 2);

			if (transportDateProperty.TableName == JobConsolTransportSchema.Constants.TableName)
			{
				transport[transportDateProperty] = newValue;
			}
			else
			{
				throw new InvalidOperationException("unknown table: " + transportDateProperty.TableName);
			}

			AssertEquals(docsAndCartageDateProperty.Name + " comes from the sailing schedule", newValue, docsAndCartage[docsAndCartageDateProperty]);
		}

		#endregion

		#region From Container

		public void TestJP_FCLAvailable_ForShipment_ComesFromContainer()
		{
			TestAvailableOrStorage_ForShipment_ComesFromContainer(JobConsolTransportSchema.JW_TerminalAvailabilityDate, JobContainerSchema.JC_FCLAvailable, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestJP_FCLStorageCommences_ForShipment_ComesFromContainer()
		{
			TestAvailableOrStorage_ForShipment_ComesFromContainer(JobConsolTransportSchema.JW_TerminalStorageDate, JobContainerSchema.JC_ArrivalCTOStorageStartDate, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestJP_LCLAvailable_ForShipment_ComesFromContainer()
		{
			TestAvailableOrStorage_ForShipment_ComesFromContainer(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobContainerSchema.JC_LCLAvailable, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestJP_LCLStorageCommences_ForShipment_ComesFromContainer()
		{
			TestAvailableOrStorage_ForShipment_ComesFromContainer(JobConsolTransportSchema.JW_DepotStorageDate, JobContainerSchema.JC_LCLStorageCommences, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestAvailableOrStorage_ForShipment_ComesFromContainer(SchemaDateTimeColumn sailingDateProperty, SchemaDateTimeColumn containerDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			TestAvailableOrStorage_ForShipment_ComesFromContainer(sailingDateProperty, containerDateProperty, docsAndCartageDateProperty, Core.Constants.ContainerModes.FCL);
			TestAvailableOrStorage_ForShipment_ComesFromContainer(sailingDateProperty, containerDateProperty, docsAndCartageDateProperty, Core.Constants.ContainerModes.LCL);
		}

		void TestAvailableOrStorage_ForShipment_ComesFromContainer(SchemaDateTimeColumn sailingDateProperty, SchemaDateTimeColumn containerDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty, ZString containerMode)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[0];
			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_PackingMode = containerMode;
			JobDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
			Transport arrivalTransport = Consol.Transports.MostInterestingTransport;
			arrivalTransport.JW_JX = arrivalSailing.PK;

			CommonContainer container1 = Consol.Containers.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;

			CommonContainer decoyContainer = Consol.Containers.AddNew();
			decoyContainer[containerDateProperty] = new ZDateTime(1999, 1, 1);
			TestAvailableOrStorage_ComesFromContainer(docsAndCartage, arrivalTransport, container1, container2, sailingDateProperty, containerDateProperty, docsAndCartageDateProperty);
			consol = null;
		}

		public void TestJP_FCLAvailable_ForStandAloneDeclaration_ComesFromContainer()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromContainer(JobConsolTransportSchema.JW_TerminalAvailabilityDate, JobContainerSchema.JC_FCLAvailable, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestJP_FCLStorageCommences_ForStandAloneDeclaration_ComesFromContainer()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromContainer(JobConsolTransportSchema.JW_TerminalStorageDate, JobContainerSchema.JC_ArrivalCTOStorageStartDate, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestJP_LCLAvailable_ForStandAloneDeclaration_ComesFromContainer()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromContainer(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobContainerSchema.JC_LCLAvailable, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestJP_LCLStorageCommences_ForStandAloneDeclaration_ComesFromContainer()
		{
			TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromContainer(JobConsolTransportSchema.JW_DepotStorageDate, JobContainerSchema.JC_LCLStorageCommences, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestAvailableOrStorage_ForStandAloneDeclaration_ComesFromContainer(SchemaDateTimeColumn sailingDateProperty, SchemaDateTimeColumn containerDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[0];
			JobDocsAndCartage docsAndCartage = (JobDocsAndCartage)Declaration["DocsAndCartage"];
			Declaration[JobDeclarationSchema.JE_TransportMode] = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration[JobDeclarationSchema.JE_VesselName] = SeaVoyage.JV_RV_NKVessel;
			Declaration[JobDeclarationSchema.JE_VoyageFlightNo] = SeaVoyage.JV_VoyageFlight;
			Declaration[JobDeclarationSchema.JE_OH_ShippingLine] = SeaVoyage.JV_OH_Line;
			Declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = arrivalSailing.Destination.JB_RL_NKPortOfDischarge;

			var transportParent = (ITransportParent)Declaration;
			var transport = transportParent.Transports[0];
			transport.JW_RL_NKLoadPort = arrivalSailing.Origin.JA_RL_NKPortOfLoading;
			transport.JW_IsLinked = true;

			BusinessObjectCollection cusContainers = (BusinessObjectCollection)Declaration["CusContainers"];
			CommonContainer container1 = (CommonContainer)cusContainers.AddNew()["JobContainer"];
			CommonContainer container2 = (CommonContainer)cusContainers.AddNew()["JobContainer"];
			TestAvailableOrStorage_ComesFromContainer(docsAndCartage, transport, container1, container2, sailingDateProperty, containerDateProperty, docsAndCartageDateProperty);
		}

		void TestAvailableOrStorage_ComesFromContainer(JobDocsAndCartage docsAndCartage, Transport arrivalTransport, CommonContainer container1, CommonContainer container2, SchemaDateTimeColumn transportDateProperty, SchemaDateTimeColumn containerDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			ZDateTime newValue = new ZDateTime(2010, 1, 1);

			if (transportDateProperty.TableName == JobConsolTransportSchema.Constants.TableName)
			{
				arrivalTransport[transportDateProperty] = newValue;
			}
			else
			{
				throw new InvalidOperationException("unknown table: " + transportDateProperty.TableName);
			}

			AssertEquals(containerDateProperty.Name + " comes from the sailing schedule", new ZDateTime(2010, 1, 1), docsAndCartage[docsAndCartageDateProperty]);

			container1[containerDateProperty] = new ZDateTime(2011, 1, 1);
			AssertEquals(containerDateProperty.Name + " comes from the minimum of schedule dates and container dates", new ZDateTime(2010, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
			container1[containerDateProperty] = new ZDateTime(2009, 1, 1);
			AssertEquals(containerDateProperty.Name + " comes from the minimum of schedule dates and container dates", new ZDateTime(2009, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
			container2[containerDateProperty] = new ZDateTime(2010, 1, 1);
			AssertEquals(containerDateProperty.Name + " comes from the minimum of schedule dates and container dates", new ZDateTime(2009, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
			container2[containerDateProperty] = new ZDateTime(2008, 1, 1);
			AssertEquals(containerDateProperty.Name + " comes from the minimum of schedule dates and container dates", new ZDateTime(2008, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
			AssertEquals("LCL dates not overridden when the date comes from a container", false, docsAndCartage.JP_LCLDatesOverrideConsol);

			container1[containerDateProperty] = ZDateTime.Empty;
			container2[containerDateProperty] = ZDateTime.Empty;
			container1.JC_OverrideFCLAvailableStorage = false;
			container1.JC_OverrideLCLAvailableStorage = false;
			container2.JC_OverrideFCLAvailableStorage = false;
			container2.JC_OverrideLCLAvailableStorage = false;
			AssertEquals(containerDateProperty.Name + " comes from the minimum of schedule dates and container dates", new ZDateTime(2010, 1, 1), docsAndCartage[docsAndCartageDateProperty]);

			container1[containerDateProperty] = new ZDateTime(2011, 1, 1);
			container1.JC_OverrideFCLAvailableStorage = true;
			container2.JC_OverrideFCLAvailableStorage = true;
			AssertEquals(containerDateProperty.Name + " comes from the minimum of schedule dates and container dates", new ZDateTime(2010, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
			container2[containerDateProperty] = new ZDateTime(2012, 1, 1);
			AssertEquals(containerDateProperty.Name + " comes from the minimum container date, all containers are overriden", new ZDateTime(2011, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
		}

		#endregion

		#region Overriding

		public void TestJP_LCLAvailable_Override()
		{
			TestLCLAvailableStorage_Override(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestJP_LCLStorage_Override()
		{
			TestLCLAvailableStorage_Override(JobConsolTransportSchema.JW_DepotStorageDate, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestLCLAvailableStorage_Override(SchemaDateTimeColumn transportDateProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[0];
			CommonShipment shipment = Consol.Shipments.AddNew();
			JobDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
			Transport arrivalTransport = Consol.Transports.MostInterestingTransport;
			arrivalTransport.JW_JX = arrivalSailing.PK;

			arrivalTransport[transportDateProperty] = new ZDateTime(2000, 1, 1);
			AssertEquals(docsAndCartageDateProperty.Name + " not overridden initially", false, docsAndCartage.JP_LCLDatesOverrideConsol);
			AssertEquals(docsAndCartageDateProperty.Name + " comes from the sailing schedule when not overridden", new ZDateTime(2000, 1, 1), docsAndCartage[docsAndCartageDateProperty]);

			docsAndCartage[docsAndCartageDateProperty] = new ZDateTime(2005, 1, 1);
			AssertEquals(docsAndCartageDateProperty.Name + " overridden", true, docsAndCartage.JP_LCLDatesOverrideConsol);
			AssertEquals(docsAndCartageDateProperty.Name + " overridden", new ZDateTime(2005, 1, 1), docsAndCartage[docsAndCartageDateProperty]);

			docsAndCartage.JP_LCLDatesOverrideConsol = false;
			AssertEquals(docsAndCartageDateProperty.Name + " comes from the sailing schedule again when not overridden", new ZDateTime(2000, 1, 1), docsAndCartage[docsAndCartageDateProperty]);
		}

		#endregion

		#endregion

		#region Test setting non-persistent does not blow up

		[ExpectNoExceptions]
		public void TestSettingNonPersistentParentDoesNotBlowUp()
		{
			AssertEquals("Precondition - ensure we are testing with a NonPersistentBusinessObject",
					true, typeof(MockJobDocsAndCartageParent).IsSubclassOf(typeof(NonPersistentBusinessObject)));

			JobDocsAndCartage jDAC = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
		}

		#endregion

		#region Implementation

		BusinessObject Declaration
		{
			get { return declaration ?? (declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>()); }
		}
		BusinessObject declaration;

		CommonConsol Consol
		{
			get { return consol ?? (consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>()); }
		}
		CommonConsol consol;

		CommonShipment Shipment
		{
			get { return shipment ?? (shipment = CommonShipment.New(Factory)); }
		}
		CommonShipment shipment;

		#endregion

		#region TestAdditionalRefTypes

		public void TestAdditionalRefTypes()
		{
			JobDocsAndCartage docCartage = JobDocsAndCartage.New(Factory.New<CommonShipment>());

			AssertCollectionNotContains("AdditionalRefTypes shouldn't contain any", ((IHaveRequiredDocuments)docCartage).AdditionalRefTypes);
		}

		#endregion

		public void TestServiceBranch()
		{
			var docCartage = JobDocsAndCartage.New(Factory.New<CommonShipment>());
			var iHaveServices = (IHaveServices)docCartage;
			AssertEquals("Service branch", Enterprise.Environment.Env.CurrentBranch.PK, iHaveServices.ServiceBranch.PK);
		}
	}
}
