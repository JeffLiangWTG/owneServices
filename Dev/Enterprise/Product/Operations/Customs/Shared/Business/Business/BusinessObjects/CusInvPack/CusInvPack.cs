using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public abstract class CusInvPack<TParent, TLookups, TValidation> : CusInvPack
		where TParent : BusinessObject
		where TLookups : CusInvPackLookups
		where TValidation : CusInvPackValidation
	{
		protected CusInvPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusInvPackLookups.UnitTypeList))]
		public override ZString B5_UnitType
		{
			get => base.B5_UnitType;
			set => base.B5_UnitType = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusInvPackLookups.TypeOfDifferenceList))]
		public override ZString B5_TypeOfDifference
		{
			get => base.B5_TypeOfDifference;
			set => base.B5_TypeOfDifference = value;
		}

		public new TParent Parent
		{
			get { return (TParent)base.Parent; }
			set { base.Parent = value; }
		}

		public new TLookups Lookups
		{
			get { return (TLookups)base.Lookups; }
		}

		public new TValidation Validation
		{
			get { return (TValidation)base.Validation; }
		}

		protected override CusInvPackLookups GetNewLookups()
		{
			return (TLookups)Activator.CreateInstance(typeof(TLookups), this);
		}

		protected override CusInvPackValidation GetNewValidation()
		{
			return (TValidation)Activator.CreateInstance(typeof(TValidation), this);
		}

		protected sealed override TypeLoaderCollection ParentLoaders
		{
			get { return new TypeLoaderCollection(typeof(TParent)); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusInvPackFetchStrategy<TParent>(this);
		}
	}

	[SingleObjectAroundARow]
	public abstract class CusInvPack : AutoCusInvPack, Integration.Customs.ICusInvPack
	{
		protected CusInvPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInvPack.Schema
		{
			public const string B5_UnitTypeDescription = "B5_UnitTypeDescription";
		}

		public BusinessObject Parent
		{
			get { return ParentLoaders.LoadBusinessObject(Factory, B5_ParentTableCode, B5_ParentID); }
			set { ParentLoaders.SetTablePrefixAndPK(value, B5_ParentTableCodeInfo, B5_ParentIDInfo); }
		}

		public ZString B5_UnitTypeDescription
		{
			get { return Lookups.UnitTypeList.GetDescriptionFromCode(B5_UnitType) ?? ZString.Empty; }
		}

		public ZPropertyInfo B5_UnitTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B5_UnitTypeDescription); }
		}

		[BusinessObjectTestExclude]
		public override ZString B5_ParentTableCode
		{
			get { return base.B5_ParentTableCode; }
			set { base.B5_ParentTableCode = value; }
		}

		protected abstract TypeLoaderCollection ParentLoaders { get; }

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new CusInvPackTypeDecider();

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			B5_ParentTableCode = "JE";
		}
#endif
	}
}
