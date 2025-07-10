using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLineLookups : SGAddInfoLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		#region CertItemUQList

		public ProductCodeUQList ProductCodeUQList
		{
			get { return productCodeUQList ?? (productCodeUQList = new ProductCodeUQList()); }
		}
		ProductCodeUQList productCodeUQList;

		#endregion

		#region Strategic Goods Lists

		#region Categories

		public CodeDescriptionPairList StrategicGoodsCategory
		{
			get { return SGCCategoriesList; }
		}

		public StrategicGoodsCategoryList SGCCategoriesList
		{
			get { return sgcCategoriesList ?? (sgcCategoriesList = new StrategicGoodsCategoryList()); }
		}
		StrategicGoodsCategoryList sgcCategoriesList;

		#endregion

		#region Product Codes

		public CodeDescriptionPairList StrategicGoodsProductCode
		{
			get { return SGCProductCodesList; }
		}

		#endregion

		#region Category Product Codes

		public CodeDescriptionPairList SGCProductCodesList
		{
			get
			{
				CodeDescriptionPairList result;
				switch (Parent.InvoiceLine.SG_StrategicGoodsCategory)
				{
					case StrategicGoodsCategoryList.Codes.Cat0:
						result = DualUseCategory0;
						break;
					case StrategicGoodsCategoryList.Codes.Cat1:
						result = DualUseCategory1;
						break;
					case StrategicGoodsCategoryList.Codes.Cat2:
						result = DualUseCategory2;
						break;
					case StrategicGoodsCategoryList.Codes.Cat3:
						result = DualUseCategory3;
						break;
					case StrategicGoodsCategoryList.Codes.Cat4:
						result = DualUseCategory4;
						break;
					case StrategicGoodsCategoryList.Codes.Cat5:
						result = DualUseCategory5;
						break;
					case StrategicGoodsCategoryList.Codes.Cat6:
						result = DualUseCategory6;
						break;
					case StrategicGoodsCategoryList.Codes.Cat7:
						result = DualUseCategory7;
						break;
					case StrategicGoodsCategoryList.Codes.Cat8:
						result = DualUseCategory8;
						break;
					case StrategicGoodsCategoryList.Codes.Cat9:
						result = DualUseCategory9;
						break;
					case StrategicGoodsCategoryList.Codes.ML:
						result = MunitionsList;
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}

				return result;
			}
		}

		public DualUseCat0 DualUseCategory0
		{
			get { return dualUseCategory0 ?? (dualUseCategory0 = new DualUseCat0()); }
		}
		DualUseCat0 dualUseCategory0;

		public DualUseCat1 DualUseCategory1
		{
			get { return dualUseCategory1 ?? (dualUseCategory1 = new DualUseCat1()); }
		}
		DualUseCat1 dualUseCategory1;

		public DualUseCat2 DualUseCategory2
		{
			get { return dualUseCategory2 ?? (dualUseCategory2 = new DualUseCat2()); }
		}
		DualUseCat2 dualUseCategory2;

		public DualUseCat3 DualUseCategory3
		{
			get { return dualUseCategory3 ?? (dualUseCategory3 = new DualUseCat3()); }
		}
		DualUseCat3 dualUseCategory3;

		public DualUseCat4 DualUseCategory4
		{
			get { return dualUseCategory4 ?? (dualUseCategory4 = new DualUseCat4()); }
		}
		DualUseCat4 dualUseCategory4;

		public DualUseCat5 DualUseCategory5
		{
			get { return dualUseCategory5 ?? (dualUseCategory5 = new DualUseCat5()); }
		}
		DualUseCat5 dualUseCategory5;

		public DualUseCat6 DualUseCategory6
		{
			get { return dualUseCategory6 ?? (dualUseCategory6 = new DualUseCat6()); }
		}
		DualUseCat6 dualUseCategory6;

		public DualUseCat7 DualUseCategory7
		{
			get { return dualUseCategory7 ?? (dualUseCategory7 = new DualUseCat7()); }
		}
		DualUseCat7 dualUseCategory7;

		public DualUseCat8 DualUseCategory8
		{
			get { return dualUseCategory8 ?? (dualUseCategory8 = new DualUseCat8()); }
		}
		DualUseCat8 dualUseCategory8;

		public DualUseCat9 DualUseCategory9
		{
			get { return dualUseCategory9 ?? (dualUseCategory9 = new DualUseCat9()); }
		}
		DualUseCat9 dualUseCategory9;

		public StrategicGoodsMunitionsList MunitionsList
		{
			get { return munitionsList ?? (munitionsList = new StrategicGoodsMunitionsList()); }
		}
		StrategicGoodsMunitionsList munitionsList;

		#endregion

		#region EndUseCodes1

		public CA_SC1CodeList EndUseCodes1
		{
			get { return endUseCodes1 ?? (endUseCodes1 = new CA_SC1CodeList()); }
		}
		CA_SC1CodeList endUseCodes1;

		#endregion

		#region EndUseCodes2

		public CA_SC2CodeList EndUseCodes2
		{
			get { return endUseCodes2 ?? (endUseCodes2 = new CA_SC2CodeList()); }
		}
		CA_SC2CodeList endUseCodes2;

		#endregion

		#region EndUseCodes3

		public CA_SC3CodeList EndUseCodes3
		{
			get { return endUseCodes3 ?? (endUseCodes3 = new CA_SC3CodeList()); }
		}
		CA_SC3CodeList endUseCodes3;

		#endregion

		#endregion
	}
}
