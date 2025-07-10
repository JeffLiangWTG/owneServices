using CargoWise.Types;
using Enterprise.Recruiter.Integration;

namespace Enterprise.Recruiter.Business
{
	public class ApplicantResume : IApplicantResume
	{
		public ZString ResumeXml { get; set; }
		public ZString Name { get; set; }
		public ZString Gender { get; set; }
		public ZString Nationality { get; set; }
		public ZString EmailAddress { get; set; }
		public ZString Mobile { get; set; }
		public ZString HomePhone { get; set; }
		public ZString Address1 { get; set; }
		public ZString Address2 { get; set; }
		public ZString City { get; set; }
		public ZString Postcode { get; set; }
		public ZString State { get; set; }
		public ZString Country { get; set; }
		public ZDateTime ReceivedTime { get; set; }
	}
}
