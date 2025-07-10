using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgServiceLevel : AutoOrgServiceLevel
	{
		public OrgServiceLevel(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return Header != null && (!Header.SecurityProvider.HasModifyOrgServiceLevelsSecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property));
		}

		#endregion

		public override RefServiceLevel ServiceLevel
		{
			get { return base.ServiceLevel ?? SrvLvl; }
		}
	}
}
