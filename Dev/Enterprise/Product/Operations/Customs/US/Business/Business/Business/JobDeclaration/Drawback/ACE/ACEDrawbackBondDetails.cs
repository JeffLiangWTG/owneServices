using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackBondDetails : IACEDrawbackBondInfo
	{
		public ACEDrawbackBondDetails(ZString bondTypeCode, ZString designationTypeCode, ZString suretyCode, ZDecimal bondAmount, ZString producerAccountNumber)
		{
			this.bondTypeCode = bondTypeCode;
			this.designationTypeCode = designationTypeCode;
			this.suretyCode = suretyCode;
			this.bondAmount = bondAmount;
			this.producerAccountNumber = producerAccountNumber;
		}
		readonly ZString bondTypeCode;
		readonly ZString designationTypeCode;
		readonly ZString suretyCode;
		readonly ZDecimal bondAmount;
		readonly ZString producerAccountNumber;

		#region IACEDrawbackBondInfo Members

		ZString IACEDrawbackBondInfo.BondType
		{
			get { return bondTypeCode; }
		}

		ZString IACEDrawbackBondInfo.BondDesignationTypeCode
		{
			get { return designationTypeCode; }
		}

		ZString IACEDrawbackBondInfo.SuretyCode
		{
			get { return suretyCode; }
		}

		ZDecimal IACEDrawbackBondInfo.BondAmount
		{
			get { return IsContinuousBond ? ZDecimal.Zero : bondAmount; }
		}

		ZString IACEDrawbackBondInfo.ProducerAccountNumber
		{
			get { return IsContinuousBond ? ZString.Empty : producerAccountNumber; }
		}

		ZBool IsContinuousBond
		{
			get { return bondTypeCode == BondTypeList.Codes.ContinuousBond; }
		}

		#endregion
	}
}
