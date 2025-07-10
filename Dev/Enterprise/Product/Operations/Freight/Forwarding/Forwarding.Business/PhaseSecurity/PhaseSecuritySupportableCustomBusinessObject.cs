using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	class PhaseSecuritySupportableCustomBusinessObject : CustomBusinessObject
	{
		public PhaseSecuritySupportableCustomBusinessObject(BusinessObjectFactory factory, BusinessObject parent, ICustomPropertyCollection properties, IPhaseSecuritySupportable phaseSecuritySupportable)
			: base(factory, parent, properties)
		{
			this.phaseSecuritySupportable = Argument.NotNull(phaseSecuritySupportable, "phaseSecuritySupportable");
		}

		readonly IPhaseSecuritySupportable phaseSecuritySupportable;

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = phaseSecuritySupportable.IsPropertyReadOnlyDueToPhase(property.Name)
				|| MetaData.GetReadOnlyExcludingMethodProvider(this, property);

			if (!result)
			{
				ICustomProperty customProperty = GetCustomProperty(property.Name);
				result = customProperty.Info == null || customProperty.Info.ReadOnly;
			}

			return result;
		}
	}
}
