using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport))]
	sealed class JobRequiredDocumentAllDocumentsReceivedEventLoggerTest_ForExport : JobRequiredDocumentAllDocumentsReceivedEventLoggerTest<JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport>
	{
		protected override JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport NewCollectionEventLogger()
		{
			return new JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport(Parent);
		}

		protected override KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
		{
			get
			{
				var result = new List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>>();
				result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(JobRequiredDocumentSchema.EQ_DateReceived, Events.AllExportDocumentsReceived));
				return result.ToArray();
			}
		}

		protected override ZString ImportOrExport
		{
			get { return JobRequiredDocument.DocUsage.Export; }
		}
	}
}
