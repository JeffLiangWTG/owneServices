using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using WTG.NUnit;
using CusEntryNumber = Enterprise.Customs.TW.Business.CusEntryNumber;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TW.DataTransfer.Universal.Testing
{
	public sealed class CustomsEntryNumberDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestPopulateCustomsEntryNumberData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			var entryNumberBO = SetupCusEntryNumber(Factory.BOFactory);
			entryNumberBO.Parent = header;
			var writer = new CustomsEntryNumberDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryNumberBO)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.Taiwan));
			var entryNumberDataObject = writer.GetDataObject(entryNumberBO);
			AssertContents(entryNumberDataObject);
		}

		CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory, ZString entryNum, ZString entryType, ZBool entryIsSystemGenerated)
		{
			return SetupCusEntryNumber(factory.New<CusEntryNumber>(), entryNum, entryType, entryIsSystemGenerated, Core.Constants.CountryCodes.Taiwan, new ZDateTime(2021, 8, 9), "C1");
		}

		CusEntryNumber SetupCusEntryNumber(CusEntryNumber cusEntryNumber, ZString entryNum, ZString entryType, ZBool entryIsSystemGenerated, ZString countryCode, ZDateTime issueDate, ZString entryStatus)
		{
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryIsSystemGenerated = entryIsSystemGenerated;
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;
			cusEntryNumber.CE_IssueDate = issueDate;
			cusEntryNumber.CE_EntryStatus = entryStatus;
			return cusEntryNumber;
		}

		CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory)
		{
			return SetupCusEntryNumber(factory, "CE00001", Common.Shared.SharedJobMessageTypeList.Codes.Import, false);
		}

		[ExpectNoExceptions]
		void AssertContents(UniversalCustoms.EntryNumber entryNumberDataObject, ZBool entryIsSystemGenerated, ZString number, ICodeDescription type, ZString entryStatus, ZString entryStatusDescription, ZDateTime issueDate)
		{
			NUnit.Framework.Assert.That(entryNumberDataObject, Is.Not.EqualTo(default(UniversalCustoms.EntryNumber)), "Precondition: entryNumberDataObject - should not be [null]");
			CombineAssertions(delegate
			{
				NUnit.Framework.Assert.That(entryNumberDataObject.EntryIsSystemGenerated, Is.EqualTo(entryIsSystemGenerated), "entryNumberDataObject.EntryIsSystemGenerated");
				NUnit.Framework.Assert.That(entryNumberDataObject.Number, Is.EqualTo(number), "entryNumberDataObject.Number");
				NUnit.Framework.Assert.That(entryNumberDataObject.Type, Is.Not.EqualTo(default(UniversalDataBuss.DataObjects.Universal.EntryType)), "entryNumberDataObject.Type - should not be [null]");
				NUnit.Framework.Assert.That(entryNumberDataObject.Type.Code, Is.EqualTo(type.Code).Using(CustomComparers.TypeComparison), "entryNumberDataObject.Type.Code");
				NUnit.Framework.Assert.That(entryNumberDataObject.Type.Description, Is.EqualTo(type.Description).Using(CustomComparers.TypeComparison), "entryNumberDataObject.Type.Description");
				NUnit.Framework.Assert.That(entryNumberDataObject.EntryStatus.Code, Is.EqualTo(entryStatus), "entryNumberDataObject.EntryStatus");
				NUnit.Framework.Assert.That(entryNumberDataObject.EntryStatus.Description, Is.EqualTo(entryStatusDescription), "entryNumberDataObject.EntryStatus Description");
				NUnit.Framework.Assert.That(entryNumberDataObject.IssueDate, Is.EqualTo(issueDate), "entryNumberDataObject.IssueDate");
			}

			);
		}

		[ExpectNoExceptions]
		void AssertContents(UniversalCustoms.EntryNumber entryNumberDataObject)
		{
			AssertContents(entryNumberDataObject, false, "CE00001", CodeDescriptionPairForTesting.New(Common.Shared.SharedJobMessageTypeList.Codes.Import, Common.Shared.SharedJobMessageTypeList.Descriptions.Import, 0), "C1", "已放行 (C1 免審免驗通關)", new ZDateTime(2021, 8, 9));
		}
	}
}
