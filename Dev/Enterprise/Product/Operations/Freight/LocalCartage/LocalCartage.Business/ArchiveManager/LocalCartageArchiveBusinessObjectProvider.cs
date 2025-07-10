using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.Business.ArchiveManager;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class LocalCartageArchiveBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
		{
			switch (item.PKColumn.TableName)
			{
				case JobCartageSchema.Constants.TableName:
					return new IArchiveableBusinessObject[] { new ArchiveCommonCartage(factory.Load<CommonCartage>(item.PK)) };
			}

			return null;
		}

		public IEnumerable<string> TableNamesSupported
		{
			get { yield return JobCartageSchema.Constants.TableName; }
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get
			{
				return new[]
				{
					ArchiveReferenceKey.CommonTypes.JobNo,
					ArchiveReferenceKey.CommonTypes.Housebill,
					FreightArchiveKeyTypes.Order,
					ArchiveCommonCartage.ArchiveReferenceKeyTypes.CartageParentJobNumber
				};
			}
		}
	}
}
