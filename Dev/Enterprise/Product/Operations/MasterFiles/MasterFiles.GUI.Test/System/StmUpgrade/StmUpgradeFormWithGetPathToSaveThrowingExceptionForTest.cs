using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StmUpgradeFormWithGetPathToSaveThrowingExceptionForTest : StmUpgradeFormForTest
	{
		public StmUpgradeFormWithGetPathToSaveThrowingExceptionForTest(StmUpgradeCollectionContainer dataSource)
			: base(dataSource)
		{
		}

		protected override DialogResult GetPathToSave(out string pathToSave)
		{
			throw new ArgumentException(null, nameof(pathToSave));
		}
	}
}
