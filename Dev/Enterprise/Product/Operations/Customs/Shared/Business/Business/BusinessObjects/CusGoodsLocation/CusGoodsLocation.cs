using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusGoodsLocation : AutoCusGoodsLocation, Integration.Customs.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusGoodsLocationTypeDecider TypeDecider = new CusGoodsLocationTypeDecider();

		public BusinessObject Parent
		{
			get => ParentLoaders.LoadBusinessObject(Factory, CGL_ParentTableCode, CGL_ParentID);
			set => ParentLoaders.SetTablePrefixAndPK(value, CGL_ParentTableCodeInfo, CGL_ParentIDInfo);
		}

		protected TypeLoaderCollection ParentLoaders => parentLoaders ?? (parentLoaders = GetParentLoaders());
		TypeLoaderCollection parentLoaders;

		protected virtual TypeLoaderCollection GetParentLoaders() => new TypeLoaderCollection(
			typeof(CusEntryInstruction),
			typeof(BaseJobDeclaration),
			typeof(CusInBondEvent),
			typeof(BaseCusInBondMoveHeader)
		);

		protected virtual void SetDefaultsForNew()
		{
		}

		#region Construction / Loading

		public static CusGoodsLocation LoadOrCreate(Type goodsLocationType, BusinessObject parent, ZString locationUse)
		{
			return Load(goodsLocationType, parent, locationUse) ?? New(goodsLocationType, parent, locationUse);
		}

		public static T LoadOrCreate<T>(BusinessObject parent, ZString locationUse) where T : CusGoodsLocation
		{
			return Load<T>(parent, locationUse) ?? New<T>(parent, locationUse);
		}

		public static CusGoodsLocation Load(Type goodsLocationType, BusinessObject parent, ZString locationUse, bool reLoadExistingRows = false)
		{
			var query = new ZQuery(CusGoodsLocationSchema.CGL_ParentID, parent.PK);
			query.AddToFilter(CusGoodsLocationSchema.CGL_ParentTableCode, parent.TablePrefix);
			query.AddToFilter(CusGoodsLocationSchema.CGL_LocationUse, locationUse);
			query.OrderBy = CusGoodsLocationSchema.CGL_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			query.ReLoadExistingRows = reLoadExistingRows;
			return (CusGoodsLocation)parent.Factory.LoadTop1(goodsLocationType, query);
		}

		public static T Load<T>(BusinessObject parent, ZString locationUse, bool reLoadExistingRows = false) where T : CusGoodsLocation
		{
			return (T)Load(typeof(T), parent, locationUse, reLoadExistingRows);
		}

		public static CusGoodsLocation New(Type goodsLocationType, BusinessObject parent, ZString locationUse)
		{
			var result = (CusGoodsLocation)parent.Factory.New(goodsLocationType);
			using (result.SuspendSettingHasChanges())
			{
				result.Parent = parent;
				result.CGL_LocationUse = locationUse;
				result.SetDefaultsForNew();
			}
			return result;
		}

		public static T New<T>(BusinessObject parent, ZString locationUse) where T : CusGoodsLocation
		{
			return (T)New(typeof(T), parent, locationUse);
		}

		#endregion

		public override bool IsSavedByFactory => isPersistent && base.IsSavedByFactory;

		public bool IsPersistent => isPersistent;
		bool isPersistent = true;

		public void MakeNonPersistent() => isPersistent = false;

		[ResourceStringData("C11D3524-FE51-4C6B-AE8F-420274AE8EA6", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.TypeList))]
		public override ZString CGL_Type
		{
			get => base.CGL_Type;
			set => base.CGL_Type = value;
		}

		[ResourceStringData("04AA6214-DCA7-4BF6-A315-EA046813D8C9", Caption = "Qualifier")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.QualifierList))]
		public override ZString CGL_Qualifier
		{
			get => base.CGL_Qualifier;
			set
			{
				var oldValue = base.CGL_Qualifier;
				base.CGL_Qualifier = value;
				if (oldValue != value && value != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier && !CGL_CustomsOffice.IsEmpty)
				{
					CGL_CustomsOffice = ZString.Empty;
				}
			}
		}

		[ResourceStringData("61CE0D4D-5D4D-4663-8AAD-861F0FD22F6E", Caption = "Additional Identifier")]
		public override ZString CGL_AdditionalIdentifier
		{
			get => base.CGL_AdditionalIdentifier;
			set => base.CGL_AdditionalIdentifier = value;
		}

		[ResourceStringData("17D840E6-2877-4B96-976D-4C2DECF0C9F7", Caption = "Customs Office")]
		public override ZString CGL_CustomsOffice
		{
			get => base.CGL_CustomsOffice;
			set => base.CGL_CustomsOffice = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.LocationUseList))]
		public override ZString CGL_LocationUse { get => base.CGL_LocationUse; set => base.CGL_LocationUse = value; }
	}
}
