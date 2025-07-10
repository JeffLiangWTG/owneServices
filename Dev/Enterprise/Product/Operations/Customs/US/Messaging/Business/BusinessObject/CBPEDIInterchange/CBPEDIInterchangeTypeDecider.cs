using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Messaging.Business
{
	[ImmutableObject(true)]
	public class CBPEDIInterchangeTypeDecider : TypeDecider, Integration.Customs.US.ICBPEDIInterchangeTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string eI_ApplicationCode = row[EDIInterchangeSchema.Constants.EI_ApplicationCode].ToString().Trim();
			string eI_InterchangeType = row[EDIInterchangeSchema.Constants.EI_InterchangeType].ToString().Trim();
			return IsCBPInterchange(eI_ApplicationCode, eI_InterchangeType) ? typeof(CBPEDIInterchange) : typeof(Enterprise.Messaging.Business.EDIInterchange);
		}

		bool IsCBPInterchange(string eI_ApplicationCode, string eI_InterchangeType)
		{
			return (eI_ApplicationCode == CBPEDIInterchange.ApplicationCodes.USCustomsImport ||
				(eI_ApplicationCode == CBPEDIInterchange.ApplicationCodes.USCustomsExport && ApplicationIdentifierCodeList.AES.IsAESApplicationCode(eI_InterchangeType)) ||
				eI_ApplicationCode == CBPEDIInterchange.ApplicationCodes.AMS);
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
