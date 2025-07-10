using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class NameExtensions
	{
		public static ZString GetFirstName(this ZString name)
		{
			return name.ToString().Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
		}

		public static ZString GetLastName(this ZString name)
		{
			var nameSplitted = name.ToString().Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
			if (nameSplitted.Length >= 2)
			{
				return nameSplitted[nameSplitted.Length - 1];
			}
			return ZString.Empty;
		}
	}
}
