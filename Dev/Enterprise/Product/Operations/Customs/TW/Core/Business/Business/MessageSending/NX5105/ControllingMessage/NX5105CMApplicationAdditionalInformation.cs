using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMApplicationAdditionalInformation : IApplicationAdditionalInformation
	{
		public NX5105CMApplicationAdditionalInformation(CusTWControllingMessageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly CusTWControllingMessageHeader header;

		ZString IApplicationAdditionalInformation.StatementDescription => header.TW1_RequestDescription;

		ZString IApplicationAdditionalInformation.DeductionSample => header.TW1_SamplingReductionReason;

		ZString IApplicationAdditionalInformation.ElectronicReceipt => header.TW1_ElectronicReceipt ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		ZString IApplicationAdditionalInformation.ProvedPaper => header.TW1_ProofOfPaper ? YesNoList.Codes.Yes : string.Empty;

		ZString IApplicationAdditionalInformation.ReturnSample => header.TW1_ApplyForSampleReturn ? YesNoList.Codes.Yes : string.Empty;

		ZString IApplicationAdditionalInformation.AddressChineseLine => header.TW1_SampleReturnAddress;

		ZString IApplicationAdditionalInformation.BulkApplicationID => null;

		ZString IApplicationAdditionalInformation.BulkPortCode => null;
	}
}
