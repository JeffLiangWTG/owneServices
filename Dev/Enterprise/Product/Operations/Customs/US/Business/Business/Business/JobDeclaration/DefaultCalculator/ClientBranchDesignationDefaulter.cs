using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	interface IClientBranchDesignationDefault : Customs.Business.IRegistryAccessingSupporter
	{
		ZString US_PaymentType { get; }
		GlbBranch Branch { get; }

		ZString US_ClientBranchDesignation { get; set; }
	}

	class ClientBranchDesignationDefaulter
	{
		public void Default(IClientBranchDesignationDefault declaration)
		{
			if (declaration.US_PaymentType == PaymentTypeList.Codes.IndividualBasis || declaration.US_PaymentType.IsEmpty)
			{
				declaration.US_ClientBranchDesignation = ZString.Empty;
			}
			else
			{
				declaration.US_ClientBranchDesignation = new ZString(USCustomsDataRegistry.Instance.ClientBranchDesignation.GetFallBackValueAtAllLevels(Guid.Empty, declaration.RegistryBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid())).Left(AddInfo.Schema.US_ClientBranchDesignationMaxLength);
			}
		}
	}
}
