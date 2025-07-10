using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressManager
	{
		public JobDocAddressManager()
		{
			fRequirements = new ArrayList();
		}

		public JobDocAddressManager(JobDocAddressDependentCollection managedDocAddressDependentCollection)
		{
			fRequirements = new ArrayList();
			ManagedCollection = managedDocAddressDependentCollection;
		}

		readonly ArrayList fRequirements;

		public JobDocAddressRequirement[] Requirements
		{
			get { return (JobDocAddressRequirement[])fRequirements.ToArray(typeof(JobDocAddressRequirement)); }
		}

		public JobDocAddressDependentCollection ManagedCollection;

		public void AddRequirement(JobDocAddressRequirement requirement)
		{
			fRequirements.Add(requirement);
		}

		public CodeDescriptionPairList GetApplicableCodeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (JobDocAddressRequirement requirement in Requirements)
			{
				CodeDescriptionPairList theseCodePairs = requirement.GetApplicableCodeList(ManagedCollection);
				foreach (CodeDescriptionPair pair in theseCodePairs)
				{
					if (!result.ContainsCode(pair.Code))
					{
						result.AddPair(pair.Code, pair.Description);
					}
				}
			}

			return result;
		}
	}
}
