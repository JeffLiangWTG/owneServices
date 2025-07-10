using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.ZA.Business
{
	public class STATACREQDOCSendingObjectParent : BaseMessageSendingObjectParent<STATACREQDOCSendingObject>
	{
		public STATACREQDOCSendingObjectParent(BusinessObjectFactory factory)
			: base(factory)
		{
			CargoWise.Common.Argument.NotNull(factory, "BusinessObjectFactory");
		}

		FinancialAccountNumberPortMapCollection FinancialAccountNumberPortMappings
		{
			get { return financialAccountNumberPortMappings ?? (financialAccountNumberPortMappings = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)); }
		}
		FinancialAccountNumberPortMapCollection financialAccountNumberPortMappings;

		protected override NonPersistentBusinessObjectCollection<STATACREQDOCSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new STATACREQDOCSendingObjectCollection(Factory);
			foreach (FinancialAccountNumberPortMap mapping in FinancialAccountNumberPortMappings)
			{
				result.Add(new STATACREQDOCSendingObject(this, mapping));
			}
			return result;
		}

		public override BusinessObject TopLevelBusinessObject => null;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsStatement);
	}
}
