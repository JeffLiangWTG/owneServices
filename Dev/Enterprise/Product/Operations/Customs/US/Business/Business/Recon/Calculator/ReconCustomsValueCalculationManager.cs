using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconCustomsValueCalculationManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ReconCustomsValueCalculationManager(ReconDeclaration reconDeclaration)
			: base(reconDeclaration.Factory)
		{
			this.reconDeclaration = reconDeclaration;
		}

		readonly ReconDeclaration reconDeclaration;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RecalculationType = ReconCustomsValueRecalculationTypeList.Codes.INC;
		}

		public void CalculateCustomsValuesForAllEntryLines()
		{
			var invoiceLines = reconDeclaration.InvoiceLines;
			foreach (JobComInvoiceLine line in invoiceLines)
			{
				if (RecalculationType == ReconCustomsValueRecalculationTypeList.Codes.INC)
				{
					line.JI_LinePrice = line.US_R_OrigCV * (1 + RecalculationPercentage / 100);
				}
				else if (RecalculationType == ReconCustomsValueRecalculationTypeList.Codes.DCR)
				{
					line.JI_LinePrice = line.US_R_OrigCV * (1 - RecalculationPercentage / 100);
				}
				line.JI_LinePriceInfo.RefreshBinding();
			}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			reconDeclaration.Logs.AddNew(Events.EditedARecord, "Recon Customs Values " + RecalculationDesc + " " + RecalculationPercentage + "%");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.RecalculationTypeList))]
		[MaxLength(3)]
		public ZString RecalculationType
		{
			get { return recalculationType; }
			set
			{
				SetNonPersistentPropertyValue(RecalculationTypeInfo, ref recalculationType, value);
				ValidateRecalculationType();
			}
		}
		ZString recalculationType;

		public ZString RecalculationDesc
		{
			get { return Lookups.RecalculationTypeList.GetDescriptionFromCode(RecalculationType); }
		}

		public ZPropertyInfo RecalculationTypeInfo
		{
			get { return GetZPropertyInfo(nameof(RecalculationType)); }
		}

		public void ValidateRecalculationType()
		{
			RecalculationTypeInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				ListValidation.ErrorIfInvalidCode(RecalculationTypeInfo, Lookups.RecalculationTypeList);

				if (RecalculationType.IsEmpty)
				{
					RecalculationTypeInfo.AddError(EmptyCalculationType);
				}
			}
		}
		internal const string EmptyCalculationType = "Please enter a Calculation Type.";

		public ZDecimal RecalculationPercentage
		{
			get { return recalculationPercentage; }
			set
			{
				SetNonPersistentPropertyValue(RecalculationPercentageInfo, ref recalculationPercentage, value);
				ValicateCalculationPercentage();
			}
		}
		ZDecimal recalculationPercentage;

		public ZPropertyInfo RecalculationPercentageInfo
		{
			get { return GetZPropertyInfo(nameof(RecalculationPercentage)); }
		}

		public void ValicateCalculationPercentage()
		{
			RecalculationPercentageInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (RecalculationPercentage <= 0)
				{
					RecalculationPercentageInfo.AddError(EnterNumberGreaterThanZero);
				}
			}
		}
		internal const string EnterNumberGreaterThanZero = "Please enter a number greater than 0.";

		public void ValidateAll()
		{
			ValidateRecalculationType();
			ValicateCalculationPercentage();
		}

		public ReconDeclarationLookups Lookups
		{
			get { return reconDeclaration.Lookups; }
		}
	}
}
