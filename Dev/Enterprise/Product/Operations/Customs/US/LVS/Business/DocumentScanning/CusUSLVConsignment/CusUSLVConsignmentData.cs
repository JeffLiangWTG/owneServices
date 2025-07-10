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
	typeof(CusUSLVConsignmentData),
	Enterprise.Core.Constants.DocManagerCodes.USLowValueEntriesConsignmentBill)]

namespace Enterprise.Customs.US.LVS.Business
{
	class CusUSLVConsignmentData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusUSLVConsignment);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("eed6bdd7-cd27-4fa0-ad91-6e3c4b87044d", "Consignment Bill");

		protected override Type CollectionType => typeof(CusUSLVConsignmentAdhocEdocsSupportCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusUSLVConsignmentAdhocEdocsSupportCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
