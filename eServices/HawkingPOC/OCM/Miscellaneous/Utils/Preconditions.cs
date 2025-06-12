using System;
using System.Collections.Generic;
using System.Text;

namespace OcmPoc.Utils
{
    public static class Preconditions
    {
		public static T CheckNotNull<T>(T parameter, string parameterName)
			where T : class
		{
			if (parameter == null)
			{
				throw new ArgumentNullException(parameterName);
			}

			return parameter;
		}
    }
}
