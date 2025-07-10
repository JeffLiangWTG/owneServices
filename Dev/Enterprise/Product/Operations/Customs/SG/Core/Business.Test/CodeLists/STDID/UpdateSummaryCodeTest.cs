using System;
using System.Reflection;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class UpdateSummaryCodeTest : TestCase
	{
		public UpdateSummaryCodeTest() : base()
		{
		}

		public void TestGetFieldDescriptionFromSummaryCode()
		{
			string fTXSummaryCode = "010000000000MF";
			AssertEquals("Field description expected", "ARRIVAL DATE (MODIFICATION)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "010000000000DF";
			AssertEquals("Field description expected", "ARRIVAL DATE (DELETED)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "010000000000AF";
			AssertEquals("Field description expected", "ARRIVAL DATE (ADDED)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "022201103000AF";
			AssertEquals("Field description expected", "CONTAINER SIZE 01 03 (ADDED)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "023201102000DF";
			AssertEquals("Field description expected", "CONTAINER WEIGHT 01 02 (DELETED)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "100101204145MF";
			AssertEquals("Field description expected", "CA/SC CODE 1 01 04 45 (MODIFICATION)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "901105000000AL";
			AssertEquals("Field description expected", "ITEM 05 (LINE ADDED)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "901103000000DL";
			AssertEquals("Field description expected", "ITEM 03 (LINE DELETED)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			var sc = new UpdateSummaryCodeExposed();
			ZString code;
			int shift = 1;
			for (int index = 1; index <= sc.strArray.Length; index++)
			{
				if (index != 7 && index != 66 && index != 165 && index != 188)
				{
					if (index > 198)
					{
						code = (index + 702).ToString("000");
					}
					else
					{
						code = index.ToString("000");
					}

					fTXSummaryCode = code + "000000000MF";
					if ((index > 99 && index < 103) || index == 207) // This is not hardcoded SqlException number
					{
						AssertEquals(index.ToString(), sc.strArray[index - shift].ToUpperInvariant() + " 00 00 00 (MODIFICATION)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
					}
					else if ((index > 19 && index < 25) || (index > 96 && index < 100) || index == 206 || index == 201 || index == 198)
					{
						AssertEquals(index.ToString(), sc.strArray[index - shift].ToUpperInvariant() + " 00 00 (MODIFICATION)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
					}
					else if ((index > 74 && index < 97) || (index > 102 && index < 160) || (index > 171 && index < 182) || index == 199 || index == 200 || (index > 201 && index < 206))
					{
						AssertEquals(index.ToString(), sc.strArray[index - shift].ToUpperInvariant() + " 00 (MODIFICATION)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
					}
					else
					{
						AssertEquals(index.ToString(), sc.strArray[index - shift].ToUpperInvariant() + " (MODIFICATION)", GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
					}
				}
				else
				{
					shift++;
				}
			}
		}

		public void TestGetTN41FieldDescriptionFromSummaryCode()
		{
			string fTXSummaryCode = "009000000000MF";
			AssertEquals("Field description expected", "ARRIVAL DATE", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "010000000000MF";
			AssertEquals("Field description expected", "DEPARTURE DATE", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "010000000000DF";
			AssertEquals("Field description expected", "DEPARTURE DATE", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "010000000000AF";
			AssertEquals("Field description expected", "DEPARTURE DATE", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "022201103000AF";
			AssertEquals("Field description expected", "CONTAINER SIZE, CONTAINER DETAILS 01, CONTAINER INFO OCC 03", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "023201102000DF";
			AssertEquals("Field description expected", "CONTAINER WEIGHT, CONTAINER DETAILS 01, CONTAINER INFO OCC 02", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "100101204145MF";
			AssertEquals("Field description expected", "SERIAL NUMBER, ITEM 01", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "102101204145MF";
			AssertEquals("Field description expected", "CA/SC PRODUCT CODE, ITEM 01, CA/SC PRODUCT OCC 04", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "901105000000AL";
			AssertEquals("Field description expected", "ITEM 05", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			fTXSummaryCode = "901103000000DL";
			AssertEquals("Field description expected", "ITEM 03", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
			var summaryCode = new TN41UpdateSummaryCodeExposed();
			ZString code;
			int shift = 1;
			for (int index = 1; index <= (summaryCode.strArray.Length - 4); index++)
			{
				if (index > 219)
				{
					code = (index + 681).ToString("000");
				}
				else
				{
					code = index.ToString("000");
				}

				fTXSummaryCode = code + "000000000MF";
				if ((index > 19 && index < 25) || code == "207" || code == "903") // This is not hardcoded SqlException number (index = ...)
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant() + " 00, CONTAINER INFO OCC 00", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
				else if ((index > 101 && index < 105) || code == "908")
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant() + " 00, CA/SC PRODUCT OCC 00", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
				else if ((index > 104 && index < 108) || code == "909")
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant() + " 00, CA/SC PRODUCT OCC 00" + ", CA/SC CODE OCC 00", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
				else if ((index > 208 && index < 212) || code == "911")
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant() + " 00, PROCESSING CODE OCC 00", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
				else if ((index > 104 && index < 108) || code == "909")
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant() + " 00 00 00", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
				else if ((index > 79 && index < 163) || (index > 173 && index < 184) || index == 208 || (index > 212 && index < 216 || (index > 216 && index < 219)) || code == "901" || code == "902" || code == "904" || code == "905" || code == "906" || code == "907" || code == "910")
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant() + " 00", GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
				else
				{
					AssertEquals(index.ToString(), summaryCode.strArray[index - shift].ToUpperInvariant(), GetTN41FieldDescriptionFromSummaryCodeForTest(fTXSummaryCode));
				}
			}
		}

		protected ZString GetFieldDescriptionFromSummaryCodeForTest(string summaryCode)
		{
			if (updateSummaryCode == null)
			{
				updateSummaryCode = new UpdateSummaryCode();
			}

			return updateSummaryCode.GetFieldDescriptionFromSummaryCode(summaryCode);
		}

		UpdateSummaryCode updateSummaryCode;
		protected ZString GetTN41FieldDescriptionFromSummaryCodeForTest(string summaryCode)
		{
			if (updateSummaryCode == null)
			{
				updateSummaryCode = new UpdateSummaryCode();
			}

			return updateSummaryCode.GetTN41FieldDescriptionFromSummaryCode(summaryCode);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidCode()
		{
			string fTXSummaryCode = "9170000000000MF";
			string result = GetFieldDescriptionFromSummaryCodeForTest(fTXSummaryCode);
		}

		public class UpdateSummaryCodeExposed : UpdateSummaryCode
		{
			public UpdateSummaryCodeExposed()
			{
				FieldDescriptions fieldDescriptions = new FieldDescriptions();
				Type t = fieldDescriptions.GetType();
				FieldInfo[] fieldInfoArray = t.GetFields();
				int i = 0;
				strArray = new string[fieldInfoArray.Length];
				foreach (FieldInfo fieldInfo in fieldInfoArray)
				{
					string res = (string)fieldInfo.GetValue(fieldDescriptions);
					strArray.SetValue(res, i);
					i++;
				}
			}

			public string[] strArray;
		}

		public class TN41UpdateSummaryCodeExposed : UpdateSummaryCode
		{
			public TN41UpdateSummaryCodeExposed()
			{
				TN41UpdateDescriptions fieldDescriptions = new TN41UpdateDescriptions();
				Type t = fieldDescriptions.GetType();
				FieldInfo[] fieldInfoArray = t.GetFields();
				int i = 0;
				strArray = new string[fieldInfoArray.Length];
				foreach (FieldInfo fieldInfo in fieldInfoArray)
				{
					if (fieldInfo.Name.Length == 4)
					{
						string res = (string)fieldInfo.GetValue(fieldDescriptions);
						strArray.SetValue(res, i);
						i++;
					}
				}
			}

			public string[] strArray;
		}
	}
}
