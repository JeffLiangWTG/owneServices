using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;

public class InnerXmlControlAnswerObject : InnerMessageObjectBase
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
	public static class ConstantsTagName
	{
		public const string DeclarationNumber = "Beyanname_no";
		public const string Code = "Kod";
		public const string Desciption = "Aciklama";
		public const string ErrorCode = "Hata_kodu";
		public const string ErrorDescription = "Hata_aciklamasi";
		public const string Line_Number = "Kalem_no";
		public const string Verification = "Dogrulama";
		public const string Type = "Tip";
		public const string Amount = "Miktar";
		public const string Rate = "Oran";
		public const string PaymentType = "Odeme_sekli";
		public const string TaxBase = "Vergi_matrahi";
		public const string AnswerCode = "Kodu";
		public const string AnswerDescription = "Aciklamasi";
		public const string AnswerLineNumber = "Sira";

		public static class QuestionType
		{
			public const string Description = "Aciklama";
			public const string Document = "Fatura";
			public const string Question = "Soru";
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
	public static class ConstantsElementName
	{
		public const string Errors = "Hatalar";
		public const string Questions = "Sorular";
		public const string Documents = "Belgeler";
		public const string Taxes = "Vergiler";
		public const string Answers = "Cevaplar";
		public const string PositiveAnswer = "Evet";
		public const string NegativeAnswer = "Hayir";
		public const string MessageTypeByControl = "Kontrol";
		public const string MessageTypeByRegistration = "Tescil";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
	static bool IsMatch(XmlDocument document) =>
		document?.GetElementByPath(new (string, string)[]
		{
			(ConstantsTagName.Type, TRMessageConstants.Xml.TempuriNamespace)
		}) is XmlElement typeElement
		&& (typeElement.InnerText == ConstantsElementName.MessageTypeByControl || typeElement.InnerText == ConstantsElementName.MessageTypeByRegistration);

	public InnerXmlControlAnswerObject(XmlDocument xmlDocument) : base(xmlDocument)
	{
		Errors = ExtractErrors(xmlDocument);
		if (IsMatch(xmlDocument))
		{
			Questions = ExtractQuestions(xmlDocument);
			Documents = ExtractDocuments(xmlDocument);
			Taxes = ExtractTaxes(xmlDocument);
		}
	}

	List<Error> ExtractErrors(XmlDocument xmlDocument)
	{
		var errorList = new List<Error>();

		var errorNodes = xmlDocument.GetElementByPath(new (string, string)[]
			{ (ConstantsElementName.Errors, TRMessageConstants.Xml.TempuriNamespace) });
		if (errorNodes == null)
		{
			return errorList;
		}

		foreach (XmlNode node in errorNodes)
		{
			errorList.Add(new Error()
			{
				Code = TryGetInnerText(node, ConstantsTagName.ErrorCode),
				Description = TryGetInnerText(node, ConstantsTagName.ErrorDescription)
			});
		}

		return errorList;
	}

	List<Question> ExtractQuestions(XmlDocument xmlDocument)
	{
		var questionList = new List<Question>();

		var questionNodes = xmlDocument.GetElementByPath(new (string, string)[]
			{ (ConstantsElementName.Questions, TRMessageConstants.Xml.TempuriNamespace) });
		if (questionNodes == null)
		{
			return questionList;
		}

		foreach (XmlNode node in questionNodes)
		{
			var que = new Question
			{
				Code = TryGetInt(TryGetInnerText(node, ConstantsTagName.Code)),
				Description = TryGetInnerText(node, ConstantsTagName.Desciption),
				LineNumber = TryGetInt(TryGetInnerText(node, ConstantsTagName.Line_Number)),
				Type = TryGetInnerText(node, ConstantsTagName.Type),
				Answers = new Answers()
			};
			questionList.Add(que);

			var answersNode = node.GetElementsByPath(new (string, string)[]
			{
				(ConstantsElementName.Answers, TRMessageConstants.Xml.TempuriNamespace)
			});

			if (answersNode == null)
			{
				continue;
			}

			var positiveAnswersNode = node.GetElementsByPath(new (string, string)[]
			{
				(ConstantsElementName.PositiveAnswer, TRMessageConstants.Xml.TempuriNamespace)
			});
			if (positiveAnswersNode != null)
			{
				que.Answers.PositiveAnswers = new List<PositiveAnswer>();

				foreach (XmlElement positiveAnswer in positiveAnswersNode)
				{
					var pAns = new PositiveAnswer
					{
						Code = TryGetInnerText(positiveAnswer, ConstantsTagName.AnswerCode),
						Description = TryGetInnerText(positiveAnswer, ConstantsTagName.AnswerDescription),
						LineNumber = TryGetInt(TryGetInnerText(positiveAnswer, ConstantsTagName.AnswerLineNumber)),
					};
					que.Answers.PositiveAnswers.Add(pAns);
				}
			}

			var negativeAnswersNode = node.GetElementsByPath(new (string, string)[]
			{
				(ConstantsElementName.NegativeAnswer, TRMessageConstants.Xml.TempuriNamespace)
			});
			if (negativeAnswersNode != null)
			{
				que.Answers.NegativeAnswers = new List<NegativeAnswer>();

				foreach (XmlElement negativeAnswer in negativeAnswersNode)
				{
					var nAns = new NegativeAnswer
					{
						Code = TryGetInnerText(negativeAnswer, ConstantsTagName.AnswerCode),
						Description = TryGetInnerText(negativeAnswer, ConstantsTagName.AnswerDescription),
						LineNumber = TryGetInt(TryGetInnerText(negativeAnswer, ConstantsTagName.AnswerLineNumber)),
					};
					que.Answers.NegativeAnswers.Add(nAns);
				}
			}
		}

		return questionList;
	}

	List<Document> ExtractDocuments(XmlDocument xmlDocument)
	{
		var documentList = new List<Document>();

		var documentNodes = xmlDocument.GetElementByPath(new (string, string)[]
			{ (ConstantsElementName.Documents, TRMessageConstants.Xml.TempuriNamespace) });
		if (documentNodes == null)
		{
			return documentList;
		}

		foreach (XmlNode node in documentNodes)
		{
			documentList.Add(new Document()
			{
				LineNumber = TryGetInt(TryGetInnerText(node, ConstantsTagName.Line_Number)),
				Code = TryGetInnerText(node, ConstantsTagName.Code),
				Description = TryGetInnerText(node, ConstantsTagName.Desciption),
				Verification = TryGetInnerText(node, ConstantsTagName.Verification),
			});
		}

		return documentList;
	}

	List<Tax> ExtractTaxes(XmlDocument xmlDocument)
	{
		var taxList = new List<Tax>();

		var taxNodes = xmlDocument.GetElementByPath(new (string, string)[]
			{ (ConstantsElementName.Taxes, TRMessageConstants.Xml.TempuriNamespace) });
		if (taxNodes == null)
		{
			return taxList;
		}

		foreach (XmlNode node in taxNodes)
		{
			taxList.Add(new Tax()
			{
				LineNumber = TryGetInt(TryGetInnerText(node, ConstantsTagName.Line_Number)),
				Code = TryGetInnerText(node, ConstantsTagName.Code),
				Description = TryGetInnerText(node, ConstantsTagName.Desciption),
				Amount = TryGetDecimal(TryGetInnerText(node, ConstantsTagName.Amount)),
				Rate = TryGetDecimal(TryGetInnerText(node, ConstantsTagName.Rate)),
				PaymentType = TryGetInnerText(node, ConstantsTagName.PaymentType),
				BaseAmount = TryGetDecimal(TryGetInnerText(node, ConstantsTagName.TaxBase))
			});
		}

		return taxList;
	}

	public List<Error> Errors { get; set; }
	public List<Question> Questions { get; set; }
	public List<Document> Documents { get; set; }
	public List<Tax> Taxes { get; set; }

	public class Error
	{
		public ZString Code { get; set; }
		public ZString Description { get; set; }
	}

	public class Question
	{
		public ZInt Code { get; set; }
		public ZString Description { get; set; }
		public ZInt LineNumber { get; set; }
		public ZString Type { get; set; }
		public Answers Answers { get; set; }
	}

	public class Document
	{
		public ZInt LineNumber { get; set; }
		public ZString Code { get; set; }
		public ZString Description { get; set; }
		public ZString Verification { get; set; }
	}

	public class Tax
	{
		public ZInt LineNumber { get; set; }
		public ZString Code { get; set; }
		public ZString Description { get; set; }
		public ZDecimal Amount { get; set; }
		public ZDecimal Rate { get; set; }
		public ZString PaymentType { get; set; }
		public ZDecimal BaseAmount { get; set; }
	}

	public class Answers
	{
		/// <summary>
		/// XML Element: Evetler
		/// </summary>
		public List<PositiveAnswer> PositiveAnswers { get; set; }

		/// <summary>
		/// XML Element: Hayirlar
		/// </summary>
		public List<NegativeAnswer> NegativeAnswers { get; set; }
	}

	/// <summary>
	/// XML Element: Evet
	/// </summary>
	public class PositiveAnswer
	{
		public ZInt LineNumber { get; set; }
		public ZString Code { get; set; }
		public ZString Description { get; set; }
	}

	/// <summary>
	/// XML Element: Hayir
	/// </summary>
	public class NegativeAnswer
	{
		public ZInt LineNumber { get; set; }
		public ZString Code { get; set; }
		public ZString Description { get; set; }
	}
}
