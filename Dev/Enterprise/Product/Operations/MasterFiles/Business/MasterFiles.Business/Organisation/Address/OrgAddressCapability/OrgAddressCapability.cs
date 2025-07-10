using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgAddressCapability : AutoOrgAddressCapability, IOrgAddressCapability
	{
		public OrgAddressCapability(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (Address != null && Address.Header != null)
			{
				shouldBeReadOnly = !Address.Header.SecurityProvider.HasModifyAddressCapabilitiesSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Properties

		#region PZ_AddressType

		public override ZString PZ_AddressType
		{
			get
			{
				return base.PZ_AddressType;
			}
			set
			{
				if (base.PZ_AddressType != value)
				{
					base.PZ_AddressType = value;
					if (Address != null)
					{
						Address.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region PZ_IsMainAddress

		public override ZBool PZ_IsMainAddress
		{
			get
			{
				return base.PZ_IsMainAddress;
			}
			set
			{
				if (base.PZ_IsMainAddress != value)
				{
					base.PZ_IsMainAddress = value;
					if (Address != null)
					{
						Address.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
