using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentCusEntryNumberProxy : ForwardingShipmentCustomsEntryNumber, ICanDelete
	{
		public ForwardingShipmentCusEntryNumberProxy(ForwardingShipment shipment, CusEntryNumber cusEntryNumber)
			: base(shipment)
		{
			Argument.NotNull(cusEntryNumber, "cusEntryNumber");
			CusEntryNumber = cusEntryNumber;
		}

		public CusEntryNumber CusEntryNumber { get; set; }

		protected override CusEntryNumber GetCusEntryNumber()
		{
			return CusEntryNumber;
		}

		protected override ZString GetEntryType()
		{
			return CusEntryNumber.GetEntryNumberTypeForDisplay(CusEntryNumber.CE_EntryType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Shipment.IsImport());
		}

		protected override bool EntryType_ReadOnly
		{
			get
			{
				return CusEntryNumber.CE_EntryIsSystemGenerated
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland
					|| (!CusEntryNumber.CE_ParentTable.IsEmpty && CusEntryNumber.CE_ParentTable != JobShipmentSchema.Constants.TableName);
			}
		}

		protected override ZString GetEntryNumber()
		{
			return CusEntryNumber.CE_EntryNum;
		}

		protected override bool EntryNumber_ReadOnly
		{
			get { return EntryType_ReadOnly || IsExemptionCode(EntryType); }
		}

		protected override ZDateTime GetIssueDate()
		{
			return CusEntryNumber.CE_IssueDate;
		}

		protected override bool IssueDate_ReadOnly
		{
			get { return EntryNumber_ReadOnly; }
		}

		protected override ZDateTime GetExpiryDate()
		{
			return CusEntryNumber.CE_ExpiryDate;
		}

		protected override bool ExpiryDate_ReadOnly
		{
			get { return EntryNumber_ReadOnly; }
		}

		public bool AllowEmptyNumber
		{
			get { return !IsNumberEmpty(EntryType, EntryNumber) || IsExemptionCode(EntryType); }
		}

		protected override bool HandleEmptyNumber(CusEntryNumber entryNumber)
		{
			return false;
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || !IsParentShipment; }
		}

		internal bool IsParentShipment
		{
			get { return CusEntryNumber.CE_ParentTable == ForwardingShipment.Schema.TableName; }
		}

		bool ICanDelete.CanDelete
		{
			get { return IsParentShipment; }
		}

		void ICanDelete.OnCannotDelete()
		{
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("2eb21c87-e6d5-4d3e-93db-27e0acc318d7", "Cannot delete non-shipment customs entries"); }
		}

		protected override ShipmentCustomsEntryNumberValidation GetNewValidation()
		{
			return new ForwardingShipmentCusEntryNumberProxyValidation(this);
		}
	}
}
