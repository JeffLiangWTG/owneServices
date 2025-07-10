using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbCompanyCampaign)]
	public class GlbCompanyCampaignCollection : BusinessObjectCollection<BusinessObject>
	{
		public GlbCompanyCampaignCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbCompanyCampaignCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new IGlbCompanyCampaign this[int index]
		{
			get { return (IGlbCompanyCampaign)Elements[index]; }
		}

		public new IGlbCompanyCampaign AddNew()
		{
			return (IGlbCompanyCampaign)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return ObjectFactory.GetType<IGlbCompanyCampaign>();
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new GlbCompanyCampaignFindBoxListProvider(this); }
		}
	}
}
