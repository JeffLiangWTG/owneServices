using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CompanyTariffData),
	Enterprise.Core.Constants.DocManagerCodes.CompanyTariff)]

namespace Enterprise.Rating.Business
{
	public class CompanyTariffData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CompanyTariff); } }
		protected override Type CollectionType
		{
			get { return typeof(CompanyTariffCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CompanyTariffCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlobalRates; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a817d02c-b633-44a7-ae3b-5bc6da1c3f51", "Company Tariff"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new RatingHeaderEDocsViaUniversalXmlSupport(this, RatingConstants.RatingHeaderTypes.Tariff);
	}
}
