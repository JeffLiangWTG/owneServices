using System;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using Microsoft.AspNetCore.Builder;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public static class QuartzServerFactory
	{
		public static QuartzServer CreateServer(ILogHelper logHelper, WebApplication app)
		{
			string typeName = ServerConfiguration.ServerImplementationType;
			Type t = Type.GetType(typeName, true);
			logHelper.LogInfo("Creating new instance of server type '" + typeName + "'");
			QuartzServer retValue = (QuartzServer)Activator.CreateInstance(t, logHelper, app);
			logHelper.LogInfo("Instance successfully created");
			return retValue;
		}
	}
}
