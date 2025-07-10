using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public static class BillNumberFormatValidation
	{
		public static void ValidateLength(ZPropertyInfo info, ZString billType)
		{
			int maxLength;
			int minLength = 4;

			switch (billType)
			{
				case BillTypeList.Codes.OceanBillOfLading:
				case BillTypeList.Codes.HouseBillOfLading:
				case BillTypeList.Codes.MasterBillOfLading:
					maxLength = 16;
					minLength = 5;
					break;
				default:
					maxLength = Customs.US.Business.Bill.Schema.CU_BillNumMaxLength;
					break;
			}

			ValidateBill(info, billType, maxLength, minLength);
		}

		public static void ValidateFormat(ZPropertyInfo info, ZString billType, BusinessObjectFactory factory)
		{
			int minLengthSCAC = 4;

			switch (billType)
			{
				case BillTypeList.Codes.OceanBillOfLading:
				case BillTypeList.Codes.HouseBillOfLading:
				case BillTypeList.Codes.MasterBillOfLading:
					var billNumber = info.Value.ToString();
					if (billNumber.Length >= minLengthSCAC)
					{
						var sCAC = billNumber.Substring(0, minLengthSCAC);
						var query = new ZQuery(USCarrierCombinedSchema.UI_Code, sCAC);
						var usCarrier = factory.LoadTop1<USCarrierCombined>(query);
						if (usCarrier == null)
						{
							info.AddMessageError(ValidationConstants.Bill.BillNumberSCACCodeValidation(new BillTypeList().GetDescriptionFromCode(billType)));
						}
					}
					break;
				default:
					break;
			}
		}

		public static void Validate(ZPropertyInfo info, ZString billType, BusinessObjectFactory factory)
		{
			ValidateLength(info, billType);
			ValidateFormat(info, billType, factory);
		}

		static void ValidateBill(ZPropertyInfo info, ZString billType, int maxLength, int minLength)
		{
			ZString billNumber = info.Value.ToString();
			if (!billNumber.IsEmpty && billNumber.Length > maxLength)
			{
				info.AddMessageError(ValidationConstants.Bill.BillNumberMaxLength(new BillTypeList().GetDescriptionFromCode(billType), maxLength));
			}

			if (!billNumber.IsEmpty && billNumber.Length < minLength)
			{
				info.AddMessageError(ValidationConstants.Bill.BillNumberMinLength(new BillTypeList().GetDescriptionFromCode(billType), minLength));
			}
		}
	}
}
