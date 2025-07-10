using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	[DependentBusinessObject(typeof(GateBooking), "GateBookingDetails")]
	public class GateBookingDetail : AutoGateBookingDetail, IUNDGDataItemProvider
	{
		public GateBookingDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region YardUnit

		[RelatedBusinessObject(nameof(YardUnit))]
		public override ZGuid GTD_GTY_YardUnit
		{
			get { return base.GTD_GTY_YardUnit; }
			set { base.GTD_GTY_YardUnit = value; }
		}

		public YardUnit YardUnit
		{
			get { return Factory.Load<YardUnit>(GTD_GTY_YardUnit); }
		}

		#endregion

		#region Facility

		[RelatedBusinessObject(nameof(Facility))]
		public override ZGuid GTD_WW_Facility
		{
			get => base.GTD_WW_Facility;
			set => base.GTD_WW_Facility = value;
		}

		public WhsWarehouse Facility => Factory.Load<WhsWarehouse>(GTD_WW_Facility);

		#endregion

		#region GateBooking

		public GateBooking GateBooking => Factory.Load<GateBooking>(GTD_GTB_GateBooking);

		#endregion

		#region GTD_VolumeUQ

		[List("Lookups.GTD_VolumeUQ_List")]
		public override ZString GTD_VolumeUQ
		{
			get => base.GTD_VolumeUQ;
			set => base.GTD_VolumeUQ = value;
		}

		#endregion

		#region GTD_WeightUQ

		[List("Lookups.GTD_WeightUQ_List")]
		public override ZString GTD_WeightUQ
		{
			get => base.GTD_WeightUQ;
			set => base.GTD_WeightUQ = value;
		}

		#endregion

		#region GTD_Purpose

		[List("Lookups.FacilityJobType_List")]
		public override ZString GTD_Purpose
		{
			get => base.GTD_Purpose;
			set => base.GTD_Purpose = value;
		}

		#endregion

		#region AdditionalReferenceNumbers

		[ChildEditable(true)]
		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					additionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					additionalReferenceNumbers.Load();
					RegisterEditableChildObject(additionalReferenceNumbers);
				}
				return additionalReferenceNumbers;
			}
		}
		CusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		#endregion

		#region UNDGs

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (uNDGs == null)
				{
					uNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(uNDGs);
				}
				return uNDGs;
			}
		}

		UNDGDataItemCollection uNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		#endregion
	}
}
