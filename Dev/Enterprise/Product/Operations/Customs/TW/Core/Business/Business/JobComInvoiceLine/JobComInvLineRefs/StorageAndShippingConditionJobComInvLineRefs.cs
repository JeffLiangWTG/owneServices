using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	[DependentBusinessObject(typeof(JobComInvoiceLine), nameof(StorageAndShippingConditionJobComInvLineRefsCollection))]
	public class StorageAndShippingConditionJobComInvLineRefs : JobComInvLineRefs
	{
		public StorageAndShippingConditionJobComInvLineRefs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new StorageAndShippingConditionJobComInvLineRefsValidation Validation => (StorageAndShippingConditionJobComInvLineRefsValidation)base.Validation;

		public new StorageAndShippingConditionJobComInvLineRefsLookups Lookups => (StorageAndShippingConditionJobComInvLineRefsLookups)base.Lookups;

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(StorageAndShippingConditionJobComInvLineRefsLookups.ReferenceTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.StorageAndShippingConditionJobComInvLineRefs.JG_ReferenceNumber", Caption = "Code", FullDescription = "The code of storage and shipping conditions.")]
		public override ZString JG_ReferenceNumber
		{
			get => base.JG_ReferenceNumber;
			set => base.JG_ReferenceNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.StorageAndShippingConditionJobComInvLineRefs.Description", Caption = "Description", ShortCaption = "Desc.")]
		public ZString Description => Lookups.ReferenceTypeList.GetDescriptionFromCode(JG_ReferenceNumber);

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JG_ReferenceType = JobComInvLineRefsType.Codes.StorageAndShippingCondition;
		}

		public override void OnSaving()
		{
			if (JG_ReferenceNumber.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		protected override JobComInvLineRefsValidation GetNewValidation() => new StorageAndShippingConditionJobComInvLineRefsValidation(this);

		protected override JobComInvLineRefsLookups GetNewLookups() => new StorageAndShippingConditionJobComInvLineRefsLookups(this);
	}
}
