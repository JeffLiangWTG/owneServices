using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.BaseJobDeclaration;

namespace Enterprise.Customs.TW.Business;

public class TWDeclarationDocManagerInfo(JobDeclaration parent) : DeclarationDocManagerInfo(parent)
{
	protected override BusinessObject[] GetRelatedObjects()
	{
		var businessObjects = new List<BusinessObject>();
		businessObjects.AddRange(base.GetRelatedObjects());

		var declaration = (JobDeclaration)BusinessEntity;
		businessObjects.AddRange(GetRelatedDocManagerInfoOrgHeaders(declaration).WhereNotNull().ToArray());
		return businessObjects.ToArray();
	}

	IEnumerable<OrgHeader> GetRelatedDocManagerInfoOrgHeaders(JobDeclaration decl)
	{
		yield return decl.SupplierDocumentaryAddress.Organisation;
		yield return decl.ImporterDocumentaryAddress.Organisation;
		yield return decl.ContainerTerminalOperatorDocAddress.Organisation;
		yield return decl.DepotDocAddress.Organisation;
		yield return decl.ContainerYardDocAddress.Organisation;
		yield return decl.Exporter;
		yield return decl.OrgConsignee;
		yield return decl.ShippingLine;
		yield return decl.Forwarder;
		yield return decl.NotifyParty;
		yield return decl.DeclarantAddress?.Header;
		yield return decl.ControllingAgent;
		yield return decl.ControllingCustomer;
		yield return decl.ExternalBroker;
	}
}
