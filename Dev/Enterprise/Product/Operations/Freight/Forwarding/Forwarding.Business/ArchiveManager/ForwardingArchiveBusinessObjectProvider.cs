using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.Business.ArchiveManager;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.ArchiveManager
{
	public class ForwardingArchiveBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		#region IArchiveableBusinessObjectProvider Members

		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
		{
			switch (item.PKColumn.TableName)
			{
				case JobOrderHeaderSchema.Constants.TableName:
					return new IArchiveableBusinessObject[] { new ArchiveCommonOrder(factory.Load<Order>(item.PK)) };
			}

			return null;
		}

		public IEnumerable<string> TableNamesSupported
		{
			get
			{
				yield return JobOrderHeaderSchema.Constants.TableName;
			}
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get
			{
				return new[]
				{
					ArchiveReferenceKey.CommonTypes.JobNo,
					ArchiveReferenceKey.CommonTypes.ShipmentNo,
					ArchiveReferenceKey.CommonTypes.DeclarationNo,
					ArchiveReferenceKey.CommonTypes.Masterbill,
					ArchiveReferenceKey.CommonTypes.Housebill,
					FreightArchiveKeyTypes.Order
				};
			}
		}

		#endregion
	}
}
