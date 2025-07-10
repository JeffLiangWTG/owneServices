using System;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AgencyBillContainerData),
	Constants.DocManagerCodes.AgencyBillContainers)]

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class AgencyBillContainerData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(AgencyShipmentContainer); }
		}

		protected override Type CollectionType
		{
			get { return null; }
		}

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("26174081-0d12-413f-b3ec-08fb18588697", "Shipping Manager Container"); } }

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new AgencyBillContainerEDocsViaUniversalXmlSupport();
	}
}
