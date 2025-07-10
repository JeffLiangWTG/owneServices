using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyUNDGDataItem : UNDGDataItem
	{
		public AgencyUNDGDataItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AgencyUNDGDataItemValidation Validation => (AgencyUNDGDataItemValidation)base.Validation;

		protected override UNDGDataItemValidation GetNewValidation()
		{
			return new AgencyUNDGDataItemValidation(this);
		}

		protected override UNDGDataItemLookups GetNewLookups()
		{
			return new AgencyUNDGDataItemLookups(this);
		}

		[RelatedBusinessObject("UNDGSubstance")]
		[List("Lookups.UNDGSubstances")]
		public override ZGuid DI_DG
		{
			get
			{
				return base.DI_DG;
			}
			set
			{
				base.DI_DG = value;
			}
		}
	}
}
