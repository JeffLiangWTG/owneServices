using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public interface ITWSequenceformatter
	{
		string FormatIntToString(int nextNumber);

		ZString FormatSequenceNumber(ZString sequenceNumberg);

		bool DoesEntryNumberFallIntoThisCategory(ZString number);

		bool IsSequenceNumberMatchEntryNumber(ZString entryNumber, ZString providerSequenceNumber);

		int FormatStringToInt(string nextNumber);

		int MaximumValue { get; }

		ZString AllowedFormatDescription { get; }
	}
}
