using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDDataValueWrapperValidation : ZValidation
	{
		public CMDDataValueWrapperValidation(CMDDataValueWrapper parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			if (!Parent.IsDeleted && !Parent.IsDeleting)
			{
				ValidatePermitOrExemptionType();
				ValidatePermitNumberOrExemptionRemarks();
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(CMDDataValueWrapperValidation); }
		}

		#region ValidatePermitOrExemptionType

		public void ValidatePermitOrExemptionType()
		{
			ValidateCalculatedProperty(Parent.PermitOrExemptionTypeInfo);
		}
		[SuppressMessage("Maintainability", "IDE0051:Remove unused private member.", Justification = "The ValidateCalculatedProperty method invoking " +
			"this method use reflection")]
		void CheckPermitOrExemptionType()
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.PermitOrExemptionTypeInfo, Parent.Lookups.EntryTypeList);
			ValidateExemptionCodeExistOnce();
			ValidateCertificate();
			ValidatePermit();
		}
		void ValidateExemptionCodeExistOnce()
		{
			if (Parent.IsTDBExemption && ParentCollection != null)
			{
				foreach (CMDDataValueWrapper entryWrapper in ParentCollection)
				{
					if (entryWrapper != Parent && entryWrapper.IsTDBExemption)
					{
						Parent.PermitOrExemptionTypeInfo.AddError("An Exemption already exists, cannot add additional exemptions.");
						break;
					}
				}
			}
		}

		void ValidateCertificate()
		{
			if (Parent.PermitOrExemptionType == CustomsEntryTypeList.Singapore.Certificate && ParentCollection != null)
			{
				foreach (CMDDataValueWrapper entryWrapper in ParentCollection)
				{
					if (entryWrapper != Parent && entryWrapper.PermitOrExemptionType == Parent.PermitOrExemptionType)
					{
						Parent.PermitOrExemptionTypeInfo.AddError("A Certificate has already been entered, cannot add another one.");
						break;
					}
				}
			}
		}

		void ValidatePermit()
		{
			if (Parent.PermitOrExemptionType == CustomsEntryTypeList.Singapore.Permit && ParentCollection != null)
			{
				foreach (CMDDataValueWrapper entryWrapper in ParentCollection)
				{
					if (entryWrapper != Parent && entryWrapper.PermitOrExemptionType == Parent.PermitOrExemptionType)
					{
						Parent.PermitOrExemptionTypeInfo.AddWarning("A Permit has already been entered.");
					}
				}
			}
		}

		CMDDataValueWrapperCollection ParentCollection
		{
			get
			{
				CMDDataValueWrapperCollection result = null;

				if (Parent.ParentCollections.Count > 0)
				{
					result = Parent.ParentCollections.First() as CMDDataValueWrapperCollection;
				}

				return result;
			}
		}

		#endregion

		#region ValidatePermitNumberOrExemptionRemarks

		public void ValidatePermitNumberOrExemptionRemarks()
		{
			ValidateCalculatedProperty(Parent.PermitNumberOrExemptionRemarksInfo);
		}

		[SuppressMessage("Maintainability", "IDE0051:Remove unused private member.", Justification = "The ValidateCalculatedProperty method invoking " +
			"this method use reflection")]
		void CheckPermitNumberOrExemptionRemarks()
		{
			MandatoryValidation.CheckEntered(Parent.PermitNumberOrExemptionRemarksInfo);
			if (ParentCollection != null)
			{
				foreach (CMDDataValueWrapper entryWrapper in ParentCollection)
				{
					if (entryWrapper != Parent && entryWrapper.PermitNumberOrExemptionRemarks == Parent.PermitNumberOrExemptionRemarks)
					{
						Parent.PermitNumberOrExemptionRemarksInfo.AddMessageError("This permit number value has already been entered.");
					}
				}
			}
		}
		#endregion

		public readonly CMDDataValueWrapper Parent;
	}
}
