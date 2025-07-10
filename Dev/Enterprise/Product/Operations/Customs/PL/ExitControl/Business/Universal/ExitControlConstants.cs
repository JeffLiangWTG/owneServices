using System.Collections.Generic;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.ExitControl.Business;

static class ExitControlConstants
{
	public static class Rules
	{
		[ThreadSafe] public static readonly IReadOnlyCollection<ZString> R0049ETransportTypeFirstNumbers = ["5", "7"];
		[ThreadSafe] public static readonly IReadOnlyCollection<ZString> R0050ETransportTypeFirstNumbers = ["2", "5", "7"];
	}

	public static class ExitReportMessageStatuses
	{
		public static string Received => "RCV";
	}

	public static class RefCusCodeListType
	{
		public const string CL046 = "CL046";
	}

	public static class InterpretationStrings
	{
		public static class MessageTitles
		{
			public static string CC521C => Res.GetString("00C162AA-51A5-480B-A54F-79A1A7290C61", "CC521C - Diversion rejection notification.");
			public static string CC557 => Res.GetString("6F9551F6-3949-4324-9B7E-31DB2FC25CFA", "CC557 - Rejection from the customs office of exit");
		}

		public static class CaptionStrings
		{
			public static string FunctionalErrors => Res.GetString("CB1D01A9-656E-4473-8793-C615A63EFC7B", "Functional errors");
		}

		public static class CommonStrings
		{
			public static string CustomsOfficeOfExit => Res.GetString("35BFF2A8-31E5-4EEF-8CFE-4B7E34B89B53", "Customs Office of Exit");
			public static string MRN => Res.GetString("A71FF296-D5A1-447D-96EA-655FBDE9903F", "MRN");
			public static string LRN => Res.GetString("896910F7-5323-4765-8469-CA86B336D816", "LRN");
			public static string Number => Res.GetString("DB9888FD-625D-4DA8-AC3E-817112331161", "No.");
			public static string RejectedMessageIdentification => Res.GetString("91749329-C23C-4998-A07D-F8C7E7C13F5A", "Rejected Message identification");
			public static string BusinessRejectionType => Res.GetString("7AA33A6F-1BA7-41E0-A854-5AB9DEB182E7", "Type of Business rejection");
			public static string RejectionDateAndTime => Res.GetString("C2F0E625-1A77-474E-9E1E-9B0BF47865B9", "Rejection date and time");
			public static string RejectionCode => Res.GetString("D722945A-61F7-4A66-AAF3-F9804800389F", "Rejection Code");
			public static string RejectionReason => Res.GetString("AAB4871B-F5DF-4002-BE47-2EE700DA888C", "Rejection Reason");
		}

		public static class DiversionRejection
		{
			public static string Date => Res.GetString("E1A6151C-7FD0-4D00-9839-8F7230EA0EAC", "Date of diversion rejection at Exit");
			public static string ReasonCode => Res.GetString("A0B1F2D3-4E5F-4C6D-8B9A-0B1C2D3E4F5A", "Diversion Rejection Reason Code");
			public static string Details => Res.GetString("639B6762-019B-48CD-8BB8-26DC1D6CE715", "Details about the reason code");
		}

		public static class FunctionalErrorsString
		{
			public static string ErrorPointer => Res.GetString("AF88E5A7-090C-420A-A5A9-00B9C2BABF2B", "Error Pointer");
			public static string ErrorCode => Res.GetString("4D24A0FE-D850-4D70-B215-4560F45B605F", "Error Code");
			public static string ErrorReason => Res.GetString("B6201FBF-6C5E-4D6C-BB40-D12E4AB9F310", "Error Reason");
			public static string OriginalAttributeValue => Res.GetString("CA99FCFA-7F44-440B-B882-E1FF19D0D2E5", "Original Attribute Value");
		}
	}
}
