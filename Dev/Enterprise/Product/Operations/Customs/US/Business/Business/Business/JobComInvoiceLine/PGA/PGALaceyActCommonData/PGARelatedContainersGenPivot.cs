using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class PGARelatedContainersGenPivot : CustomsGenPivot
		, IContainerNumber
		, Integration.Customs.US.IPGARelatedContainersGenPivot
	{
		public const string RelationType = GenPivotTypeDecider.Types.PGARelatedContainersGenPivot;
		public PGARelatedContainersGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.US.Business.PGARelatedContainersGenPivot|XX_Relation1ID", Caption = "PGA")]
		public override ZGuid XX_Relation1ID
		{
			get { return base.XX_Relation1ID; }
			set { base.XX_Relation1ID = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.PGARelatedContainersGenPivot|XX_Relation2ID", Caption = "Container")]
		public override ZGuid XX_Relation2ID
		{
			get { return base.XX_Relation2ID; }
			set { base.XX_Relation2ID = value; }
		}

		public new PGA Relation1Object
		{
			get { return base.Relation1Object as PGA; }
			set { base.Relation1Object = value; }
		}

		public new CusContainerInvoiceLinePivot Relation2Object
		{
			get { return base.Relation2Object as CusContainerInvoiceLinePivot; }
			set { base.Relation2Object = value; }
		}

		public CusContainer Container
		{
			get
			{
				CusContainerInvoiceLinePivot containerInvoiceLinePivot = Relation2Object;
				return containerInvoiceLinePivot != null ? containerInvoiceLinePivot.Container : null;
			}
		}

		public new PGARelatedContainersGenPivotValidation Validation
		{
			get { return (PGARelatedContainersGenPivotValidation)base.Validation; }
		}

		protected override GenPivotValidation GetNewValidation()
		{
			return new PGARelatedContainersGenPivotValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = RelationType;
			XX_Relation1TableCode = CusAddInfoSchema.Constants.Prefix;
			XX_Relation2TableCode = CusContainerInvoiceLinePivotSchema.Constants.Prefix;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(PGARelatedContainersGenPivot pivot)
				: base(pivot)
			{
			}

			protected new PGARelatedContainersGenPivot BusinessObject
			{
				get { return (PGARelatedContainersGenPivot)base.BusinessObject; }
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(CusAddInfoSchema.PK, BusinessObject.XX_Relation1ID);
			}
		}

		#region IContainerNumber Members

		ZString IContainerNumber.ContainerEquipmentID
		{
			get
			{
				BaseCusContainer container = Container;
				return container != null ? container.CO_ContainerNumber : ZString.Empty;
			}
		}

		#endregion
	}
}
