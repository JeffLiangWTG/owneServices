using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class UserIgnoreStatus
	{
		UserIgnoreStatus(string name)
		{
			Name = name;
		}

		public string Name { get; set; }

		public static UserIgnoreStatus TemporaryIgnore
		{
			get { return new UserIgnoreStatus(PatternMatchingResult.StatusCodes.TemporaryIgnore); }
		}
		public static UserIgnoreStatus PermanentIgnore
		{
			get { return new UserIgnoreStatus(PatternMatchingResult.StatusCodes.PermanentIgnore); }
		}
		public static UserIgnoreStatus ExcludedIgnore
		{
			get { return new UserIgnoreStatus(PatternMatchingResult.StatusCodes.Excluded); }
		}

		public static IEnumerable<UserIgnoreStatus> List()
		{
			return new[]
			{
				TemporaryIgnore,
				PermanentIgnore,
				ExcludedIgnore
			};
		}

		public static UserIgnoreStatus FromString(string name)
		{
			return List().Single(status => string.Equals(status.Name, name, StringComparison.OrdinalIgnoreCase));
		}
	}
}
