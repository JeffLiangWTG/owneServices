using System.Text;
using System;

namespace CargoWise.eHub.Clients.ECU.Transforms.Helper
{
    public class DocAddressExtractor
    {
		public string ExtractOwnerCode(string address)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(address))
			{
				int length = Math.Min(address.Length, 25);
				result = address.Substring(0, length).Trim();
			}

			return result;
		}

		public string ExtractName(string address)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(address))
			{
				int length = Math.Min(address.Length, 50);
				result = address.Substring(0, length).Trim();
			}
			
			return result;
		}

		public string ExtractAddressLine1(string address)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(address) && address.Length > 50)
			{
				int length = Math.Min(address.Length - 50, 50);
				result = address.Substring(50, length).Trim();
			}

			return result;
		}

		public string ExtractAddressLine2(string address)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(address) && address.Length > 100)
			{
				int length = Math.Min(address.Length - 100, 50);
				result = address.Substring(100, length).Trim();
			}

			return result;
		}

		public string ExtractCityOrSuburb(string address)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(address) && address.Length > 150)
			{
				int length = Math.Min(address.Length - 150, 25);
				result = address.Substring(150, length).Trim();
			}

			return result;
		}

		public string ExtractPhone(string address)
		{
			return ExtractPhone(address, "TEL");
		}

		public string ExtractFax(string address)
		{
			return ExtractPhone(address, "FAX");
		}

        string ExtractPhone(string rawaddressStr, string phNumType)
        {
            var addressStr = rawaddressStr.ToUpper();
            var phoneNumberStr = new StringBuilder();

			string typeString = phNumType + ":";
			var index = addressStr.IndexOf(typeString);
			if (index < 0)
			{
				typeString = phNumType + ".:";
				index = addressStr.IndexOf(typeString);
			}
			
			if (index < 0)
			{
				typeString = phNumType + " ";
				index = addressStr.IndexOf(typeString);
			}

            if (index != -1)
            {
                foreach (var ch in addressStr.Substring(index + phNumType.Length))
                {
                    if (!char.IsLetter(ch)) 
					{
						if (char.IsDigit(ch) || ch == ' ' || ch == '-' || ch == '(' || ch == ')')
						{
							phoneNumberStr.Append(ch);
						}
					}
                    else 
					{ 
						break; 
					}
                }
            }

            return phoneNumberStr.ToString().Trim();
        }
    }
}
