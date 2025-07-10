using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgParkContainerType : AutoOrgParkContainerType
	{
		public OrgParkContainerType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.StorageClassList")]
		public override ZString PT_ContainerStorageClass
		{
			get
			{
				return base.PT_ContainerStorageClass;
			}
			set
			{
				base.PT_ContainerStorageClass = value;
			}
		}

		[List("Lookups.OrganisationContacts")]
		public override ZGuid PT_OC_CYWorkOrderApprovedBy
		{
			get { return base.PT_OC_CYWorkOrderApprovedBy; }
			set { base.PT_OC_CYWorkOrderApprovedBy = value; }
		}

		public ZString ContainerStorageClassDescription
		{
			get { return Lookups.StorageClassList.GetDescriptionFromCode(PT_ContainerStorageClass); }
		}

		public ZPropertyInfo ContainerStorageClassDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerStorageClassDescription)); }
		}

		public OrgCarrierAppointedAgentPorts ParentCarrierAgentType { get; set; }

		public override OrgAppointedAgentPorts AppointedAgentPorts
		{
			get { return ParentCarrierAgentType ?? base.AppointedAgentPorts; }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (AppointedAgentPorts != null && AppointedAgentPorts.Header != null &&
				!AppointedAgentPorts.Header.SecurityProvider.HasModifyCarrierSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
