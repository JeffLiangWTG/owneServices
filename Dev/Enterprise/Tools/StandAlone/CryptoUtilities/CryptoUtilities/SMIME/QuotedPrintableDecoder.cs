using System;
using System.Globalization;
using System.Text;

namespace Enterprise.CryptoUtilities.SMIME
{
	public class QuotedPrintableDecoder
	{
		public string Decode(string CodedString)
		{
			StringBuilder ResultBuilder = new StringBuilder();
			bool Escaped = false;
			for (int i = 0; i < CodedString.Length; i++)
			{
				if (!Escaped)
				{
					if (CodedString[i] == '=')
					{
						Escaped = true;
					}
					else
					{
						ResultBuilder.Append(CodedString[i]);
					}
				}
				else
				{
					if (CodedString[i] != '\r' && CodedString[i] != '\n')
					{
						string HexString = CodedString.Substring(i, 2);
						byte ASCIIByte = (byte)Int32.Parse(HexString, NumberStyles.HexNumber);
						ResultBuilder.Append(Encoding.ASCII.GetChars(new byte[] { ASCIIByte }));
						i++;
						Escaped = false;
					}
					else
					{
						while (CodedString.Length > (i + 1) && (CodedString[i + 1] == '\r' || CodedString[i + 1] == '\n'))
						{
							i++;
						}
						Escaped = false;
					}
				}

			}

			return ResultBuilder.ToString().TrimEnd('\r', '\n');
		}
	}
}
