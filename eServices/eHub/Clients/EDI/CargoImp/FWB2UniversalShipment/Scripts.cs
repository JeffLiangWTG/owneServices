using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Clients.EDI.Transforms.CargoImp.FWB2UniversalShipment
{
	public class Scripts
	{
		public string ParseTotalNoOfPacks(string input)
		{
			ParsePackWeightVolumeIfHasNot(input);
			return TotalNoOfPacks;
		}

		public string ParseTotalWeightCode(string input)
		{
			ParsePackWeightVolumeIfHasNot(input);
			return TotalWeightCode;
		}

		public string ParseTotalWeight(string input)
		{
			ParsePackWeightVolumeIfHasNot(input);
			return TotalWeight;
		}

		public string ParseTotalVolumeCode(string input)
		{
			ParsePackWeightVolumeIfHasNot(input);
			return TotalVolumeCode;
		}

		public string ParseTotalVolume(string input)
		{
			ParsePackWeightVolumeIfHasNot(input);
			return TotalVolume;
		}

		#region Parse Pack Weight Volume Implementation

		public string TotalNoOfPacks;
		public string TotalWeightCode;
		public string TotalWeight;
		public string TotalVolumeCode;
		public string TotalVolume;
		public bool HasParsedPackWeightVolume;

		public void ParsePackWeightVolumeIfHasNot(string input)
		{
			if (!HasParsedPackWeightVolume)
			{
				ParsePackWeightVolume(input, out TotalNoOfPacks, out TotalWeightCode, out TotalWeight, out TotalVolumeCode, out TotalVolume);
				HasParsedPackWeightVolume = true;
			}
		}

		public void ParsePackWeightVolume(string input, out string numberOfPieces, out string weightCode, out string weight, out string volumeCode, out string volume)
		{
			numberOfPieces = FindLettersOrDigits(input);

			int startIndex = numberOfPieces.Length;
			weightCode = FindLettersOrDigits(input, startIndex);

			startIndex += weightCode.Length;
			weight = FindLettersOrDigits(input, startIndex);

			startIndex += weight.Length;
			volumeCode = FindLettersOrDigits(input, startIndex);

			startIndex += volumeCode.Length;
			volume = FindLettersOrDigits(input, startIndex);
		}


		public string FindLettersOrDigits(string input, int startIndex = 0)
		{
			if (startIndex >= input.Length)
			{
				return string.Empty;
			}
			else if (startIndex == input.Length - 1)
			{
				return input.Substring(startIndex);	//last char
			}
			else
			{
				for (int i = startIndex; i < input.Length - 1; i++)
				{
					char thisChar = input[i];
					char nextChar = input[i + 1];
					bool isTypeSame = (Char.IsLetter(thisChar) && Char.IsLetter(nextChar)) || ((Char.IsDigit(thisChar) || thisChar == '.') && (Char.IsDigit(nextChar) || nextChar == '.'));

					if (!isTypeSame)
					{
						return input.Substring(startIndex, i - startIndex + 1);
					}
				}

				return input.Substring(startIndex);
			}
		}

		#endregion
	}
}
