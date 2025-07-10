using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PGA))]
	class LaceyPGADataCorrectionTest : PGADataCorrectionlTest<PGA>
	{
		protected override PGA[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(ILaceyActCommon);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is PGA || bo is USPGAAddInfo;
		}

		protected override PGA GetNewPGA()
		{
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.LaceyActLines.AddNew();
			result.US_PGACommercialDescription = "COMDES";
			result.US_InvCurrPGAValue = 5m;
			result.US_UnknownBreakdown = ZBool.True;
			var containers = result.ContainersForPGALine;
			return result;
		}

		protected override void SetPGAData(PGA pga)
		{
			pga.US_PGACommercialDescription = "NEWCOMDES";
			pga.US_InvCurrPGAValue = 6m;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode") ||
				fieldName.EndsWith("PGARelatedContainersGenPivot.XX_Relation2ID") ||
				fieldName.EndsWith("PGARelatedContainersGenPivot.XX_Relation2TableCode") ||
				fieldName.EndsWith("PGARelatedContainersGenPivot.XX_Relation1ID") ||
				fieldName.EndsWith("PGARelatedContainersGenPivot.XX_Relation1TableCode") ||
				fieldName.EndsWith("PGARelatedContainersGenPivot.XX_RelationType") ||
				fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_LACEYACTSignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_LaceyIndicator");
			return result;
		}
	}
}
