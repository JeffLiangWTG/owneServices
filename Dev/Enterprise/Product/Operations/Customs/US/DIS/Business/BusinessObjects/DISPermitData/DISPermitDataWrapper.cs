using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISPermitDataWrapper : IDISPermit
	{
		public DISPermitDataWrapper(DISPermitData permitData, string importerOfRecordNo)
		{
			this.permitData = permitData;
			this.importerOfRecordNo = importerOfRecordNo;
		}

		readonly DISPermitData permitData;
		readonly string importerOfRecordNo;

		ZString IDISPermit.Number
		{
			get { return permitData.PermitNumber; }
		}

		ZString IDISPermit.Type
		{
			get { return permitData.PermitType; }
		}

		ZString IDISPermit.ApprovalNumber
		{
			get { return permitData.ApprovalNumber; }
		}

		ZString IDISPermit.Statement
		{
			get { return permitData.Statement; }
		}

		ZDateTime IDISPermit.StartDate
		{
			get { return permitData.StartDate; }
		}

		ZDateTime IDISPermit.EndDate
		{
			get { return permitData.EndDate; }
		}

		ZString IDISPermit.ImporterOfRecord
		{
			get { return importerOfRecordNo; }
		}
	}
}
