using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWebCSSImages
	{
		Dictionary<string, byte[]> WebImages { get; }
		ZString WebStyleSheet { get; }
	}
}
