using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.TW;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public abstract class TWMessageHelper : NonPersistentBusinessObject
	{
		protected TWMessageHelper(TWMessage message)
			: base(message.Factory)
		{
			Message = message;
		}

		public readonly TWMessage Message;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected const string FirstHeadStyle = "font-size: 11.0pt;background: #203764; color: white; font-weight:700;vertical-align: middle;border: .5pt solid #AEAAAA;";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected const string HeadStyle = "font-size: 11.0pt;background: #203764; color: white; font-weight:700;border-left: .5pt solid #AEAAAA;border-bottom: .5pt solid #AEAAAA;vertical-align: middle;border-right: .5pt solid #AEAAAA;";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected const string TitleStyle = "font-size: 10.0pt;border-left:.5pt solid #AEAAAA;border-bottom:.5pt solid #AEAAAA;border-right:.5pt solid #AEAAAA; vertical-align: middle; background: #8EA9DB; color: black; font-weight:700;";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected const string SubTitleStyle = "font-size: 9.0pt;border-left:.5pt solid #AEAAAA;border-bottom:.5pt solid #AEAAAA;border-right:.5pt solid #AEAAAA; vertical-align: middle;background: #B4C6E7; color: black; font-weight:700;";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected const string CellStyle = "font-size: 8.0pt;font-family: Arial, sans-serif;vertical-align: middle;background: #D9E1F2;white-space: normal;color: black;border-left:.5pt solid #AEAAAA;border-bottom:.5pt solid #AEAAAA;";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected const string CellBoldStyle = "font-size: 9.0pt;font-family: Arial, sans-serif;vertical-align: middle;background: #D9E1F2;white-space: normal;color: black;border-left:.5pt solid #AEAAAA;border-bottom:.5pt solid #AEAAAA;font-weight:700;";

		public static TWMessageHelper NewIncomingHelper(TWMessage message, BusinessObject bizObj = null)
		{
			switch (message.EM_MessageType)
			{
				case MessageTypeList.Codes._402:
					return new NX402MessageHelper(message);
				case MessageTypeList.Codes.ARM:
					return new NX5106MessageHelper(message);
				case MessageTypeList.Codes.ERM:
					return new N5204MessageHelper(message);
				case MessageTypeList.Codes.IEM:
					return new N5109MessageHelper(message);
				case MessageTypeList.Codes.IRM:
					return new N5116MessageHelper(message);
				case MessageTypeList.Codes.RFM:
					return new N5107MessageHelper(message);
				case MessageTypeList.Codes.UHC:
					return new N5168MessageHelper(message);
				case MessageTypeList.Codes.ECD:
				case MessageTypeList.Codes.ICD:
				case MessageTypeList.Codes.ADM:
				case MessageTypeList.Codes.IEA:
				case MessageTypeList.Codes.FHM:
					return new TWCustomsDeliveryNotificationMessageHelper(message);
				case MessageTypeList.Codes._101:
				case MessageTypeList.Codes._201:
				case MessageTypeList.Codes._207:
				case MessageTypeList.Codes._301:
				case MessageTypeList.Codes._31A:
				case MessageTypeList.Codes._31D:
				case MessageTypeList.Codes._401:
				case MessageTypeList.Codes._601:
				case MessageTypeList.Codes._603:
					return new TWControllingAgencyDeliveryNotificationMessageHelper(message);
				case MessageTypeList.Codes.TPC:
					return new N5110MessageHelper(message);
				case MessageTypeList.Codes._32D:
					return new NX302_DNMessageHelper(message);
				case MessageTypeList.Codes.TAD:
					return new N5111MessageHelper(message);
				case MessageTypeList.Codes.TRN:
					return new N5302MessageHelper(message);
				case MessageTypeList.Codes.FHR:
					return new N5108MessageHelper(message);
				case MessageTypeList.Codes._302:
					return new NX302MessageHelper(message);
				case MessageTypeList.Codes._102:
					return new NX102MessageHelper(message);
				case MessageTypeList.Codes._202:
					return new NX202MessageHelper(message);
				case MessageTypeList.Codes._602:
					return new NX602MessageHelper(message);
				case MessageTypeList.Codes._901:
					return new NX901MessageHelper(message);
				case MessageTypeList.Codes._902:
					return new NX902MessageHelper(message, bizObj);
				case MessageTypeList.Codes._903:
					return new NX903MessageHelper(message);
				case MessageTypeList.Codes._32A:
					return new NX302_AXMessageHelper(message);
				default:
					return null;
			}
		}

		public static TWMessageHelper NewOutgoingHelper(TWMessage message)
		{
			switch (message.EM_MessageType)
			{
				case MessageTypeList.Codes.ECD:
					return new N5203MessageHelper(message);
				case MessageTypeList.Codes.ICD:
				case MessageTypeList.Codes.CAA:
					return new NX5105MessageHelper(message);
				default:
					return null;
			}
		}

		#region Lookups
		public MessageHelperLookups Lookups => lookups ?? (lookups = new MessageHelperLookups(Message));
		MessageHelperLookups lookups;

		protected string GetDescriptionFromCode(string code, Func<string, string> getDescriptionFromCode)
		{
			var result = code;
			if (!string.IsNullOrEmpty(code))
			{
				var description = getDescriptionFromCode(code);
				if (!string.IsNullOrEmpty(description))
				{
					result = code + " " + description;
				}
			}
			return result;
		}

		public string GetCPT_025_ExtraCondition(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_025_ExtraConditionCodeList.GetDescriptionFromCode(itemCode));
		}

		public string GetModeofCustomsClearance(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.ModeofCustomsClearanceCodeList.GetDescriptionFromCode(itemCode));
		}

		public string GetProcessCode(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.ProcessCodeList.GetDescriptionFromCode(itemCode));
		}

		public string GetTypeofReleaseNote(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.TypeofReleaseNoteCodeList.GetDescriptionFromCode(itemCode));
		}

		public string GetUnabletoHandleContainer(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.UnabletoHandleContainerCodeList.GetDescriptionFromCode(itemCode));
		}

		public virtual string GetResponseCode(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.ResponseCodeList.GetDescriptionFromCode(itemCode));
		}

		public string GetFunctionCode(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.FunctionCodeList.GetDescriptionFromCode(itemCode));
		}

		public virtual string GetRejectionReason(string code)
		{
			return GetDescriptionFromCode(code, (itemCode
				) => Lookups.RejectionReasonList.GetDescriptionFromCode(itemCode));
		}

		public virtual string GetRequiredFormalities(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.RequiredFormalitiesList.GetDescriptionFromCode(itemCode));
		}

		public string GetICIRefCusCodeList(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.ICIRefCusCodeList.GetDescriptionFromCode(itemCode));
		}

		public string GetPROURefCusCodeList(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.PROURefCusCodeList.GetDescriptionFromCode(itemCode));
		}
		#endregion

		#region Write Message Html
		public virtual string ToHtml()
		{
			var table = new HtmlTableCreator(new NameValueCollection { { (NoResString)"border", "0" } }) { EnableHTMLEncoding = false };
			WriteTable(table);
			return table.ToHtml();
		}

		protected abstract void WriteTable(HtmlTableCreator table);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		public void WriteRow(HtmlTableCreator table, string name, string value)
		{
			var cell = new CellWithFormatting() { CellValue = FormattableString.Invariant($"<span style='font-weight:bold;text-decoration:underline;'>{name}:</span><span>&nbsp;{value}</span>") };
			cell.HtmlAttributes.Add((NoResString)"style", "font-size: 12px;color: #000000;border:0px;margin: 10px;");
			table.WriteRow(cell);
		}

		protected void WriteRow(HtmlTableCreator table, string name, decimal? value, bool shouldWriteWhenZeor = false)
		{
			var shouldWrite = shouldWriteWhenZeor || (value.HasValue && value.Value != 0);
			var stringValue = shouldWrite
				? (value != null ? value.Value.ToString() : "0")
				: string.Empty;
			WriteRow(table, name, stringValue);
		}

		protected void WriteRowWithTwoCells(HtmlTableCreator table, string name, IEnumerable<string> value, string style = CellStyle)
		{
			WriteRowWithTwoCells(table, name, (value?.Any() ?? false) ? string.Join("", value) : string.Empty, style);
		}

		protected void WriteRowWithTwoCells(HtmlTableCreator table, string name, decimal? value, bool shouldWriteWhenZeor = false)
		{
			var shouldWrite = shouldWriteWhenZeor || (value.HasValue && value.Value != 0);
			var stringValue = shouldWrite
				? (value != null ? value.Value.ToString() : "0")
				: string.Empty;
			WriteRowWithTwoCells(table, name, stringValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected void WriteRowWithTwoCells(HtmlTableCreator table, string name, string value, string style = CellStyle, bool shouldWriteWhenEmpty = false)
		{
			if (shouldWriteWhenEmpty || !string.IsNullOrEmpty(value))
			{
				var leftCell = new CellWithFormatting() { CellValue = name };
				leftCell.HtmlAttributes.Add("style", FormattableString.Invariant($"text-align:right;{style}"));
				leftCell.HtmlAttributes.Add("width", "50%");
				var rightCell = new CellWithFormatting() { CellValue = value };
				rightCell.HtmlAttributes.Add("style", FormattableString.Invariant($"text-align:left;{style};border-right:.5pt solid #AEAAAA;"));
				rightCell.HtmlAttributes.Add("width", "50%");
				table.WriteRow(new NameValueCollection { { "style", "height:15.0pt" } }, leftCell, rightCell);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		protected void WriteTitleRow(HtmlTableCreator table, string name, string style)
		{
			var cell = new CellWithFormatting() { CellValue = name };
			cell.HtmlAttributes.Add("style", FormattableString.Invariant($"font-family: Arial, sans-serif;text-align:center;{style}"));
			cell.HtmlAttributes.Add("colspan", "2");
			table.WriteRow(new NameValueCollection { { "style", "height:15.0pt" } }, cell);
		}

		protected string GetTtile(string name, string value) => FormattableString.Invariant($"{name}：{value}");

		public static class Captions
		{
			#region ARM (NX5106 - Response Message for Goods Declaration)
			public static string Remark => Res.GetString("{BC14FDA0-AB21-4F07-9505-7974CE1541DE}", "備註Remark");

			public static string ResponseCode => Res.GetString("{63268BCD-14FD-43AD-9272-37E71CF8FEA0}", "回應狀況代碼Response code");

			public static string DateAndTimeOfNotification => Res.GetString("{F0102EA3-1AE3-436F-8EE5-8E1226EF8290}", "通知日期與時間Date and time of notification");

			public static string GoodsItemNumber => Res.GetString("{F888ACA4-A30B-47D8-A6BA-672CE69BD78A}", "項次Goods item number");

			public static string Description => Res.GetString("{1C95BC99-AD7F-4822-9C19-EF1597F3B887}", "說明說明Description");

			public static string OfficeOfDeclaration => Res.GetString("{D209C387-B403-495B-B263-EB013C26F53B}", "受理單位Office of declaration");

			public static string ProcessingNumber => Res.GetString("{52055EE8-5327-4DB3-A45B-7F9129E60D02}", "收件編號Processing number");

			public static string ProcessingDateAndTime => Res.GetString("{6E5C9A63-6C22-42ED-99D6-0A170B423355}", "收件日期與時間Processing date and time");
			#endregion

			#region ERM (N5204 - Release Notice: Export Goods)
			public static string FunctionCode => Res.GetString("{15472BDA-AC52-4C9F-AC5F-0BCA0F41466B}", "訊息功能代碼Function Code");

			public static string ExtraRequirement => Res.GetString("{DC695AB5-553E-4A71-820A-65FDF2FC8858}", "放行附帶條件代碼Extra Requirement");

			public static string ModeOfCustomsClearance => Res.GetString("{65A87EFD-C2F8-40E4-9C5A-DD6F47ADE6BF}", "通關方式Mode of Customs Clearance");

			public static string ReleaseDateTime => Res.GetString("{FF398C06-2BF2-44BC-923B-AF3CD67C5B88}", "放行日期與時間Release Date Time");

			public static string PackageReleased => Res.GetString("{CA6E3A8D-74F9-4B95-A78A-732DEFE23F3F}", "放行件數Package Released");

			public static string PackageUnreleased => Res.GetString("{3D87296C-8480-4BE3-B987-3F853A2D8427}", "未放行件數Package Unreleased");

			public static string PackageUnit => Res.GetString("{9E80B2EE-EF02-43D2-B399-B824D1A300C6}", "件數單位Package Unit");

			public static string FullContainerNo => Res.GetString("{0CBA6AB5-B03B-4AE7-9FFB-EDF5AEA4904D}", "實櫃號碼Full Container No.");

			public static string ProcessCode => Res.GetString("{38CCEE26-A0C6-4F4C-8CF1-5286DF6EAC3C}", "處理註記Process Code");
			#endregion

			#region IEM (N5109 - Examination Required Notice)
			public static string NoticeNumber => Res.GetString("{F34D8D01-C0DD-475E-81D3-CED6F31C5448}", "查驗貨物通知編號Notice number");

			public static string ExaminationDispatchedDateAndTime => Res.GetString("{C52487A3-56F5-4659-A675-A0179AD1F9D1}", "派驗日期與時間Examination dispatched date and time");

			public static string ContainerNumber => Res.GetString("{7EE2B62E-FDC6-4561-858E-BCEDD51A3BA2}", "貨櫃號碼Container number");

			public static string InstrumentInspectionStationCoded => Res.GetString("{ACCBC035-BEA5-47F3-9474-58CD8709D31F}", "儀檢站代碼Instrument inspection station, coded");

			public static string PlaceOfPhysicalExaminationCoded => Res.GetString("{6AA92C59-36C9-4BF1-B39A-034820D2A750}", "查驗區代碼Place of physical examination, coded");

			public static string ExaminerCoded => Res.GetString("{A32A0038-ADB0-43E0-93BF-0DE7ABFE768E}", "驗貨關員代號Examiner, coded");
			#endregion

			#region IRM (N5116 - Import Goods Release Notice)
			public static string TypeOfReleaseNote => Res.GetString("{6341AA95-1632-4091-9133-60DF69BDD609}", "放行通知類別Type of release note");

			public static string ShortlandedNote => Res.GetString("{9FADA1EC-D292-4D2D-B34B-1E608A88FE0A}", "有否短卸Shortlanded note");

			public static string ExaminationNote => Res.GetString("{64303CF0-6933-4A2C-84C4-767B5BCA7868}", "有否查驗Examination note");
			#endregion

			#region RFM (N5107 - Documents Required Notice)
			public static string Deadline => Res.GetString("{D73FB34F-E26F-498B-9F4D-51BA0C9B2231}", "補辦期限Deadline");

			public static string RequiredFormalities => Res.GetString("{2EC44ECF-3A28-4996-B6BB-23CA04E31721}", "應補辦事項代碼Required formalities");
			#endregion

			#region UHC (N5168 - Unable to Handle Container Notice)
			public static string ReasonForUnableToHandleContainerCoded => Res.GetString("{B8D49052-3899-4FF7-B5F7-61F9BBD4FDEE}", "無法吊櫃原因代碼Reason for unable to handle container, coded");
			#endregion

			public static string EventTime => Res.GetString("{C548AC8F-9620-446F-B00D-2A98160B8255}", "訊息時間");

			public static string EventType => Res.GetString("{CD4288C6-093E-4DD2-AB80-013C0BB345F5}", "傳送結果");

			public static string ErrorCode => Res.GetString("{75109AE3-910B-4D5E-B7CF-2CAF2469392D}", "錯誤代碼");

			public static string ErrorDescription => Res.GetString("{BFB39333-452A-4E22-99D2-92B76A9A4A9A}", "錯誤日誌");

			public static string ErrorSuggestion => Res.GetString("184B1D19-253C-4BF7-8737-1013F13C33C5", "建議");

			public static string EntryNumber => Res.GetString("{9A7DDE6A-CFA0-486E-99E6-CCE7ED6E4093}", "報單號碼");

			public static string FunctionalReferenceID => Res.GetString("{E5BFB2E0-0FF6-45F0-AB59-7B8941DDA228}", "訊息編號");

			public static string EntryNumberType => Res.GetString("{2E9DBC32-96DE-4986-8EE7-E48662BBE409}", "進出口別");

			public static string MessageType => Res.GetString("{8239212B-2D22-4D06-B393-8CCD8A3F373C}", "訊息類別");

			public static string EDIInterchangeNumber => Res.GetString("{397B9E07-5C57-43BB-93FA-0B38E007D659}", "原Interchange號碼");
		}

		public static class MessageTypeDescriptions
		{
			public static string ECD => Res.GetString("{C3FE7977-0AC1-4C97-8467-A4C65835D4C9}", "出口報單 N5203");
			public static string ICD => Res.GetString("{138E01D7-4B8A-4434-91B7-632CB8386167}", "進口報單 NX5105");
			public static string ADM => Res.GetString("{7BE238BD-EC6C-4647-A63F-2023628E7A3C}", "檢附申辦文件訊息 NX5901");
			public static string IEA => Res.GetString("{DDD11E6C-94C5-4630-88CE-11B8F6D2B353}", "進口貨物查驗申請書 N5167");
			public static string FHM => Res.GetString("{F557B742-B045-4479-B6E7-FE9E0FAE8B84}", "進口貨物分艙單 N5101H");
		}

		public static class EntryNumberTypeDescriptions
		{
			public static string Import => Res.GetString("{1043FAA3-4041-4DF3-B3DF-05601BFC00E0}", "進口");
			public static string Export => Res.GetString("{DD970691-8629-486D-A417-FD60B53D5439}", "出口");
		}
		#endregion

		#region link CusEntryHeader & CusInBondHeader & AsycudaManifestHeader
		public static CusEntryHeader LookForCusEntryHeader(BusinessObjectFactory factory, TWMessage message)
		{
			return LookForCusEntryHeader(factory, message?.EntryNumber ?? ZString.Empty, message?.EntryType ?? ZString.Empty);
		}

		public static CusEntryHeader LookForCusEntryHeader(BusinessObjectFactory factory, ZString entryNum, ZString entryType)
		{
			if (entryNum.IsEmpty || entryType.IsEmpty)
			{
				return null;
			}
			var dbQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNum);
			subQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			dbQuery.AddSubQuery(subQuery, JoinCondition.And);
			return factory.LoadTop1<CusEntryHeader>(dbQuery);
		}

		public static CusInBondHeader LookForCusInBondHeader(BusinessObjectFactory factory, ZString entryNum)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNum);
			subQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusInBondHeaderSchema.Constants.TableName);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Taiwan.Transhipment);
			dbQuery.AddSubQuery(subQuery, JoinCondition.And);
			return factory.LoadTop1<CusInBondHeader>(dbQuery);
		}

		public static CusTWControllingMessageHeader LookForCusTWControllingMessageHeader(BusinessObjectFactory factory, ZString functionalReferenceID, string entryNumber = "")
		{
			var dbQuery = new ZDBOnlyQuery(typeof(CusTWControllingMessageHeader));
			dbQuery.AddToFilter(CusTWControllingMessageHeaderSchema.TW1_FunctionalReferenceId, functionalReferenceID);

			if (!string.IsNullOrEmpty(entryNumber))
			{
				var entryHeaderSubQueryEntryNumberSubQuery = GetEntryNumberSubQuery(CusEntryHeaderSchema.Constants.TableName, entryNumber);
				var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_CEI_Instruction);
				entryHeaderSubQuery.AddSubQuery(entryHeaderSubQueryEntryNumberSubQuery, JoinCondition.And);

				var declSubQueryEntryNumberSubQuery = GetEntryNumberSubQuery(JobDeclarationSchema.Constants.TableName, entryNumber);
				var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.PK);
				entryInstructionSubQuery.AddSubQuery(CusEntryInstructionSchema.CEI_JE, declSubQueryEntryNumberSubQuery, JoinCondition.And);

				entryHeaderSubQuery.AddAsUnionQuery(entryInstructionSubQuery, true);
				dbQuery.AddSubQuery(CusTWControllingMessageHeaderSchema.TW1_CEI, entryHeaderSubQuery, JoinCondition.And);
			}
			return factory.LoadTop1<CusTWControllingMessageHeader>(dbQuery);
		}

		static ZDBOnlySubQuery GetEntryNumberSubQuery(ZString parentTable, ZString entryNumber)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			subQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, parentTable);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			return subQuery;
		}

		public static AsycudaManifestHeader LookForAsycudaManifestHeader(BusinessObjectFactory factory, ZString voyage, ZString masterBillNumber, ZString transportMode)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			ZString queryText = $@" AMA_ClusterKey =
			(
				SELECT
					TOP 1 AMA_ClusterKey
				FROM
					dbo.AsycudaManifestHeader
					JOIN dbo.AsycudaBill ON AMA_ClusterKey = ABL_ClusterKey
				WHERE
					AMA_Voyage = @Voyage
					AND AMA_TransportMode =  @TransportMode
					AND ABL_BolType = 'BOL'
					AND ABL_BillNumber = @masterBillNumber
				ORDER BY ABL_E_ARV DESC
			)";
			var parameters = new ZSqlParameterCollection
			{
				{ "@Voyage", voyage, AsycudaManifestHeaderSchema.AMA_Voyage },
				{ "@TransportMode", transportMode, AsycudaManifestHeaderSchema.AMA_TransportMode },
				{ "@masterBillNumber", masterBillNumber, AsycudaBillSchema.ABL_BillNumber }
			};

			dbQuery.AddFilterAndZSQLParameterCollection(queryText, parameters);
			return factory.Load<AsycudaManifestHeader>(dbQuery).FirstOrDefault();
		}

		public static AsycudaBill[] LookForAsycudaManifestBillsWithHeaderPK(BusinessObjectFactory factory, ZString functionalReferenceID, ZGuid headerPK)
		{
			var query = LookForAsycudaManifestBillsQuery(functionalReferenceID, true);
			query.AddToFilter(AsycudaBillSchema.ABL_AMA, headerPK);
			var bills = factory.Load<AsycudaBill>(query);
			return bills;
		}

		public static AsycudaBill[] LookForAsycudaManifestBills(BusinessObjectFactory factory, ZString entryNum)
		{
			var bills = factory.Load<AsycudaBill>(LookForAsycudaManifestBillsQuery(entryNum));
			return bills;
		}

		static ZDBOnlyQuery LookForAsycudaManifestBillsQuery(ZString entryNum, bool ignoreEntryNumWhenItIsEmpty = false)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			AddCusEntryNumFiltersForForwarderHouseManifest(subQuery, entryNum, ignoreEntryNumWhenItIsEmpty);
			dbQuery.AddSubQuery(subQuery, JoinCondition.And);
			return dbQuery;
		}

		public static CusEntryNumber LookForCusEntryNumber(BusinessObjectFactory factory, ZString entryNum)
		{
			if (entryNum.IsEmpty)
			{
				return null;
			}
			var dbQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			dbQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNum);
			dbQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			dbQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBillSchema.Constants.TableName);
			dbQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			dbQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, MessageTypeList.Codes.FHM);
			return factory.LoadTop1<CusEntryNumber>(dbQuery);
		}

		static void AddCusEntryNumFiltersForForwarderHouseManifest(ZQuery query, ZString entryNum, bool ignoreEntryNumWhenItIsEmpty)
		{
			if (!(ignoreEntryNumWhenItIsEmpty && entryNum.IsEmpty))
			{
				query.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNum);
			}
			query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBillSchema.Constants.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, MessageTypeList.Codes.FHM);
		}
		#endregion

		#region Get MessageCode & MessageType
		public static string GetMessageTypeByCode(string messageCode) => new MessageTypeCodeList().GetCodeFromDescription(messageCode) ?? string.Empty;

		public static ZString GetMessageTypeByXml(ZString xmlString)
		{
			var result = ZString.Empty;
			var pattern = (NoResString)@"(xmlns=""urn:wco:datamodel:TW:)(?<MessageType>NX\d{3,4}|N\d{4}|NX\d{3}_[A-Z]{2})(:R-)";
			var collection = Regex.Matches(xmlString, pattern, RegexOptions.IgnoreCase);
			if (collection.Count > 0 && collection[0].Groups["MessageType"] is Group messageType)
			{
				result = GetMessageTypeByCode(messageType.Value);
			}
			return new MessageTypeList().ContainsCode(result) ? result : ZString.Empty;
		}
		#endregion

		#region Helper
		public static string GetMessageTypeDescription(string messageType)
		{
			return new MessageTypeList().GetDescriptionFromCode(messageType);
		}

		public static string ValidMessageXML(ZString xml)
		{
			if (xml.IsEmpty)
			{
				return Res.GetString("9FDA032D-B1C4-4A9E-919A-527F16FEDD14", "The Message XML is Empty");
			}
			try
			{
				new XmlDocument().LoadXml(xml);
			}
			catch (XmlException)
			{
				return Res.GetString("572F3052-B2E2-48ED-8797-B89892B97EFD", "The Message XML is not a valid XML");
			}
			return string.Empty;
		}

		public static ZString Serialize<T>(T dataObj, bool removeIndentAndLineBreak = false)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = !removeIndentAndLineBreak;
			settings.NewLineOnAttributes = !removeIndentAndLineBreak;

			using (var stream = new StringWriter(CultureInfo.InvariantCulture))
			using (var xmlWritter = XmlWriter.Create(stream, settings))
			{
				var serializer = ZXmlSerializer.New(typeof(T));
				var xmlSerializerNamespaces = new XmlSerializerNamespaces();
				xmlSerializerNamespaces.Add("", "");
				serializer.Serialize(xmlWritter, dataObj, xmlSerializerNamespaces);
				return stream.ToString();
			}
		}

		public static TWMessageInfo CreateTWMessageInfoFromTWMessage(TWMessage message)
		{
			var twMessageInfo = new TWMessageInfo();

			if (message.EM_LinkedObject is ITWMessageInfoProvider provider)
			{
				twMessageInfo.EntryNumber = provider.EntryNumber;
				twMessageInfo.EntryNumberType = provider.EntryNumberType;
				twMessageInfo.StaffCode = provider.StaffCode;
				twMessageInfo.CompanyID = provider.CompanyID;
				twMessageInfo.PasswordType = provider.PasswordType;
			}

			twMessageInfo.MailBox = message.IsLicensingMessageDeliveryNotification ?
				ObjectFactory.New<ITWGlbCompanyWrapper>(GlbCompany.CurrentCompany).LicensingCertificate.GP_MailBoxID
				: message.EM_ApplicationReference;
			twMessageInfo.MessageType = message.EM_MessageType;

			return twMessageInfo;
		}

		#endregion

		public static void FillMessagePlaceHolder(EDIMessage tWMessage, ZString oldValue, ZString newValue)
		{
			var messageText = tWMessage.EM_MessageText;
			if (messageText.IndexOf(oldValue, StringComparison.Ordinal) != -1)
			{
				messageText = messageText.Replace(oldValue, newValue);
			}
			var placeHolderHtml = WebUtility.HtmlEncode(oldValue);
			if (messageText.IndexOf(placeHolderHtml, StringComparison.Ordinal) != -1)
			{
				messageText = messageText.Replace(placeHolderHtml, newValue);
			}
			if (tWMessage.EM_MessageText != messageText)
			{
				tWMessage.EM_MessageText = messageText;
			}
		}

		public static void UpdateAsycudaHeaderMessageStatusFromBills(AsycudaManifestHeader manifestHeader)
		{
			var messageStatuses = manifestHeader.Bills.Select(b => b.ABL_MessageStatus).Distinct();
			if (messageStatuses.Count() == 1)
			{
				manifestHeader.AMA_MessageStatus = messageStatuses.First();
			}
			else if (messageStatuses.Contains(TWMessageStatusCodeList.Codes.TransmissionError))
			{
				manifestHeader.AMA_MessageStatus = TWMessageStatusCodeList.Codes.TransmissionError;
			}
			else if (messageStatuses.Contains(TWMessageStatusCodeList.Codes.NotSent))
			{
				manifestHeader.AMA_MessageStatus = TWMessageStatusCodeList.Codes.NotSent;
			}
			else if (messageStatuses.Contains(TWMessageStatusCodeList.Codes.Unknown))
			{
				manifestHeader.AMA_MessageStatus = TWMessageStatusCodeList.Codes.Unknown;
			}
		}

		public static void UpdateAsycudaHeaderCustomsStatusFromBills(AsycudaManifestHeader manifestHeader)
		{
			var status = ZString.Empty;
			var customsStatuses = manifestHeader.Bills.Select(b => b.ABL_BillStatus).Distinct();
			if (customsStatuses.Count() == 1)
			{
				status = customsStatuses.First();
			}
			else if (customsStatuses.Contains(Constants.CustomsManifestStatus.RE))
			{
				status = Constants.CustomsManifestStatus.RE;
			}
			else if (customsStatuses.Contains(Constants.CustomsManifestStatus.EX))
			{
				status = Constants.CustomsManifestStatus.EX;
			}
			else if (customsStatuses.Contains(Constants.CustomsManifestStatus.AK))
			{
				status = Constants.CustomsManifestStatus.AK;
			}
			if (!status.IsEmpty)
			{
				var registrationEntryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, manifestHeader.AMA_RN_NKCountry);
				registrationEntryNumber.CE_EntryStatus = status;
			}
		}
	}
}
