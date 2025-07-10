using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AIIERecord))]
	sealed class AIIERecordTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return AIIERecord;
		}

		AIIERecord AIIERecord
		{
			get { return aiieRecord ?? (aiieRecord = new AIIERecord()); }
		}
		AIIERecord aiieRecord;

		#endregion
	}
}
