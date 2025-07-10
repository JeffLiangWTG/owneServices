using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgUserFlag : NonPersistentBusinessObject
	{
		public OrgUserFlag(OrgHeader header, OrgUserFlagType orgUserFlagType)
		{
			Argument.NotNull(header, "header");

			this.header = header;
			this.orgUserFlagType = orgUserFlagType;
			this.orgHeaderColumnPropertyInfo = (ZPropertyInfoBool)header.ZPropertyInfoHash[orgUserFlagType.OrgHeaderColumn.Name];
		}

		readonly OrgHeader header;
		readonly OrgUserFlagType orgUserFlagType;
		readonly ZPropertyInfoBool orgHeaderColumnPropertyInfo;

		#region Properties

		#region FlagNumber

		public ZString FlagNumber
		{
			get { return orgUserFlagType.FlagNumber; }
		}

		#endregion

		#region IsSelected

		public ZBool IsSelected
		{
			get { return orgHeaderColumnPropertyInfo.Value; }
			set
			{
				orgHeaderColumnPropertyInfo.Value = value;
			}
		}

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsSelected), (x) => orgHeaderColumnPropertyInfo); }
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return orgUserFlagType.Label; }
		}

		#endregion

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !HasModifySalesClientSummarySecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		bool HasModifySalesClientSummarySecurity
		{
			get { return header.SecurityProvider.HasModifySalesClientSummarySecurity; }
		}

		#endregion
	}
}
