using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class SendingMessageValidationHelper
	{
		public SendingMessageValidationHelper(CusInBondHeader header, BusinessObject bizObj, bool shouldSend = true)
		{
			this.header = header;
			this.bizObj = bizObj;
			this.shouldSend = shouldSend;
		}
		readonly CusInBondHeader header;
		readonly BusinessObject bizObj;
		readonly bool shouldSend;

		protected CusInBondMoveHeader MoveHeader => bizObj as CusInBondMoveHeader;
		protected CusInBondBill Bill => bizObj as CusInBondBill;
		protected CusInBondContainer Container => bizObj as CusInBondContainer;

		public bool ShouldValidation => (!IsArrivalValidationMode && !IsExportationValidationMode) || (shouldSend && (IsArrivalValidationMode || IsExportationValidationMode));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void CheckPropertiesWhenSendingMessage()
		{
			var moveHeaderValidation = MoveHeader?.Validation;
			var billValidation = Bill?.Validation;
			var containerValidation = Container?.Validation;

			if (IsInBondLevelArrivalValidationMode)
			{
				moveHeaderValidation?.ValidateInBondNumber();
				moveHeaderValidation?.ValidateBM_DestinationPortCode();
			}
			else if (IsBillOfLadingArrivalValidationMode)
			{
				moveHeaderValidation?.ValidateBM_FIRMS();
				moveHeaderValidation?.ValidateBM_ArrivalDate();
				moveHeaderValidation?.ValidateBM_DestinationPortCode();
				billValidation?.ValidateB0_IssuerCode();
				billValidation?.ValidateB0_MasterBillNumber();
			}
			else if (IsContainerArrivalValidationMode)
			{
				moveHeaderValidation?.ValidateInBondNumber();
				moveHeaderValidation?.ValidateBM_FIRMS();
				moveHeaderValidation?.ValidateBM_ArrivalDate();
				moveHeaderValidation?.ValidateBM_DestinationPortCode();
				billValidation?.ValidateB0_IssuerCode();
				billValidation?.ValidateB0_MasterBillNumber();
				containerValidation?.ValidateBC_ContainerNum();
			}
			else if (IsInBondLevelExportationValidationMode)
			{
				moveHeaderValidation?.ValidateInBondNumber();
			}
			else if (IsBillOfLadingExportationValidationMode)
			{
				moveHeaderValidation?.ValidateBM_ExportDate();
				moveHeaderValidation?.ValidateBM_ExportTransportMode();
				moveHeaderValidation?.ValidateBM_ExportLadenOn();
				billValidation?.ValidateB0_IssuerCode();
				billValidation?.ValidateB0_MasterBillNumber();
			}
			else if (IsContainerExportationValidationMode)
			{
				moveHeaderValidation?.ValidateInBondNumber();
				moveHeaderValidation?.ValidateBM_ExportDate();
				moveHeaderValidation?.ValidateBM_ExportTransportMode();
				moveHeaderValidation?.ValidateBM_ExportLadenOn();
				billValidation?.ValidateB0_IssuerCode();
				billValidation?.ValidateB0_MasterBillNumber();
				containerValidation?.ValidateBC_ContainerNum();
			}
		}

		#region ValidationModes
		public bool IsArrivalValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsArrivalValidationMode;
			}
		}

		public bool IsInBondLevelArrivalValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsInBondLevelArrivalValidationMode;
			}
		}

		public bool IsBillOfLadingArrivalValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsBillOfLadingArrivalValidationMode;
			}
		}

		public bool IsContainerArrivalValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsContainerArrivalValidationMode;
			}
		}

		public bool IsExportationValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsExportationValidationMode;
			}
		}

		public bool IsInBondLevelExportationValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsInBondLevelExportationValidationMode;
			}
		}

		public bool IsBillOfLadingExportationValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsBillOfLadingExportationValidationMode;
			}
		}

		public bool IsContainerExportationValidationMode
		{
			get
			{
				var header = this.header;
				return header != null && header.IsContainerExportationValidationMode;
			}
		}
		#endregion
	}
}
