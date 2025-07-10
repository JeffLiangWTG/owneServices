using System.Collections.Generic;
using Enterprise.Integration.LandTransport;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	public interface ILandTransportFactLoader
	{
		public IEnumerable<IInputFact> GetFacts(IDtbConsignment entity, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment);
	}
}
