using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport))]
	sealed class JobRequiredDocumentAllDocumentsReceivedEventLoggerTest_ForImport_WrongOrigin : JobRequiredDocumentAllDocumentsReceivedEventLoggerTest<JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport>
	{
		public override void TestCancelEventsOnParentForCollection()
		{
			//not testing this here
			Assert(true);
		}

		public override void TestAddAndCancelEventsOnParentCollection_WithEstimateEvent()
		{
			//not testing this here
			Assert(true);
		}

		protected override void TestLogEventsOnParentForCollection(SchemaDateTimeOffsetColumn property, Event ev)
		{
			var businessObject1 = NewCollectionElement(Parent);
			businessObject1[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);
			Factory.Save();
			AssertEventNotRaised("Event not raised until all container dates set", ev);

			var businessObject2 = NewCollectionElement(Parent);
			businessObject2[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);
			Factory.Save();
			AssertEventNotRaised("Event still not raised, because origin is wrong (AddedEDocs)", ev);

			DeleteBusinessObject(businessObject1);
			DeleteBusinessObject(businessObject2);
			Factory.Save();
		}

		protected override BusinessObject NewCollectionElement(DummyRequiredDocumentsParent shipment)
		{
			var result = shipment.RequiredDocuments.AddNew();
			result.EQ_DocUsage = ImportOrExport;
			result.Origin = JobRequiredDocument.JRDOrigin.AddedEDoc;
			return result;
		}

		protected override BusinessObject LoadCollectionElementFromOtherFactory(BusinessObjectFactory factory, BusinessObject businessObject)
		{
			var result = (JobRequiredDocument)base.LoadCollectionElementFromOtherFactory(factory, businessObject);
			result.ParentType = Parent.GetType();
			result.Origin = JobRequiredDocument.JRDOrigin.AddedEDoc;
			return result;
		}

		protected override JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport NewCollectionEventLogger()
		{
			return new JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport(Parent);
		}

		protected override KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
		{
			get
			{
				var result = new List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>>();
				result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(JobRequiredDocumentSchema.EQ_DateReceived, Events.AllImportDocumentsReceived));
				return result.ToArray();
			}
		}

		protected override ZString ImportOrExport
		{
			get { return JobRequiredDocument.DocUsage.Import; }
		}
	}
}
