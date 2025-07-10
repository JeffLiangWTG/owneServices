using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class CusInBondEquipment : AutoCusInBondEquipment
	{
		public CusInBondEquipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Header")]
		public override ZGuid BJ_BH_Header
		{
			get { return base.BJ_BH_Header; }
			set { base.BJ_BH_Header = value; }
		}

		public NctsHeader Header
		{
			get { return Factory.Load<NctsHeader>(BJ_BH_Header); }
		}

		public static CusInBondEquipment LoadOrCreateEquipment(NctsHeader master, ZString aceid)
		{
			var query = new ZQuery(CusInBondEquipmentSchema.BJ_BH_Header, master.PK);
			query.AddToFilter(CusInBondEquipmentSchema.BJ_ACEID, aceid);
			query.FetchOnlyFromLocalCache = !master.IsInDatabase;

			var equipment = master.Factory.LoadTop1<CusInBondEquipment>(query);
			if (equipment == null)
			{
				equipment = master.Factory.New<CusInBondEquipment>();
				using (equipment.SuspendSettingHasChanges())
				using (equipment.GetValidationSuspender())
				{
					equipment.BJ_BH_Header = master.PK;
					equipment.BJ_ACEID = aceid;
				}
			}
			return equipment;
		}

		public override void OnSaving()
		{
			var aceid = BJ_ACEID;
			if ((aceid == NctsHeader.Schema.Trailer1 || aceid == NctsHeader.Schema.Trailer2) && BJ_RegistrationNumber.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}
	}
}
