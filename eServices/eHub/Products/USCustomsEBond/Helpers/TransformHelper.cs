using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.eServices.Encryption.Server.Decryptor;


namespace CargoWise.eHub.Products.USCustoms.eBond.Helpers
{
	public class TransformHelper
	{
		public string ComputeSHA1Hash(string input)
		{
			return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.Default.GetBytes(input)));
		}

		public string DecryptPassword(string encrypedPassword)
		{
			return EhubServerDecryptor.Decrypt(encrypedPassword);
		}

		public static string RemoveNamespace(string xmlString)
		{
			var pattern = @" xmlns:?(\w+)?=""[\w:/.]*""";
			
			Regex replace = new Regex(pattern);
			var temp = replace.Replace(xmlString, String.Empty);
			foreach (Match match in replace.Matches(xmlString))
			{
				if (match.Success && match.Groups.Count > 1)
				{
					if (!String.IsNullOrEmpty(match.Groups[1].Value))
					{
						var alias = new Regex(@"\b" + match.Groups[1].Value + ":");
						temp = alias.Replace(temp, String.Empty);
					}
				}
			}
			return temp;
		}

		public static void InsertOrUpdateXmlElement(XmlDocument xmlDoc, string elementName, string value, string xpathToAdd)
		{
			var xpath = $"//*[local-name()='{elementName}']";
			if (xmlDoc.SelectSingleNode(xpath) == null)
			{
				var tempXElement = xmlDoc.CreateElement(elementName);
				tempXElement.InnerText = value;
				var node = xmlDoc.SelectSingleNode(xpathToAdd);
				if(node == null)
				{
					throw new InvalidOperationException("Invalid xpath: " + xpathToAdd);
				}
				node.PrependChild(tempXElement);
			}
			else
			{
				var tempXElement = xmlDoc.SelectSingleNode(xpath);
				tempXElement.InnerText = value;
			}
		}
	}
}
