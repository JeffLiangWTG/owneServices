using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Registry
{
	public interface IPhaseSecurity
	{
		bool IsEnabled { get; }
		IEnumerable<IPhase> Phases { get; }
	}

	public interface IPhase
	{
		ZString Code { get; }
		ZString Description { get; }
		IEnumerable<IPhaseRule> Rules { get; }
	}

	public interface IPhaseRule
	{
		ZGuid DepartmentPK { get; }
		ZString Location { get; }
		IEnumerable<IPhaseDependant> ReadOnlyDependants { get; }
		IEnumerable<IPhaseDependant> MandatoryDependants { get; }
		IEnumerable<IPhaseDependant> Dependants { get; }
	}

	public interface IPhaseDependant
	{
		ZString DependantType { get; }
		ZString Name { get; }
		ZString Description { get; }

		ZBool IsMandatory { get; set; }
		ZBool IsReadOnly { get; set; }
	}
}
