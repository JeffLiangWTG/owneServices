using System;

namespace Enterprise.Customs.US.AMS.Business
{
	public interface IStowPlanNotificationProvider
	{
		Guid TargetPK { get; }
		string TargetCode { get; }
		string TargetSubject { get; }
	}
}
