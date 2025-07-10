using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCarrierCode : AutoRefCarrierCode
	{
		public RefCarrierCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Collection

		[ChildEditable]
		public RefCarrierCodeAttributeCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new RefCarrierCodeAttributeCollection(this);
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}
		RefCarrierCodeAttributeCollection attributes;

		#endregion

		#region Vessels
		public RefVesselZZCollection Vessels
		{
			get
			{
				return vessels ?? (vessels = GetVesselsForThisCarrierUsingProperPivot());
			}
		}

		RefVesselZZCollection GetVesselsForThisCarrierUsingProperPivot()
		{
			var vesselCollection = new RefVesselZZCollection(Factory, FilterVesselsForSpecificCarrier);
			vesselCollection.SetReadOnlyIncludingChildren(true);
			return vesselCollection;
		}

		ZQuery FilterVesselsForSpecificCarrier
		{
			get
			{
				ZDBOnlyQuery psq = new ZDBOnlyQuery(typeof(RefVesselZZ));
				psq.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, ZZ4_ZZZ_NKDataGrouping);
				var refCarrierVesselPivotQuery = new ZDBOnlySubQuery(typeof(RefCarrierVesselPivot), RefCarrierVesselPivotSchema.ZZQ_ZZO);
				var carrierQuery = new ZDBOnlySubQuery(typeof(RefCarrierCode), RefCarrierVesselPivotSchema.ZZQ_ZZ4);
				carrierQuery.AddToFilter(RefCarrierCodeSchema.PK, PK);
				refCarrierVesselPivotQuery.AddSubQuery(carrierQuery, JoinCondition.And);
				psq.AddSubQuery(refCarrierVesselPivotQuery, JoinCondition.And);
				return psq;
			}
		}

		RefVesselZZCollection vessels;
		#endregion

		#region Override Method

		public override void Delete()
		{
			Attributes.DeleteAll();
			base.Delete();
		}

		#endregion

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZ4_ZZZ_NKDataGrouping
		{
			get { return base.ZZ4_ZZZ_NKDataGrouping; }
			set { base.ZZ4_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZ4_ZZZ_NKDataGrouping); }
		}
	}
}
