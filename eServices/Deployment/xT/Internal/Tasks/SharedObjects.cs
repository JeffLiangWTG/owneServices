using System.Collections.Generic;
using System.ServiceProcess;

namespace XT.Internal.API.tasks
{
	public static class SharedObjects
	{
		public static XtProxy xtProxy;

		public static bool hasProxyBeenCreated = false;

		public static bool haveImportOptionsBeenConfigured = false;

		public static bool haveExportOptionsBeenConfigured = false;

		public static bool hasConfigurationBeenImported = false;

		public static bool hasConfigurationBeenExported = false;

		public static Snapshot snapshot;

		public static bool hasSnapshitBeenCreated = false;

		public static bool hasSnapshitBeenRestored = false;

		public static bool hasSnapshitBeenDeleted = false;

		public static ServiceController xtService = new ServiceController();

		public static List<DirectedRelation> ExternalRelations;

		public static bool isPostDeploymentCheckPassed = false;
	}
}
