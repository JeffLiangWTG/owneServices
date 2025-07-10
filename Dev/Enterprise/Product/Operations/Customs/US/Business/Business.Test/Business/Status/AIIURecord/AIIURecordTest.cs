using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AIIURecord))]
	sealed class AIIURecordTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return AIIURecord;
		}

		AIIURecord AIIURecord
		{
			get { return aiiuRecord ?? (aiiuRecord = new AIIURecord()); }
		}
		AIIURecord aiiuRecord;

		#endregion
	}
}
