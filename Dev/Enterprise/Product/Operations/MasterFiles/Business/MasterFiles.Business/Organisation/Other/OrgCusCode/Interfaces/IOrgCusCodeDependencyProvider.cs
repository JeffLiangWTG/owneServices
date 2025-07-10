using System.Collections.Generic;
using System.Diagnostics;

namespace Enterprise.MasterFiles.Business
{
	interface IOrgCusCodeDependencyProvider
	{
		IEnumerable<OrgCusCodeDependency> GetOrgCusCodeDependencies(OrgCusCode orgCusCode);
	}

	[DebuggerDisplay("ParentCode: {ParentCode}, ErrorMessage: {ErrorMessage}")]
	public readonly struct OrgCusCodeDependency
	{
		public string ParentCode { get; }
		public string ErrorMessage { get; }

		public OrgCusCodeDependency(string parentCode, string errorMessage)
		{
			ParentCode = parentCode;
			ErrorMessage = errorMessage;
		}
	}
}
