using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class OrgSupplierPartFilterLookups
	{
		public OrgSupplierPartFilterLookups(OrgSupplierPartFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}
		readonly OrgSupplierPartFilterStripBusinessObject filterBizObj;

		public USCTariffCollection Tariffs => new USCTariffCollection(Factory);

		public CodeDescriptionPairList TariffTypeList => Factory.GetCachedValue<ClassificationTypeList>();

		BusinessObjectFactory Factory => filterBizObj.Factory;
	}
}
