using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonPrimaryRelationship : AutoGlbPersonPrimaryRelationship
	{
		public GlbPersonPrimaryRelationship(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IGlbPersonPrimarySource Primary
		{
			get
			{
				if (primary == null && PPR_PrimaryId.IsValid && !PPR_PrimaryId.IsEmpty)
				{
					switch (PPR_PrimaryTableCode)
					{
						case GlbStaffSchema.Constants.Prefix:
							primary = Factory.Load<GlbStaff>(PPR_PrimaryId);
							break;
						case OrgContactSchema.Constants.Prefix:
							primary = Factory.Load<OrgContact>(PPR_PrimaryId);
							break;
						default:
							primary = (IGlbPersonPrimarySource)Factory.Load<GlbStaff>(PPR_PrimaryId) ?? Factory.Load<OrgContact>(PPR_PrimaryId);
							break;
					}
				}

				return primary;
			}
			set
			{
				primary = value;
				PPR_PrimaryId = primary.PK;
				PPR_PrimaryTableCode = primary.TableCode;
			}
		}

		IGlbPersonPrimarySource primary;
	}
}
