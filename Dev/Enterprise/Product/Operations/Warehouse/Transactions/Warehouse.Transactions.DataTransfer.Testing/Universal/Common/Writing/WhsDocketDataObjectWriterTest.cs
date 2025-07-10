using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsDocketDataObjectWriterTest<TDocket, TWriter> : WhsUniversalTestCase
		where TDocket : WhsDocket
		where TWriter : WhsDocketDataObjectWriter<TDocket>
	{
		protected abstract TDocket GetNewDocket();
		protected TWriter GetNewDataObjectWriter(BusinessObject topLevelBO) => GetNewDataObjectWriter(topLevelBO, null);
		protected abstract TWriter GetNewDataObjectWriter(BusinessObject topLevelBO, INotifications notifications);

		#region TestNotes

		public void TestNotes()
		{
			var whsDocketBO = GetNewDocket();
			var noteBO1 = whsDocketBO.Notes.AddNew(true, "CAT EATER!!", "Feee-lix the cat, what a wonderful-wonderful cat.");
			noteBO1.ST_NoteType = nameof(StmNoteVisibility.PUB);
			var noteBO2 = whsDocketBO.Notes.AddNew(false, "Internal Work Notes", "Flintstones, meet the Flintstones.");
			noteBO2.ST_NoteContext = "DEB";

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertNotNull("whsDocketData.NoteCollection", whsDocketData.NoteCollection);
			AssertEquals("whsDocketData.NoteCollection.Count", 2, whsDocketData.NoteCollection.Count);

			var note1 = whsDocketData.NoteCollection[0];

			CombineAssertions(() =>
			{
				AssertEquals("note1.Description", "CAT EATER!!", note1.Description);
				AssertEquals("note1.IsCustomDescription", ZBool.True, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", note1.NoteText);
				AssertEquals("note1.NoteContext.Code", "AAA", note1.NoteContext.Code);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});

			var note2 = whsDocketData.NoteCollection[1];

			CombineAssertions(() =>
			{
				AssertEquals("note2.Description", "Internal Work Notes", note2.Description);
				AssertEquals("note2.IsCustomDescription", ZBool.False, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Flintstones, meet the Flintstones.", note2.NoteText);
				AssertEquals("note2.NoteContext.Code", "DEB", note2.NoteContext.Code);
				AssertEquals("note2.NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "INT", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "INTERNAL", note2.Visibility.Description);
			});
		}

		#endregion

		#region TestWorkflowCustomFieldsAreExported

		public void TestWorkflowCustomFieldsAreExported()
		{
			var whsDocketBO = GetNewDocket();

			whsDocketBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			whsDocketBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			whsDocketBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			whsDocketBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			whsDocketBO.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			var writer = GetNewDataObjectWriter(whsDocketBO);
			var shipmentData = writer.GetDataObject(whsDocketBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(() =>
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		#endregion

		#region TestOrganisationLevelCustomFieldsAreExported

		public void TestOrganisationLevelCustomFieldsAreExported()
		{
			if (IsCustomFieldsSupported)
			{
				var docket = GetNewDocket();
				docket.WD_OH_Client = Helper.CreateClient().PK;
				Data.SetupCustomLabels(docket);
				Data.AddCustomFieldsToBizO(new WhsDocket.CustomLabelsProvider(docket), docket);

				var writer = GetNewDataObjectWriter(docket);
				var docketDataObject = writer.GetDataObject(docket);
				AssertNotNull("Precondition: orderData", docketDataObject);

				var customFields = docketDataObject.CustomizedFieldCollection;
				AssertNotNull(customFields);

				CombineAssertions(() =>
				{
					AssertEquals("customFields.Count", 5, customFields.Count);
					AssertCustomFieldsAreExported(customFields);
				});
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool IsCustomFieldsSupported => true;

		#endregion
	}
}
