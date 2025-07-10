using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module
{
	public class EntryLineFilterLookups
	{
		public EntryLineFilterLookups(EntryLineFilterBusinessObject filterBizObj)
		{
			this.FilterBizObj = filterBizObj;
		}

		protected readonly EntryLineFilterBusinessObject FilterBizObj;

		protected BusinessObjectFactory Factory => FilterBizObj.Factory;

		public OrgHeaderCollection Consignees => new ConsigneeCollection(Factory);

		public MasterFiles.Business.OrgSupplierPartCollection PartsList
		{
			get
			{
				MasterFiles.Business.OrgSupplierPartCollection result = new MasterFiles.Business.OrgSupplierPartCollection(Factory);
				if (Importer != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", Importer.PK));
				}
				return result;
			}
		}

		public OrgHeader Importer
		{
			get { return fImporter; }
			set { fImporter = value; }
		}
		OrgHeader fImporter;

		public virtual BusinessObjectCollection ClassificationList => new BaseClassificationCollection<BaseCusClassification>(Factory);

		public RefCountryCollection CountryList => new RefCountryCollection(Factory);
	}
}
