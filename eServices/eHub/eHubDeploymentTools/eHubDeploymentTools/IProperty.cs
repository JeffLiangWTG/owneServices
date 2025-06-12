using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eHubDeploymentTools
{
	public delegate void PropertyUpdated(object sender);

	interface IProperty
	{
		bool IsValid { get; }
		event PropertyUpdated Updated;
		Property property { get; }
	}
}
