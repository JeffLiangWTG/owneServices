using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ConsolidatedDeclarationData),
	Enterprise.Core.Constants.DocManagerCodes.ConsolidatedDeclaration)]

namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;

	public class ConsolidatedDeclarationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ConsolidatedDeclaration); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("14C6CB62-56E2-4DE0-97C9-31E5C3DF8263", "Consolidated Declaration"); } }
	}
}
