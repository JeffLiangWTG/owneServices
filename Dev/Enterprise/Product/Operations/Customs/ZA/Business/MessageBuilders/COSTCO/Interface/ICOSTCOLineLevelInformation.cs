using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface ICOSTCOLineLevelInformation : ICNI_ConsignmentInformation
		, IRFF_CargoCarrierCode
		, IRFF_MasterCargoCarrierCode
		, IRFF_ExternalReference
		, IRFF_MasterTransportDocumentNumber
		, IRFF_LRNExit
	{
		IEnumerable<ICOSTCOPackLineInformation> Packs { get; }
	}

	public interface ICNI_ConsignmentInformation
	{
		ZString LineNumber { get; }
		ZString TransportDocumentNumber { get; }
	}

	public interface IRFF_CargoCarrierCode
	{
		ZString CargoCarrierCode { get; }
	}

	public interface IRFF_ExternalReference
	{
		ZString ExternalReference { get; }
	}

	public interface IRFF_MasterCargoCarrierCode
	{
		ZString MasterCargoCarrierCode { get; }
	}

	public interface IRFF_MasterTransportDocumentNumber
	{
		ZString MasterBillOfLadingNumber { get; }
		ZString ConsolidationIndicator { get; }
	}

	public interface IRFF_LRNExit
	{
		ZString LRNExit { get; }
		ZString ExportProcedure { get; }
	}
}
