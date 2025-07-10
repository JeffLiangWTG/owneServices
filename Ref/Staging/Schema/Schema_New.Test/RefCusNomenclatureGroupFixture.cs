using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusNomenclatureGroupFixture
	{
		[Test]
		public void BuildRefCusConditionApplicabilities_RefCusNomenclatureGroup()
		{
			var nomenclatureGroup = new RefCusNomenclatureGroup { ZZ5_PK = Guid.NewGuid() };
			var cond = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ5_Nomenclature = nomenclatureGroup.ZZ5_PK };
			nomenclatureGroup.RefCusConditions.Add(cond);
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			nomenclatureGroup.BuildNonPersistentObjects();

			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities.Single().S07_ZZ5_Nomenclature == nomenclatureGroup.ZZ5_PK);
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_RefCusNomenclatureGroup_Multi()
		{
			var nomenclatureGroup = new RefCusNomenclatureGroup { ZZ5_PK = Guid.NewGuid() };
			var rate1 = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ5_Nomenclature = nomenclatureGroup.ZZ5_PK };
			var rate2 = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ5_Nomenclature = nomenclatureGroup.ZZ5_PK };
			nomenclatureGroup.RefCusConditions.Add(rate1);
			nomenclatureGroup.RefCusConditions.Add(rate2);

			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate1.RefCusApplicabilities.Add(app);
			}
			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate2.RefCusApplicabilities.Add(app);
			}

			nomenclatureGroup.BuildNonPersistentObjects();

			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities.Count == 6);
			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities.All(x => x.S07_ZZ5_Nomenclature == nomenclatureGroup.ZZ5_PK));
		}
	}
}
