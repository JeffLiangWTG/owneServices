//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAMSLineAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAMSLineAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USAMSLineAddInfoLookups : AutoUSAMSLineAddInfoLookups
	{
		public USAMSLineAddInfoLookups(AutoUSAMSLineAddInfo parent) : base(parent)
		{
			var parentAddInfo = (USAMSLineAddInfo)Parent;
			amsLine = parentAddInfo.AMSLine;
		}

		readonly AMSLine amsLine;

		ZString ProgramCode
		{
			get { return amsLine?.Parent?.US_Program ?? ZString.Empty; }
		}

		ZString ProductType
		{
			get { return amsLine?.Parent?.ProductType ?? ZString.Empty; }
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public ABIUnitOfMeasureList UnitOfMeasureList
		{
			get { return Factory.GetCachedValue<ABIUnitOfMeasureList>(); }
		}

		public ICollection ProductNumberCodes
		{
			get
			{
				if (ShouldUseList)
				{
					return GetProductNumberCodeList;
				}
				else
				{
					return GetProductNumberCodeCollection;
				}
			}
		}

		CodeDescriptionPairList GetProductNumberCodeList
		{
			get
			{
				var productNumberCodeList = ProductType.IsEmpty ? new CodeDescriptionPairList() : Factory.GetCachedValue("AMSLineProductNumberCodeList|" + ProductType, () =>
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					var productNumberList = new USAMSLineProductNumberCollection(Factory, ProductType, true);
					productNumberList.Load();
					codeDescriptionPairList.AddRange(productNumberList);
					return codeDescriptionPairList;
				});
				return productNumberCodeList;
			}
		}

		USAMSLineProductNumberCollection GetProductNumberCodeCollection
		{
			get
			{
				return new USAMSLineProductNumberCollection(Factory, ProductType, false);
			}
		}

		bool ShouldUseList
		{
			get
			{
				var result = false;
				switch (ProgramCode)
				{
					case AMSProgramList.Codes.EG1:
					case AMSProgramList.Codes.EG2:
					case AMSProgramList.Codes.PN1:
						result = true;
						break;
					default:
						break;
				}
				return result;
			}
		}

		public USCCountryCollection USCountries
		{
			get { return new USCCountryCollection(Factory); }
		}

		public CodeDescriptionPairList CertTypeCodeList
		{
			get
			{
				return Factory.GetCachedValue("CertTypeCodeList|" + ProgramCode,
					delegate
					{
						var result = new CodeDescriptionPairList();
						if (ProgramCode == AMSProgramList.Codes.OR2)
						{
							result.AddPair(LPCOTransactionTypeList.Codes.SingleUse, LPCOTransactionTypeList.Descriptions.SingleUse);
							result.AddPair(LPCOTransactionTypeList.Codes.Continuous, LPCOTransactionTypeList.Descriptions.Continuous);
						}
						else
						{
							result.AddPair(LPCOTypeList.Codes.AM2, LPCOTypeList.Descriptions.AM2);
							result.AddPair(LPCOTypeList.Codes.AM6, LPCOTypeList.Descriptions.AM6);
							result.AddPair(LPCOTypeList.Codes.AM7, LPCOTypeList.Descriptions.AM7);
							result.AddPair(LPCOTypeList.Codes.AM9, LPCOTypeList.Descriptions.AM9);
						}
						return result;
					});
			}
		}

		public CodeDescriptionPairList InspectionLocationCodeList
		{
			get
			{
				var inspectionAgency = amsLine != null ? amsLine.US_Party : ZString.Empty;
				return Factory.GetCachedValue("InspectionLocationCodeList|" + inspectionAgency,
					delegate
					{
						var result = new CodeDescriptionPairList();
						if (inspectionAgency == FoodInspectionAgencyList.Codes.CA)
						{
							result = new CanadaStatesList();
						}
						return result;
					});
			}
		}

		public FoodInspectionAgencyList InspectionAgencyList
		{
			get { return Factory.GetCachedValue<FoodInspectionAgencyList>(); }
		}

		public LotNumberQualifierList LotEntityList
		{
			get { return Factory.GetCachedValue<LotNumberQualifierList>(); }
		}
	}
}
