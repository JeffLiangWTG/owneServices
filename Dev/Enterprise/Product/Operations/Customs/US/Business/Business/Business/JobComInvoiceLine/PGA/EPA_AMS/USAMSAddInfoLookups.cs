//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAMSAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAMSAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

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
	public class USAMSAddInfoLookups : AutoUSAMSAddInfoLookups
	{
		public USAMSAddInfoLookups(AutoUSAMSAddInfo parent) : base(parent)
		{
		}

		public USAMSAddInfoLookups(AMS ams)
			: base(ams.AddInfo)
		{
			this.aMS = ams;
		}
		readonly AMS aMS;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public CodeDescriptionPairList IntendedUseCodeList
		{
			get
			{
				var amsLine = aMS;
				var programCode = amsLine != null ? amsLine.US_Program : ZString.Empty;
				return Factory.GetCachedValue("AMSProgramList" + programCode, delegate
				{
					var list = new CodeDescriptionPairList();
					switch (programCode)
					{
						case AMSProgramList.Codes.MO1:
						case AMSProgramList.Codes.MO2:
						case AMSProgramList.Codes.MO5:
							list.AddPair(AMSIntendedUseCodesList.Codes._230000, AMSIntendedUseCodesList.Descriptions._230000);
							break;
						case AMSProgramList.Codes.PN1:
							list.AddPair(AMSIntendedUseCodesList.Codes._010000, AMSIntendedUseCodesList.Descriptions._010000);
							list.AddPair(AMSIntendedUseCodesList.Codes._230000, AMSIntendedUseCodesList.Descriptions._230000);
							list.AddPair(AMSIntendedUseCodesList.Codes._240000, AMSIntendedUseCodesList.Descriptions._240000);
							list.AddPair(AMSIntendedUseCodesList.Codes._250000, AMSIntendedUseCodesList.Descriptions._250000);
							list.AddPair(AMSIntendedUseCodesList.Codes._980000, AMSIntendedUseCodesList.Descriptions._980000);
							break;
						case AMSProgramList.Codes.MO3:
							list.AddPair(AMSIntendedUseCodesList.Codes._010000, AMSIntendedUseCodesList.Descriptions._010000);
							list.AddPair(AMSIntendedUseCodesList.Codes._240000, AMSIntendedUseCodesList.Descriptions._240000);
							list.AddPair(AMSIntendedUseCodesList.Codes._250000, AMSIntendedUseCodesList.Descriptions._250000);
							list.AddPair(AMSIntendedUseCodesList.Codes._980000, AMSIntendedUseCodesList.Descriptions._980000);
							break;
						case AMSProgramList.Codes.MO6:
							list.AddPair(AMSIntendedUseCodesList.Codes._250000, AMSIntendedUseCodesList.Descriptions._250000);
							break;
						case AMSProgramList.Codes.MO4:
							list.AddPair(AMSIntendedUseCodesList.Codes._010000, AMSIntendedUseCodesList.Descriptions._010000);
							list.AddPair(AMSIntendedUseCodesList.Codes._230000, AMSIntendedUseCodesList.Descriptions._230000);
							list.AddPair(AMSIntendedUseCodesList.Codes._240000, AMSIntendedUseCodesList.Descriptions._240000);
							list.AddPair(AMSIntendedUseCodesList.Codes._250000, AMSIntendedUseCodesList.Descriptions._250000);
							list.AddPair(AMSIntendedUseCodesList.Codes._980000, AMSIntendedUseCodesList.Descriptions._980000);
							break;
						case AMSProgramList.Codes.EG1:
						case AMSProgramList.Codes.EG2:
							list.AddPair(AMSIntendedUseCodesList.Codes._010000, AMSIntendedUseCodesList.Descriptions._010000);
							list.AddPair(AMSIntendedUseCodesList.Codes._025000, AMSIntendedUseCodesList.Descriptions._025000);
							list.AddPair(AMSIntendedUseCodesList.Codes._180000, AMSIntendedUseCodesList.Descriptions._180000);
							list.AddPair(AMSIntendedUseCodesList.Codes._210000, AMSIntendedUseCodesList.Descriptions._210000);
							list.AddPair(AMSIntendedUseCodesList.Codes._230000, AMSIntendedUseCodesList.Descriptions._230000);
							list.AddPair(AMSIntendedUseCodesList.Codes._250000, AMSIntendedUseCodesList.Descriptions._250000);
							break;
						case AMSProgramList.Codes.OR1:
							list.AddPair(AMSIntendedUseCodesList.Codes._025000, AMSIntendedUseCodesList.Descriptions._025000);
							list.AddPair(AMSIntendedUseCodesList.Codes._130000, AMSIntendedUseCodesList.Descriptions._130000);
							list.AddPair(AMSIntendedUseCodesList.Codes._150000, AMSIntendedUseCodesList.Descriptions._150000);
							list.AddPair(AMSIntendedUseCodesList.Codes._155000, AMSIntendedUseCodesList.Descriptions._155000);
							list.AddPair(AMSIntendedUseCodesList.Codes._010000, AMSIntendedUseCodesList.Descriptions._010000);
							list.AddPair(AMSIntendedUseCodesList.Codes._230000, AMSIntendedUseCodesList.Descriptions._230000);
							list.AddPair(AMSIntendedUseCodesList.Codes._250000, AMSIntendedUseCodesList.Descriptions._250000);
							break;
					}
					list.Sort();
					return list;
				});
			}
		}

		public CodeDescriptionPairList ProgramList
		{
			get
			{
				var amsInd = ZString.Empty;
				var nopInd = ZString.Empty;
				var parent = aMS.Parent;
				if (parent is JobComInvoiceLine invoiceLine)
				{
					amsInd = invoiceLine.US_AMSInd;
					nopInd = invoiceLine.US_NOPInd;
				}
				else if (parent is CusClassPartPivot pivot)
				{
					amsInd = pivot.CD_AMSIndicator;
					nopInd = pivot.CD_NOPIndicator;
				}

				return Factory.GetCachedValue("AMSProgramList" + "AMS" + amsInd + "NOP" + nopInd,
					delegate
					{
						var result = new CodeDescriptionPairList();

						if (OGAIndicatorList.IsToBeDeclared(amsInd))
						{
							result.AddPair(AMSProgramList.Codes.EG1, AMSProgramList.Descriptions.EG1);
							result.AddPair(AMSProgramList.Codes.EG2, AMSProgramList.Descriptions.EG2);
							result.AddPair(AMSProgramList.Codes.MO1, AMSProgramList.Descriptions.MO1);
							result.AddPair(AMSProgramList.Codes.MO2, AMSProgramList.Descriptions.MO2);
							result.AddPair(AMSProgramList.Codes.MO3, AMSProgramList.Descriptions.MO3);
							result.AddPair(AMSProgramList.Codes.MO4, AMSProgramList.Descriptions.MO4);
							result.AddPair(AMSProgramList.Codes.MO5, AMSProgramList.Descriptions.MO5);
							result.AddPair(AMSProgramList.Codes.MO6, AMSProgramList.Descriptions.MO6);
							result.AddPair(AMSProgramList.Codes.MO7, AMSProgramList.Descriptions.MO7);
							result.AddPair(AMSProgramList.Codes.MO8, AMSProgramList.Descriptions.MO8);
							result.AddPair(AMSProgramList.Codes.PN1, AMSProgramList.Descriptions.PN1);
						}

						if (OGAIndicatorList.IsToBeDeclared(nopInd))
						{
							result.AddPair(AMSProgramList.Codes.OR1, AMSProgramList.Descriptions.OR1);
							result.AddPair(AMSProgramList.Codes.OR2, AMSProgramList.Descriptions.OR2);
						}

						return result;
					}
				);
			}
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public ABIUnitOfMeasureList UnitOfMeasureList
		{
			get { return Factory.GetCachedValue<ABIUnitOfMeasureList>(); }
		}
	}
}
