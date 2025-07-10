using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class DummyMerger : BusinessObjectMerger<DummyBusinessObject>
	{
		public DummyMerger(IEnumerable<DummyBusinessObject> sourceList)
			: base(sourceList)
		{
		}

		public bool HasErrorMessage { get; set; }

		public override string CheckMerger()
		{
			return HasErrorMessage ? "Error Message" : string.Empty;
		}

		public override void DoMerge()
		{
			DummyBusinessObject firstDummyObject = null;
			foreach (var obj in SourceList.ToArray())
			{
				if (obj != null)
				{
					if (firstDummyObject == null)
					{
						firstDummyObject = obj;
						firstDummyObject.Z0_Decimal = SourceList.Sum(c => c.Z0_Decimal);
					}
					else
					{
						obj.Z0_Decimal = 0;
					}
				}
			}
		}

		protected override IEnumerable<SchemaColumn> GetComparableColumnsCore()
		{
			return new List<SchemaColumn>();
		}

		protected override IEnumerable<SchemaColumn> GetIgnoredColumnsCore()
		{
			return DummyBizoSchema.All.Cast<SchemaColumn>();
		}
	}
}
