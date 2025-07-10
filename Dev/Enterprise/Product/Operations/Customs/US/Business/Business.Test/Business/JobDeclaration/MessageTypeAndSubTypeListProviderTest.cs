using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MessageTypeAndSubTypeListProvider))]
	public class MessageTypeAndSubTypeListProviderTest : Customs.Business.Testing.MessageTypeAndSubTypeListProviderTest
	{
		protected override string[] ExpectedMessageTypeList => new Common.US.USJobMessageTypeList().GetAllCodes();

		protected override string[] ExpectedMessageSubTypeListFor(Customs.Business.BaseJobDeclaration declaration)
		{
			string[] expectedMessageSubTypes;
			switch (declaration.JE_MessageType)
			{
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.ImportByExternalBroker:
				case JobMessageTypeList.Codes.Miscellaneous:
					expectedMessageSubTypes = EntryTypeList.GetACEList().GetAllCodes();
					break;
				case JobMessageTypeList.Codes.Drawback:
					expectedMessageSubTypes = ACEDrawbackProvisionsList.GetDrawbackProvisionList(Factory).GetAllCodes();
					break;
				default:
					expectedMessageSubTypes = System.Array.Empty<string>();
					break;
			}

			return expectedMessageSubTypes;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "64", "TFTEA 5062(C) - TFTEA Distilled spirits, wines, or beer which are unmerchantable or do not conform to sample or specifications", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();
		}
	}
}
