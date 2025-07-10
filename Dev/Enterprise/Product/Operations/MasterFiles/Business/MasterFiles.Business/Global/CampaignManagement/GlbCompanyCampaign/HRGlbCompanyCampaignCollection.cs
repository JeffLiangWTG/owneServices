using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.HRGlbCompanyCampaign)]
	public class HRGlbCompanyCampaignCollection : GlbCompanyCampaignCollection
	{
		public HRGlbCompanyCampaignCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public HRGlbCompanyCampaignCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new IHRGlbCompanyCampaign AddNew()
		{
			return (IHRGlbCompanyCampaign)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return ObjectFactory.GetType<IHRGlbCompanyCampaign>();
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new GlbCompanyCampaignFindBoxListProvider(this, true); }
		}
	}
}
