using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	internal class JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport : JobRequiredDocumentAllDocumentsReceivedEventLogger
	{
		public JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport(IHaveRequiredDocuments parent)
			: base(parent, JobRequiredDocument.DocUsage.Import)
		{
		}

		protected override KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
		{
			get
			{
				List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>> result = new List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>>();
				result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(JobRequiredDocumentSchema.EQ_DateReceived, Events.AllImportDocumentsReceived));
				return result.ToArray();
			}
		}
	}
}
