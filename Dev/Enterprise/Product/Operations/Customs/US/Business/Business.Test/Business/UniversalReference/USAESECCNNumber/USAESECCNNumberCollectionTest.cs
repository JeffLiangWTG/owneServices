using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USAESECCNNumberCollection))]
	sealed class USAESECCNNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(USAESECCNNumberCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USAESECCNNumberCollection(Factory, "");
		}

		void CreateNewOrGetExistingCusCodeListAndAddAttribute(UniversalReferenceTestDataHelper helper, ZString code, ZString[] attributeValues)
		{
			var refCusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, code, code, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = refCusCode.PK;
			foreach (var value in attributeValues)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, RefCusCodeListAttributeTypes.Codes.LicenseType, value);
			}
		}

		void CreateECCNNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber);

			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "A1001", new ZString[] {
				RefCusCodeListAttributeTypes.Codes.LicenseType,
			USAESLicenseCode.Codes.C30,USAESLicenseCode.Codes.C31, USAESLicenseCode.Codes.C32 });

			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "A1002", new ZString[] {
				RefCusCodeListAttributeTypes.Codes.LicenseType,
			USAESLicenseCode.Codes.C30,USAESLicenseCode.Codes.C31, USAESLicenseCode.Codes.C32 });

			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "B1001", new ZString[] {
				RefCusCodeListAttributeTypes.Codes.LicenseType,
			USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C33 });

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateECCNNumber();
		}
	}
}
