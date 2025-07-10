using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.TransportBookings.Shared.Testing
{
	[TestsSubclassesOf(typeof(IDtbBookingParent), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class IDtbBookingParentTestCase<T> : DtbBookingSharedTestCaseWithFactory where T : IDtbBookingParent
	{
		public virtual void TestRelatedTransportBookingObjectsWithEvents()
		{
			if (!IsHiddenParent)
			{
				var parent = GetNewParent();
				var consolidationBooking = Helper.CreateConsolidation(parent);
				var booking1 = Helper.CreateBooking(consolidationBooking);
				var booking2 = Helper.CreateBooking(consolidationBooking);
				var outOfScopeBooking = Helper.CreateBooking();

				var eParent = parent as IStmALogParent;
				var relatedObjectsWithEvents = eParent.BusinessObjectsWithRelatedEvents;
				AssertCollectionContains("Should contain both Related Transport Booking 1.", booking1, relatedObjectsWithEvents);
				AssertCollectionContains("Should contain both Related Transport Booking 2.", booking2, relatedObjectsWithEvents);
				AssertCollectionNotContains("Should NOT contain outOfScopeBooking.", outOfScopeBooking, relatedObjectsWithEvents);

				var consolidationBooking2 = Helper.CreateConsolidation(parent);
				var booking3 = Helper.CreateBooking(consolidationBooking2);
				relatedObjectsWithEvents = eParent.BusinessObjectsWithRelatedEvents;
				AssertCollectionContains("Should contain both Related Transport Booking 1.", booking1, relatedObjectsWithEvents);
				AssertCollectionContains("Should contain both Related Transport Booking 2.", booking2, relatedObjectsWithEvents);
				AssertCollectionContains("Should contain both Related Transport Booking 3.", booking3, relatedObjectsWithEvents);
				AssertCollectionNotContains("Should NOT contain outOfScopeBooking.", outOfScopeBooking, relatedObjectsWithEvents);

				var ebooking1 = (EnterpriseBusinessObject)booking1;
				var ebooking2 = (EnterpriseBusinessObject)booking2;
				var ebooking3 = (EnterpriseBusinessObject)booking3;
				var eOutOfScopeBooking = (EnterpriseBusinessObject)outOfScopeBooking;
				var eConsolidationBooking = (EnterpriseBusinessObject)consolidationBooking;
				var eConsolidationBooking2 = (EnterpriseBusinessObject)consolidationBooking2;

				AssertCollectionContains("Booking1 should contain the related Parent.", parent, ebooking1.BusinessObjectsWithRelatedEvents);
				AssertCollectionContains("Booking2 should contain the related Parent.", parent, ebooking2.BusinessObjectsWithRelatedEvents);
				AssertCollectionContains("Booking3 should contain the related Parent.", parent, ebooking3.BusinessObjectsWithRelatedEvents);
				AssertCollectionNotContains("OutOfScope Booking should NOT contain the related Parent.", parent, eOutOfScopeBooking.BusinessObjectsWithRelatedEvents);
				AssertCollectionNotContains("ConsolidationBooking should NOT contain the related Parent.", parent, eConsolidationBooking.BusinessObjectsWithRelatedEvents);
				AssertCollectionNotContains("ConsolidationBooking2 should NOT contain the related Parent.", parent, eConsolidationBooking2.BusinessObjectsWithRelatedEvents);
			}
			else
			{
				// Hidden Parent means that the Booking will now show Parent Events
				Assert(true);
			}
		}

		public virtual void TestRelatedTransportBookingObjectsWithEDocs()
		{
			if (!IsHiddenParent)
			{
				var parent = GetNewParent();
				var consolidationBooking1 = Helper.CreateConsolidation(parent);
				var consolidationBooking2 = Helper.CreateConsolidation(parent);
				var booking1a = Helper.CreateBooking(consolidationBooking1);
				var booking1b = Helper.CreateBooking(consolidationBooking1);
				var booking2a = Helper.CreateBooking(consolidationBooking2);
				var outOfScopeBooking = Helper.CreateBooking();

				var iParent = (IDocManagerSupport)parent;
				var relatedObjectsWithEDocs = iParent.DocManagerInfo.RelatedObjects;
				AssertCollectionContains("Should contain both Related Transport Booking 1a.", booking1a, relatedObjectsWithEDocs);
				AssertCollectionContains("Should contain both Related Transport Booking 1b.", booking1b, relatedObjectsWithEDocs);
				AssertCollectionContains("Should contain both Related Transport Booking 2a.", booking2a, relatedObjectsWithEDocs);
				AssertCollectionContains("Should contain both Related Transport Booking Consolidation 1.", consolidationBooking1, relatedObjectsWithEDocs);
				AssertCollectionContains("Should contain both Related Transport Booking Consolidation 2.", consolidationBooking2, relatedObjectsWithEDocs);
				AssertCollectionNotContains("Should NOT contain outOfScopeBooking.", outOfScopeBooking, relatedObjectsWithEDocs);

				var expectedRelatedObjects = new[] { (BusinessObject)iParent };
				AssertContainsExactElementsInAnyOrder(expectedRelatedObjects, ((IDocManagerSupport)booking1a).DocManagerInfo.RelatedObjects);
				AssertContainsExactElementsInAnyOrder(expectedRelatedObjects, ((IDocManagerSupport)booking1b).DocManagerInfo.RelatedObjects);
				AssertContainsExactElementsInAnyOrder(expectedRelatedObjects, ((IDocManagerSupport)booking2a).DocManagerInfo.RelatedObjects);
				AssertCollectionContains(iParent, ((IDocManagerSupport)consolidationBooking1).DocManagerInfo.RelatedObjects);
				AssertCollectionContains(iParent, ((IDocManagerSupport)consolidationBooking2).DocManagerInfo.RelatedObjects);
			}
			else
			{
				// Hidden Parent means that the Booking will now show Parent in eDocs
				Assert(true);
			}
		}

		public void TestConsolidationDocumentSupporter_SupportedChildBusinessContexts()
		{
			if (!IsHiddenParent)
			{
				var parent = GetNewParent();
				var consolidation = Helper.CreateConsolidation(parent);
				var booking = Helper.CreateBooking(consolidation);

				var iParent = (IDocumentSupportable)parent;
				var iConsolidation = (IDocumentSupportable)consolidation;
				var parentDocumentSupporter = iParent.DocumentSupporter;
				var consolidationDocumentSupporter = iConsolidation.DocumentSupporter;
				AssertCollectionContains(string.Format("Should contain Parent Business Context {0}.", parentDocumentSupporter.BusinessContext), parentDocumentSupporter.BusinessContext, consolidationDocumentSupporter.SupportedChildBusinessContexts);
			}
			else
			{
				// Hidden Parent means that the Booking will refer to the Parent for Documents
				Assert(true);
			}
		}

		public void TestConsolidationDocumentSupporter_GetChildCollection()
		{
			if (!IsHiddenParent)
			{
				var parent = GetNewParent();
				var consolidation = Helper.CreateConsolidation(parent);
				var booking = Helper.CreateBooking(consolidation);

				var iParent = (IDocumentSupportable)parent;
				var iConsolidation = (IDocumentSupportable)consolidation;
				var parentDocumentSupporter = iParent.DocumentSupporter;
				var consolidationDocumentSupporter = iConsolidation.DocumentSupporter;
				AssertCollectionContains("Should contain Parent. If it doesn't check business context has been added to SupportedChildBusinessContexts", iParent, consolidationDocumentSupporter.GetChildCollection(null, parentDocumentSupporter.BusinessContext, null));
			}
			else
			{
				// Hidden Parent means that the Booking will refer to the Parent for Documents
				Assert(true);
			}
		}

		public void TestBookingDocumentSupporter_SupportedChildBusinessContexts()
		{
			if (!IsHiddenParent)
			{
				var parent = GetNewParent();
				var consolidation = Helper.CreateConsolidation(parent);
				var booking = Helper.CreateBooking(consolidation);

				var iParent = (IDocumentSupportable)parent;
				var iBooking = (IDocumentSupportable)booking;
				var parentDocumentSupporter = iParent.DocumentSupporter;
				var bookingDocumentSupporter = iBooking.DocumentSupporter;
				AssertCollectionContains(string.Format("Should contain Parent Business Context {0}.", parentDocumentSupporter.BusinessContext), parentDocumentSupporter.BusinessContext, bookingDocumentSupporter.SupportedChildBusinessContexts);
			}
			else
			{
				// Hidden Parent means that the Booking will refer to the Parent for Documents
				Assert(true);
			}
		}

		public void TestWorkflowDescriptor_SupportsCreateTransportBooking()
		{
			var parent = GetNewParent();
			var workflowProvider = parent as IWorkflowProviderCore;
			if (workflowProvider != null)
			{
				WorkflowDescriptor descriptor;
				WorkflowDescriptors.Instance.TryGetValue(workflowProvider.WorkflowType, out descriptor);
				AssertEquals("", IsWorkflowDescriptorSupportsCreateTransportBooking(), descriptor.SupportsCreateTransportBooking);
			}
			else
			{
				AssertEquals("Parent does not have workflow.", false, IsWorkflowDescriptorSupportsCreateTransportBooking());
			}
		}

		protected abstract bool IsWorkflowDescriptorSupportsCreateTransportBooking();

		public void TestStandaloneBookingDocumentSupporter_GetChildCollection()
		{
			if (!IsHiddenParent)
			{
				var parent = GetNewParent();

				var iParent = (IDocumentSupportable)parent;
				var parentDocumentSupporter = iParent.DocumentSupporter;

				var standaloneBooking = Helper.CreateBooking();
				var standaloneBookingDocumentSupporter = ((IDocumentSupportable)(standaloneBooking)).DocumentSupporter;
				var standAloneTB_DocumentSupporterChildCollection_ParentBusinessContext = standaloneBookingDocumentSupporter.GetChildCollection(null, parentDocumentSupporter.BusinessContext, null);
				AssertNotNull("Should not return null if there is no parent.", standAloneTB_DocumentSupporterChildCollection_ParentBusinessContext);
				AssertContainsExactElementsInAnyOrder("Should not have any document supportables as this booking isnt valid for the passed in context.", Array.Empty<IDocumentSupportable>(), standAloneTB_DocumentSupporterChildCollection_ParentBusinessContext);

				var standAloneTB_DocumentSupporterChildCollection_BookingBusinessContext = standaloneBookingDocumentSupporter.GetChildCollection(null, BusinessContext.DtbBooking, null);
				AssertContainsExactElementsInAnyOrder("Should return the booking as the context passed in is DtbBooking.", new[] { standaloneBooking }, standAloneTB_DocumentSupporterChildCollection_BookingBusinessContext);
			}
			else
			{
				// Hidden Parent means that the Booking will refer to the Parent for Documents
				Assert(true);
			}
		}

		public void TestBookingDocumentSupporter_GetChildCollection_HandlesTransportBookingContext()
		{
			var parent = GetNewParent();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);

			var iBooking = (IDocumentSupportable)booking;
			var actualBookingDocumentSupporter = iBooking.DocumentSupporter;

			var documentSupporterChildCollection = actualBookingDocumentSupporter.GetChildCollection(null, BusinessContext.DtbBooking, null);
			AssertContainsExactElementsInAnyOrder("Child collection should contain a booking, not the parent, as the context passed in is DtbBooking", new[] { iBooking }, documentSupporterChildCollection);
		}

		public void TestImplementsIRelatedJob()
		{
			Assert("IDtbBookingParent consumers should implement IRelatedJob", GetNewParent() is IRelatedJob);
		}

		public void TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob()
		{
			// arrange
			var testJob = GetNewParentForJobInvoicingPluginTests();

			(var runTest, var testJobInvoicingPlugin, var testJobInvoicingSupporter) = RequiresTestsForChildJobsAttachingToJobHeaderOnJobCreating(testJob);
			if (runTest && CanHaveDirectCartageChild)
			{
				var accounting = ObjectFactory.Get<IAccounting>();

				using (accounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var cartage = Factory.New<ICommonCartage>();
					Factory.Save(); // to populate JJ_ConsignmentID

					cartage.JJ_ParentID = testJob.PK;
					cartage.JJ_ParentTableCode = testJob.TablePrefix;
					Factory.Save();

					var cartageJobHeader = new JobHeader.Loader((IJobHeaderParent)cartage).TryLoadOrCreate();
					AssertNotNull("Check before act - should have non-null cartage job header", cartage.Job);
					AssertEquals("Check before act - cartageJobHeader is in fact cartage's job header", cartageJobHeader.PK, cartage.Job.PK);
					Assert("Check before act - cartage job header should not yet have parent job header", cartage.Job.JH_JH_ParentJob.IsEmpty);
					AssertNull("Check before act - test job has no job header as yet", testJobInvoicingSupporter.Job);
					ChildCartageTestExtraAssertionsPreAct(cartage);

					// act
					var testJobHeader = new JobHeader.Loader(testJobInvoicingPlugin).TryLoadOrCreate();

					// assert
					AssertTestJobHeaderIsNowParentOfDescendantJobHeader(testJobHeader, testJobInvoicingSupporter, cartage.Job, "cartage");
				}
			}
			else
			{
				Assert("No need to run this test as T is not an IJobInvoicingPlugin with a job invoicing supporter that can create an invoicing job, or T does not support direct cartage child", true);
			}
		}

		public void TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
		{
			// arrange
			var testJob = GetNewParentForJobInvoicingPluginTests();

			(var runTest, var testJobInvoicingPlugin, var testJobInvoicingSupporter) = RequiresTestsForChildJobsAttachingToJobHeaderOnJobCreating(testJob);
			if (runTest)
			{
				var accounting = ObjectFactory.Get<IAccounting>();

				using (accounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var booking = GetDtbBooking(testJob);
					Assert("Check before act - booking child of test job has no job header", booking.JobHeaderPK.IsEmpty);

					var cartage = Factory.New<ICommonCartage>();
					cartage.JJ_ParentID = booking.PK;
					cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
					Factory.Save();

					var cartageJobHeader = new JobHeader.Loader((IJobHeaderParent)cartage).TryLoadOrCreate();
					AssertNotNull("Check before act - should have non-null cartage job header", cartage.Job);
					AssertEquals("Check before act - cartageJobHeader is in fact cartage's job header", cartageJobHeader.PK, cartage.Job.PK);
					Assert("Check before act - cartage job header should not yet have parent job header", cartage.Job.JH_JH_ParentJob.IsEmpty);
					AssertNull("Check before act - test job has no job header as yet", testJobInvoicingSupporter.Job);

					// act
					var testJobHeader = new JobHeader.Loader(testJobInvoicingPlugin).TryLoadOrCreate();

					// assert
					AssertTestJobHeaderIsNowParentOfDescendantJobHeader(testJobHeader, testJobInvoicingSupporter, cartage.Job, "cartage");
				}
			}
			else
			{
				Assert("No need to run this test as T is not an IJobInvoicingPlugin with a job invoicing supporter that can create an invoicing job", true);
			}
		}

		public void TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
		{
			// arrange
			var testJob = GetNewParentForJobInvoicingPluginTests();

			(var runTest, var testJobInvoicingPlugin, var testJobInvoicingSupporter) = RequiresTestsForChildJobsAttachingToJobHeaderOnJobCreating(testJob);
			if (runTest)
			{
				var accounting = ObjectFactory.Get<IAccounting>();

				using (accounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var booking = GetDtbBooking(testJob);
					Assert("Check before act - booking child of test job has no job header", booking.JobHeaderPK.IsEmpty);

					var consignment = Factory.New<IDtbConsignment>();
					consignment.LTC_KM_Booking = booking.PK;
					consignment.LTC_Direction = Core.Constants.CartageDirection.Local;
					consignment.LTC_Status = TransportStatuses.Codes.Booked;
					Factory.Save();

					var consignmentJobHeader = new JobHeader.Loader((IJobHeaderParent)consignment).TryLoadOrCreate();
					AssertEquals("Check before act - consignment job header should be linked to consignment", consignment.PK, consignmentJobHeader.Parent.PK);
					Assert("Check before act - cartage job header should not yet have parent job header", consignmentJobHeader.JH_JH_ParentJob.IsEmpty);
					AssertNull("Check before act - test job has no job header as yet", testJobInvoicingSupporter.Job);

					// act
					var testJobHeader = new JobHeader.Loader(testJobInvoicingPlugin).TryLoadOrCreate();

					// assert
					AssertTestJobHeaderIsNowParentOfDescendantJobHeader(testJobHeader, testJobInvoicingSupporter, consignmentJobHeader, "land transport consignment");
				}
			}
			else
			{
				Assert("No need to run this test as T is not an IJobInvoicingPlugin with a job invoicing supporter that can create an invoicing job", true);
			}
		}

		bool IsHiddenParent => Attribute.IsDefined(typeof(T), typeof(HiddenBookingParentAttribute));

		protected abstract T GetNewParent();

		protected virtual T GetNewParentForJobInvoicingPluginTests()
		{
			return GetNewParent();
		}

		(bool result, IJobInvoicingPlugIn plugin, IJobInvoicingSupporter supporter) RequiresTestsForChildJobsAttachingToJobHeaderOnJobCreating(T testJob)
		{
			// as per JobHeader.IsJobAllowedToBeCreated
			if (testJob is IJobInvoicingPlugIn testJobInvoicingPlugin &&
				testJobInvoicingPlugin.InvoicingSupporter is IJobInvoicingSupporter testJobInvoicingSupporter &&
				testJobInvoicingSupporter.CanCreateInvoicingJob)
			{
				return (result: true, plugin: testJobInvoicingPlugin, supporter: testJobInvoicingSupporter);
			}
			else
			{
				return (result: false, plugin: null, supporter: null);
			}
		}

		protected virtual IDtbBooking GetDtbBooking(T testJob)
		{
			// default is to create new booking but eg for AgencyShipment we use the booking that is already there
			var consolidation = Helper.CreateConsolidation(testJob);
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();
			return booking;
		}

		protected abstract bool CanHaveDirectCartageChild { get; }

		protected virtual void ChildCartageTestExtraAssertionsPreAct(ICommonCartage cartage)
		{
		}

		void AssertTestJobHeaderIsNowParentOfDescendantJobHeader(JobHeader testJobHeader, IJobInvoicingSupporter testJobInvoicingSupporter, IJobHeader descendantJobHeader, string descendantDescription)
		{
			CombineAssertions(
				"This unit test is checking that a job header which has its parent set to this kind of job also checks for child job headers to attach to this job header in descendant Cartage jobs or Consignment jobs. If it has failed then it is likely that you have forgotten to implement the calls to CartageHelper.AttachCartageJobsToParentJob() or ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob() in this class' OnJobCreating() method (or in a method called from that method)",
				() =>
				{
					AssertEquals("Created job header is now job header of test job", testJobHeader.PK, testJobInvoicingSupporter.Job.PK);
					AssertEquals("New job header created on test job is now parent of " + descendantDescription + " job header as most likely should have happened in IJobHeader.OnJobCreating()", testJobInvoicingSupporter.Job.PK, descendantJobHeader.JH_JH_ParentJob);
				});
		}
	}
}
