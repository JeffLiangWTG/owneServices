using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
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
	public class FDARelatedContainersGenPivot : CustomsGenPivot
		, IUSContainer
		, Integration.Customs.US.IFDARelatedContainersGenPivot
	{
		public const string RelationType = GenPivotTypeDecider.Types.FDARelatedContainersGenPivot;

		public FDARelatedContainersGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FDARelatedContainersGenPivot|XX_Relation1ID", Caption = "FDA")]
		public override ZGuid XX_Relation1ID
		{
			get { return base.XX_Relation1ID; }
			set { base.XX_Relation1ID = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FDARelatedContainersGenPivot|XX_Relation2ID", Caption = "Container")]
		public override ZGuid XX_Relation2ID
		{
			get { return base.XX_Relation2ID; }
			set { base.XX_Relation2ID = value; }
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

		public new FDARelatedContainersGenPivotValidation Validation
		{
			get { return (FDARelatedContainersGenPivotValidation)base.Validation; }
		}

		protected override GenPivotValidation GetNewValidation()
		{
			return new FDARelatedContainersGenPivotValidation(this);
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
			public Strategy(FDARelatedContainersGenPivot pivot)
				: base(pivot)
			{
			}

			protected new FDARelatedContainersGenPivot BusinessObject
			{
				get { return (FDARelatedContainersGenPivot)base.BusinessObject; }
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
				CusContainer container = Container;
				return container != null ? container.CO_ContainerNumber : ZString.Empty;
			}
		}

		#endregion

		#region IUSContainer Members

		ZString IUSContainer.USContainerCode
		{
			get
			{
				ZString result = ZString.Empty;
				CusContainer container = Container;
				if (container != null)
				{
					RefContainer refContainer = container.Container;
					if (refContainer != null)
					{
						result = refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					}
				}

				return result;
			}
		}

		ZBool IUSContainer.IsRailCar
		{
			get
			{
				ZBool result = false;
				CusContainer container = Container;
				if (container != null)
				{
					RefContainer refContainer = container.Container;
					if (refContainer != null)
					{
						result = container.IsRailCar(refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
					}
				}

				return result;
			}
		}

		#endregion
	}
}
