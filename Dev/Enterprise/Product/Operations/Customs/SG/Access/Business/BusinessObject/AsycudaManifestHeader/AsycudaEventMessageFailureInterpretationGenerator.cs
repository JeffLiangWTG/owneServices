using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.SG.Access.Business
{
	internal class AsycudaEventMessageFailureInterpretationGenerator : ASYCUDA.Business.AsycudaEventMessageInterpretationGenerator
	{
		public AsycudaEventMessageFailureInterpretationGenerator(BusinessObjectFactory factory, Event universalEvent)
			: base(universalEvent)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		protected override string ActionPurposeCodeDesc => "AIR" + universalEvent.DataContext.ActionPurposeCode + " SG ACCESS";

		protected override List<KeyValuePair<string, string>> GetContentFields()
		{
			return new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Manifest Country", "ManifestCountry"),
				new KeyValuePair<string, string>("Original Interchange Number", "OriginalInterchangeNumber"),
				new KeyValuePair<string, string>("UEN Number", "UENNumber"),
				new KeyValuePair<string, string>("Original Create Date", "OriginalCreateDate" ),
				new KeyValuePair<string, string>("Original Serial Number", "OriginalSerialNumber"),
				new KeyValuePair<string, string>("Batch Date", "BatchDate")
			};
		}

		protected override void WriteConsignmentReference(Context consignmentReference, HtmlTableCreator tableCreator)
		{
			var consignmentNumber = FindContextValue("ConsignmentNumber", consignmentReference.SubContextCollection);
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("Consignment", ColspanBold(2)),
				new CellWithFormatting(consignmentNumber, Colspan(5))
			);
		}

		protected override void WriteHouseBills(Context masterBill, HtmlTableCreator tableCreator)
		{
			var eventVersion = FindContextValue("Version", universalEvent.ContextCollection) ?? ZString.Empty;
			var isImportJob = universalEvent.EventReference?.Contains(ImportManifestEventReference, StringComparison.OrdinalIgnoreCase) ?? ZBool.False;
			var houseBills = FindContext("HouseBill", masterBill.SubContextCollection).ToArray();
			if (eventVersion == "2" && isImportJob)
			{
				WriteImportHouseBills(houseBills, tableCreator);
			}
			else
			{
				WriteExportHouseBills(houseBills, tableCreator);
			}
		}

		void WriteExportHouseBills(Context[] houseBills, HtmlTableCreator tableCreator)
		{
			var houseBillsGrouped =
				from houseBill in houseBills
				let errorConsignmentReferences = FindContext("ConsignmentReference", houseBill.SubContextCollection).FirstOrDefault(x => !string.IsNullOrEmpty(FindContextValue("ErrorDescription", x.SubContextCollection)))?.SubContextCollection
				group errorConsignmentReferences
				by new
				{
					houseBill.Type.Type,
					value = houseBill.Value,
					MessageStatusCode = FindContextValue("MessageStatusCode", houseBill.SubContextCollection),
					ManifestPermitNumber = FindContextValue("ManifestPermitNumber", errorConsignmentReferences),
					ConsignmentNumber = FindContextValue("ConsignmentNumber", errorConsignmentReferences)
				} into grouped
				select new
				{
					grouped.Key.value,
					grouped.Key.MessageStatusCode,
					grouped.Key.ManifestPermitNumber,
					grouped.Key.ConsignmentNumber,
					errors = grouped
				};

			foreach (var consignment in houseBillsGrouped)
			{
				tableCreator.WriteRowWithFormatting();

				tableCreator.WriteRowWithFormatting(
					new CellWithFormatting("House Bill", ColspanBold(2)),
					new CellWithFormatting(consignment.value, Colspan(2)),
					new CellWithFormatting(consignment.MessageStatusCode, Colspan(2)),
					new CellWithFormatting(consignment.ManifestPermitNumber)
				);

				tableCreator.WriteRowWithFormatting(
					new CellWithFormatting("Consignment", ColspanBold(2)),
					new CellWithFormatting(consignment.ConsignmentNumber, Colspan(5))
				);
				foreach (var errorInfo in consignment.errors)
				{
					var code = FindContextValue("MessageStatusCode", errorInfo);
					var errCode = FindContextValue("ErrorCode", errorInfo);
					var errDesp = FindContextValue("ErrorDescription", errorInfo);
					var segmentGroup = FindContextValue("SegmentGroup", errorInfo);
					var groupOccuranceNumber1 = FindContextValue("GroupOccuranceNumber1", errorInfo);
					var groupOccuranceNumber2 = FindContextValue("GroupOccuranceNumber2", errorInfo);
					var ordinalNumber1 = FindContextValue("OrdinalNumber1", errorInfo);
					var segmentTag = FindContextValue("SegmentTag", errorInfo);

					tableCreator.WriteRow(code, errCode, errDesp, segmentGroup, groupOccuranceNumber1, groupOccuranceNumber2.IsNullOrEmpty() ? ordinalNumber1 : groupOccuranceNumber2, segmentTag);
					AddZZErrorCommentary(errCode, tableCreator);
				}
			}
		}

		void WriteImportHouseBills(Context[] houseBills, HtmlTableCreator tableCreator)
		{
			foreach (var houseBill in houseBills)
			{
				tableCreator.WriteRowWithFormatting();

				tableCreator.WriteRowWithFormatting(
					new CellWithFormatting("House Bill", ColspanBold(2)),
					new CellWithFormatting(houseBill.Value, Colspan(2)),
					new CellWithFormatting(FindContextValue("MessageStatusCode", houseBill.SubContextCollection), Colspan(2))
				);

				foreach (var consignment in FindContext("ConsignmentReference", houseBill.SubContextCollection).Where(x => !FindContextValue("ErrorCode", x.SubContextCollection).IsNullOrEmpty()))
				{
					var consignmentSubContexts = consignment.SubContextCollection;
					var errorCode = FindContextValue("ErrorCode", consignmentSubContexts);
					var groupOccuranceNumber2 = FindContextValue("GroupOccuranceNumber2", consignmentSubContexts);

					tableCreator.WriteRowWithFormatting(
						new CellWithFormatting("Consignment", ColspanBold(2)),
						new CellWithFormatting(consignment.Value, Colspan(5))
					);

					tableCreator.WriteRow(
						FindContextValue("MessageStatusCode", consignmentSubContexts),
						errorCode,
						FindContextValue("ErrorDescription", consignmentSubContexts),
						FindContextValue("SegmentGroup", consignmentSubContexts),
						FindContextValue("GroupOccuranceNumber1", consignmentSubContexts),
						groupOccuranceNumber2.IsNullOrEmpty() ? FindContextValue("OrdinalNumber1", consignmentSubContexts) : groupOccuranceNumber2,
						FindContextValue("SegmentTag", consignmentSubContexts));

					AddZZErrorCommentary(errorCode, tableCreator);
				}
			}
		}

		void AddZZErrorCommentary(ZString errorCode, HtmlTableCreator tableCreator)
		{
			ZString countryCode = FindContextValue("ManifestCountry", universalEvent.ContextCollection);
			if (!errorCode.IsEmpty && !countryCode.IsEmpty)
			{
				var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary;
				var commentary = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, errorCode, countryCode, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
				if (!commentary.IsEmpty)
				{
					tableCreator.WriteRowWithFormatting(
						new CellWithFormatting(),
						new CellWithFormatting(commentary, Colspan(6))
					);
				}
			}
		}

		const string ImportManifestEventReference = "MST=MGI";
	}
}
