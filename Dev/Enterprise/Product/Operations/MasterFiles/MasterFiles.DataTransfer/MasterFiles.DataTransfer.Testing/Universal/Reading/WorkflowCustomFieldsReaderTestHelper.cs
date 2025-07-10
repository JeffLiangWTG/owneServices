using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public static class WorkflowCustomFieldsReaderTestHelper
	{
		public static void TestWorkflowCustomFields<TDataObject, TBizo>(Func<TDataObject, IXmlImportLogger, DataObjectReader<TDataObject, TBizo>> getNewReader, TDataObject newDataObject = default(TDataObject), TestErrorLogger logger = null)
			where TDataObject : IDataObject, ICustomizedFieldContainer, new()
			where TBizo : BusinessObject
		{
			#region Setup Template

			var factory = new BusinessObjectFactory();
			var processTaskTemplate = factory.New<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "TBM";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Decimal";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			#endregion

			var dataObject = newDataObject == null ? new TDataObject() : newDataObject;
			dataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			dataObject.CustomizedFieldCollection.Add("Textual context", new ZString("HELLO"));
			dataObject.CustomizedFieldCollection.Add("Date", new ZDateTime(2015, 10, 4));
			dataObject.CustomizedFieldCollection.Add("Decimal", new ZDecimal(0.3));
			dataObject.CustomizedFieldCollection.Add("Flagger", ZBool.True);
			dataObject.CustomizedFieldCollection.Add(new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean });
			dataObject.CustomizedFieldCollection.Add("Integer", new ZInt(42));
			dataObject.CustomizedFieldCollection.Add(new ZString("TOO LONG").PadRight(GenCustomAddOnValueSchema.XV_Name.MaxLength + 1, '1'), new ZInt(2332));
			dataObject.CustomizedFieldCollection.Add(new ZString("Data"), new ZString("DATA TOO LONG").PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1'));
			dataObject.CustomizedFieldCollection.Add(new ZString("TOO LONG").PadRight(GenCustomAddOnValueSchema.XV_Name.MaxLength + 1, '1'), new ZString("DATA TOO LONG").PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1'));

			logger = logger ?? new TestErrorLogger();
			var reader = getNewReader(dataObject, logger);
			var bizoRead = reader.ReadIntoBusinessObject();

			TestCase.CombineAssertions(delegate
			{
				var customFields = bizoRead.GetUserDefinedValues();
				var result = "";
				foreach (var customField in customFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				TestCase.AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra",
@"Data - DATA TOO LONG111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
Date - 04-Oct-15 00:00:00
Decimal - 0.3
Flagger - Y
Integer - 42
Textual context - HELLO
TOO LONG1111111111111111111111111111111111111111111111111111 - DATA TOO LONG111111111111111111111111111111111111111111111111111111111111111111111111111111111111111".TrimEnd(), result);

				TestCase.AssertEquals(true, logger.GetWarnings().Contains(
@"Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Attempted to insert 61 characters into Field [TOO LONG11111111111111111111111111111111111111111111111111111] which has a maximum length of 60 characters. Field was truncated.
Attempted to insert 101 characters into Field [Data] which has a maximum length of 100 characters. Field was truncated.
Attempted to insert 101 characters into Field [TOO LONG11111111111111111111111111111111111111111111111111111] which has a maximum length of 100 characters. Field was truncated.
Attempted to insert 61 characters into Field [TOO LONG11111111111111111111111111111111111111111111111111111] which has a maximum length of 60 characters. Field was truncated."));
				TestCase.AssertEquals(false, logger.HasErrors);
				TestCase.AssertEquals(true, logger.HasWarnings);
			});
		}
	}
}
