using System;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers
{
	[Serializable]
	public class OverridingFilename
	{
		public static OverridingFilename Parse(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentNullException(text);
			}

			var tokens = text.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);

			if (tokens.Length != 3)
			{
				throw new ArgumentException("Expecting 3 values: ENT and DB code, CSP and Badge!");
			}

			return new OverridingFilename
			{
				EnterpriseDatabaseCode = tokens[0],
				Badge = tokens[2]
			};
		}

		public string EnterpriseDatabaseCode { get; set; }

		public string Badge { get; set; }
	}
}