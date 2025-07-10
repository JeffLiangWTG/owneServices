using System;

namespace CargoWise.RefDbRepo.JPReferenceData.CmdLine
{
	public sealed class StartupArgumentAttribute : Attribute
	{
		public StartupArgumentAttribute(string argument)
		{
			Argument = argument;
		}

		public string Argument { get; }
	}
}
