using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest
	{
		public void TestCustomsEntryNumberMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var entryNumberBO = SetupCusEntryNumber(Factory.BOFactory);
			entryNumberBO.Parent = header;

			var writer = new CustomsEntryNumberDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryNumberBO)), CurrentCompanyHelper);
			var entryNumberDataObject = writer.GetDataObject(entryNumberBO);

			AssertContents(entryNumberDataObject);
		}

		CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory, ZString entryNum, ZString entryType, ZString entryLineReference, ZBool entryIsSystemGenerated)
		{
			return SetupCusEntryNumber(factory.New<CusEntryNumber>(), entryNum, entryType, entryLineReference, entryIsSystemGenerated, Core.Constants.CountryCodes.Australia, new ZDateTime(2011, 6, 1));
		}

		CusEntryNumber SetupCusEntryNumber(CusEntryNumber cusEntryNumber, ZString entryNum, ZString entryType, ZString entryLineReference, ZBool entryIsSystemGenerated, ZString countryCode, ZDateTime issueDate)
		{
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryLineReference = entryLineReference;
			cusEntryNumber.CE_EntryIsSystemGenerated = entryIsSystemGenerated;
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;
			cusEntryNumber.CE_IssueDate = issueDate;

			return cusEntryNumber;
		}

		CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory)
		{
			return SetupCusEntryNumber(factory, "CE00001", JobMessageTypeList.Codes.Import, "REFERENCE", false);
		}

		CusEntryNumber SetupCusEntryNumber2(BusinessObjectFactory factory)
		{
			return SetupCusEntryNumber(factory, "CE00002", JobMessageTypeList.Codes.Export, "REFERENCE2", true);
		}

		void AssertContents(UniversalCustoms.EntryNumber entryNumberDataObject, ZBool entryIsSystemGenerated, ZString entryLineReference, ZString number, ICodeDescription type)
		{
			AssertNotNull("Precondition: entryNumberDataObject", entryNumberDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("entryNumberDataObject.EntryIsSystemGenerated", entryIsSystemGenerated, entryNumberDataObject.EntryIsSystemGenerated);
				//				AssertEquals("entryNumberDataObject.EntryLineReference", entryLineReference, entryNumberDataObject.EntryLineReference);// speak to Ben before adding this back
				AssertEquals("entryNumberDataObject.Number", number, entryNumberDataObject.Number);
				AssertNotNull("entryNumberDataObject.Type", entryNumberDataObject.Type);
				AssertEquals("entryNumberDataObject.Type.Code", type.Code, entryNumberDataObject.Type.Code);
				AssertEquals("entryNumberDataObject.Type.Description", type.Description, entryNumberDataObject.Type.Description);
			});
		}

		void AssertContents(UniversalCustoms.EntryNumber entryNumberDataObject)
		{
			AssertContents(entryNumberDataObject, false, "REFERENCE", "CE00001", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, JobMessageTypeList.Descriptions.Import));
		}

		void AssertContents2(UniversalCustoms.EntryNumber entryNumberDataObject)
		{
			AssertContents(entryNumberDataObject, true, "REFERENCE2", "CE00002", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export));
		}
	}
}
