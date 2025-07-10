namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using System;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Business;

	public interface IMAFPlugInSupport : IMAFMessagingSource, IMAFMessagingFallback, IDocManagerSupport, IHaveNZAddInfo, IJobNumber
	{
		Logs Logs { get; }

		bool IsPlugInNew { get; }

		bool PlugInVisible { get; }
		bool CargoTypeVisible { get; }
		event EventHandler PlugInVisibilityDataChanged;

		IDocumentEvents DocumentEvents { get; }
	}
}
