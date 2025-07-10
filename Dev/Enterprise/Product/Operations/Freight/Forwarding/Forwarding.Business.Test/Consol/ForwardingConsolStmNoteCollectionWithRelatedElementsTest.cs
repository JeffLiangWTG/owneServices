using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolStmNoteCollectionWithRelatedElements))]
	sealed class ForwardingConsolStmNoteCollectionWithRelatedElementsTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions()]
		public override void TestLoad()
		{
			try
			{
				var top1Filter = new ZQuery(StmNoteSchema.ST_Table, Consol.TableName);
				top1Filter.MaximumRows = 1;
				Collection.Load(top1Filter);
			}
			catch (NotSupportedException)
			{
				Assert(true);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest() => new ForwardingConsolStmNoteCollectionWithRelatedElements(Consol);

		ForwardingConsol Consol => consol ?? (consol = Factory.New<ForwardingConsol>());
		ForwardingConsol consol;

		#endregion
	}
}
