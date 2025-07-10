using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using static Enterprise.Customs.TR.Business.CusEntryMessageConstants;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryCPDecProvider : IQuestionsAndAnswers
	{
		public CusEntryCPDecProvider(CusEntryCPDec entryLine, ZString messageType, ZInt lineNumber)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			MessageType = messageType;
			LineNumber = lineNumber;
		}
		protected readonly CusEntryCPDec EntryLine;
		protected readonly ZString MessageType;
		protected readonly ZInt LineNumber;

		public int LineNo => LineNumber;
		public string QuestionNo => EntryLine.ON_CPDecNum.ToString();
		public string Answer
		{
			get
			{
				var answerCode = ZString.Empty;
				if (MessageType == TRMessageTypes.Codes.EUT)
				{
					answerCode = EntryLine.ON_AnswerCode == YesNoList.Codes.Yes ? ExportUnionConstants.Answers.YesValue : ExportUnionConstants.Answers.NoValue;
				}
				else
				{
					answerCode = EntryLine.ON_AnswerCode == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
				}

				return answerCode;
			}
		}
	}
}
