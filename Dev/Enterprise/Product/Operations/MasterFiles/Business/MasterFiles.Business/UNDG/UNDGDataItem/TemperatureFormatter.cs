using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class TemperatureFormatter
	{
		public static ZString FormatTemperatureString(ZString temperatureToFormat)
		{
			ZString formattedResult = "";
			if (!temperatureToFormat.IsEmpty)
			{
				bool numberStarted = false;
				ZString numberContents = "";
				char[] characters = temperatureToFormat.ToString().ToCharArray();
				foreach (char thisChar in characters)
				{
					if (numberStarted)
					{
						if ("-+1234567890.".IndexOf(thisChar) >= 0)
						{
							numberContents += thisChar.ToString();
						}
						else
						{
							break;
						}
					}
					else
					{
						if ("-+1234567890".IndexOf(thisChar) >= 0)
						{
							numberContents += thisChar.ToString();
							numberStarted = true;
						}
					}
				}

				if (numberStarted)
				{
					ZDecimal temperature = System.Convert.ToDecimal(numberContents);
					ZString stringTemp = temperature.ToString(1);
					ZString stringPrefix = "";
					if (stringTemp.Left(1) == "-")
					{
						stringPrefix = "-";
						stringTemp = stringTemp.Substring(1);
					}
					formattedResult = stringPrefix + stringTemp.PadLeft(4, '0');
				}
			}

			return formattedResult;
		}
	}
}
