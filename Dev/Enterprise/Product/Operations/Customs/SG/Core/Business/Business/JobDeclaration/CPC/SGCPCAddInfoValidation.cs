//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSGCPCAddInfoValidation
//
//    This class should be used for overriding validation in AutoSGCPCAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.SG
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.SG.V4.Business;
	using Enterprise.Customs.Universal;

	public class SGCPCAddInfoValidation : AutoSGCPCAddInfoValidation
	{
		public SGCPCAddInfoValidation(AutoSGCPCAddInfo parent) : base(parent)
		{
		}

		public SGCPC SGCPC
		{
			get
			{
				if (fSGCPC == null)
				{
					fSGCPC = (SGCPC)Parent.Parent;
				}
				return fSGCPC != null && !fSGCPC.IsDeleted ? fSGCPC : null;
			}
		}
		SGCPC fSGCPC;

		public JobDeclaration Declaration
		{
			get
			{
				if (SGCPC != null)
				{
					if (fDeclaration == null)
					{
						fDeclaration = Parent.Factory.Load<JobDeclaration>(SGCPC.B7_ParentID);
					}
				}
				return fDeclaration != null && !fDeclaration.IsDeleted ? fDeclaration : null;
			}
		}
		JobDeclaration fDeclaration;

		protected override void CheckSG_CPCCode()
		{
			base.CheckSG_CPCCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_CPCCodeInfo, Parent.Lookups.CPCCodeList);
			if (Declaration != null && Declaration.IsSeaStore && Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
			{
				SGCPC.SG_CPCCodeInfo.AddMessageError("Sea Store details are only valid on INP, TNP and OUT Declarations.");
			}
		}

		ZBool IsSeastoreProcedureCode => SGCPC?.AdditionalProcedureCode?.HasAttribute(AttributeNames.Codes.ISSEASTORE) ?? false;

		ZBool IsPC1Mandatory
		{
			get
			{
				var cusProcedure = SGCPC?.AdditionalProcedureCode;
				return cusProcedure != null && (cusProcedure.HasAttribute(Universal.AttributeNames.Codes.PC1));
			}
		}

		ZBool IsPC2Mandatory
		{
			get
			{
				var cusProcedure = SGCPC?.AdditionalProcedureCode;
				return cusProcedure != null && (cusProcedure.HasAttribute(Universal.AttributeNames.Codes.PC2));
			}
		}

		protected override void CheckSG_PC1()
		{
			base.CheckSG_PC1();
			if (IsSeastoreProcedureCode && IsPC1Mandatory)
			{
				if (Parent.SG_PC1.IsEmpty)
				{
					var mandatoryValueRequired = SGCPC?.AdditionalProcedureCode?.GetAttributeValue(Universal.AttributeNames.Codes.PC1, SeaStoreCrewRequired);
					Parent.SG_PC1Info.AddMessageError(mandatoryValueRequired);
				}
				else if (!Parent.SG_PC1.IsNumbersOnlyOrEmpty)
				{
					Parent.SG_PC1Info.AddMessageError(SeaStoreCrewAsNumber);
				}
			}
		}
		internal const string SeaStoreCrewRequired = "For CPC 'SEASTORE', no. of crew must be entered in Processing Code 1.";
		internal const string SeaStoreCrewAsNumber = "For CPC 'SEASTORE', no. of crew must be entered as a number, (e.g. 15), in Processing Code 1.";

		protected override void CheckSG_PC2()
		{
			base.CheckSG_PC2();
			if (IsSeastoreProcedureCode && IsPC2Mandatory)
			{
				if (Parent.SG_PC2.IsEmpty)
				{
					var mandatoryValueRequired = SGCPC?.AdditionalProcedureCode?.GetAttributeValue(Universal.AttributeNames.Codes.PC2, SeaStoreVoyageDurationRequired);
					Parent.SG_PC2Info.AddMessageError(mandatoryValueRequired);
				}
				else if (!Parent.SG_PC2.IsNumbersOnlyOrEmpty)
				{
					Parent.SG_PC2Info.AddMessageError(SeaStoreVoyageDurationAsNumber);
				}
			}
		}
		internal const string SeaStoreVoyageDurationRequired = "For CPC 'SEASTORE', the Voyage duration in days must be entered in Processing Code 2.";
		internal const string SeaStoreVoyageDurationAsNumber = "For CPC 'SEASTORE', the Voyage duration in days must be entered as a number, (e.g. 8), in Processing Code 2.";

		protected override void CheckSG_PC3()
		{
			base.CheckSG_PC3();
			if (IsSeastoreProcedureCode)
			{
				if (!Parent.SG_PC3.IsEmpty)
				{
					Parent.SG_PC3Info.AddMessageError(SeaStorePC3NotRequired);
				}
			}
		}
		const string SeaStorePC3NotRequired = "For CPC 'SEASTORE', Processing Code 3 is expected to be empty.";

		protected override void CheckSG_PC5()
		{
			base.CheckSG_PC5();
			if (!Parent.SG_PC5.IsEmpty && Parent.SG_PC4.IsEmpty)
			{
				Parent.SG_PC5Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC6()
		{
			base.CheckSG_PC6();
			if (!Parent.SG_PC6.IsEmpty && Parent.SG_PC4.IsEmpty)
			{
				Parent.SG_PC6Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC7()
		{
			base.CheckSG_PC7();
			if (!Parent.SG_PC7.IsEmpty && Parent.SG_PC4.IsEmpty)
			{
				Parent.SG_PC7Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC8()
		{
			base.CheckSG_PC8();
			if (!Parent.SG_PC8.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty))
			{
				Parent.SG_PC8Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC9()
		{
			base.CheckSG_PC9();
			if (!Parent.SG_PC9.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty))
			{
				Parent.SG_PC9Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC10()
		{
			base.CheckSG_PC10();
			if (!Parent.SG_PC10.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty))
			{
				Parent.SG_PC10Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC11()
		{
			base.CheckSG_PC11();
			if (!Parent.SG_PC11.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty || Parent.SG_PC10.IsEmpty))
			{
				Parent.SG_PC11Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC12()
		{
			base.CheckSG_PC12();
			if (!Parent.SG_PC12.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty || Parent.SG_PC10.IsEmpty))
			{
				Parent.SG_PC12Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC13()
		{
			base.CheckSG_PC13();
			if (!Parent.SG_PC13.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty || Parent.SG_PC10.IsEmpty))
			{
				Parent.SG_PC13Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC14()
		{
			base.CheckSG_PC14();
			if (!Parent.SG_PC14.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty || Parent.SG_PC10.IsEmpty || Parent.SG_PC13.IsEmpty))
			{
				Parent.SG_PC14Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		protected override void CheckSG_PC15()
		{
			base.CheckSG_PC15();
			if (!Parent.SG_PC15.IsEmpty && (Parent.SG_PC4.IsEmpty || Parent.SG_PC7.IsEmpty || Parent.SG_PC10.IsEmpty || Parent.SG_PC13.IsEmpty))
			{
				Parent.SG_PC15Info.AddMessageError(ProcessingCodesMustBeSequential);
			}
		}

		const string ProcessingCodesMustBeSequential = "Additional Processing Codes must be entered in sequential groups. This value will not be sent in the message.";
	}
}
