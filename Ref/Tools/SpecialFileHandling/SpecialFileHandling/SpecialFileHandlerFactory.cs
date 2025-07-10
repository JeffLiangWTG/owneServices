using System;
using System.Collections.Generic;
using Dat.Integration.SpecialFileHandling;

namespace CargoWise.RefDbRepo.SpecialFileHandling
{
	public class SpecialFileHandlerFactory : ISpecialFileHandlerFactory
	{
		public SpecialFileHandlerFactory(SpecialFileHandlerContext context) => this.context = context ?? throw new ArgumentNullException(nameof(context));

		readonly SpecialFileHandlerContext context;

		/// <summary>
		/// Changes made to SpecialFileHandling project needs to be built and added to prebuilt folder within this project.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ISpecialFileHandler> GetSpecialFileHandlers()
		{
			return
			[
				new SafeSchemaVersionChangeRequest(context),
				new StagingSchemaVersionChangeRequest(context),
				new UpgradeScriptProviderLatestVersionChangeRequest(context),
				new DataTransformationMapperSafe(context),
				new PreUpgradeTransformationMapperSafe(context),
				new DataTransformationMapperStaging(context),
				new PreUpgradeTransformationMapperStaging(context)
			];
		}
	}
}
