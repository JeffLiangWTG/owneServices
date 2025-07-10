using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class DocumentPickupDeliveryConfirm : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentPickupDeliveryConfirm(CommonPickupDeliveryConfirm confirm)
			: base(confirm.Factory)
		{
			this.confirm = confirm;
		}

		#region Schema

		public static class Schema
		{
			public const string PrintConfirm = "PrintConfirm";
			public const string Identifier = "Identifier";
		}

		#endregion

		#region Confirm

		public CommonPickupDeliveryConfirm Confirm
		{
			get { return confirm; }
		}
		readonly CommonPickupDeliveryConfirm confirm;

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PrintConfirm = true;
		}

		#endregion

		#region Properties

		#region PrintConfirm

		public ZBool PrintConfirm
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return printConfirm; }
			set { SetNonPersistentPropertyValue(PrintConfirmInfo, ref printConfirm, value); }
		}
		ZBool printConfirm;

		public ZPropertyInfo PrintConfirmInfo
		{
			get { return GetZPropertyInfo(Schema.PrintConfirm); }
		}

		#endregion

		#region Identifier

		public ZString Identifier
		{
			get
			{
				ZString result = "";

				if (Confirm.IsContainerised)
				{
					result = Confirm.Container.JC_ContainerNum;
				}
				else
				{
					result = Confirm.TotalDeliveredPackages + Confirm.TotalPackagesUnit + "/" +
							Confirm.TotalDeliveredWeight + " " + Confirm.TotalWeightUnit + "/" +
							Confirm.TotalDeliveredVolume + " " + Confirm.TotalVolumeUnit;
				}

				return result;
			}
		}

		public ZPropertyInfo IdentifierInfo
		{
			get { return GetZPropertyInfo(Schema.Identifier); }
		}

		#endregion

		#endregion
	}
}
