using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.JobRequiredDocument;

namespace Enterprise.MasterFiles.Business
{
	internal class JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport : JobRequiredDocumentAllDocumentsReceivedEventLogger
	{
		public JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport(IHaveRequiredDocuments parent)
			: base(parent, DocUsage.Export)
		{
		}

		protected override KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
		{
			get
			{
				List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>> result = new List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>>();
				result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(JobRequiredDocumentSchema.EQ_DateReceived, Events.AllExportDocumentsReceived));
				return result.ToArray();
			}
		}
	}
}
