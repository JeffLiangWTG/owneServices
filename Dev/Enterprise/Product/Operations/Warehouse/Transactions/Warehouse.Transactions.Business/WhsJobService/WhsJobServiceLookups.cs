using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsJobServiceLookups : JobServiceLookups
	{
		public WhsJobServiceLookups(WhsJobService parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GetNewJobServiceType_List()
		{
			CodeDescriptionPairList result = base.GetNewJobServiceType_List();

			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in WarehouseDataRegistry.Instance.JobServices.Value)
			{
				result.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return result;
		}
	}
}
