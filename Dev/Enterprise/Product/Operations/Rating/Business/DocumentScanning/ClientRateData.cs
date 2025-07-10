using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ClientRateData),
	Enterprise.Core.Constants.DocManagerCodes.ClientRate)]

namespace Enterprise.Rating.Business
{
	public class ClientRateData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ClientRate); } }
		protected override Type CollectionType
		{
			get { return typeof(RateCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RateCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ClientRates; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a091bc56-ecd4-4362-bc5b-31ee4a2a8638", "Client Rate"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new RatingHeaderEDocsViaUniversalXmlSupport(this, RatingConstants.RatingHeaderTypes.ClientRate);
	}
}
