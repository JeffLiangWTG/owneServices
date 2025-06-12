using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace CargoWise.eHub.Core.Transforms.Helper
{
	public class MSLibraryHelper
	{
		public virtual object GetRegistryValue(string keyName, string valueName, object defaultValue)
		{
			return Registry.GetValue(keyName, valueName, defaultValue);
		}

		public virtual void SetRegistryValue(string keyName, string valueName, object value)
		{
			Registry.SetValue(keyName, valueName, value);
		}
	}
}
