using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MawbParentForTest : DummyEnterpriseBusinessObject, IMAWBParent, IMAWBAllocationParent
	{
		public MawbParentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "MawbParentForTest"; }
		}

		public override string TablePrefix => "JS";

		public ZString AWBServiceLevel { get; set; }

		public ZString MasterBill { get; set; }

		public ZString MasterBillAirlinePrefix
		{
			get { return Z0_Code; }
			set { Z0_Code = value; }
		}

		public ZString MasterBillMAWB
		{
			get { return Z0_Description; }
			set { Z0_Description = value; }
		}

		public ZString MawbBookingReference { get; set; }

		public ZString TwoLetterAirlineCode { get; set; }

		public ZString MawbPortOfLoading { get; set; }

		public ZString MawbPortOfDischarge { get; set; }

		public ZBool IsNeutralMaster { get; set; }

		ZGuid IMAWBParent.PK
		{
			get { return PK; }
		}

		IMAWBAllocationParent IMAWBParent.MAWBAllocationParent
		{
			get { return this; }
		}

		BusinessObjectFactory IMAWBAllocationParent.Factory
		{
			get { return Factory; }
		}

		ZGuid IMAWBAllocationParent.PK
		{
			get { return PK; }
		}

		ZString IMAWBAllocationParent.Prefix
		{
			get { return Schema.TablePrefix; }
		}

		MAWBAllocation IMAWBAllocationParent.MAWBAllocation
		{
			get { return MAWBAllocation; }
		}

		ZPropertyInfo IMAWBAllocationParent.MasterBillMAWBInfo
		{
			get { return new ZPropertyInfoString(this, "MasterBillMAWB"); }
		}

		bool IMAWBAllocationParent.IsAir
		{
			get { return true; }
		}

		ZBool IMAWBAllocationParent.IsValidForNeutralMaster
		{
			get { return true; }
		}

		ZArchitecture.Business.IStmNoteParent IMAWBAllocationParent.NotesParent
		{
			get { return this; }
		}

		ZString IMAWBAllocationParent.MasterBillNeutralMAWB
		{
			get
			{
				return MAWBAllocation.AllocatedMawb == null ?
new ZString("Pending Allocation...")
: ZString.Format("{0}-{1}", MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix, MAWBAllocation.AllocatedMawb.JM_MAWB);
			}
		}

		ZPropertyInfo IMAWBAllocationParent.MasterBillNeutralMAWBInfo { get { return new ZPropertyInfoString(this, "MasterBillNeutralMAWB"); } }

		public MAWBAllocation MAWBAllocation
		{
			get { return mawbAllocation ?? (mawbAllocation = new MAWBAllocation(this)); }
		}
		MAWBAllocation mawbAllocation;
	}
}
