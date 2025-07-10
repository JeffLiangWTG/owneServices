using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgContactAttribute : AutoOrgContactAttribute
	{
		public OrgContactAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.AttributeTypes")]
		public override ZString PC_Type
		{
			get
			{
				return base.PC_Type;
			}
			set
			{
				base.PC_Type = value;
				if (PC_URL_ReadOnly)
				{
					PC_URL = ZString.Empty;
				}
			}
		}

		#region AttributeDescription

		public ZString AttributeDescription
		{
			get { return Lookups.AttributeTypes.GetDescriptionFromCode(PC_Type); }
		}

		public ZPropertyInfo AttributeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeDescription)); }
		}

		#endregion

		public bool PC_URL_ReadOnly
		{
			get { return !PC_Type.Equals("SNL"); }
		}

		#endregion

		#region Logging

		protected override ZString CustomLogReferenceSuffix
		{
			get { return PC_Type + (Contact != null ? (NoResString)" for contact " + Contact.OC_ContactName : ""); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Contact != null && Contact.Header != null && !Contact.Header.SecurityProvider.HasModifyContactSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
