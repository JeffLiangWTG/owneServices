using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PPQForm368NoticeOfArrivalData : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string US_PPQForm368Box13A = USPPQForm368DataAddInfo.Schema.US_PPQForm368Box13A;
			public const string US_PPQForm368Box13B = USPPQForm368DataAddInfo.Schema.US_PPQForm368Box13B;
			public const string US_PPQForm368Box13C = USPPQForm368DataAddInfo.Schema.US_PPQForm368Box13C;

			public const int US_PPQForm368Box13AMaxLength = USPPQForm368DataAddInfo.Schema.US_PPQForm368Box13AMaxLength;
			public const int US_PPQForm368Box13BMaxLength = USPPQForm368DataAddInfo.Schema.US_PPQForm368Box13BMaxLength;
			public const int US_PPQForm368Box13CMaxLength = USPPQForm368DataAddInfo.Schema.US_PPQForm368Box13CMaxLength;
		}

		#endregion

		public PPQForm368NoticeOfArrivalData(IPPQForm368NoticeOfArrivalSupportable noticeOfArrivalData)
		{
			this.noticeOfArrivalData = noticeOfArrivalData;
			fUS_PPQForm368Box13A = noticeOfArrivalData.US_PPQForm368Box13A;
			fUS_PPQForm368Box13B = noticeOfArrivalData.US_PPQForm368Box13B;
			fUS_PPQForm368Box13C = noticeOfArrivalData.US_PPQForm368Box13C;
		}

		readonly IPPQForm368NoticeOfArrivalSupportable noticeOfArrivalData;

		[CargoWise.ComponentModel.MaxLength(Schema.US_PPQForm368Box13AMaxLength)]
		public ZString US_PPQForm368Box13A
		{
			get { return fUS_PPQForm368Box13A; }
			set { SetNonPersistentPropertyValue(US_PPQForm368Box13AInfo, ref fUS_PPQForm368Box13A, value); }
		}
		ZString fUS_PPQForm368Box13A;

		public ZPropertyInfo US_PPQForm368Box13AInfo
		{
			get { return GetZPropertyInfo(Schema.US_PPQForm368Box13A); }
		}

		[CargoWise.ComponentModel.MaxLength(Schema.US_PPQForm368Box13BMaxLength)]
		public ZString US_PPQForm368Box13B
		{
			get { return fUS_PPQForm368Box13B; }
			set { SetNonPersistentPropertyValue(US_PPQForm368Box13BInfo, ref fUS_PPQForm368Box13B, value); }
		}
		ZString fUS_PPQForm368Box13B;

		public ZPropertyInfo US_PPQForm368Box13BInfo
		{
			get { return GetZPropertyInfo(Schema.US_PPQForm368Box13B); }
		}

		[CargoWise.ComponentModel.MaxLength(Schema.US_PPQForm368Box13CMaxLength)]
		public ZString US_PPQForm368Box13C
		{
			get { return fUS_PPQForm368Box13C; }
			set { SetNonPersistentPropertyValue(US_PPQForm368Box13CInfo, ref fUS_PPQForm368Box13C, value); }
		}
		ZString fUS_PPQForm368Box13C;

		public ZPropertyInfo US_PPQForm368Box13CInfo
		{
			get { return GetZPropertyInfo(Schema.US_PPQForm368Box13C); }
		}

		public void LoadDefault()
		{
			noticeOfArrivalData.UpdateDefaultData(this);
		}

		public void SaveData()
		{
			noticeOfArrivalData.US_PPQForm368Box13A = US_PPQForm368Box13A;
			noticeOfArrivalData.US_PPQForm368Box13B = US_PPQForm368Box13B;
			noticeOfArrivalData.US_PPQForm368Box13C = US_PPQForm368Box13C;
		}
	}
}
