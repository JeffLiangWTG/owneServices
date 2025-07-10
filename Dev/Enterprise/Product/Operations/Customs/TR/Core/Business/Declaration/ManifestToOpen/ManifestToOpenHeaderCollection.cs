using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenHeaderCollection : ActiveBusinessObjectCollection<ManifestToOpenHeader>
	{
		public ManifestToOpenHeaderCollection(BusinessObject master)
			: base(master.Factory,
				  master,
				  GetFilter(master),
				  CusEntryNumSchema.CE_ParentID)
		{
			Master = master;
		}

		BusinessObject Master { get; }

		static ZQuery GetFilter(BusinessObject master)
		{
			var result = new ZQuery(CusEntryNumSchema.CE_ParentTable, master.TableName);
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Turkey.ManifestToOpen);
			result.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Turkey);
			return result;
		}

		protected override void SetDefaultsForNewElementCore(ManifestToOpenHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.CE_ParentTable = Master.TableName;
			newElement.CE_EntryType = CusEntryNumberTypes.Turkey.ManifestToOpen;
			newElement.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
		}
	}
}
