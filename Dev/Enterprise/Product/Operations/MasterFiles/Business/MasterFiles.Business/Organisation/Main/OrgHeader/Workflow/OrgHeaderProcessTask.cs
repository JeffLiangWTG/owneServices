using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderProcessTask : ProcessTasks
	{
		public OrgHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Organisation; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(OrgHeader); }
		}

		public new OrgHeader Parent
		{
			get { return (OrgHeader)base.Parent; }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return base.GetShouldPropertiesBeReadOnly(property)
					|| Parent == null
					|| !Parent.SecurityProvider.HasModifyDetailsSecurity
					|| (GetType() == property.ComponentType && MetaData.GetReadOnlyExcludingMethodProvider(this, property));
		}

		protected internal sealed override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return GetReadOnlySecurity(property);
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || Parent == null || !Parent.SecurityProvider.HasModifyDetailsSecurity; }
			set { base.ReadOnly = value; }
		}

		#endregion
	}
}
