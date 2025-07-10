using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

[assembly: AssemblyDataProvider(
	typeof(CusUSLVClearanceData),
	Enterprise.Core.Constants.DocManagerCodes.USLowValueEntries)]

namespace Enterprise.Customs.US.LVS.Business
{
	class CusUSLVClearanceData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusUSLVClearance);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("9272e573-df4f-4902-aaa3-af647fc3e565", "Low Value Entries");

		protected override Type CollectionType => typeof(CusUSLVClearanceAdhocEdocsSupportCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusUSLVClearanceAdhocEdocsSupportCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
