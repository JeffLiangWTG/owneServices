using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ConsolRestrictionUNDGDataObjectReader : DataObjectReader<UNDG, ConsolDGRestrictions>
	{
		public ConsolRestrictionUNDGDataObjectReader(UNDG dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<UNDG, ConsolDGRestrictions> undgBizObjFinder = null)
			: base(dataObject, logger, factory)
		{
			this.undgBizObjFinder = undgBizObjFinder;
		}

		readonly Func<UNDG, ConsolDGRestrictions> undgBizObjFinder;

		protected override ConsolDGRestrictions GetExistingBusinessObject()
		{
			return undgBizObjFinder != null ? undgBizObjFinder(dataObject) : factory.New<ConsolDGRestrictions>();
		}

		protected override void PopulateBusinessObject(ConsolDGRestrictions targetBO)
		{
			SetValue(targetBO, JobConsolDGRestrictionsSchema.JKD_Class, dataObject.IMOClass);
			SetValue(targetBO, JobConsolDGRestrictionsSchema.JKD_UNNO, dataObject.UNDGCode?.Left(4));
			SetValue(targetBO, JobConsolDGRestrictionsSchema.JKD_Variant, dataObject.UNDGCode?.SubstringSafe(4, 2));
		}
	}
}
