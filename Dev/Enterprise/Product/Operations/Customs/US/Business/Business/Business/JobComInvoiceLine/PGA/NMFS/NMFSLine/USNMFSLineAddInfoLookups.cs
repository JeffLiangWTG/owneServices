//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNMFSLineAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNMFSLineAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USNMFSLineAddInfoLookups : AutoUSNMFSLineAddInfoLookups
	{
		public USNMFSLineAddInfoLookups(AutoUSNMFSLineAddInfo parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList NMFSPrograms
		{
			get
			{
				bool is370 = false;
				bool isAMR = false;
				bool isHMS = false;
				bool isSIMP = false;
				bool isCOA = false;
				bool isExport = false;
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null)
				{
					is370 = OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFS370Ind);
					isAMR = OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSAMRInd);
					isHMS = OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSHMSInd);
					isSIMP = OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSSIMPInd);
					isCOA = OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSCOAInd);
					isExport = InvoiceLine.IsExport;
				}
				else
				{
					var parent = Line.Pivot;
					if (parent != null)
					{
						var details = parent.Details;
						is370 = OGAIndicatorList.IsToBeDeclared(details.CD_NMFS370Indicator);
						isAMR = OGAIndicatorList.IsToBeDeclared(details.CD_NMFSAMRIndicator);
						isHMS = OGAIndicatorList.IsToBeDeclared(details.CD_NMFSHMSIndicator);
						isSIMP = OGAIndicatorList.IsToBeDeclared(details.CD_NMFSSIMPIndicator);
						isCOA = OGAIndicatorList.IsToBeDeclared(details.CD_NMFSCOAIndicator);
						isExport = parent.IsExportTariff;
					}
				}

				return NMFSProgramCodeList.GetListFor(Factory, is370, isAMR, isHMS, isSIMP, isCOA, isExport);
			}
		}

		public FishStateList FishStateList
		{
			get { return Factory.GetCachedValue<FishStateList>(); }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public ICodeDescriptionPairList DISDocumentIDList
		{
			get
			{
				ICodeDescriptionPairList result = null;
				var nmfsLine = Line;
				var invoiceLine = nmfsLine == null ? null : nmfsLine.InvoiceLine;

				var declaration = invoiceLine == null ? null : invoiceLine.Declaration;
				if (declaration != null)
				{
					result = declaration.DISDocumentIDList;
				}
				return result ?? new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList DocumentTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				var nmfsLine = Line;
				if (nmfsLine.IsAMRProgramType)
				{
					if (nmfsLine.IsFrozenToothfish)
					{
						result = Factory.GetCachedValue<CodeDescriptionPairList>();
					}
					else
					{
						result = Factory.GetCachedValue<NMFSAMRDocumentIdentifierList>();
					}
				}
				else if (nmfsLine.IsHMSProgramType)
				{
					result = Factory.GetCachedValue<NMFSHMSDocumentIdentifierList>();
				}
				else
				{
					result = Factory.GetCachedValue<NMFS370DocumentIdentifierList>();
				}
				return result;
			}
		}

		public DolphinSafeStatusList DolphinSafeStatusList
		{
			get { return Factory.GetCachedValue<DolphinSafeStatusList>(); }
		}

		protected new USNMFSLineAddInfo Parent
		{
			get { return (USNMFSLineAddInfo)base.Parent; }
		}

		protected NMFSLine Line
		{
			get { return Parent.Parent; }
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				var nmfsLine = Line;
				return nmfsLine == null ? null : nmfsLine.InvoiceLine;
			}
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public ICodeDescriptionPairList SourceTypes
		{
			get { return SourceTypeCodesList.GetListForNMFS(Factory, Line.US_ProgramType); }
		}

		public ICodeDescriptionPairList OceanAreaCodeList
		{
			get { return Factory.GetCachedValue<OceanGeographicAreaCodeList>(); }
		}

		public CodeDescriptionPairList ProcessingTypeList
		{
			get { return UniversalReferenceDataHelper.GetRefCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NMFSCategoryCode); }
		}

		public ZZRefCusCodeListCombinedCollection SpeciesCodeList
		{
			get
			{
				if (fSpeciesCodeList == null)
				{
					fSpeciesCodeList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, ZDateTime.Today);
				}
				return fSpeciesCodeList;
			}
		}
		ZZRefCusCodeListCombinedCollection fSpeciesCodeList;

		public ABIUnitOfMeasureList UnitOfMeasureList
		{
			get { return Factory.GetCachedValue<ABIUnitOfMeasureList>(); }
		}

		public ICodeDescriptionPairList AuthorizationTypes
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionPairList>("AuthorizationTypes", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("1", "One Time Use");
					result.AddPair("2", "Ongoing/Multiple Use");
					return result;
				});
			}
		}
	}
}
