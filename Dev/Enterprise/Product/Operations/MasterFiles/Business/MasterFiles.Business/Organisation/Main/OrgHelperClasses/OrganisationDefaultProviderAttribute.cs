using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class OrganisationDefaultProviderAttribute : Attribute
	{
		public OrganisationTypes OrganisationType { get; set; }
		public string OrganisationSubType { get; set; }
		public string DocAddressType { get; set; }
	}
}
