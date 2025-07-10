using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class ShipmentDeclarationFetchStrategyTest : TestCaseWithFactory
	{
		#region TestPropertiesBasedOnDocsAndCartage

		public void TestPropertiesBasedOnDocsAndCartage()
		{
			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.EstimatedPickupDate),
				new TableColumn("", ShipmentDeclarationSchema.Constants.PickupDateRequiredBy),
				new TableColumn("", ShipmentDeclarationSchema.Constants.EstimatedDeliveryDate),
				new TableColumn("", ShipmentDeclarationSchema.Constants.DeliveryDateRequiredBy),
				new TableColumn("", ShipmentDeclarationSchema.Constants.DeliveryDate),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ActualPickupDate)
			};

			AssertFetchHints(columns);
		}

		#endregion

		#region TestOrderReference

		public void TestOrderReference()
		{
			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.OrderReference),
			};

			AssertFetchHints(columns);
		}

		#endregion

		#region TestTop3Containers

		public void TestTop3Containers()
		{
			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.Top3Containers),
			};

			AssertFetchHints(columns, 0, 0, true);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.HouseBill)
			};

			AssertFetchHints(columns, 1, 0, false);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.MasterBill)
			};

			AssertFetchHints(columns, 1, 0, false);
		}

		#endregion

		#region TestCurrentLoadPort

		public void TestCurrentLoadPort()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.CurrentLoadPort) },
							 1,
							 0,
							 false);
		}

		#endregion

		#region TestCurrentDischargePort

		public void TestCurrentDischargePort()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.CurrentDischargePort) },
							 1,
							 0,
							 false);
		}

		#endregion

		#region TestMainLoadPort

		public void TestMainLoadPort()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.MainLoadPort) },
							 1,
							 0,
							 false);
		}

		#endregion

		#region TestMainDischargePort

		public void TestMainDischargePort()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.MainDischargePort) },
							 1,
							 0,
							 false);
		}

		#endregion

		#region Test Consignor/Consignee RelatedProperties

		public void TestConsignorOrConsigneeRelatedProperties()
		{
			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsignorFullAddress),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsignorName),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsignorAddress),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsignorPostCode),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsignorCity),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsignorState),

				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsigneeFullAddress),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsigneeName),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsigneeAddress),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsigneePostCode),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsigneeCity),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ConsigneeState)
			};

			AssertFetchHints(columns);
		}

		#endregion

		#region TestPropertiesBasedOnMilestones

		public void TestPropertiesBasedOnMilestones()
		{
			LoadCollection(Factory);
			foreach (IShipmentDeclaration bizO in TestCollection)
			{
				if (bizO is TrackingDeclaration)
				{
					AddSomeMilestones(((TrackingDeclaration)bizO).Declaration);
				}
				else
				{
					AddSomeMilestones((IWorkflowProvider)bizO);
				}
			}
			Factory.Save();

			TableColumn[] columns = new[]
			{
				new TableColumn("", string.Format("Milestones+{0}+{1}", TrackingMilestoneCollection.Schema.LastMilestone, TrackingMilestone.Schema.Description)),
				new TableColumn("", string.Format("Milestones+{0}+{1}", TrackingMilestoneCollection.Schema.LastMilestone, TrackingMilestone.Schema.DisplayDate)),
				new TableColumn("", string.Format("Milestones+{0}+{1}", TrackingMilestoneCollection.Schema.NextMilestone, TrackingMilestone.Schema.Description)),
				new TableColumn("", string.Format("Milestones+{0}+{1}", TrackingMilestoneCollection.Schema.NextMilestone, TrackingMilestone.Schema.EstimatedDate)),
			};

			AssertFetchHints(columns, 1, 0, true);
		}

		void AddSomeMilestones(IWorkflowProvider provider)
		{
			ProcessTask m1 = provider.WorkflowItems.AddNew();
			m1.P9_Type = Core.Constants.Workflow.MilestoneType;
			m1.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-1));
			m1.P9_Description = "m1";

			ProcessTask m2 = provider.WorkflowItems.AddNew();
			m2.P9_Type = Core.Constants.Workflow.MilestoneType;
			m2.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now.AddDays(1));
			m2.P9_Description = "m2";
		}

		#endregion

		#region TestVesselVoyage

		public void TestMainVessel()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.MainVessel) },
							 1,
							 0,
							 false);
		}

		public void TestMainVoyageFlightNoWithSuppression()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.MainVoyageWithSuppression) },
							 1,
							 0,
							 false);
		}

		public void TestCurrentVessel()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.CurrentVessel) },
							 1,
							 0,
							 false);
		}

		public void TestCurrentVoyageFlightNoWithSuppression()
		{
			AssertFetchHints(new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.CurrentVoyageWithSuppression) },
							 1,
							 0,
							 false);
		}

		#endregion

		#region TestTop3JobNotes

		[HttpContextEnabledTest]
		public void TestTop3JobNotes()
		{
			WebTestHelper.TestSiteUser.Login(WebTestHelper.TestOrg.OH_Code, WebTestHelper.TestContact.OC_Email, WebTestHelper.TestContact.PasswordForTesting);
			LoadCollection(Factory, WebTestHelper.TestSiteUser);

			TestCollection.FetchStrategy.FetchForView(TestCollection.ToArray<BusinessObject>(), new[] { new TableColumn("", ShipmentDeclarationSchema.Constants.Top3JobNotes) });

			var firstBizo = (BusinessObject)TestCollection[0];
			_ = firstBizo[ShipmentDeclarationSchema.Constants.Top3JobNotes];
			var initialNoteTableHitCount = Factory.GetTableHitCount(StmNoteSchema.Constants.TableName);

			for (int i = 1; i < TestCollection.Count; i++)
			{
				var bizO = (BusinessObject)TestCollection[i];
				_ = bizO[ShipmentDeclarationSchema.Constants.Top3JobNotes];

				AssertEquals(initialNoteTableHitCount, Factory.GetTableHitCount(StmNoteSchema.Constants.TableName));
			}
		}

		#endregion

		#region Implementation

		TestHelper WebTestHelper
		{
			get { return webTestHelper ?? (webTestHelper = new TestHelper(Factory)); }
		}
		TestHelper webTestHelper;

		#region Overriden Methods

		protected override void SetUp()
		{
			base.SetUp();

			previousIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			TrackingShipment shipment1 = factory.New<TrackingShipment>();
			TrackingShipment shipment2 = factory.New<TrackingShipment>();
			BaseJobDeclaration declaration1 = factory.New<BaseJobDeclaration>();
			BaseJobDeclaration declaration2 = factory.New<BaseJobDeclaration>();

			PKs = new ZGuid[] { shipment1.PK, shipment2.PK, declaration1.PK, declaration2.PK };

			factory.Save();
		}

		bool previousIsWeb;

		protected override void TearDown()
		{
			base.TearDown();
			Globals.IsWeb = previousIsWeb;
		}

		#endregion

		ZGuid[] PKs;
		ShipmentDeclarationCollection TestCollection;

		void AssertFetchHints(TableColumn[] columns)
		{
			AssertFetchHints(columns, 0, 0, false);
		}

		void AssertFetchHints(TableColumn[] columns, int expectedShipmentDBHitsIncrease, int expectedDeclarationDBHitsIncrease, bool hintsForShipmentAndDeclarationDiffer)
		{
			AssertFetchHints(columns, expectedShipmentDBHitsIncrease, expectedDeclarationDBHitsIncrease, hintsForShipmentAndDeclarationDiffer, null);
		}

		void AssertFetchHints(TableColumn[] columns, int expectedShipmentDBHitsIncrease, int expectedDeclarationDBHitsIncrease, bool hintsForShipmentAndDeclarationDiffer, TrackingSiteUser siteUser)
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			LoadCollection(factory1, siteUser);
			AssertWithFetchHintsOff(factory1, columns);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			LoadCollection(factory2, siteUser);
			AssertWithFetchHintsOn(factory2, columns, expectedShipmentDBHitsIncrease, expectedDeclarationDBHitsIncrease, hintsForShipmentAndDeclarationDiffer);
		}

		void AssertWithFetchHintsOn(BusinessObjectFactory factory, TableColumn[] columns, int expectedShipmentDBHitsIncrease, int expectedDeclarationDBHitsIncrease, bool hintsForShipmentAndDeclarationDiffer)
		{
			TestCollection.FetchStrategy.FetchForView(TestCollection.ToArray<BusinessObject>(), columns);

			AssertNotNull("Property of the first shipment is called", ((TrackingShipment)TestCollection[0])[columns[0].ColumnName]);

			if (hintsForShipmentAndDeclarationDiffer)
			{
				AssertNotNull("Property of the first declaration is called", ((TrackingDeclaration)TestCollection[2])[columns[0].ColumnName]);
			}

			int hitCountAfterFetchHintsExecuted = factory.DatabaseLoadCount;

			if (!hintsForShipmentAndDeclarationDiffer)
			{
				for (int i = 1; i < TestCollection.Count; i++)
				{
					BusinessObject bizO = (BusinessObject)TestCollection[i];

					foreach (TableColumn column in columns)
					{
						string message = string.Format("Business object index is {0}, property name is {1}. ", i, column.ColumnName);

						AssertNotNull(message + "Property is called", bizO[column.ColumnName]);

						if (bizO is TrackingShipment)
						{
							hitCountAfterFetchHintsExecuted += expectedShipmentDBHitsIncrease;
						}
						else
						{
							hitCountAfterFetchHintsExecuted += expectedDeclarationDBHitsIncrease;
						}

						AssertEquals(message + "DB hits count should not change", hitCountAfterFetchHintsExecuted, factory.DatabaseLoadCount);
					}
				}
			}
		}

		void AssertWithFetchHintsOff(BusinessObjectFactory factory, TableColumn[] columns)
		{
			string propertyName = columns[0].ColumnName;

			BusinessObject bizO1 = (BusinessObject)TestCollection[0];
			AssertNotNull("Property of the first bizO is called", bizO1[propertyName]);
			int hitCountAfterFetchHintsExecuted = factory.DatabaseLoadCount;

			BusinessObject bizO2 = (BusinessObject)TestCollection[1];
			AssertNotNull("Property of the second bizO is called", bizO2[propertyName]);

			Assert(@"Property: " + propertyName + @". DB hits count should be greater then before.
If a test fails on this line that means that a tested fetch hint is not required and has to be removed.
Probably it can occurs after changing/improving business logic of the tested business object.",
				factory.DatabaseLoadCount > hitCountAfterFetchHintsExecuted);
		}

		void LoadCollection(BusinessObjectFactory factory)
		{
			LoadCollection(factory, null);
		}

		void LoadCollection(BusinessObjectFactory factory, TrackingSiteUser siteUser)
		{
			TrackingShipment shipment1 = factory.Load<TrackingShipment>(PKs[0]);
			shipment1.SiteUser = siteUser;
			TrackingShipment shipment2 = factory.Load<TrackingShipment>(PKs[1]);
			shipment2.SiteUser = siteUser;

			TestCollection = new ShipmentDeclarationCollection(factory)
							 {
								shipment1,
								shipment2,
								new TrackingDeclaration(factory.Load<BaseJobDeclaration>(PKs[2]), siteUser),
								new TrackingDeclaration(factory.Load<BaseJobDeclaration>(PKs[3]), siteUser)
							 };
		}

		#endregion
	}
}
