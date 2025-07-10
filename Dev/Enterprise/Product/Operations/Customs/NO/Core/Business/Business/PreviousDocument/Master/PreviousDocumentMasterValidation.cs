using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business;

public class PreviousDocumentMasterValidation : ZValidation
{
	public PreviousDocumentMasterValidation(PreviousDocumentMaster parent) : base(parent)
	{
		this.parent = parent;
	}
	readonly PreviousDocumentMaster parent;

	public override Type AutoValidationType => typeof(PreviousDocumentMasterValidation);

	public override void ValidateAll()
	{
		ValidateCSI_Procedure();
	}

	public void ValidateCSI_Procedure()
	{
		ValidateCalculatedProperty(parent.CSI_ProcedureInfo);
	}

	protected void CheckCSI_Procedure()
	{
		var procedureInfo = parent.CSI_ProcedureInfo;
		ListValidation.MessageErrorIfInvalidCode(procedureInfo);
		ValidateProcedureValueAgainstGoodsLocationAndPrevProcedureCode(procedureInfo);
	}

	void ValidateProcedureValueAgainstGoodsLocationAndPrevProcedureCode(ZPropertyInfo procedureInfo)
	{
		if (parent.Parent is CusEntryInstruction { IsProcedureCodeWithOutOfWarehouse: true } entryInstruction)
		{
			var expectedProcedureValue = entryInstruction.GetPreviousProcedureValue();
			if (expectedProcedureValue != parent.CSI_Procedure)
			{
				procedureInfo.AddMessageError(ResString.GetMultilingualString(resourceKey: "d8f871ae-9157-412f-97ed-3ef5623752bc",
					englishText: "The combination of Procedure on Entry Instruction and Goods Location on Declaration Level requires Previous Procedure to be '{0}'.",
					expectedProcedureValue));
			}
		}
	}
}
