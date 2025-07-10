using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CYDYardStorageFreeDaysCollection : ActiveBusinessObjectCollection<CYDYardStorageFreeDays>
	{
		public CYDYardStorageFreeDaysCollection(OrgHeader client)
			: base(client.Factory, client.CompanyData, null, CYDYardStorageFreeDaysSchema.YFD_OB_Client)
		{
		}

		protected override bool RunPreSaveValidationCore()
		{
			this.ForEach(x => x.ClearRowNotifications());
			CheckDuplicateEntries();
			return base.RunPreSaveValidationCore();
		}

		void CheckDuplicateEntries()
		{
			var duplicates = this.GroupBy(x => (x.YFD_WW_Yard, x.YFD_TransportMode, x.YFD_UnitType, x.YFD_YardUnitLength, x.YFD_ContainerClass, x.YFD_UnitLoad))
				.Where(g => g.Count() > 1)
				.SelectMany(g => g);

			foreach (var entry in duplicates)
			{
				entry.AddRowError(DuplicateEntryErrorMessage);
			}
		}

		static string DuplicateEntryErrorMessage => Res.GetString("23d32cb9-ee19-4888-818a-dbc76446d78b", "You have entered duplicate entries. The combination of Yard, Transport Mode, Unit Type, Size, Container Class, Empty/Laden must be unique.");
	}
}
