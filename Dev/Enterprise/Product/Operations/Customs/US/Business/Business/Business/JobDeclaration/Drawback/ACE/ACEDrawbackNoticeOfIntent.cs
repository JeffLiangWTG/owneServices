using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackNoticeOfIntent : IACEDrawbackNoticeOfIntent
	{
		public ACEDrawbackNoticeOfIntent(ZString recordIndicator, ZString nameOfCBPPersonnel, ZString personnelBadge, ZString personnelPhone, ZDateTime processingDate)
		{
			this.recordIndicator = recordIndicator;
			this.nameOfCBPPersonnel = nameOfCBPPersonnel;
			this.personnelBadge = personnelBadge;
			this.personnelPhone = personnelPhone;
			this.processingDate = processingDate;
		}
		readonly ZString recordIndicator;
		readonly ZString nameOfCBPPersonnel;
		readonly ZString personnelBadge;
		readonly ZString personnelPhone;
		readonly ZDateTime processingDate;

		#region IACEDrawbackNoticeOfIntent Members

		ZString IACEDrawbackNoticeOfIntent.RecordIndicator
		{
			get { return recordIndicator; }
		}

		ZString IACEDrawbackNoticeOfIntent.NameOfCBPPersonnel
		{
			get { return nameOfCBPPersonnel; }
		}

		ZString IACEDrawbackNoticeOfIntent.CBPPersonnelBadge
		{
			get { return personnelBadge; }
		}

		ZString IACEDrawbackNoticeOfIntent.CBPPersonnelPhone
		{
			get { return personnelPhone; }
		}

		ZDateTime IACEDrawbackNoticeOfIntent.ProcessingExaminAtionDate
		{
			get { return processingDate; }
		}

		#endregion
	}
}
