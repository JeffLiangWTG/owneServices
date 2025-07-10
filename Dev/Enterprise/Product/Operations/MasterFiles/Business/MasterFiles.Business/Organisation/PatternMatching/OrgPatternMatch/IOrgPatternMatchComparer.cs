using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class IOrgPatternMatchComparer : IEqualityComparer<IOrgPatternMatch>
	{
		public bool Equals(IOrgPatternMatch match1, IOrgPatternMatch match2)
		{
			return
				match1.OS_FullCompanyName == match2.OS_FullCompanyName &&
				match1.OS_Address1 == match2.OS_Address1 &&
				match1.OS_Address2 == match2.OS_Address2 &&
				match1.OS_Address3 == match2.OS_Address3 &&
				match1.OS_Address4 == match2.OS_Address4 &&
				match1.OS_BusinessRegNo == match2.OS_BusinessRegNo;
		}

		public int GetHashCode(IOrgPatternMatch match)
		{
			return
				match.OS_FullCompanyName.GetHashCode() ^
				match.OS_Address1.GetHashCode() ^
				match.OS_BusinessRegNo.GetHashCode();
		}
	}
}
