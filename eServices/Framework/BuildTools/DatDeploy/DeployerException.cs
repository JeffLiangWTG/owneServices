using System;

namespace eServices.BuildTools.DatDeploy
{
	public class DeployerException : Exception
	{
		public DeployerException(string message) : base(message) { }

		public DeployerException(string message, Exception innerException) : base(message, innerException) { }
	}
}
