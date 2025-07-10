using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CompanySpecificOrgDataData),
	Enterprise.Core.Constants.DocManagerCodes.CompanySpecificOrg)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;

	public class CompanySpecificOrgDataData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(OrgCompanyData); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ffabf2c0-05f3-4956-97ea-571e778aeed8", "Company Specific Org. Data"); } }
	}
}
