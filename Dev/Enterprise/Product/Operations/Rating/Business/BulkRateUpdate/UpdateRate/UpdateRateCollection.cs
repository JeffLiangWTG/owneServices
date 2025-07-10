using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class UpdateRateCollection : NonPersistentBusinessObjectCollection<UpdateRate>
	{
		public UpdateRateCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UpdateRate(Factory);
		}

		public void Load(ZDateTime lastRunDate)
		{
			RemoveAndDeleteAll();

			if (lastRunDate.IsValid)
			{
				var paramiters = new ZSqlParameterCollection();
				paramiters.Add(ZSqlParameter.New("@CompanyPK", GlbCompany.CurrentCompany.PK, RatingHeaderSchema.TH_GC));
				paramiters.Add(ZSqlParameter.New("@LastRunTime", lastRunDate, RateEntrySchema.TI_SystemLastEditTimeUtc));

				var collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load("EXECUTE xt_RateUpdates @CompanyPK, @LastRunTime", paramiters);

				foreach (DynamicBusinessObject org in collection)
				{
					var rate = new UpdateRate(Factory);

					using (rate.GetValidationSuspender())
					{
						rate.ClientPK = new ZGuid(org["OrgPK"]);
					}

					Add(rate);
				}
			}
		}

		public override void Load()
		{
			throw new NotSupportedException("Call Load(ZDateTime LastRunDate) instead");
		}

		public bool Contains(ZString orgCode)
		{
			foreach (UpdateRate rate in this)
			{
				if (rate.Client != null && rate.Client.OH_Code == orgCode)
				{
					return true;
				}
			}

			return false;
		}
	}
}

