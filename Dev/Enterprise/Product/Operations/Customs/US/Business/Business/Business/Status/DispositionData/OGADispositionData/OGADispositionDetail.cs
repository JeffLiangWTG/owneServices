using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class OGADispositionDetail : AutoOGADispositionDetail
	{
		#region Schema

		public new class Schema : AutoOGADispositionDetail.Schema
		{
			public const string ReferenceQualifierCodeDesc = "ReferenceQualifierCodeDesc";
			public const string SubReasonCodeDesc1 = "SubReasonCodeDesc1";
			public const string SubReasonCodeDesc2 = "SubReasonCodeDesc2";
			public const string SubReasonCodeDesc3 = "SubReasonCodeDesc3";
			public const string SubReasonCodeDesc4 = "SubReasonCodeDesc4";
			public const string SubReasonCodeDesc5 = "SubReasonCodeDesc5";
			public const string SubReasonCodeDesc6 = "SubReasonCodeDesc6";
			public const string SubReasonCodeDesc7 = "SubReasonCodeDesc7";
			public const string SubReasonCodeDesc8 = "SubReasonCodeDesc8";
			public const string SubReasonCodeDesc9 = "SubReasonCodeDesc9";
			public const string SubReasonCodeDesc10 = "SubReasonCodeDesc10";
		}

		#endregion

		public OGADispositionDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public ZString ReferenceQualifierCodeDesc
		{
			get { return US_ReferenceIDQualifier + " - " + Factory.GetCachedValue<OGADispositionReferenceQualifierList>().GetDescriptionFromCode(US_ReferenceIDQualifier); }
		}

		public ZPropertyInfo ReferenceQualifierCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.ReferenceQualifierCodeDesc); }
		}

		public ZString SubReasonCodeDesc1
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode1, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc1Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc1); }
		}

		public ZString SubReasonCodeDesc2
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode2, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc2Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc2); }
		}

		public ZString SubReasonCodeDesc3
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode3, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc3Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc3); }
		}

		public ZString SubReasonCodeDesc4
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode4, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc4Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc4); }
		}

		public ZString SubReasonCodeDesc5
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode5, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc5Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc5); }
		}

		public ZString SubReasonCodeDesc6
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode6, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc6Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc6); }
		}

		public ZString SubReasonCodeDesc7
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode7, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc7Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc7); }
		}

		public ZString SubReasonCodeDesc8
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode8, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc8Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc8); }
		}

		public ZString SubReasonCodeDesc9
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode9, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc9Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc9); }
		}

		public ZString SubReasonCodeDesc10
		{
			get { return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_LineSubReasonCode10, RefCusCodeType); }
		}

		public ZPropertyInfo SubReasonCodeDesc10Info
		{
			get { return GetZPropertyInfo(Schema.SubReasonCodeDesc10); }
		}

		ZString RefCusCodeType
		{
			get { return Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason; }
		}

		public IEnumerable<ZString> SubReasonCodes
		{
			get
			{
				if (!US_LineSubReasonCode1.IsEmpty)
				{
					yield return US_LineSubReasonCode1;
				}

				if (!US_LineSubReasonCode2.IsEmpty)
				{
					yield return US_LineSubReasonCode2;
				}

				if (!US_LineSubReasonCode3.IsEmpty)
				{
					yield return US_LineSubReasonCode3;
				}

				if (!US_LineSubReasonCode4.IsEmpty)
				{
					yield return US_LineSubReasonCode4;
				}

				if (!US_LineSubReasonCode5.IsEmpty)
				{
					yield return US_LineSubReasonCode5;
				}

				if (!US_LineSubReasonCode6.IsEmpty)
				{
					yield return US_LineSubReasonCode6;
				}

				if (!US_LineSubReasonCode7.IsEmpty)
				{
					yield return US_LineSubReasonCode7;
				}

				if (!US_LineSubReasonCode8.IsEmpty)
				{
					yield return US_LineSubReasonCode8;
				}

				if (!US_LineSubReasonCode9.IsEmpty)
				{
					yield return US_LineSubReasonCode9;
				}

				if (!US_LineSubReasonCode10.IsEmpty)
				{
					yield return US_LineSubReasonCode10;
				}
			}

			set
			{
				US_LineSubReasonCode1 = value.ElementAtOrDefault(0);
				US_LineSubReasonCode2 = value.ElementAtOrDefault(1);
				US_LineSubReasonCode3 = value.ElementAtOrDefault(2);
				US_LineSubReasonCode4 = value.ElementAtOrDefault(3);
				US_LineSubReasonCode5 = value.ElementAtOrDefault(4);
				US_LineSubReasonCode6 = value.ElementAtOrDefault(5);
				US_LineSubReasonCode7 = value.ElementAtOrDefault(6);
				US_LineSubReasonCode8 = value.ElementAtOrDefault(7);
				US_LineSubReasonCode9 = value.ElementAtOrDefault(8);
				US_LineSubReasonCode10 = value.ElementAtOrDefault(9);
			}
		}

		#endregion

		public new OGADispositionData Parent
		{
			get { return Factory.Load<OGADispositionData>(B7_ParentID); }
		}
	}
}
