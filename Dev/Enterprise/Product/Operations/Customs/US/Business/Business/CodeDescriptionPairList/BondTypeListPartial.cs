using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	partial class BondTypeList
	{
		public static bool IsRelevantFor(string bondType, string fieldName)
		{
			bool result = true;

			switch (fieldName)
			{
				case USAddInfoSchema.Constants.US_BondAmount:
				case USAddInfoSchema.Constants.US_BondAmount2:
				case USAddInfoSchema.Constants.US_BondDispositionCode:
				case USAddInfoSchema.Constants.US_BondDispositionCode2:
				case USAddInfoSchema.Constants.US_CBPBondNo:
				case USAddInfoSchema.Constants.US_CBPBondNo2:
					return bondType == Codes.SingleTransactionBond;

				case USAddInfoSchema.Constants.US_BondSuperseding:
					return bondType == Codes.ContinuousBond;

				case USAddInfoSchema.Constants.US_SuretyCode:
				case USAddInfoSchema.Constants.US_ADDCVDSuretyCode:
				case USAddInfoSchema.Constants.US_BondProducerAccNo:
				case USAddInfoSchema.Constants.US_BondProducerAccNo2:
					return bondType == Codes.ContinuousBond || bondType == Codes.SingleTransactionBond;
			}

			return result;
		}
	}
}
