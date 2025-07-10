using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class SupplementaryCode : BaseSupplementaryCode
{
	public SupplementaryCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override ZShort CY_Order
	{
		get => base.CY_Order;
		set
		{
			base.CY_Order = value;
			Parent?.MarkAsNeedingValidation();
		}
	}

	public override ZString CY_Type
	{
		get => base.CY_Type;
		set
		{
			base.CY_Type = value;
			Parent?.MarkAsNeedingValidation();
		}
	}

	public override ZGuid CY_ParentID
	{
		get => base.CY_ParentID;
		set
		{
			base.CY_ParentID = value;
			Parent?.MarkAsNeedingValidation();
		}
	}

	public override ZString CY_ParentTableCode
	{
		get => base.CY_ParentTableCode;
		set
		{
			base.CY_ParentTableCode = value;
			Parent?.MarkAsNeedingValidation();
		}
	}

	protected override TypeLoaderCollection parentLoaders => new(typeof(JobComInvoiceLine));

	protected override CusCodeDataValidation GetNewValidation() => new SupplementaryCodeValidation(this);

	protected override CusCodeDataLookups GetNewLookups() => new SupplementaryCodeLookups(this);
}
