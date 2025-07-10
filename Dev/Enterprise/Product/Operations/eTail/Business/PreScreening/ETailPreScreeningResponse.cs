using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.eTail.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business
{
	public class ETailPreScreeningResponse : IETailPreScreeningResponse
	{
		public void AddPreScreeningResult(IHVLVConsignmentPreScreeningResult result)
		{
			PreScreeningResults.Add(result);
		}

		public bool Finished => string.IsNullOrEmpty(ErrorMessage);

		public bool Passed => Results != null && Results.All(n => n.PreScreeningStatus != HVLVConsignmentPreScreeningStatusCodes.Codes.Failed);

		public string ErrorMessage { get; protected set; }

		public ReadOnlyCollection<IHVLVConsignmentPreScreeningResult> Results => Finished ? new ReadOnlyCollection<IHVLVConsignmentPreScreeningResult>(PreScreeningResults) : null;

		List<IHVLVConsignmentPreScreeningResult> PreScreeningResults => preScreeningResults ?? (preScreeningResults = new List<IHVLVConsignmentPreScreeningResult>());

		public string ErrorMessageDetail
		{
			get => errorMessageDetail ?? ErrorMessage;
			set => errorMessageDetail = value;
		}
		string errorMessageDetail;

		List<IHVLVConsignmentPreScreeningResult> preScreeningResults;
	}

	public class ETailPreScreeningConsignmentNotFoundResponse : ETailPreScreeningResponse
	{
		public ETailPreScreeningConsignmentNotFoundResponse(Guid consignmentPK)
		{
			ErrorMessage = Res.GetString("1b803ccc-6601-48f6-b422-4fd8fe0d9593", "Request failed: Consignment {0} not found", consignmentPK);
		}
	}

	public class ETailPreScreeningRegistryDisabledResponse : ETailPreScreeningResponse
	{
		public ETailPreScreeningRegistryDisabledResponse()
		{
			ErrorMessage = Res.GetString("c2694187-50fe-4176-ade2-df9b98d31eb0", "Request failed: HVLV Pre-Screening is disabled");
			ErrorMessageDetail = Res.GetString("69bfdbbd-28a5-4969-9c8a-42098287d72b", @"HVLV Pre-Screening has not been enabled.
To enable go to Registry -> {0} -> {1}.", HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.Inner.Location, HVLVDetailsPreScreeningConfiguration.IsEnabledUICaption.Caption);
		}
	}

	public class ETailPreScreeningUnsupportedConsignmentParentTypeResponse : ETailPreScreeningResponse
	{
		public ETailPreScreeningUnsupportedConsignmentParentTypeResponse(string consignmentParentTableCode)
		{
			ErrorMessage = Res.GetString("a0b4ff65-eeb0-4be7-abfc-83a0d99e63e5", "Request failed: Unsupported consignment parent type {0}", consignmentParentTableCode);
		}
	}

	public class ETailPreScreeningConsignmentParentNotFoundResponse : ETailPreScreeningResponse
	{
		public ETailPreScreeningConsignmentParentNotFoundResponse(string consignmentParentTableCode, Guid consignmentParentPK)
		{
			ErrorMessage = Res.GetString("5d7d59fb-0b09-485f-afde-c735b3ffee50", "Request failed: Consignment parent {0} not found with table code {1}", consignmentParentPK, consignmentParentTableCode);
		}
	}

	public class ETailPreScreeningEmptyConsignmentCollectionResponse : ETailPreScreeningResponse
	{
		public ETailPreScreeningEmptyConsignmentCollectionResponse(string consignmentParentTableCode, Guid consignmentParentPK)
		{
			ErrorMessage = Res.GetString("9645c226-6e66-4658-bf9b-3ef871545b4b", "Request failed: Consignment parent {0} with table code {1} has no consignment", consignmentParentPK, consignmentParentTableCode);
		}
	}
}
