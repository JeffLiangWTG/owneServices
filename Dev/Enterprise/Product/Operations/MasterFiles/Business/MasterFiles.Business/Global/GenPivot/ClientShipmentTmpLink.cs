using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ClientShipmentTmpLink : GenPivot
	{
		public ClientShipmentTmpLink(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_Relation1TableCode = OrgHeaderSchema.Constants.Prefix;
			XX_Relation2TableCode = JobShipmentSchema.Constants.Prefix;
			XX_RelationType = Constants.GenPivotTypes.ClientShipmentLink;
		}

		public static void Store(ZGuid clientPK, ZGuid bookingPK, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			if (clientPK.IsValid && bookingPK.IsValid)
			{
				var pivot = LoadPivot(bookingPK, factory) ?? factory.New<ClientShipmentTmpLink>();
				pivot.XX_Relation1ID = clientPK;
				pivot.XX_Relation2ID = bookingPK;
			}
		}

		public static ZGuid Consume(ZGuid bookingPK, BusinessObjectFactory factory)
		{
			var pivot = LoadPivot(bookingPK, factory);

			if (pivot != null)
			{
				var result = pivot.XX_Relation1ID;
				pivot.Delete();
				return result;
			}

			return ZGuid.Empty;
		}

		static ClientShipmentTmpLink LoadPivot(ZGuid bookingPK, BusinessObjectFactory factory)
		{
			if (bookingPK.IsValid)
			{
				ZQuery pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Constants.GenPivotTypes.ClientShipmentLink);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, bookingPK);
				return factory.Load<ClientShipmentTmpLink>(pivotQuery).FirstOrDefault();
			}

			return null;
		}
	}
}
