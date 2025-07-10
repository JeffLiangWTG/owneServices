using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusReconSnapshot : AutoCusReconSnapshot, Integration.Customs.ICusReconSnapshot
	{
		public CusReconSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusReconSnapshotTypeDecider TypeDecider = new CusReconSnapshotTypeDecider();

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static CusReconSnapshot GetSingleCusReconSnapshot(BusinessObjectFactory factory, ZString snapshotType, CusReconEntry parentReconEntry)
			{
				Argument.NotNull(parentReconEntry, nameof(parentReconEntry));
				return GetSingleCusReconSnapshot(factory, snapshotType, new ZQuery(CusReconSnapshotSchema.CRS_CRE_Entry, parentReconEntry.PK));
			}

			public static CusReconSnapshot GetSingleCusReconSnapshot(BusinessObjectFactory factory, ZString snapshotType, CusReconEntryLine parentReconEntryLine)
			{
				Argument.NotNull(parentReconEntryLine, nameof(parentReconEntryLine));
				return GetSingleCusReconSnapshot(factory, snapshotType, new ZQuery(CusReconSnapshotSchema.CRS_CRL_Line, parentReconEntryLine.PK));
			}

			static CusReconSnapshot GetSingleCusReconSnapshot(BusinessObjectFactory factory, ZString snapshotType, ZQuery parentQueryFilter)
			{
				Argument.NotNull(factory, nameof(factory));
				CusReconSnapshot result = null;
				if (!snapshotType.IsEmpty)
				{
					parentQueryFilter.AddToFilter(CusReconSnapshotSchema.CRS_Type, snapshotType);
					result = factory.Load<CusReconSnapshot>(parentQueryFilter).FirstOrDefault();
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusReconSnapshot);
		}

		protected override bool SupportsCloneCore() => true;
	}
}
