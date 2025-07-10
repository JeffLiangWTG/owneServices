using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineManyToManyCollectionFreightTest : BaseFreightTest
	{
		public void TestRegisterConcurrencyCheck()
		{
			var consol = Factory.New<CommonConsol>();

			AssertEquals("concurrency check has not yet been registered", false, ContainerPackLineConcurrencyCheck.IsRegistered(Factory));

			var collection = new PackLineManyToManyCollection(consol.Containers.AddNew());
			collection.Load();

			AssertEquals("concurrency check has been registered", true, ContainerPackLineConcurrencyCheck.IsRegistered(Factory));
		}

		public void TestAddingAContainerToAnInnerPackLine()
		{
			PackLine packLine = Shipment.InnerPackLines.AddNew();
			Container.PackLines.Add(packLine);
			AssertEquals("Should not have added the packline", false, container.PackLines.Contains(packLine));
		}

		public void TestOnAddedAndOnRemovedStayInSync()
		{
			CFSDataRegistry.Instance.AutopackContainers.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 5;
			CommonContainer container = consol.Containers.AddNew();
			shipment.OuterPackLines[0].SetContainer(container.PK);
			AssertEquals("CommonShipment PackLines / Containers PackLines Count After Set Container", consol.Shipments[0].OuterPackLines.Count, consol.Containers[0].PackLines.Count);
			PackLine addedPackLine = shipment.OuterPackLines.AddNew();
			AssertEquals("CommonShipment PackLines / Containers PackLines Count After Add New To Shipment", consol.Shipments[0].OuterPackLines.Count, consol.Containers[0].PackLines.Count);
			shipment.OuterPackLines.Remove(addedPackLine);
			AssertEquals("CommonShipment PackLines / Containers PackLines Count After Remove", consol.Shipments[0].OuterPackLines.Count, consol.Containers[0].PackLines.Count);
			consol.Containers.Remove(container);
			AssertEquals("Containers should have no packlines", 0, container.PackLines.Count);
		}

		public void TestAddAndRemove()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			JobService containerFumigation = container.Services.AddNew();
			CommonShipment shipment = CommonShipment.New(Factory);
			containerFumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			PackLine line = shipment.OuterPackLines.AddNew();
			Assert("Precondition - Packline CommonShipment should not have fumigation", !line.Shipment.DocsAndCartage.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));
			container.PackLines.Add(line);
			PackLine line2 = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(line2);
			Assert("Expecting container to contain line.", container.PackLines.Contains(line.PK));
			Assert("Expecting line to contain container.", line.Containers.Contains(container.PK));
			Assert("Expecting line to have fumigation", line.Shipment.DocsAndCartage.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));

			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_OuterPacksInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualVolumeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfVolumeInfo.ReadOnly);

			container.PackLines.Remove(line);
			Assert("Not expecting container to contain line.", !container.PackLines.Contains(line.PK));
			Assert("Not expecting line to contain container.", !line.Containers.Contains(container.PK));
			Assert("Expecting line to not have fumigation", !line.Shipment.DocsAndCartage.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));

			Assert("Expecting Line to contain container2", container.PackLines.Contains(line2));

			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_OuterPacksInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfWeightInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_ActualVolumeInfo.ReadOnly);
			AssertEquals("Expecting number fields not to be readonly", false, line.Shipment.JS_UnitOfVolumeInfo.ReadOnly);
		}

		public void TestUpdatingContainersCollectionOnAdd()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			using (var callCounter = ContainerPackLineRelationshipHelper.GetNewCallCounter())
			{
				var packLine = shipment.OuterPackLines.AddNew();

				AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { packLine }, container.PackLines);
				AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { container }, packLine.Containers);

				AssertEquals("should only once notify helper that packline was added to container", 1, callCounter.GetTotalCalls("PackLineAddedToContainer"));
			}
		}

		public void TestUpdatingContainersCollectionOnRemove()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { packLine }, container.PackLines);
			AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { container }, packLine.Containers);

			using (var callCounter = ContainerPackLineRelationshipHelper.GetNewCallCounter())
			{
				container.PackLines.Remove(packLine);

				AssertEquals("should only once notify helper that packline was removed from container", 1, callCounter.GetTotalCalls("PackLineRemovedFromContainer"));
			}
		}

		public void TestUpdateFCLAvailableDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_FCLAvailable, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestUpdateFCLStorageDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_ArrivalCTOStorageStartDate, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestUpdateLCLAvailableDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_LCLAvailable, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestUpdateLCLStorageDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_LCLStorageCommences, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestUpdateAvailableStorageDatesOnAddRemove(SchemaDateTimeColumn containerDateProperty, SchemaDateTimeColumn docsAndCartageProperty)
		{
			Container[containerDateProperty] = new ZDateTime(2000, 2, 2);
			Container2[containerDateProperty] = new ZDateTime(2000, 1, 1);

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(Consol, Container);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(Consol, Container2);

			AssertEquals(docsAndCartageProperty.Name + " initially", new ZDateTime(2000, 1, 1), Shipment.DocsAndCartage[docsAndCartageProperty]);
			Container2.PackLines.Remove(packLine2);
			AssertEquals(docsAndCartageProperty.Name + " date after removing pack line", new ZDateTime(2000, 2, 2), Shipment.DocsAndCartage[docsAndCartageProperty]);
			Container2.PackLines.Add(packLine2);
			AssertEquals(docsAndCartageProperty.Name + " after adding pack line", new ZDateTime(2000, 1, 1), Shipment.DocsAndCartage[docsAndCartageProperty]);

			Container2.PackLines.Remove(packLine2);
			Factory.Save();
			CommonShipment loadedShipment = (CommonShipment)new BusinessObjectFactory().Load(Shipment.GetType(), Shipment.PK);
			AssertEquals("After saving and re-loading", new ZDateTime(2000, 2, 2), loadedShipment.DocsAndCartage[docsAndCartageProperty]);
		}

		#region Implementation

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;

		CommonShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Consol.Shipments.AddNew();
				}
				return shipment;
			}
		}
		CommonShipment shipment;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Consol.Containers.AddNew();
				}
				return container;
			}
		}
		CommonContainer container;

		CommonContainer Container2
		{
			get
			{
				if (container2 == null)
				{
					container2 = Consol.Containers.AddNew();
				}
				return container2;
			}
		}
		CommonContainer container2;

		#endregion
	}
}
