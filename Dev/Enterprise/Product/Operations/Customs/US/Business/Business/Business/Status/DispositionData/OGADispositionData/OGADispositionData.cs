using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OGADispositionData : AutoOGADispositionData, ICusAddInfoTypeSupporter
	{
		#region Schema

		public new class Schema : AutoOGADispositionData.Schema
		{
			public const string DispositionCodeDesc = "DispositionCodeDesc";
			public const string EntryDispositionCodeDesc = "EntryDispositionCodeDesc";
			public const string EntryLineDispositionCodeDesc = "EntryLineDispositionCodeDesc";
			public const string ReviewReasonCodeDesc = "ReviewReasonCodeDesc";
			public const string AgencyCodeDesc = "AgencyCodeDesc";
		}

		#endregion

		public OGADispositionData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public OGADispositionDetailCollection OGADispositionDetails
		{
			get
			{
				if (fOGADispositionDetails == null)
				{
					fOGADispositionDetails = new OGADispositionDetailCollection(this);
					fOGADispositionDetails.Load();
					RegisterEditableChildObject(fOGADispositionDetails);
				}
				return fOGADispositionDetails;
			}
		}
		OGADispositionDetailCollection fOGADispositionDetails;

		public ZString DispositionCodeDesc
		{
			get
			{
				if (US_Source == OGADispositionSourceList.Codes.PGA)
				{
					return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
				}
				else
				{
					return FDALineLevelDispositionCodeList.GetDescriptionFromCode(US_Code);
				}
			}
		}

		public ZString DocumentTypeDesc
		{
			get
			{
				var result = ZString.Empty;
				if (!US_DocumentType.IsEmpty)
				{
					result = DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, US_DocumentType);
				}
				return result;
			}
		}

		public ZPropertyInfo DispositionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.DispositionCodeDesc); }
		}

		public ZString EntryDispositionCodeDesc
		{
			get { return PGADispositionCodeList.GetPGADispositionCodeList(Factory).GetDescriptionFromCode(US_OGADispositionStatusCode); }
		}

		public ZPropertyInfo EntryDispositionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.EntryDispositionCodeDesc); }
		}

		public ZString EntryLineDispositionCodeDesc
		{
			get { return PGADispositionCodeList.GetPGADispositionCodeList(Factory).GetDescriptionFromCode(US_OGADispositionStatusCodeEntryLine); }
		}

		public ZPropertyInfo EntryLineDispositionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.EntryLineDispositionCodeDesc); }
		}

		public ZString ReviewReasonCodeDesc
		{
			get
			{
				return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, US_ReviewReasonCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason);
			}
		}

		public ZPropertyInfo ReviewReasonCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.ReviewReasonCodeDesc); }
		}

		public ZString AgencyCodeDesc
		{
			get { return PGAAgencyCodeDescList.GetDescriptionFromCode(US_ProgramCode); }
		}

		public ZPropertyInfo AgencyCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.AgencyCodeDesc); }
		}

		CodeDescriptionPairList PGAAgencyCodeDescList
		{
			get { return Factory.GetCachedValue<PGAAgencyProgramCodeList>(); }
		}

		CodeDescriptionPairList FDALineLevelDispositionCodeList
		{
			get
			{
				return Factory.GetCachedValue<FDALineLevelDispositionCodeList>();
			}
		}

		public new JobDeclaration Parent
		{
			get { return Factory.Load<JobDeclaration>(B7_ParentID); }
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USOGADispositionDetail, typeof(OGADispositionDetail));
			return result;
		}

		#endregion

		public override void Delete()
		{
			OGADispositionDetails.DeleteAll();
			base.Delete();
		}
	}
}
