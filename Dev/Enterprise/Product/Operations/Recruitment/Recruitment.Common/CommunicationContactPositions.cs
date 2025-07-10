using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruitment.Common
{
	public static class CommunicationContactPositions
	{
		public static CodeDescriptionPair Candidate => new CodeDescriptionPair("CAN", Res.GetString("b571f311-77b2-4d62-8228-24c7aac6cf33", "Candidate"));
		public static CodeDescriptionPair Recruiter => new CodeDescriptionPair("REC", Res.GetString("1dccdeff-2ffc-45a6-a67d-71c9a3d5677c", "Recruiter"));
		public static CodeDescriptionPair Manager => new CodeDescriptionPair("MAN", Res.GetString("504911f2-62c2-461d-8fb5-9815e2043a24", "Manager"));
		public static CodeDescriptionPair InterestedParty => new CodeDescriptionPair("INT", Res.GetString("752e7932-5d78-405b-8bd1-bf36a9f23fba", "Interested Party"));
		public static CodeDescriptionPair SeniorDeveloper => new CodeDescriptionPair("DEV", Res.GetString("96820af1-7dc7-4f4b-a5ea-704e441da8ef", "Senior Developer"));
		public static CodeDescriptionPair TeamLead => new CodeDescriptionPair("TML", Res.GetString("4298e345-2c33-468b-9a8d-50eb8120cc1a", "Team Lead"));
		public static CodeDescriptionPair OtherPosition => new CodeDescriptionPair("OTH", Res.GetString("d4bb70f6-973c-45a0-9c63-cb06a595503e", "Other Position"));
		public static CodeDescriptionPair Reference => new CodeDescriptionPair("REF", Res.GetString("b32547b1-5e54-49c6-b607-6d229692b6c2", "Reference"));
	}
}
