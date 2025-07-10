using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	[DependentBusinessObject(typeof(JobComInvoiceLine), "ChassisJobComInvLineRefsCollection")]
	public class ChassisJobComInvLineRefs : JobComInvLineRefs
	{
		public ChassisJobComInvLineRefs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			if (JG_ReferenceNumber.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		[MaxLength(18)]
		[ResourceStringData("Enterprise.Customs.TW.Business.ChassisJobComInvLineRefs|JG_ReferenceNumber", Caption = "Chassis No", FullDescription = "The identification number of the car at the time of manufacture.")]
		public override ZString JG_ReferenceNumber
		{
			get => base.JG_ReferenceNumber;
			set
			{
				var oldValue = JG_ReferenceNumber;
				base.JG_ReferenceNumber = value;
				if (!IsCopying && oldValue != JG_ReferenceNumber)
				{
					InvoiceLine.ChassisJobComInvLineRefsCollection.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JG_ReferenceType = JobComInvLineRefsType.Codes.Chassis;
		}

		protected override JobComInvLineRefsValidation GetNewValidation()
		{
			return new ChassisJobComInvLineRefsValidation(this);
		}

		public new ChassisJobComInvLineRefsValidation Validation => (ChassisJobComInvLineRefsValidation)base.Validation;

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;
	}
}
