using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class AdditionalInformationDocWrapper : NonPersistentBusinessObject, IAdditionalInformation
	{
		public AdditionalInformationDocWrapper(IAdditionalInformation input)
		{
			if (input != null)
			{
				Code = input.Code;
				Value = input.Value;
				Group = input.Group;
			}
		}

		public AdditionalInformationDocWrapper(IAdditionalInformation input, ZDateTime dateOfAssessment, BusinessObjectFactory factory) : this(input)
		{
			if (input != null)
			{
				Code = ParseCode(input.Code, dateOfAssessment, factory);
			}
		}

		public AdditionalInformationDocWrapper(ZString input)
		{
			Code = input.SubstringSafe(0, 3);
			Value = input.SubstringSafe(3);

			if (Code == UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount)
			{
				Value = FormatSuretyBondValueForSadDocument(Value);
			}
		}

		ZString FormatSuretyBondValueForSadDocument(ZString value)
		{
			ZString formattedValue = value;
			if (!string.IsNullOrEmpty(value))
			{
				if (long.TryParse(value, out long number))
				{
					formattedValue = number.ToString("#0", Culture.Invariant);
				}
			}
			return formattedValue;
		}

		ZString ParseCode(ZString code, ZDateTime dateOfAssessment, BusinessObjectFactory factory)
		{
			var rooList = ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(factory, dateOfAssessment);
			if (rooList.ContainsCode(code))
			{
				return code.PadRight(3);
			}

			return code;
		}

		public ZString Code { get; private set; }

		public ZString Value { get; private set; }

		public ZInt? Group { get; private set; }
	}

	public class AdditionalInformationDocWrapperWithLineNumber : AdditionalInformationDocWrapper
	{
		public AdditionalInformationDocWrapperWithLineNumber(AdditionalInformationDocWrapper input, ZString lineNumber, ZDateTime dateOfAssessment, BusinessObjectFactory factory)
			: base(input, dateOfAssessment, factory)
		{
			LineNumber = lineNumber;
		}

		public ZString LineNumber { get; private set; }
	}
}
