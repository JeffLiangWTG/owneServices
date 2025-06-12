using System;
using System.Collections.Generic;

namespace CargoWise.eHub.DataModel.IntegrationTests.Utils
{
	public static class ExceptionalClrTypes
	{
		public static readonly Dictionary<string, Type> Mappings = new Dictionary<string, Type>
		{
			{"char", typeof (string)},
            {"ntext", typeof (string)},
			{"nvarchar", typeof (string)},
            {"text", typeof (string)},
			{"timestamp", typeof (byte[])},
			{"varchar", typeof (string)},
			{"smallmoney", typeof (decimal)},
			{"tinyint", typeof (byte)}
		};
	}
}
