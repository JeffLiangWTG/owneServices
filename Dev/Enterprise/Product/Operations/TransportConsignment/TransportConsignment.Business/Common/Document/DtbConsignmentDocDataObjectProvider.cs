namespace Enterprise.TransportConsignment.Business
{
	sealed public class DtbConsignmentDocDataObjectProvider : IDtbConsignmentDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext)
		{
			if (parent is DtbConsignment dtbConsignment && dataContext == DataContext.CMRConsignmentNote)
			{
				return new DtbConsignmentCMRConsignmentNoteBuilder(dtbConsignment).Build();
			}

			if (parent is DtbConsignmentRunSheet runSheet && dataContext == DataContext.CMRConsignmentNote)
			{
				return new DtbConsignmentRunSheetCMRConsignmentNoteBuilder(runSheet).Build();
			}

			return null;
		}
	}
}
