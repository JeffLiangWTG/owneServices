using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class RatingContractNamedAccountPivot : AutoRatingContractNamedAccountPivot, IRatingContractNamedAccountPivot, IPivotBusinessObject
	{
		public RatingContractNamedAccountPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Organisation))]
		public override ZGuid RNP_OH_NamedAccount
		{
			get { return base.RNP_OH_NamedAccount; }
			set { base.RNP_OH_NamedAccount = value; }
		}

		public OrgHeader Organisation => Factory.Load<OrgHeader>(RNP_OH_NamedAccount);

		public BusinessObject Parent
		{
			get
			{
				switch (RNP_ParentTableCode)
				{
					case RatingContractSchema.Constants.Prefix:
						return Factory.Load<RatingContract>(RNP_ParentID);

					case RatingContractAllocationLineSchema.Constants.Prefix:
						return Factory.Load<RatingContractAllocationLine>(RNP_ParentID);

					default:
						return null;
				}
			}
		}

		#region IPivotBusinessObject Members

		public ZGuid Relation1ID { get => RNP_ParentID; set => RNP_ParentID = value; }

		public BusinessObject Relation1Object => Parent;

		public ZGuid Relation2ID { get => RNP_OH_NamedAccount; set => RNP_OH_NamedAccount = value; }

		public BusinessObject Relation2Object => Organisation;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RNP_ParentTableCode = RatingContractSchema.Constants.Prefix;
		}

#endif
	}
}
