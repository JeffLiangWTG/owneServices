using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Contains some helper utilities for business logic
	/// Designed so an instance of this class is contained in a business object or filter business object
	/// </summary>
	public class BusinessHelper
	{
		protected BusinessObjectFactory Factory;

		public BusinessHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		/// <summary>
		/// Gets a PK of Organisation based on Organisation's Code
		/// </summary>
		/// <param name="code">Organisation Code (OH_Code)</param>
		/// <returns>ZGuid.Empty if empty Code, PK of OrgHeader if found or ZGuid.Invalid in all other cases</returns>
		public ZGuid GetPKFromOrgCode(ZString code)
		{
			code = code.Trim();
			if (code == ZString.Empty)
			{
				return ZGuid.Empty;
			}

			OrgHeader org = (OrgHeader)Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, code);
			return (org != null) ? org.PK : ZGuid.Invalid;
		}

		/// <summary>
		/// Gets Organisation's code (OH_Code) based on OrgHeader PK
		/// </summary>
		/// <param name="orgPK">PK of an organisation</param>
		/// <returns>Value of OH_Code in OrgHeader record if OrgHeader with OrgPK exists and ZString.Empty otherwise</returns>
		public ZString GetOrgCode(ZGuid orgPK)
		{
			ZString result = ZString.Empty;
			if (orgPK.IsValid)
			{
				OrgHeader org = (OrgHeader)Factory.Load(typeof(OrgHeader), orgPK);
				if (org != null)
				{
					result = org.OH_Code;
				}
			}
			return result;
		}
	}
}
