using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public class AccRegistrationNumberFeatureControlData
	{
		public List<RegistrationNumberEntry> RegistrationNumbers;
	}

	public class RegistrationNumberEntry
	{
		public string Country;
		public string Type;
		public List<NumberTuple> Numbers;
	}

	public class NumberTuple
	{
		public string Number;
		public string Description;
	}
}
