using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry.AWB
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class AWBDisplayOptionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new AWBDisplayOption AddNew()
		{
			return (AWBDisplayOption)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWBDisplayOptionCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AWBDisplayOption();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new AWBDisplayOption this[int i]
		{
			get { return (AWBDisplayOption)Elements[i]; }
		}

		public AWBDisplayOption this[string key]
		{
			get
			{
				foreach (AWBDisplayOption displayOption in this)
				{
					if (displayOption.IATACode == key)
					{
						return displayOption;
					}
				}

				return null;
			}
		}

		public static AWBDisplayOptionCollection GetDefault(AWBDisplayOptionType displayOptionType)
		{
			AWBDisplayOptionCollection result = new AWBDisplayOptionCollection();

			foreach (CodeDescriptionPair iATApair in IATAList)
			{
				AWBDisplayOption displayOption = result.AddNew();

				displayOption.IATACode = iATApair.Code;
				displayOption.IATADescription = iATApair.MultilingualDescription;
				displayOption.Visibility = nameof(AWBDisplayOptionVisibility.Show);

				if (displayOptionType == AWBDisplayOptionType.MAWB)
				{
					if (iATApair.MultilingualDescription.GetUnresolvedString().ToUpper().Contains("AGENT"))
					{
						displayOption.Entitlement = Core.Constants.AWB.EntitlementCode.Agent;
					}
					else
					{
						displayOption.Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
					}
				}
				else
				{
					displayOption.Entitlement = Core.Constants.AWB.EntitlementCode.Agent;
				}
			}

			return result;
		}

		internal static CodeDescriptionPairList IATAList
		{
			get
			{
				if (fIATAList == null)
				{
					fIATAList = new CodeDescriptionPairList(OLookUpEditType.AWBChargeCodes);
					fIATAList.AddPair(AWBDisplayOption.MissingIATACode, AWBDisplayOption.MissingIATADescription);
				}
				return fIATAList;
			}
		}

		[ThreadStatic]
		static CodeDescriptionPairList fIATAList;
	}
}
