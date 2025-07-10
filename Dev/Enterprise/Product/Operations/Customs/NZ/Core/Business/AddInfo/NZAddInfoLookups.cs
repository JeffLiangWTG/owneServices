//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	public class NZAddInfoLookups : AutoNZAddInfoLookups
	{
		public NZAddInfoLookups(AutoNZAddInfo parent)
			: base(parent)
		{
		}

		#region OH_ClientList
		public OrgHeaderCollection OH_ClientList
		{
			get
			{
				if (fOH_ClientList == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
					filter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignee, SQLComparisonOperator.Equal, true);
					fOH_ClientList = new OrgHeaderCollection(Factory, filter);
					fOH_ClientList.SetOverrideNotificationWhenAdditionalFilterNotMet("An Organization selected from here must have 'Consignee' or 'Consignor' selected to indicate that they are a Client.");
				}
				return fOH_ClientList;
			}
		}
		protected OrgHeaderCollection fOH_ClientList;
		#endregion

		#region OH_CustomsControlledAreaList
		public OrgHeaderCollection OH_CustomsControlledAreaList
		{
			get
			{
				if (fOH_CustomsControlledAreaList == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
					fOH_CustomsControlledAreaList = new OrgHeaderCollection(Factory, filter);
					fOH_CustomsControlledAreaList.SetOverrideNotificationWhenAdditionalFilterNotMet("An Organization selected from here must have 'Services' selected to indicate that it has a Customs Controlled Area.");
				}
				return fOH_CustomsControlledAreaList;
			}
		}
		protected OrgHeaderCollection fOH_CustomsControlledAreaList;
		#endregion

		public YesNoList YesNoList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}

		public OrgHeaderCollection ManufacturersGrowersProducers
		{
			get { return new OrgHeaderCollection(Factory); }
		}
	}
}
