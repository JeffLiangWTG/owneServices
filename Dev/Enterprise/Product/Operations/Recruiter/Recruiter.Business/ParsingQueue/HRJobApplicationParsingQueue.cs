using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationParsingQueue : AutoHRJobApplicationParsingQueue
	{
		public HRJobApplicationParsingQueue(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		[RelatedBusinessObject("Application")]
		public override ZGuid HPQ_HP
		{
			get => base.HPQ_HP;
			set => base.HPQ_HP = value;
		}

		public HRJobApplication Application => Factory.Load<HRJobApplication>(HPQ_HP);
	}
}
