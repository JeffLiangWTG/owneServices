using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class SupportingDocument : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SupportingDocument(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public SupportingDocument(BusinessObjectFactory factory, CodeDescriptionPairList typeList, CodeDescriptionPairList billNumberList = null)
			: base(factory)
		{
			this.typeList = typeList;
			this.billNumberList = billNumberList;
		}

		#region Contants
		public static class Constants
		{
			public static string MaxTotalFileSize200MB => Res.GetString("FF457BC4-7D2B-4880-83D1-C255051DE43E", "Maximum file size for all document: 200 MB");
			public static string MaxFileSize10MB => Res.GetString("535B64E0-03EA-4C6F-B6E5-A6F1248A9912", "Maximum file size for each document: 10 MB");
			public static string OnlyAllowAlphanumeric => Res.GetString("8B129114-003F-4D74-97CB-75DDC089BD45", "Only alphanumeric characters are allowed.");
			public static string OnlyOneCanBeSent => Res.GetString("8990C05E-7139-4A65-BADC-3D3575A50100", "This document has already been selected.");
		}
		#endregion

		#region EDoc
		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|EDoc", Caption = "Document")]
		public ZGuid EDoc
		{
			get { return edoc; }
			set
			{
				SetNonPersistentPropertyValue(EDocInfo, ref edoc, value);
				if (!IsValidationSuspended)
				{
					ValidateEDoc();
				}
			}
		}
		ZGuid edoc;

		public ZPropertyInfo EDocInfo => GetZPropertyInfo(nameof(EDoc));

		protected void ValidateEDoc()
		{
			EDocInfo.ClearAllNotifications();
			ValidateEDocCore();
		}

		protected virtual void ValidateEDocCore()
		{
			MandatoryValidation.CheckEntered(EDocInfo);
			ListValidation.ErrorIfInvalidPK(EDocInfo, StorageDocs);

			if (ParentCollections?.Count == 1)
			{
				int occuranceOfEdoc = 0;
				var collection = ParentCollections.First().Cast<SupportingDocument>();
				foreach (var des in collection)
				{
					if (des.EDoc == EDoc)
					{
						occuranceOfEdoc++;
						if (occuranceOfEdoc > 1)
						{
							EDocInfo.AddError(Constants.OnlyOneCanBeSent);
							return;
						}
					}
				}
			}

			if (DocumentSize >= (10 * 1024 * 1024))
			{
				EDocInfo.AddError(Constants.MaxFileSize10MB);
			}

			var totalSize = ParentCollection?.Cast<SupportingDocument>().Sum(c => c.DocumentSize) ?? 0;
			if (totalSize >= (200 * 1024 * 1024))
			{
				EDocInfo.AddError(Constants.MaxTotalFileSize200MB);
			}
		}

		#region StorageDocs

		public CodeDescriptionPairList StorageDocs => ParentCollection?.StorageDocs;

		protected virtual SupportingDocumentCollection ParentCollection => ParentCollections?.OfType<SupportingDocumentCollection>().FirstOrDefault();

		long DocumentSize => GetDocumentSizeCore();

		protected virtual long GetDocumentSizeCore()
		{
			var doc = ParentCollection?.GetDocs()?.FirstIeDoc();
			return doc != null ? doc.ImageData.Length : 0;
		}

		#endregion
		#endregion

		#region DocumentNo
		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|DocumentNo", Caption = "Document No")]
		[MaxLength(50)]
		public ZString DocumentNo
		{
			get { return documentNo; }
			set
			{
				CheckMaximumLength(DocumentNoInfo, value);
				SetNonPersistentPropertyValue(DocumentNoInfo, ref documentNo, value);
				if (!IsValidationSuspended)
				{
					ValidateDocumentNo();
				}
			}
		}
		ZString documentNo;

		public ZPropertyInfo DocumentNoInfo => GetZPropertyInfo(nameof(DocumentNo));

		protected void ValidateDocumentNo()
		{
			DocumentNoInfo.ClearAllNotifications();
			ValidateDocumentNoCore();
		}

		protected virtual void ValidateDocumentNoCore()
		{
			if (!IsAlphaNumeric(DocumentNo))
			{
				DocumentNoInfo.AddError(Constants.OnlyAllowAlphanumeric);
			}
		}

		#endregion

		#region Remarks
		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|Remarks", Caption = "Remarks")]
		[MaxLength(60)]
		public ZString Remarks
		{
			get { return remarks; }
			set
			{
				CheckMaximumLength(RemarksInfo, value);
				SetNonPersistentPropertyValue(RemarksInfo, ref remarks, value);
			}
		}
		ZString remarks;

		public ZPropertyInfo RemarksInfo => GetZPropertyInfo(nameof(Remarks));

		#endregion

		#region ControllingAgency

		public ICodeDescriptionPairList ControllingAgencyList => TWRefCusCodeListTypes.GetControllingAgencyList(Factory);

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|ControllingAgency", Caption = "Control Agency")]
		[List(nameof(ControllingAgencyList))]
		[MaxLength(2)]
		public ZString ControllingAgency
		{
			get { return controllingAgency; }
			set
			{
				SetNonPersistentPropertyValue(ControllingAgencyInfo, ref controllingAgency, value);
				if (!IsValidationSuspended)
				{
					ValidateControllingAgency();
				}
			}
		}
		ZString controllingAgency;

		public ZPropertyInfo ControllingAgencyInfo => GetZPropertyInfo(nameof(ControllingAgency));

		protected void ValidateControllingAgency()
		{
			ControllingAgencyInfo.ClearAllNotifications();
			ValidateControllingAgencyCore();
		}

		protected virtual void ValidateControllingAgencyCore()
		{
			ListValidation.MessageErrorIfInvalidCode(ControllingAgencyInfo, ControllingAgencyList);
		}

		#endregion

		bool IsAlphaNumeric(string val)
		{
			return Regex.IsMatch(val, @"^[A-Z0-9]*$", RegexOptions.IgnoreCase);
		}

		#region Bill Number

		CodeDescriptionPairList billNumberList;

		public CodeDescriptionPairList BillNumberList => billNumberList ??= new CodeDescriptionPairList();

		ZString billNumber;

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|BillNumber", Caption = "Bill Number")]
		[List(nameof(BillNumberList))]
		public ZString BillNumber
		{
			get => billNumber;
			set
			{
				SetNonPersistentPropertyValue(BillNumberInfo, ref billNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateBillNumber();
				}
			}
		}

		public ZPropertyInfo BillNumberInfo => GetZPropertyInfo(nameof(BillNumber));

		void ValidateBillNumber()
		{
			BillNumberInfo.ClearAllNotifications();
			ValidateBillNumberCore();
		}

		protected virtual void ValidateBillNumberCore()
		{
		}

		#endregion

		#region Type & TypeList

		CodeDescriptionPairList typeList;

		public CodeDescriptionPairList TypeList => typeList ?? (typeList = new CodeDescriptionPairList());

		ZString type;

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|Type", Caption = "Type")]
		[List(nameof(TypeList))]
		public ZString Type
		{
			get => type;
			set
			{
				SetNonPersistentPropertyValue(TypeInfo, ref type, value);
				if (!IsValidationSuspended)
				{
					ValidateType();
				}
			}
		}

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		void ValidateType()
		{
			TypeInfo.ClearAllNotifications();
			ValidateTypeCore();
		}

		protected virtual void ValidateTypeCore()
		{
		}

		#endregion

		#region LineNumber

		ZInt lineNumber = 1;

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.SupportingDocument|LineNumber", Caption = "Document Line Number", ShortCaption = "Doc. Line No.", MediumCaption = "Document Line No.")]
		public ZInt LineNumber
		{
			get => lineNumber;
			set
			{
				if (SetNonPersistentPropertyValue(LineNumberInfo, ref lineNumber, value) && !IsValidationSuspended)
				{
					ValidateLineNumber();
				}
			}
		}

		public ZPropertyInfo LineNumberInfo => GetZPropertyInfo(nameof(LineNumber));

		void ValidateLineNumber()
		{
			LineNumberInfo.ClearAllNotifications();
			ValidateLineNumberCore();
		}

		protected virtual void ValidateLineNumberCore()
		{
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(LineNumberInfo);
		}

		#endregion

		public void ValidateAll()
		{
			ValidateEDoc();
			ValidateDocumentNo();
			ValidateControllingAgency();
			ValidateBillNumber();
			ValidateType();
			ValidateLineNumber();
		}
	}
}
