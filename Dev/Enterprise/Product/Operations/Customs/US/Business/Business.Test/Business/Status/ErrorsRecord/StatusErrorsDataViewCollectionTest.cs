using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatusErrorsDataViewCollection))]
	sealed class StatusErrorsDataViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StatusErrorsDataViewCollection>
	{
		public void TestPopulate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "DIS Form List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "APH01", "APH_STAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var coll = GetCollectionToTest();
			var declaration = Factory.New<JobDeclaration>();
			var dispData1 = declaration.DispositionCodes.AddNewIfNotExist("96", ZDateTime.BrettsBirthday);
			dispData1.US_DocumentType = DocumentTypeCodeList.Codes._01;
			var dispData2 = declaration.DispositionCodes.AddNewIfNotExist("BB", ZDateTime.BrettsBirthday.AddDays(1));
			dispData2.US_DocumentType = DocumentTypeCodeList.Codes._04;
			var dispData3 = declaration.DispositionCodes.AddNewIfNotExist("AA", ZDateTime.BrettsBirthday.AddDays(2));
			dispData3.US_DocumentType = DocumentTypeCodeList.Codes._05;

			coll.Populate(declaration.DispositionCodes.OfType<DispositionData>());
			AssertEquals(3, coll.Count);

			var coll1 = coll.Cast<ErrorsRecord>().FirstOrDefault(x => x.ErrorMessageIdentifier == "96");
			Assert(coll1.NarrativeMessage.Contains("Doc Req./" + DocumentTypeCodeList.Descriptions._01));

			var coll2 = coll.Cast<ErrorsRecord>().FirstOrDefault(x => x.ErrorMessageIdentifier == "BB");
			Assert(!coll2.NarrativeMessage.Contains("Doc Req./"));

			var coll3 = coll.Cast<ErrorsRecord>().FirstOrDefault(x => x.ErrorMessageIdentifier == "AA");
			Assert(!coll3.NarrativeMessage.Contains("Doc Req./"));

			declaration.DispositionCodes.RemoveAndDeleteAll();
			coll.RemoveAndDeleteAll();
			var dispData4 = declaration.DispositionCodes.AddNewIfNotExist("96", ZDateTime.BrettsBirthday.AddDays(3));
			dispData4.US_DocumentType = "APH01";

			coll.Populate(declaration.DispositionCodes.OfType<DispositionData>());
			AssertEquals(1, coll.Count);

			var coll4 = coll.Cast<ErrorsRecord>().FirstOrDefault(x => x.ErrorMessageIdentifier == "96");
			Assert(coll4.NarrativeMessage.Contains("Doc Req./" + "APH_STAT"));
		}

		protected override StatusErrorsDataViewCollection GetCollectionToTest()
		{
			return new StatusErrorsDataViewCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ErrorsRecord();
		}
	}
}
