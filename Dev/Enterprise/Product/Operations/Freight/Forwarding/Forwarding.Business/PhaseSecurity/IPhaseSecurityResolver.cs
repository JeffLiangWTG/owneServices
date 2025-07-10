using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IPhaseSecurityResolver
	{
		CodeDescriptionPairList GetPhaseList();
		bool IsEditingAllowed { get; }
		bool IsPropertyMandatory(ZString propertyName);
		bool IsPropertyReadOnly(ZString propertyName);
		bool IsChildPropertyReadOnly(ZGuid childPK, ZString childPropertyName);
		bool IsChildPropertyMandatory(ZGuid childPK, ZString childPropertyName);
		void RegisterEditableChild(IBusiness child, ZString childName);
		IEnumerable<ZPropertyInfo> InitialisePhaseDependantMandatoryValidation(IEnumerable<ZPropertyInfo> propertyInfos, ZGuid? childPK = null);
	}
}
