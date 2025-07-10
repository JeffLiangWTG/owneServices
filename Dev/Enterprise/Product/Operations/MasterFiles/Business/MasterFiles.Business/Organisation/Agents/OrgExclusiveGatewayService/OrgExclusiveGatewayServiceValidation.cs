using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgExclusiveGatewayServiceValidation : AutoOrgExclusiveGatewayServiceValidation
	{
		public OrgExclusiveGatewayServiceValidation(AutoOrgExclusiveGatewayService parent)
			: base(parent)
		{
		}

		protected override void CheckO7_RS_NKGatewayService()
		{
			base.CheckO7_RS_NKGatewayService();

			var info = Parent.O7_RS_NKGatewayServiceInfo;

			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info, Parent.Lookups.GatewayServices);

			CheckDuplicatedSetting();
		}

		protected override void CheckO7_RS_NKShipmentServiceLevel()
		{
			base.CheckO7_RS_NKShipmentServiceLevel();

			var info = Parent.O7_RS_NKShipmentServiceLevelInfo;

			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info, Parent.Lookups.ShipmentServiceLevels);

			CheckDuplicatedSetting();
		}

		void CheckDuplicatedSetting()
		{
			var parentCollection = ((IBusinessObjectInternals)Parent).ParentCollections.OfType<OrgExclusiveGatewayServiceCollection>().FirstOrDefault();
			var allItems = parentCollection?.Cast<AutoOrgExclusiveGatewayService>() ?? Enumerable.Empty<AutoOrgExclusiveGatewayService>();

			foreach (var groupedItems in allItems.GroupBy(GetKey))
			{
				var items = groupedItems.ToList();
				var duplicatedExists = items.Count > 1;

				foreach (var item in items)
				{
					if (duplicatedExists)
					{
						item.AddRowError(DuplicatedSettingExists);
					}
					else
					{
						item.RemoveRowError(DuplicatedSettingExists);
					}
				}
			}

			string GetKey(AutoOrgExclusiveGatewayService item)
				=> System.FormattableString.Invariant($"{item.O7_RS_NKGatewayService}|{item.O7_RS_NKShipmentServiceLevel}");
		}

		readonly MultilingualString DuplicatedSettingExists = ResString.GetMultilingualString("96CA7624-CBBB-4C21-AA81-230589588233", "Duplicated setting exists!");
	}
}
