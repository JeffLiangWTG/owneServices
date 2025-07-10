using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactAllocation : OrgContactAttribute
	{
		public OrgContactAllocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgContactAttributeValidation GetNewValidation()
		{
			return new OrgContactAllocationValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.PC_IsAllocatedContact = true;
		}

		#region Properties

		[List("Lookups.AllocationTypes")]
		public override ZString PC_Type
		{
			get { return base.PC_Type; }
			set { base.PC_Type = value; }
		}

		#endregion

		#region AllocationDescription

		public ZString AllocationDescription
		{
			get { return Lookups.AllocationTypes.GetDescriptionFromCode(PC_Type); }
		}

		public ZPropertyInfo AllocationDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AllocationDescription)); }
		}

		#endregion
	}
}
