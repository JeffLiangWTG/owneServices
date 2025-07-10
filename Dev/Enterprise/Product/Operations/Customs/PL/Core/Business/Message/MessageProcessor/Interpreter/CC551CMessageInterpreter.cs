using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC551CMessageInterpreter(CusEntryHeader cusEntryHeader) : ImpExpMessageInterpreter<ICC551C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC551C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC551);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(paramValues: new ParamValueCollection {
			{ CommonStrings.MRN, dataProvider.MRN },
			{ CommonStrings.MessageSentOn, dataProvider.PreparationDateAndTime },
			{ CommonStrings.CustomsOfficeOfExport, dataProvider.CustomsOfficeOfExport?.ReferenceNumber },
			{ CommonStrings.CustomsOfficeOfPresentation, dataProvider.CustomsOfficeOfPresentation?.ReferenceNumber },
			{ CommonStrings.RefusalReason, dataProvider.OtherThingsToReport },
			{ CommonStrings.DateOfControl, dataProvider.ControlResult?.Date },
			{ CommonStrings.AdditionalRefusalRemark, dataProvider.ControlResult?.Text },
		});
		htmlWriter.WriteThematicBreak();

		var jobDocAddress = GetExporterJobDocAddressWithSupplierFallback();
		htmlWriter.WriteParamValueTable(
			caption: CaptionStrings.Exporter,
			paramValues: new ParamValueCollection {
				{ CommonStrings.EORI, dataProvider.Exporter?.IdentificationNumber },
				{ CommonStrings.Name, (dataProvider.Exporter?.Name).IfNullOrEmpty(() => GetName(jobDocAddress)) },
				{ CommonStrings.StreetAndAddress, (dataProvider.Exporter?.Address?.StreetAndNumber).IfNullOrEmpty(() => jobDocAddress.Address1) },
		});

		return;

		JobDocAddress GetExporterJobDocAddressWithSupplierFallback()
			=> Declaration.ExporterDocAddress.IsValidAddress
				? Declaration.ExporterDocAddress
				: Declaration.SupplierDocumentaryAddress;

		string GetName(JobDocAddress address) => address.E2_AddressOverride
			? address.E2_CompanyName
			: address.Address?.Header?.OH_FullName;
	}
}
