using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	[NonPersistentObject]
	public class RefCusRateApplicabilityUOM : INonPersistentBusinessObject
	{
		public RefCusRateApplicabilityUOM(INonPersistentBusinessObjectFlatten topLevelNonPersistentObject)
		{
			S02_PK = Guid.NewGuid();
			TopLevelNonPersistentObjects.Add(topLevelNonPersistentObject);
		}

		public RefCusRateApplicabilityUOM(RefCusRateUOM uom, INonPersistentBusinessObjectFlatten topLevelNonPersistentObject) : this(topLevelNonPersistentObject)
		{
			S02_UOM = uom.ZXG_UOM;
			RefCusRateUOM = uom;
			uom.RefCusRateApplicabilityUOMs.Add(this);
		}

		public void Link(RefCusRateUOM uom)
		{
			RefCusRateUOM = uom;
			if (!uom.RefCusRateApplicabilityUOMs.Contains(this))
			{
				uom.RefCusRateApplicabilityUOMs.Add(this);
			}
		}

		public void Update()
		{
			var uom = RefCusRateUOM;
			if (uom != null)
			{
				uom.ZXG_UOM = S02_UOM;
			}
		}

		public IEnumerable<object> Unlink()
		{
			var uom = RefCusRateUOM;
			uom?.RefCusRateApplicabilityUOMs.Remove(this);
			RefCusRateUOM = null;
			return new[] { uom };
		}

		public ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects { get; } = new HashSet<INonPersistentBusinessObjectFlatten>();

		public Guid S02_PK { get; set; }
		public Guid S02_S01_RateApplicability { get; set; }
		public string S02_UOM { get; set; }
		public RefCusRateUOM RefCusRateUOM { get; set; }
	}
}
