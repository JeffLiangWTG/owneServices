using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class HazardousMaterial : IHazardousMaterial
	{
		public HazardousMaterial(UNDGDataItem undg)
		{
			Argument.NotNull(undg, "undg");
			this.undg = undg;
		}
		readonly UNDGDataItem undg;

		#region IHazardousMaterial Members

		public ZString ContactName
		{
			get
			{
				var contact = undg.DGContact;
				return contact == null ? null : contact.OC_ContactName;
			}
		}

		public ZDecimal FlashPointTemp
		{
			get { return undg.DI_DGFlashPoint; }
		}

		public ZString HazMatClass
		{
			get
			{
				var subs = undg.Substance;
				return subs != null ? subs.DG_Class : ZString.Empty;
			}
		}

		public ZString HazMatClassificationDesc
		{
			get { return MessageBlockStringDataCorrector.ReplaceInvalidCharacters(undg.DI_TechnicalName, AMSCharacterTypeString.Constants.Special, '?'); }
		}

		public ZString HazMatCode
		{
			get
			{
				var subs = undg.Substance;
				return subs != null ? "UN" + subs.DG_UNNO.ToUpper() : ""; // TODO Currenlty UNDGSubstance is for IMO type only; change this when Richard adds other type in UNDGSubstance
			}
		}

		public ZString HazMatDesc
		{
			get
			{
				var subs = undg.Substance;
				return subs != null ? subs.DG_PSN : ZString.Empty;
			}
		}

		public ZString HazMatQualifier
		{
			get { return HazMatQualifierCore; }
		}

		protected virtual ZString HazMatQualifierCore
		{
			get { return HazMatQualifierList.Codes.IMOCode; } // TODO: Change this when Richard adds other type in UNDGSubstance
		}

		public ZBool IsFlashPointTempRelevant
		{
			get
			{
				var subs = undg.Substance;
				return subs != null && !subs.DG_FlashPoint.IsEmpty;
			}
		}

		public ZBool IsHazRelevant
		{
			get { return undg.Substance != null; }
		}

		#endregion
	}
}
