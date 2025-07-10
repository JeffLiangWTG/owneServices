using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class EPaymentReasonCodes
	{
		public static List<EPaymentReason> GetPaymentReasonsForAllProviders()
		{
			var reasons = new List<EPaymentReason>();
			foreach (var reason in OFXReasonCodes.CodesList.ToArray())
			{
				reasons.Add(new EPaymentReason() { ProviderCode = EPaymentProviderCodes.Codes.OFX, ReasonCode = reason.Code, ReasonDescription = reason.Description });
			}
			return reasons;
		}

		public static class OFXReasonCodes
		{
			public const string AccountingServices = "ACS";
			public const string BusinessConsultancyAndPRSevices = "BCS";
			public const string EmployeePaymentSalaryWages = "EPS";
			public const string GoodsPaymentPurchase = "GPP";
			public const string HardwareConsultancyImplementation = "HCI";
			public const string SoftwareConsultancyImplementation = "SCI";
			public const string ServicesTrade = "SVT";

			public static CodeDescriptionPairList CodesList
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.Add(new CodeDescriptionPair(AccountingServices, ResString.GetMultilingualString("fa1f8587-3a9a-424b-ace3-47173ef9ab24", "Accounting services")));
					codes.Add(new CodeDescriptionPair(BusinessConsultancyAndPRSevices, ResString.GetMultilingualString("1af3f48f-1bd4-4876-9a7c-48639fa9c5f3", "Business consultancy and PR Services")));
					codes.Add(new CodeDescriptionPair(EmployeePaymentSalaryWages, ResString.GetMultilingualString("7888bb56-b0ae-47b5-91a5-b43d419a8670", "Employee payment, salary/wages")));
					codes.Add(new CodeDescriptionPair(GoodsPaymentPurchase, ResString.GetMultilingualString("7c553c49-843a-46a2-b48e-3a9895832826", "Goods payment, purchase")));
					codes.Add(new CodeDescriptionPair(HardwareConsultancyImplementation, ResString.GetMultilingualString("8ec8b56a-d0bc-4e19-a954-30e70ad18408", "Hardware consultancy/implementation")));
					codes.Add(new CodeDescriptionPair(SoftwareConsultancyImplementation, ResString.GetMultilingualString("65c1ecf7-e26e-4c9a-a069-3031e80c97b0", "Software consultancy/implementation")));
					codes.Add(new CodeDescriptionPair(ServicesTrade, ResString.GetMultilingualString("2a240fdf-401c-4343-a7e5-f354717f42cb", "Services trade")));
					return codes;
				}
			}
		}
	}
}
