using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class SummaryEntryStatusCalculator
	{
		public SummaryEntryStatusCalculator(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public ZString SummaryMessageStatus
		{
			get
			{
				ZString result = ZString.Empty;

				CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

				if (entry != null)
				{
					if (result.IsEmpty)
					{
						result = entry.CH_Status;
					}
				}

				return result;
			}
		}
	}
}
