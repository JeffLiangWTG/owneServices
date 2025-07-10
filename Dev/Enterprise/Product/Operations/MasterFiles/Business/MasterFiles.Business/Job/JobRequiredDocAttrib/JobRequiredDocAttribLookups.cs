//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobRequiredDocAttribLookups
//
//    This class should be used for overriding collections in AutoJobRequiredDocAttribLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocAttribLookups : AutoJobRequiredDocAttribLookups
	{
		public JobRequiredDocAttribLookups(AutoJobRequiredDocAttrib parent)
			: base(parent)
		{
		}

		protected new JobRequiredDocAttrib Parent => (JobRequiredDocAttrib)base.Parent;

		public JobRequiredDocAttribTypeList AttributeNameList => Factory.GetCachedValue("JobRequiredDocAttribLookups.JobRequiredDocAttribTypeList",
			() => WorkflowDataRegistry.Instance.EnableWorkflowValidation.Value.Count > 0
				? new JobRequiredDocCustomAttribTypeList()
				: new JobRequiredDocAttribTypeList());

		public CodeDescriptionPairList AttributeValueList
		{
			get
			{
				var parent = Parent;
				if (parent.IsDirection)
				{
					return new ImportExportCodeList();
				}
				else if (parent.IsCostaRicaEXVDocumentType)
				{
					return new CostaRicaEXVDocumentTypeList();
				}
				else if (parent.IsCustomsDistrict)
				{
					return new TaiwanCustomsDistrictList();
				}
				else if (parent.IsBoxNumber)
				{
					return GetBoxNumberListCore(parent.RequiredDocument);
				}
				else if (parent.IsBondedID)
				{
					return GetBondedIdList(parent.RequiredDocument);
				}
				else if (parent.IsTradePreferenceCode && parent.IsCARelatedCountry)
				{
					var helper = ObjectFactory.Get<Enterprise.Integration.Customs.CA.ILookupsHelper>();
					return (CodeDescriptionPairList)helper.TreatmentCodes(Factory);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		CodeDescriptionPairList GetBondedIdList(JobRequiredDocument requiredDocument)
		{
			var result = new CodeDescriptionPairList();
			if (requiredDocument?.Parent is OrgHeader orgHeader)
			{
				var effectiveBondedIdRegistrationNumberTypes = new string[]
				{
					OrgCusCode.CodeTypes.ControlledPremisesID,
					OrgCusCode.CodeTypes.WarehouseControlledPremisesID,
					OrgCusCode.TaiwanCodeTypes.CBF,
					OrgCusCode.TaiwanCodeTypes.EPZ,
					OrgCusCode.TaiwanCodeTypes.FTZ,
					OrgCusCode.TaiwanCodeTypes.SciencePark,
					OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark
				};

				foreach (var cusCode in orgHeader.CustomsCodes.Cast<OrgCusCode>().Where(x => !x.OK_CustomsRegNo.IsEmpty && x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Taiwan && effectiveBondedIdRegistrationNumberTypes.Contains<string>(x.OK_CodeType)))
				{
					result.AddPairIfNotExist(cusCode.OK_CustomsRegNo, cusCode.OK_CodeType);
				}
			}
			return result;
		}

		CodeDescriptionPairList GetBoxNumberListCore(JobRequiredDocument requiredDocument)
		{
			var result = new CodeDescriptionPairList();
			var list = requiredDocument?.GetBoxNumberProvider()?.GetBoxNumberList(Factory, requiredDocument.CustomsDistrict);
			if (list != null)
			{
				foreach (ICodeDescription codeDesc in list)
				{
					result.AddPairIfNotExist(codeDesc.Code, codeDesc.Description);
				}
			}
			return result;
		}
	}
}
