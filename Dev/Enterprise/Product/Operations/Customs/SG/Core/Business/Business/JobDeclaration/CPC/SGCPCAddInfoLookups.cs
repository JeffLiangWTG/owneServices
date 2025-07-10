//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSGCPCAddInfoLookups
//
//    This class should be used for overriding collections in AutoSGCPCAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG
{
	public class SGCPCAddInfoLookups : AutoSGCPCAddInfoLookups
	{
		public SGCPCAddInfoLookups(AutoSGCPCAddInfo parent) : base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get
			{
				JobDeclaration declaration = null;
				SGCPCAddInfo cpcAddInfo = (SGCPCAddInfo)Parent;
				if (cpcAddInfo != null)
				{
					declaration = cpcAddInfo.Parent.Parent;
				}

				return declaration;
			}
		}

		#region Additional Procedure Code List

		public CodeDescriptionPairList CPCCodeList
		{
			get
			{
				var messageType = Declaration?.JE_MessageType ?? ZString.Empty;
				var decType = Declaration?.JE_MessageSubType ?? ZString.Empty;
				var hasCO = Declaration?.HasCofO ?? false;
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_RefCusProcedures_ProcedureCode", "SG", messageType, decType, hasCO);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();
					if (Declaration != null)
					{
						foreach (RefCusProcedure cpcCode in Declaration.CPCCollection)
						{
							if (hasCO)
							{
								if (cpcCode.HasAttribute(AttributeNames.Codes.ISCOO))
								{
									result.AddPair(cpcCode.ZZ6_ProcedureCode + cpcCode.ZZ6_Concession, cpcCode.ZZ6_Description);
								}
							}
							else
							{
								if (!cpcCode.HasAttribute(AttributeNames.Codes.ISCOO))
								{
									result.AddPair(cpcCode.ZZ6_ProcedureCode + cpcCode.ZZ6_Concession, cpcCode.ZZ6_Description);
								}
							}
						}
						result.Sort();
					}
					return result;
				});
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "SG Customs provided description list")]
		public CodeDescriptionPairList CPCDescList
		{
			get
			{
				var messageType = Declaration?.JE_MessageType ?? ZString.Empty;
				var decType = Declaration?.JE_MessageSubType ?? ZString.Empty;
				var hasCO = Declaration?.HasCofO ?? false;
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_CPCDescList", "SG", messageType, decType, hasCO);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();
					if (Declaration != null)
					{
						foreach (RefCusProcedure cpcCode in Declaration.CPCCollection)
						{
							string desc = cpcCode.ZZ6_Description;
							string cpcDesc = desc.Split('(').First().Trim();
							if ((hasCO && cpcCode.HasAttribute(AttributeNames.Codes.ISCOO)) || (!hasCO && !cpcCode.HasAttribute(AttributeNames.Codes.ISCOO)))
							{
								if (cpcCode.HasAttribute(AttributeNames.Codes.ISAEO) || cpcDesc == "AEO")
								{
									result.AddPair("AEO", "Authorised Economic Operator");
								}
								else if (cpcCode.HasAttribute(AttributeNames.Codes.IsCWC) || cpcDesc == "CWC")
								{
									result.AddPair("CWC", "Chemical Weapons Convention");
								}
								else if (cpcCode.HasAttribute(AttributeNames.Codes.ISSEASTORE) || cpcDesc == "SEASTORE")
								{
									result.AddPair("SEASTORE", "Seastore");
								}
								else if (cpcCode.HasAttribute(AttributeNames.Codes.IsSTS) || cpcDesc == "STS")
								{
									result.AddPair("STS", "Strategic Trade Scheme");
								}
								else if (cpcCode.HasAttribute(AttributeNames.Codes.IsSTSAndCWC) || cpcDesc == "STS and CWC")
								{
									result.AddPair("STS and CWC", "STS and CWC");
								}
								else if (cpcCode.HasAttribute(AttributeNames.Codes.IsDeferredPrintingForCoO) || cpcDesc == "Deferred Printing for COO")
								{
									result.AddPair("Deferred Printing for COO", "Deferred printing for Certificate of Origin");
								}
								else if (cpcCode.HasAttribute(AttributeNames.Codes.IsCNB) || cpcDesc == "CNB")
								{
									result.AddPair("CNB", "Central Narcotics Bureau");
								}
								else
								{
									result.AddPair(cpcCode.ZZ6_Description, cpcCode.ZZ6_ProcedureCode + cpcCode.ZZ6_Concession);     // To handle any future update to Additional Product Code uses by SG Customs...
								}
							}
						}
						result.Sort();
					}
					return result;
				});
			}
		}
	}
}
