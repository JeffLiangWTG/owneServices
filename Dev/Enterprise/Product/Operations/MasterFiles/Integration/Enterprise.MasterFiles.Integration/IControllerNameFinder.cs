using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface IControllerNameFinder
	{
		string GetControllerNameForType(Type type);
	}
}
