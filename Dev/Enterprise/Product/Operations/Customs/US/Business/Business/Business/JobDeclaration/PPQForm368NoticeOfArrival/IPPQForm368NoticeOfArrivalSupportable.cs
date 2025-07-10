using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IPPQForm368NoticeOfArrivalSupportable
	{
		ZString US_PPQForm368Box13A { get; set; }
		ZString US_PPQForm368Box13B { get; set; }
		ZString US_PPQForm368Box13C { get; set; }
		void UpdateDefaultData(PPQForm368NoticeOfArrivalData noticeOfArrivalData);
	}
}
