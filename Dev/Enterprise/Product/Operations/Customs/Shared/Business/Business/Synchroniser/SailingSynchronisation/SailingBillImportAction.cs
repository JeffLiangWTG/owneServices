using CargoWise.EntityFramework;
using CargoWise.Types;
using BillOfLading = Enterprise.Freight.Agency.Business.BillOfLading;
using EnterpriseBusinessObject = Enterprise.ZArchitecture.EnterpriseBusinessObject;

namespace Enterprise.Customs.Business
{
	public enum ImportAction
	{
		Replace,
		Delete
	}

	public abstract class SailingBillImportAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected SailingBillImportAction(EnterpriseBusinessObject bill)
			: base(bill.Factory)
		{
			this.bill = bill;
			Action = IsSourceBillOfLadingValid ? ImportAction.Replace : ImportAction.Delete;
			IsSelected = true;
		}

		protected virtual bool IsSourceBillOfLadingValid
		{
			get { return BillOfLadingSource != null; }
		}

		protected BillOfLading BillOfLadingSource
		{
			get { return ((ISailingSynchronisationTarget<BillOfLading>)bill).Source; }
		}

		internal readonly EnterpriseBusinessObject bill;

		protected virtual EnterpriseBusinessObject Bill { get { return bill; } }

		public ImportAction Action { get; private set; }

		public static class Schema
		{
			public const string IsSelected = "IsSelected";
		}

		public virtual ZBool IsSelected
		{
			get { return fIsSelected; }
			set { SetNonPersistentPropertyValue(IsSelectedInfo, ref fIsSelected, value); }
		}
		ZBool fIsSelected;

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSelected); }
		}

		public ZString ActionDesc
		{
			get { return Action.ToString().ToUpperInvariant(); }
		}

		public abstract ZString BillNumber { get; }

		//public object BillOfLading { get; set; }
	}
}
