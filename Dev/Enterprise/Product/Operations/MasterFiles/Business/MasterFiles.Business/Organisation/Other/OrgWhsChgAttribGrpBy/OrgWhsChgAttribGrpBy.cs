using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgWhsChgAttribGrpBy : AutoOrgWhsChgAttribGrpBy
	{
		public OrgWhsChgAttribGrpBy(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public OrgCompanyData Company
		{
			get { return Factory.Load<OrgCompanyData>(PX_OB); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Company != null && Company.Header != null && !Company.Header.SecurityProvider.HasModifyWarehouseSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
