using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NO.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("used in next workflow")]
public class NOInterchange : EDIInterchange
{
	public NOInterchange(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EI_ApplicationCode = ApplicationCodeList.Codes.NOCustoms;
	}

	protected override Type GetMessageTypeToCreate(ZString messageText)
	{
		return typeof(NOInterchange);
	}

	public override UNCharacterSet CharacterSet => new NOCharacterSet();
}
