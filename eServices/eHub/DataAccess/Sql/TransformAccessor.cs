using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	public class TransformAccessor : ITransformAccessor
	{
		public TransformAccessor() : this(new eServices.eHubDataAccess.Sql.TransformAccessor()) { }

		public TransformAccessor(eServices.eHubDataAccess.Integration.ITransformAccessor transformAccessor)
		{
			this.transformAccessor = transformAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.ITransformAccessor transformAccessor;

		public List<TransformSet> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType, bool? tsDirection)
			=> transformAccessor.SelectTransformsByPartiesMessage(senderID, recipientID, sourceMessageType, tsDirection).Select(s => (TransformSet)s).ToList();

		public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
		{
			if (testingContext != null)
			{
				return GetRecipientCodeTest(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, key5);
			}

			return transformAccessor.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, key5);
		}

		string GetRecipientCodeTest(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
		{
			string info = String.Join("+", (new[] { senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, key5 }).Where(a => a != null).ToArray());

			var value = (from cv in testingContext.eHubCodeMapValues
						 where cv.eHubCodeSetResult.CR_Name == resultField
						  && cv.eHubCodeSetResult.eHubCodeSet.CS_Name == codeSet
						  && cv.eHubCodeSetResult.eHubCodeSet.eHubTransformationSet.TS_Name == transformationName
						  && cv.eHubCodeSetResult.eHubCodeSet.eHubClient_Sender.CC_ID == senderClientCode
						  && cv.eHubCodeSetResult.eHubCodeSet.eHubClient_Recipient.CC_ID == recipientClientCode
						  && ((key1 == null && cv.eHubCodeMapKey.CK_Key1Value == null) || (key1 != null && Regex.IsMatch(key1, "^" + cv.eHubCodeMapKey.CK_Key1Value.Replace("%", ".*") + "$")))
						  && ((key2 == null && cv.eHubCodeMapKey.CK_Key2Value == null) || (key2 != null && Regex.IsMatch(key2, "^" + cv.eHubCodeMapKey.CK_Key2Value.Replace("%", ".*") + "$")))
						  && ((key3 == null && cv.eHubCodeMapKey.CK_Key3Value == null) || (key3 != null && Regex.IsMatch(key3, "^" + cv.eHubCodeMapKey.CK_Key3Value.Replace("%", ".*") + "$")))
						  && ((key4 == null && cv.eHubCodeMapKey.CK_Key4Value == null) || (key4 != null && Regex.IsMatch(key4, "^" + cv.eHubCodeMapKey.CK_Key4Value.Replace("%", ".*") + "$")))
						  && ((key5 == null && cv.eHubCodeMapKey.CK_Key5Value == null) || (key5 != null && Regex.IsMatch(key5, "^" + cv.eHubCodeMapKey.CK_Key5Value.Replace("%", ".*") + "$")))
						 orderby cv.eHubCodeMapKey.CK_Order
						 select cv).FirstOrDefault();

			if (value == null)
			{
				throw new Exception("Code set definition error for: " + info);
			}

			switch (value.CV_PassThroughKey)
			{
				case null:
					return value.CV_OutputCode;
				case 1:
					return key1;
				case 2:
					return key2;
				case 3:
					return key3;
				case 4:
					return key4;
				case 5:
					return key5;
			}

			throw new ApplicationException("Invalid code execution.");
		}

		public bool IsFlatFile(string messageType, out string charset)
			=> transformAccessor.IsFlatFile(messageType, out charset);

		public bool IsEDI(string messageType)
			=> transformAccessor.IsEDI(messageType);

		public bool IsJson(string messageType)
			=> transformAccessor.IsJson(messageType);

		public string CallActionProcedure(string procedure, string outputParm, params string[] inputParms)
		{
			if (testingContext != null)
			{
				return testingContext.ActionProcedures.First(p => p.Procedure == procedure && p.OutputParm == outputParm && p.InputParms.SequenceEqual(inputParms)).Result;
			}

			return transformAccessor.CallActionProcedure(procedure, outputParm, inputParms);
		}

		public bool IsPostAssembleMapping(string messageType, out string postAssembleWrapper)
			=> transformAccessor.IsPostAssembleMapping(messageType, out postAssembleWrapper);

		public void SetCodeMapsTestingContext(CodeMapsTestingContext ctx)
		{
			testingContext = ctx;
		}

		public string GetUNLOCOFromIATA(string IATACode)
		{
			if (testingContext != null)
			{
				return GetUNLOCOFromIATATest(IATACode);
			}

			return transformAccessor.GetUNLOCOFromIATA(IATACode);
		}

		public string GetStateFromUNLOCO(string UNLOCO)
		{
			if (testingContext != null)
			{
				return GetStateFromUNLOCOTest(UNLOCO);
			}

			return transformAccessor.GetStateFromUNLOCO(UNLOCO);
		}

		string GetUNLOCOFromIATATest(string IATACode)
		{
			var result = (from loc in testingContext.eHubUNLOCOList
						  where loc.IATACode == IATACode
						  select loc.UNLOCOCode).FirstOrDefault();
			return result;
		}

		string GetStateFromUNLOCOTest(string UNLOCO)
		{
			var result = testingContext.eHubStateList
				.Find(state => (state.StatePK == testingContext.eHubUNLOCOList
									.Find(location => location.UNLOCOCode == UNLOCO).StateRef))
				.Code;

			return result;
		}

		private static CodeMapsTestingContext testingContext;
	}
}
