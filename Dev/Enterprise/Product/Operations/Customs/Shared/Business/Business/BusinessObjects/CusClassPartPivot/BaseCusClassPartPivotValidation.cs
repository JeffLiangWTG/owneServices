using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseCusClassPartPivotValidation : CusClassPartPivotValidation
	{
		public BaseCusClassPartPivotValidation(BaseCusClassPartPivot parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckAttributesRequirement();
		}

		void CheckAttributesRequirement()
		{
			if (Parent.IsImportClassification)
			{
				var part = (OrgSupplierPart)Parent.Part;
				if (part != null)
				{
					CheckAttributesRequirement(part, x => x.Attributes1, CusAttributeFilterValidation.AttributeName.Attribute1, CusAttributeFilter.AttributeFilterName.AT1);
					CheckAttributesRequirement(part, x => x.Attributes2, CusAttributeFilterValidation.AttributeName.Attribute2, CusAttributeFilter.AttributeFilterName.AT2);
					CheckAttributesRequirement(part, x => x.Attributes3, CusAttributeFilterValidation.AttributeName.Attribute3, CusAttributeFilter.AttributeFilterName.AT3);
				}
			}
		}

		void CheckAttributesRequirement(OrgSupplierPart part, GetAttributesDelegate getAttributes, string attributeName, CusAttributeFilter.AttributeFilterName filterName)
		{
			if (getAttributes(Parent).Count == 0)
			{
				var pivot = part.PivotsForBinding.GetAnotherHTIPivotWithAttributeSetupFor(filterName, Parent);
				if (pivot != null)
				{
					Parent.AddRowError(GetAttributeIsRequiredMessage(attributeName, pivot.CI_ChildType, pivot.RelatedOrganisationDescription));
				}
			}
		}

		public static string GetAttributeIsRequiredMessage(string attributeName, string classificationType, string organisation)
		{
			return Res.GetString("b413a7c7-744c-42d6-b359-e0c879501401", "{0} must be specified as it's specified on another {1} Classification (Related Organization: {2}).", attributeName, classificationType, organisation);
		}

		delegate CusAttributeFilterCollection GetAttributesDelegate(BaseCusClassPartPivot pivot);

		#region CheckCI_ChildType

		protected override void CheckCI_ChildType()
		{
			base.CheckCI_ChildType();

			if (Parent.CI_ChildType.IsEmpty)
			{
				Parent.CI_ChildTypeInfo.AddError(ClassificationTypeIsMandatory);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.CI_ChildTypeInfo, Parent.Lookups.ClassificationTypes);
			}

			if (Parent.CI_CI_Parent.IsEmpty)
			{
				if (!Parent.CI_ChildTypeInfo.HasNotifications())
				{
					var part = Parent.Part;
					if (part != null)
					{
						var isHTI = Parent.IsImportClassification;
						var errorMessage = ZString.Empty;

						if (!isHTI
							|| (Parent.Attributes1.Count == 0
								&& Parent.Attributes2.Count == 0
								&& Parent.Attributes3.Count == 0))
						{
							var dateStart = Parent.CI_DateStart.IsValid ? Parent.CI_DateStart : ZDateTime.MinSmallDateTimeValue;
							var dateEnd = Parent.CI_DateEnd.IsValid ? Parent.CI_DateEnd : ZDateTime.MaxSmallDateTime;
							var orgPK = Parent.CI_OH;
							var parentPK = Parent.PK;
							var childType = Parent.CI_ChildType;
							foreach (BaseCusClassPartPivot pivot in ((OrgSupplierPart)Parent.Part).GetPivots<BaseCusClassPartPivot>(Parent.CI_RN_NKCountry))
							{
								if (IsOverlappingPivot(parentPK, childType, dateStart, dateEnd, orgPK, pivot))
								{
									if (Parent.IsHTB)
									{
										errorMessage = DuplicateAttributeForNoneHTI;
									}
									else
									{
										errorMessage = isHTI ? DuplicateAttributeForHTIWithoutAttributes : DuplicateAttributeForNoneHTI;
									}
									break;
								}
							}
							if (errorMessage.IsEmpty)
							{
								errorMessage = GetInvalidOrganizationCombinationIfHTBIsUsedWithOtherType();
							}
						}
						if (isHTI && errorMessage.IsEmpty)
						{
							errorMessage = GetDuplicationNotificationForHTIIfExist();
						}
						if (!errorMessage.IsEmpty)
						{
							Parent.CI_ChildTypeInfo.AddError(errorMessage);
						}
					}
				}
			}

			ValidateCI_OH();
		}

		ZString GetInvalidOrganizationCombinationIfHTBIsUsedWithOtherType()
		{
			var result = ZString.Empty;
			var part = Parent.Part as OrgSupplierPart;
			if (part != null)
			{
				var currentPivotPK = Parent.PK;
				var orgRelationPK = Parent.CI_OH;
				Func<BaseCusClassPartPivot, bool> filter;
				if (Parent.IsHTB)
				{
					filter = x => x.PK != currentPivotPK && x.CI_OH == orgRelationPK;
				}
				else
				{
					filter = x => x.PK != currentPivotPK && x.CI_OH == orgRelationPK && x.IsHTB;
				}
				var otherPivot = part.GetPivots<BaseCusClassPartPivot>(Parent.CI_RN_NKCountry).GetNonDeletedPivots().FirstOrDefault(filter);
				if (otherPivot != null)
				{
					result = InvalidOrganizationCombinationWithHTB(otherPivot);
				}
			}
			return result;
		}

		public string InvalidOrganizationCombinationWithHTB(BaseCusClassPartPivot otherPivot)
		{
			var classificationProvider = Parent.GetClassificationTypeProvider();
			if (otherPivot.IsHTB)
			{
				return Parent.IsImportClassification ?
				Res.GetString("{8A300519-E677-498C-A045-41AB9611215D}", "One organization cannot have a Type of '{0}' and '{1}', consider adding a type of '{2}'.", classificationProvider.HTICode, classificationProvider.HTBCode, classificationProvider.HTECode) :
				Res.GetString("{003C7481-F5ED-45B6-B101-3F4412836755}", "One organization cannot have a Type of '{0}' and '{1}', consider adding a type of '{2}'.", classificationProvider.HTECode, classificationProvider.HTBCode, classificationProvider.HTICode);
			}
			else
			{
				return otherPivot.IsImportClassification ?
					Res.GetString("{8A300519-E677-498C-A045-41AB9611215D}", "One organization cannot have a Type of '{0}' and '{1}', consider adding a type of '{2}'.", classificationProvider.HTICode, classificationProvider.HTBCode, classificationProvider.HTECode) :
					Res.GetString("{003C7481-F5ED-45B6-B101-3F4412836755}", "One organization cannot have a Type of '{0}' and '{1}', consider adding a type of '{2}'.", classificationProvider.HTECode, classificationProvider.HTBCode, classificationProvider.HTICode);
			}
		}

		public static string ClassificationTypeIsMandatory
		{
			get { return Res.GetString("e079eb83-2015-46ed-a710-659a625d9a39", "Classification Type is mandatory"); }
		}

		public string DuplicateAttributeForHTIWithoutAttributes
		{
			get { return Res.GetString("8b44b632-4094-40e5-8303-0b250335aed4", "The combination of Classification Type and Organization should be unique for {0} classifications. Duplicates are only allowed where Attributes are specified.", Parent.GetClassificationTypeProvider().HTICode); }
		}

		public virtual string DuplicateAttributeForNoneHTI
		{
			get { return Res.GetString("53277C57-91D0-45CA-86DA-A1275009FB43", "The combination of Classification Type and Organization should be unique for {0} and {1} classifications.", Parent.GetClassificationTypeProvider().HTECode, Parent.GetClassificationTypeProvider().HTBCode); }
		}

		static bool IsOverlappingPivot(ZGuid pivotPK, ZString childType, ZDateTime dateStart, ZDateTime dateEnd, ZGuid orgPK, BaseCusClassPartPivot pivotToCheck)
		{
			var result = !pivotToCheck.IsDeleted && pivotToCheck.PK != pivotPK && pivotToCheck.CI_ChildType == childType && orgPK == pivotToCheck.CI_OH;
			result = result && IsMatchingDate(dateStart, dateEnd, pivotToCheck);
			return result;
		}

		static bool IsMatchingDate(ZDateTime dateStart, ZDateTime dateEnd, BaseCusClassPartPivot pivotToCheck)
		{
			var pivotToCheckDateStart = pivotToCheck.CI_DateStart.IsValid ? pivotToCheck.CI_DateStart : ZDateTime.MinSmallDateTimeValue;
			var pivotToCheckDateEnd = pivotToCheck.CI_DateEnd.IsValid ? pivotToCheck.CI_DateEnd : ZDateTime.MaxSmallDateTime;
			return (dateStart.Date == pivotToCheckDateStart.Date && dateEnd.Date == pivotToCheckDateEnd.Date);
		}

		ZString GetDuplicationNotificationForHTIIfExist()
		{
			var part = Parent.Part as OrgSupplierPart;
			if (part != null)
			{
				var relatedOrganisationDescription = Parent.RelatedOrganisationDescription;
				foreach (KeyValuePair<BaseCusClassPartPivot, CusAttributeFilter> attrib1Pair in GetPivotsHavingSameValue1Attribute(x => x.Attributes1, part.GetPivots<BaseCusClassPartPivot>(Parent.CI_RN_NKCountry).GetNonDeletedPivots().ToArray()))
				{
					foreach (KeyValuePair<BaseCusClassPartPivot, CusAttributeFilter> attrib2Pair in GetPivotsHavingSameValue1Attribute(x => x.Attributes2, attrib1Pair.Key))
					{
						foreach (KeyValuePair<BaseCusClassPartPivot, CusAttributeFilter> attrib3Pair in GetPivotsHavingSameValue1Attribute(x => x.Attributes3, attrib2Pair.Key))
						{
							return DuplicateAttributeForHTIWithAttributes(GetValue1(attrib1Pair.Value), GetValue1(attrib2Pair.Value), GetValue1(attrib3Pair.Value), relatedOrganisationDescription);
						}
					}
				}
			}
			return ZString.Empty;
		}

		public static string DuplicateAttributeForHTIWithAttributes(string partAttrib1Value, string partAttrib2Value, string partAttrib3Value, string relatedOrgCode)
		{
			return Res.GetString("2d0a1278-6225-447d-813e-2847dcc1d532", "There is another HTI classification matching Attribute 1 ({0}), Attribute 2 ({1}), Attribute 3 ({2}) and Related Organization ({3}).", partAttrib1Value, partAttrib2Value, partAttrib3Value, relatedOrgCode);
		}

		static ZString GetValue1(CusAttributeFilter attrib)
		{
			return attrib == null ? new ZString(Res.GetString("7d4847a4-4c5a-45f9-be3f-98ef79cac309", "NO VALUE")) : attrib.BG_AttributeValue1;
		}

		IEnumerable<KeyValuePair<BaseCusClassPartPivot, CusAttributeFilter>> GetPivotsHavingSameValue1Attribute(GetAttributesDelegate getAttributes, params BaseCusClassPartPivot[] pivots)
		{
			Dictionary<BaseCusClassPartPivot, List<CusAttributeFilter>> result = new Dictionary<BaseCusClassPartPivot, List<CusAttributeFilter>>();
			CusAttributeFilterCollection attribs = getAttributes(Parent);
			bool isNoAttribs = attribs.Count == 0;
			var dateStart = Parent.CI_DateStart.IsValid ? Parent.CI_DateStart : ZDateTime.MinSmallDateTimeValue;
			var dateEnd = Parent.CI_DateEnd.IsValid ? Parent.CI_DateEnd : ZDateTime.MaxSmallDateTime;

			foreach (BaseCusClassPartPivot pivot in pivots)
			{
				if (!pivot.IsDeleted && pivot.PK != Parent.PK && pivot.IsImportClassification && pivot.CI_OH == Parent.CI_OH && IsMatchingDate(dateStart, dateEnd, pivot))
				{
					var collection = getAttributes(pivot);
					if (isNoAttribs)
					{
						if (collection.Count == 0)
						{
							yield return new KeyValuePair<BaseCusClassPartPivot, CusAttributeFilter>(pivot, null);
						}
					}
					else
					{
						List<CusAttributeFilter> matchedAttribs = new List<CusAttributeFilter>();
						foreach (CusAttributeFilter attribute in attribs)
						{
							if (collection.HasSameValue1(attribute))
							{
								yield return new KeyValuePair<BaseCusClassPartPivot, CusAttributeFilter>(pivot, attribute);
							}
						}
					}
				}
			}
		}

		#endregion

		#region CheckCI_OH

		protected override void CheckCI_OH()
		{
			base.CheckCI_OH();

			ValidateCI_ChildType();
		}

		#endregion

		#region CheckCI_CC

		protected override void CheckCI_CC()
		{
			base.CheckCI_CC();
			ValidateClassTariff(Parent.CI_CCInfo);
			ValidateCI_TariffNum();
			if (Parent.Classification != null && !Parent.Classification.CC_IsActive)
			{
				Parent.CI_CCInfo.AddWarning(WarnThatClassificationNoActive);
			}
		}

		protected virtual void ValidateClassTariff(ZPropertyInfo info)
		{
			if (Parent.CI_CI_Parent.IsEmpty && Parent.CI_CC.IsEmpty && Parent.CI_TariffNum.IsEmpty)
			{
				info.AddError(OneOfTariffOrClassificationIsMandatory);
			}
		}

		public static string OneOfTariffOrClassificationIsMandatory
		{
			get { return Res.GetString("b250ecd1-6be5-4ccd-8cdd-474227905912", "One of Tariff or Classification is Mandatory"); }
		}

		#endregion

		#region CheckCI_TariffNum

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			ValidateClassTariff(Parent.CI_TariffNumInfo);
			ValidateCI_CC();
		}

		#endregion

		#region CheckCI_DateStart

		protected override void CheckCI_DateStart()
		{
			base.CheckCI_DateStart();
			if (!Parent.CI_DateEnd.IsEmpty && Parent.CI_DateEnd < Parent.CI_DateStart)
			{
				Parent.CI_DateStartInfo.AddError(StartDateMustBeforeEndDate);
			}
			ValidateCI_ChildType();
		}

		#endregion

		#region CheckCI_DateEnd

		protected override void CheckCI_DateEnd()
		{
			base.CheckCI_DateEnd();
			if (!Parent.CI_DateStart.IsEmpty && Parent.CI_DateEnd < Parent.CI_DateStart)
			{
				Parent.CI_DateEndInfo.AddError(StartDateMustBeforeEndDate);
			}
			ValidateCI_ChildType();
		}

		#endregion

		public static string StartDateMustBeforeEndDate
		{
			get
			{
				return Res.GetString("21c6274a-7b19-4a06-af8c-30d8218eb588", "Start date must be before end date.");
			}
		}

		public static string WarnThatClassificationNoActive
		{
			get
			{
				return Res.GetString("42DCC7D5-13E9-4720-BC52-51EB1327E8E9", "The selected classification is not Active");
			}
		}

		protected override void CheckCI_AddInfoIsWesternEuropean()
		{
			if (!(Parent is INAddInfoSupporter))
			{
				base.CheckCI_AddInfoIsWesternEuropean();
			}
		}
	}
}
