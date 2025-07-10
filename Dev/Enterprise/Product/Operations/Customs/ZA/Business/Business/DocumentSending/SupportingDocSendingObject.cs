using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders.DocumentSending;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class SupportingDocSendingObject : JobDeclarationSupportingDocSendingObject, IZASupportingDocumentMessageDataProvider
	{
		public SupportingDocSendingObject(JobDeclaration declaration) : base(declaration)
		{
		}

		public new IZASupportingDocSendingObject SupportingDocObject => (IZASupportingDocSendingObject)base.SupportingDocObject;

		#region Override Properties

		[List(nameof(Entries))]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.SupportingDocSendingObject|LocalReferenceNumber", ShortCaption = "Entry (LRN)", Caption = "Entry (LRN)")]
		public override ZString LocalReferenceNumber
		{
			get { return base.LocalReferenceNumber; }
			set
			{
				var oldValue = LocalReferenceNumber;
				base.LocalReferenceNumber = value;
				if (oldValue != LocalReferenceNumber)
				{
					header = null;
					DefaultCaseNumber();
				}
			}
		}

		[List(nameof(CaseNumbers))]
		public override ZString CaseNumber
		{
			get { return base.CaseNumber; }
			set { base.CaseNumber = value; }
		}

		#endregion

		#region New Properties

		protected override Customs.Business.CusEntryHeader GetEntryHeader()
		{
			if (header == null && !LocalReferenceNumber.IsEmpty)
			{
				header = ((JobDeclaration)SupportingDocObject).ActiveEntryHeaders?.Cast<CusEntryHeader>().FirstOrDefault(x => x.CH_BGMReference == LocalReferenceNumber);
			}
			return header;
		}

		#endregion

		#region Lookup Lists

		protected override ZString DocumentTypeCode => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ZADocumentType;

		public override CodeDescriptionPairList Entries
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (CusEntryHeader entryHeader in ((JobDeclaration)SupportingDocObject).ActiveEntryHeaders)
				{
					result.Add(new CodeDescriptionPair(entryHeader.CH_BGMReference.ToString(), "Procedure Code: " + entryHeader.CustomsProcedureCode));
				}
				return result;
			}
		}

		protected override List<ZString> ExtensionFilter => new List<ZString> { Core.Constants.FileFormats.PDF };

		public override CodeDescriptionPairList CaseNumbers
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var caseNumbers = ((CusEntryHeader)Header)?.EntryInstruction?.CaseNumbers;
				if (caseNumbers != null)
				{
					foreach (CaseNumber caseNumber in caseNumbers)
					{
						result.Add(new CodeDescriptionPair(caseNumber.CY_Data.ToString(), caseNumber.CY_Code.ToString()));
					}
				}
				return result;
			}
		}

		public ZString DualProfileCode => SupportingDocObject.AgentDualProfileCode;

		public ZString TradingPartyID => SupportingDocObject.TradingPartyID;

		#endregion

		#region Implementation

		protected override void DefaultDocType()
		{
			if (Document != null)
			{
				DocumentType = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.SouthAfrica, RefCusMapTypeList.Codes.ZADOC, Document.DocType, CusEntryInstruction.GetEffectiveAssessmentDate(((CusEntryHeader)Header)?.EntryInstruction, Factory));
			}
		}

		protected override void DefaultLocalReferenceNumber()
		{
			if (((JobDeclaration)SupportingDocObject).ActiveEntryHeaders.Count == 1)
			{
				LocalReferenceNumber = ((JobDeclaration)SupportingDocObject).ActiveEntryHeaders[0]?.CH_BGMReference ?? ZString.Empty;
				DefaultCaseNumber();
			}
		}

		void DefaultCaseNumber()
		{
			if (!LocalReferenceNumber.IsEmpty)
			{
				var caseNumbers = ((CusEntryHeader)Header)?.EntryInstruction?.CaseNumbers;
				if (caseNumbers != null && caseNumbers.Count == 1)
				{
					CaseNumber = caseNumbers[0].CY_Data.SubstringSafe(0, CaseNumberInfo.MaxLength);
				}
			}
		}

		#endregion

		protected ZString GetDualProfileCode() => SupportingDocObject.AgentDualProfileCode;

		protected ZString GetTradingPartyID() => SupportingDocObject.TradingPartyID;

		protected override Customs.Business.SupportingDocSendingObjectValidation GetNewValidation()
		{
			return new SupportingDocSendingObjectValidation(this);
		}

		protected override Customs.Business.AvailableEDocList GetAvailableEDocList(List<ZString> filter, params IStorageDocsBaseCollection[] eDocCollections)
		{
			return new AvailableEDocList(ExtensionFilter, eDocCollections);
		}

		public override SupportingDocUniversalEventBuilder GetSupportingDocUniversalEventBuilder()
		{
			return new ZASupportingDocUniversalEventBuilder(this);
		}
	}
}
