using System.Collections.Generic;

using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class BondDataValueProvider
	{
		public BondDataValueProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public IEnumerable<IDISBondDataDefault> BondData
		{
			get
			{
				if (!declaration.IsRecon)
				{
					if (!declaration.US_BondType.IsEmpty && declaration.US_BondType != BondTypeList.Codes.NoBondRequired)
					{
						yield return new BondDataWrapper()
						{
							BondType = declaration.US_BondType,
							SuretyCode = declaration.US_SuretyCode,
							BondNumber = declaration.US_BondProducerAccNo,
							BondAmount = declaration.US_BondAmount,
							Filer = declaration.US_EntryFilerCode,
							BondName = GetBondNameTypeFromBondType(declaration.US_BondType),
							Code = BondDataDefaultCode.Bond1,
							Description = GetDescription(declaration.US_BondType, declaration.US_SuretyCode, declaration.US_BondProducerAccNo, declaration.US_BondAmount)
						};
					}

					if (!declaration.US_BondType2.IsEmpty && declaration.US_BondType2 != BondTypeList.Codes.NoBondRequired)
					{
						yield return new BondDataWrapper()
						{
							BondType = declaration.US_BondType2,
							SuretyCode = declaration.US_ADDCVDSuretyCode,
							BondNumber = declaration.US_BondProducerAccNo2,
							BondAmount = declaration.US_BondAmount2,
							Filer = declaration.US_EntryFilerCode,
							BondName = GetBondNameTypeFromBondType(declaration.US_BondType2),
							Code = BondDataDefaultCode.Bond2,
							Description = GetDescription(declaration.US_BondType2, declaration.US_ADDCVDSuretyCode, declaration.US_BondProducerAccNo2, declaration.US_BondAmount2)
						};
					}
				}
			}
		}

		static BondNameType GetBondNameTypeFromBondType(ZString bondType)
		{
			return bondType == BondTypeList.Codes.SingleTransactionBond ? BondNameType.Single : BondNameType.Other;
		}

		static string GetDescription(ZString bondType, ZString suretyCode, ZString accountNo, ZDecimal bondAmount)
		{
			var result = "";
			switch (bondType)
			{
				case BondTypeList.Codes.SingleTransactionBond:
					result = string.Format(SingleBondTypeDesc, bondType, accountNo, bondAmount.ToString(2));
					break;

				case BondTypeList.Codes.ContinuousBond:
					result = string.Format(ContinuousBondTypeDesc, bondType, suretyCode);
					break;
			}
			return result;
		}

		const string SingleBondTypeDesc = "Bond Type: {0}, Account No: {1}, Bond Amount: {2}";
		const string ContinuousBondTypeDesc = "Bond Type: {0}, Surety Code: {1}";

		class BondDataWrapper : IDISBondDataDefault
		{
			public ZString AgentIDNumber
			{
				get;
				internal set;
			}

			public ZDecimal BondAmount
			{
				get;
				internal set;
			}

			public BondNameType BondName
			{
				get;
				internal set;
			}

			public ZString BondNumber
			{
				get;
				internal set;
			}

			public ZString BondType
			{
				get;
				internal set;
			}

			public ZString Filer
			{
				get;
				internal set;
			}

			public ZString SuretyCode
			{
				get;
				internal set;
			}

			public ZString Code
			{
				get;
				internal set;
			}

			public ZString Description
			{
				get;
				internal set;
			}
		}
	}
}
