using CargoWise.EntityFramework;
using CargoWise.Types;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusDecHouseContainerPackValidation
//
//    This class should be used for overriding validation in AutoCusDecHouseContainerPackValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusDecHouseContainerPackValidation : AutoCusDecHouseContainerPackValidation
	{
		public CusDecHouseContainerPackValidation(AutoCusDecHouseContainerPack parent) : base(parent)
		{
		}

		public BasePackage Package
		{
			get { return (BasePackage)Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCW_ContainerNoOrEquipmentNo();
			ValidateCW_HouseBill();
		}

		#region Validation for CW_HouseBill
		public void ValidateCW_HouseBill()
		{
			ValidateCalculatedProperty(Package.CW_HouseBillInfo);
		}

		public static string SelectAValidHouseBill
		{
			get { return Res.GetString("d18f0cdf-2c80-4fda-b413-dedf708e065e", "Packing Line requires a valid Bill - please select a valid a bill for this packing line."); }
		}

		protected virtual void CheckCW_HouseBill()
		{
			if (Package.Declaration != null)
			{
				if (Package.Bill == null)
				{
					Package.CW_HouseBillInfo.AddError(SelectAValidHouseBill);
				}
			}
		}
		#endregion

		#region Validation for CW_ContainerNoOrEquipmentNo
		public void ValidateCW_ContainerNoOrEquipmentNo()
		{
			ValidateCalculatedProperty(Package.CW_ContainerNoOrEquipmentNoInfo);
		}

		protected virtual void CheckCW_ContainerNoOrEquipmentNo()
		{
			ListValidation.ErrorIfInvalidCode(Package.CW_ContainerNoOrEquipmentNoInfo);
		}
		#endregion

		protected override void CheckCW_CR_HouseContainerIsNotEmpty()
		{
			// don't do this, we validate through the CW_HouseBill instead
		}

		#region Validation for CW_PackQty

		protected override void CheckCW_PackQty()
		{
			base.CheckCW_PackQty();
			CheckTotalNumberOfPacksExceedsCW_PackQty();
		}

		protected virtual void CheckTotalNumberOfPacksExceedsCW_PackQty()
		{
			var package = Package;
			var declaration = package.Declaration;

			if (package.IsLowestPackage
				&& declaration != null
				&& (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking || declaration.SupportsChcPivotBetweenInvoiceLineAndPacking))
			{
				var totalNumbers = package.TotalUsageCount;

				if (totalNumbers > package.CW_PackQty)
				{
					var notificationType = declaration.ErrorTypeForCWPackQtyExceededError == CargoWise.EntityFramework.NotificationTypes.Warning
						? CargoWise.ComponentModel.NotificationType.Warning
						: CargoWise.EntityFramework.NotificationType.MessageError;

					var messsage = TotalNumberOfPacksExceedsCW_PackQtyMessage(totalNumbers);

					package.CW_PackQtyInfo.AddNotification(notificationType, messsage);
				}
			}
		}

		protected virtual ZString TotalNumberOfPacksExceedsCW_PackQtyMessage(ZInt totalNumbers) => Res.GetString("7f505069-2c3c-492e-89e7-b45ac426ee99",
						"The total number of packs included in invoice(s) and invoice line(s) is {0}, it exceeds this package quantity.", totalNumbers);

		#endregion

		protected override void CheckCW_MarksAndNos()
		{
			base.CheckCW_MarksAndNos();
			var validationMessage = Package?.Declaration?.PackageMarksAndNumbersAlwaysRequiredValidationMessage ?? ZString.Empty;
			if (!validationMessage.IsEmpty && Package.CW_MarksAndNos.IsEmpty)
			{
				Package.CW_MarksAndNosInfo.AddMessageError(validationMessage);
			}
		}
	}
}
