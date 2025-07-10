using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public interface IDispositionCodeDateParent
	{
		CodeDescriptionPairList DispositionCodeDescriptionList { get; }
		string GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code);
	}

	public interface IDispositionCodeColumnsForFastSearchProvider
	{
		SchemaColumn[] GetColumnsForFastSearch();
	}

	public class DispositionDataByCodeComparer : IComparer<DispositionData>
	{
		int IComparer<DispositionData>.Compare(DispositionData x, DispositionData y)
		{
			return x.US_Code.CompareTo(y.US_Code);
		}
	}

	public class DispositionDataByDispDateAndMsgDateComparer : IComparer<DispositionData>
	{
		int IComparer<DispositionData>.Compare(DispositionData x, DispositionData y)
		{
			int result = x.US_DispositionDate.CompareTo(y.US_DispositionDate);
			result *= -1;
			return result;
		}
	}

	[SystemDefinedValues]
	public class DispositionData : AutoDispositionData, Integration.Customs.US.IDispositionData
	{
		#region Schema

		public new class Schema : AutoDispositionData.Schema
		{
			public const string DispositionCodeDesc = "DispositionCodeDesc";
			public const string ReleaseOriginDesc = "ReleaseOriginDesc";
			public const string IDTypeNumber = "IDTypeNumber";
		}

		#endregion

		public DispositionData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString DispositionCodeAndDateString
		{
			get { return US_Code + " (" + US_DispositionDate.ToString("MMM-dd-yyyy HH:mm") + ")"; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DispositionData|US_Code", Caption = "Code")]
		public override ZString US_Code
		{
			get { return base.US_Code; }
			set { base.US_Code = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DispositionData|US_DispositionDate", Caption = "Date")]
		public override ZDateTime US_DispositionDate
		{
			get { return base.US_DispositionDate; }
			set { base.US_DispositionDate = value; }
		}

		#region Disposition Code Description

		[ResourceStringData("Enterprise.Customs.US.Business.DispositionData|DispositionCodeDesc", Caption = "Description")]
		public virtual ZString DispositionCodeDesc
		{
			get
			{
				return Parent == null ? string.Empty :
				(
					US_Source.IsEmpty ?
					Parent.DispositionCodeDescriptionList.GetDescriptionFromCode(US_Code) :
					Parent.GetDispositionDescriptionBasedOnSource(US_Source, US_Code)
				);
			}
		}

		public ZPropertyInfo DispositionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.DispositionCodeDesc); }
		}

		#endregion

		#region Release Origin Description

		public ZString ReleaseOriginDesc
		{
			get { return Factory.GetCachedValue<ReleaseOriginCodeList>().GetDescriptionFromCode(US_ReleaseOrigin); }
		}

		public ZPropertyInfo ReleaseOriginDescInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseOriginDesc); }
		}

		#endregion

		#region for the “ID” type and number.

		[ResourceStringData("Enterprise.Customs.US.Business.DispositionData|IDTypeNumber", Caption = "ID type and number")]
		public ZString IDTypeNumber
		{
			get
			{
				if (iDTypeNumberCached == null)
				{
					iDTypeNumberCached = new CachedProperty<ZString>(Factory, () =>
					{
						var result = US_FTZIDType + " " + US_FTZNumber;
						return result.TrimEnd();
					});
				}
				return iDTypeNumberCached.Value;
			}
		}
		CachedProperty<ZString> iDTypeNumberCached;

		#endregion

		public new IDispositionCodeDateParent Parent
		{
			get { return base.Parent as IDispositionCodeDateParent; }
		}

		internal SchemaColumn[] GetColumnsForFastSearch()
		{
			var result = new List<SchemaColumn>();
			var columnsForFastSearchProvider = Parent as IDispositionCodeColumnsForFastSearchProvider;
			if (columnsForFastSearchProvider != null)
			{
				result.AddRange(columnsForFastSearchProvider.GetColumnsForFastSearch());
			}
			return result.ToArray();
		}
	}
}
