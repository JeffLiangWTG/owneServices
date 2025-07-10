using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDummyWithWorkflow
	{
		IProcessTask AddNewTrigger();
		IProcessTask AddNewMilestone();
		IProcessTask AddNewTask();
		void TurnOnAutoLogging();
		void TurnOnAutoLoggingToQueueOnly();
		void SetCustomLogReferenceSuffix(string suffix);
		ZDateTime Z0_AnotherDate { get; set; }
		ZDecimal Z0_AnotherDecimal { get; set; }
		ZInt Z0_AnotherNumber { get; set; }
		ZBool Z0_BitFalse { get; set; }
		ZBool Z0_BitTrue { get; set; }
		ZBool Z0_Bool { get; set; }
		ZByte Z0_Byte { get; set; }
		ZString Z0_Code { get; set; }
		ZDateTime Z0_Date { get; set; }
		ZDecimal Z0_Decimal { get; set; }
		ZString Z0_Description { get; set; }
		ZString Z0_FK_Code { get; set; }
		ZGuid Z0_Guid { get; set; }
		ZDecimal Z0_Money { get; set; }
		ZInt Z0_Number { get; set; }
		ZString Z0_NVarChar { get; set; }
		ZString Z0_NVarCharMax { get; set; }
		ZShort Z0_Short { get; set; }
		ZDateTime Z0_SmallDateTime { get; set; }
		ZBlob Z0_VarBinaryMax { get; set; }
		ZString Z0_VarCharMax { get; set; }
		ZString Z0_Xml { get; set; }
	}
}
