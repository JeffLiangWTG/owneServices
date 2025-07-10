using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent)
			: base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGoodsType();
			ValidateGSTPaid();
		}

		public void ValidateGoodsType()
		{
			ValidateCalculatedProperty(Parent.GoodsTypeInfo);
		}

		protected void CheckGoodsType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.GoodsTypeInfo);
			var parent = Parent;
			var type = parent.GoodsType;
			var tariff = parent.Tariff;
			var effectiveDate = parent.EffectiveDateForDutyRate;
			var tariffControlledType = (tariff != null) && ((tariff.IsUnderExportControl(effectiveDate)) || (tariff.IsUnderImportControl(effectiveDate)));
			var isMGI = IsMGI;
			var dutyAmount = parent.API_DutyAmount;
			if (!parent.API_Tariff.IsEmpty && (!isMGI || dutyAmount.IsEmpty) && tariffControlledType)
			{
				if (type != Constants.GoodsType.ControlledGoods)
				{
					parent.GoodsTypeInfo.AddMessageError(@"'CT' must be used when the Tariff is not Blank and is a controlled type.");
				}
			}

			if (isMGI)
			{
				if (IsMajorExporter(parent.Consignee) && (tariff != null) && !tariff.IsUnderImportControl(effectiveDate))
				{
					if (type != Constants.GoodsType.MajorExporter)
					{
						parent.GoodsTypeInfo.AddMessageError(@"'ME' must be used when the Tariff is not Blank and is a controlled type.");
					}
				}
				else if (!dutyAmount.IsEmpty)
				{
					if (type != Constants.GoodsType.DutiableGoods)
					{
						parent.GoodsTypeInfo.AddMessageError(@"'DT' must be used when the Duty amount is not 0");
					}
				}
			}
		}

		bool IsMajorExporter(OrgHeader org)
		{
			return org != null && org.CustomsCodes.OfType<OrgCusCode>().Any(x => x.OK_CustomsRegNo == SGPartyStatusList.Codes.Y && x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Singapore && x.OK_CodeType == OrgCusCode.SingaporeCodeTypes.PartyStatusType);
		}

		public void ValidateGSTPaid()
		{
			ValidateCalculatedProperty(Parent.GSTPaidInfo);
		}

		protected void CheckGSTPaid()
		{
			var parent = Parent;

			if (parent.Header?.IsOVRApplicable ?? false)
			{
				var bill = parent.Bill;

				if (!string.IsNullOrWhiteSpace(parent.GSTPaid))
				{
					ListValidation.MessageErrorIfInvalidCode(parent.GSTPaidInfo);

					var deminimus = GetDeminimus(parent.Factory).Round(2);
					if (bill.ABL_CustomsValue <= deminimus)
					{
						if (parent.Header.IsAir)
						{
							parent.GSTPaidInfo.AddMessageError(Res.GetString("B7047582-6AF4-4356-A4C3-72FBF110399F", "The GST Paid indicator must be blank when the Bill Customs Value is less than or equal to ${0} and the Transport Mode is Air Freight.", deminimus));
						}
						else if (parent.Header.IsRoad && bill.GSTNReferenceNo.IsEmpty)
						{
							parent.GSTPaidInfo.AddMessageError(Res.GetString("A232828C-0A95-4CE4-917C-5625FD326AAD", "The GST Paid indicator must be blank when the Bill Customs Value is less than or equal to ${0} and the Transport Mode is Road Freight and the GSTN Reference, (on the Bill Details), is blank.", deminimus));
						}
					}
					else if (bill.GSTNReferenceNo.IsEmpty)
					{
						parent.GSTPaidInfo.AddMessageError(Res.GetString("780748A7-BB22-449C-84EE-72484EC98DE0", "The GST Paid indicator must be blank when the Bill Customs Value is more than ${0} and the GSTN Reference, (on the Bill Details), is blank.", deminimus));
					}
				}
				else if (!bill.GSTNReferenceNo.IsEmpty)
				{
					parent.GSTPaidInfo.AddMessageError(ValidationConstants.Bill.GSTPaidFlagMustNotBeBlank);
				}
			}
		}

		static ZDecimal GetDeminimus(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("SG.UniversalReferenceHelper.Deminimus", () =>
			{
				return new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Singapore, RateTypes.Deminimus, ZDateTime.Today)?.ZZF_Value ?? ZDecimal.Zero;
			});
		}

		protected override void CheckAPI_RN_NKGoodsOrigin()
		{
			base.CheckAPI_RN_NKGoodsOrigin();
			if (Parent.API_RN_NKGoodsOrigin.IsEmpty)
			{
				Parent.API_RN_NKGoodsOriginInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.FieldIsMandatory("Goods Origin is a required field", "Singapore"));
			}
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			if (Parent.API_GoodsDescription.IsEmpty)
			{
				Parent.API_GoodsDescriptionInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.FieldIsMandatory("Goods Description is a required field", "Singapore"));
			}
		}

		protected override void CheckAPI_CustomsQty()
		{
			base.CheckAPI_CustomsQty();
			if (Parent.API_CustomsQty.IsEmpty)
			{
				Parent.API_CustomsQtyInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.FieldIsMandatory("Customs Qty is a required field", "Singapore"));
			}
		}

		protected override void CheckAPI_CustomsUQ()
		{
			base.CheckAPI_CustomsUQ();
			if (Parent.API_CustomsUQ.IsEmpty)
			{
				Parent.API_CustomsUQInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.API_CustomsUQInfo.HumanReadableName));
			}
		}

		protected override void CheckAPI_TaxAmount()
		{
			base.CheckAPI_TaxAmount();

			var parent = Parent;
			if (parent.API_TaxAmount.IsEmpty)
			{
				var customsValue = parent.API_CustomsValue;
				if ((customsValue <= 0 || customsValue >= Constants.Deminimis))
				{
					Parent.API_TaxAmountInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.FieldIsMandatory("Tax Amount is a required field", "Singapore"));
				}
			}
		}

		protected override void CheckAPI_DutyAmount()
		{
			base.CheckAPI_DutyAmount();
			var parent = Parent;
			if (parent.API_DutyAmount.IsEmpty
				&& parent.API_CustomsValue >= Constants.Deminimis
				&& (parent.Pack?.Bill?.IsImport ?? false)
				&& (parent.Tariff?.IsDutiableType() ?? false))
			{
				Parent.API_DutyAmountInfo.AddMessageError("Duty is applicable and cannot be calculated. Please supply the duty amount");
			}
		}

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_TariffInfo);
		}

		#region Implementation

		bool IsMGI => Parent.Header?.IsImport ?? false;

		#endregion
	}
}
