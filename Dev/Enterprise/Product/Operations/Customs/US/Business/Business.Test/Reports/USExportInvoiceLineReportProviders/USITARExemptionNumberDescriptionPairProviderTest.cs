using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Reports.Testing
{
	[TestedType(typeof(USITARExemptionNumberDescriptionPairProvider))]
	sealed class USITARExemptionNumberDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, "123.11B", "22 CFR 123.11 (b)", startDate, endDate);
			newFactory.Save();
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusITARENCodeConstants.GetITARExemptionNumberCodeList(newFactory));
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new USITARExemptionNumberDescriptionPairProvider();
	}
}
