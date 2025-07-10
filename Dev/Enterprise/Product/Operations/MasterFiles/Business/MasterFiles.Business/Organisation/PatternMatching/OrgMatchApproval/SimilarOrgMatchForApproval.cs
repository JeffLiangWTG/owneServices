using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SimilarOrgMatchForApproval : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string OwnerCode = "OwnerCode";
		}

		protected SimilarOrgMatchForApproval(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch) : base(matchApproval.Factory)
		{
			this.fMatchApproval = matchApproval;
			this.fOrgPatternMatch = patternMatch;
		}

		#region Constructor

		public static SimilarOrgMatchForApproval New(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch)
		{
			SimilarOrgMatchForApproval result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(matchApproval, patternMatch);
			}
			else
			{
				result = new SimilarOrgMatchForApproval(matchApproval, patternMatch);
			}
			return result;
		}

		protected delegate SimilarOrgMatchForApproval NewDelegate(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		public OrgMatchApproval MatchApproval
		{
			get { return fMatchApproval; }
		}
		readonly OrgMatchApproval fMatchApproval;

		public OrgPatternMatch OrgPatternMatch
		{
			get { return fOrgPatternMatch; }
		}
		readonly OrgPatternMatch fOrgPatternMatch;

		public OrgAddressDependentCollection Addresses
		{
			get
			{
				OrgAddressDependentCollection result = OrgPatternMatch.Header.Addresses;
				result.SetReadOnlyIncludingChildren(true);
				return result;
			}
		}

		#region New Bound Properties

		#region ClosestPortCode

		public ZString ClosestPortCode
		{
			get { return OrgPatternMatch.Header.OH_RL_NKClosestPort; }
		}
		public ZPropertyInfo ClosestPortCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ClosestPortCode)); }
		}

		#endregion

		#region MatchedByUserInitials

		public ZString MatchedByUserInitials
		{
			get
			{
				ZString result = "";
				if (OrgPatternMatch.OS_OH == MatchApproval.P2_OH_MatchOrg1)
				{
					result = MatchApproval.P2_MatchUser1;
				}
				else if (OrgPatternMatch.OS_OH == MatchApproval.P2_OH_MatchOrg2)
				{
					result = MatchApproval.P2_MatchUser2;
				}
				return result;
			}
		}

		#endregion

		#region MatchedByUserFullName

		public ZString MatchedByUserFullName
		{
			get { return (MatchedByUser == null) ? "" : (string)MatchedByUser.GS_FullName; }
		}
		public ZPropertyInfo MatchedByUserFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(MatchedByUserFullName)); }
		}

		#endregion

		#region OwnerCode

		public ZString OwnerCode
		{
			get { return OrgPatternMatch.Header.CustomsCodes.GetCustomsRegNo(OwnerCodeType, GlbCompany.CurrentCompany.Country); }
		}

		public ZPropertyInfo OwnerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OwnerCode)); }
		}

		protected virtual ZString OwnerCodeType
		{
			get { return OrgCusCode.CodeTypes.LegacySystemCode; }
		}

		#endregion

		GlbStaff MatchedByUser
		{
			get { return (GlbStaff)Factory.LoadFromNaturalKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, MatchedByUserInitials); }
		}

		#endregion
	}
}
