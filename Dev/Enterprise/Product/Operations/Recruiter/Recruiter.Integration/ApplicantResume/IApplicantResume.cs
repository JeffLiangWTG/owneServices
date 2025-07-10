using CargoWise.Types;

namespace Enterprise.Recruiter.Integration
{
	public interface IApplicantResume
	{
		ZString ResumeXml { get; }
		ZString Name { get; }
		ZString Gender { get; }
		ZString Nationality { get; }
		ZString EmailAddress { get; set; }
		ZString Mobile { get; set; }
		ZString HomePhone { get; }
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString City { get; }
		ZString Postcode { get; }
		ZString State { get; }
		ZString Country { get; }
		ZDateTime ReceivedTime { get; }
	}
}
