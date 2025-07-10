using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefCityTownSchema.Constants.R9_InternationalName), DescriptionProperty("Description")]
	public class RefCityTown : AutoRefCityTown
	{
		public RefCityTown(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DataRow row = ((IBusinessObjectInternals)this).Row;

			row[RefCityTownSchema.Constants.R9_IsSystem] = false;
		}

		#region Properties

		public ZString Description
		{
			get { return R9_LocalLanguageName.IsEmpty ? R9_InternationalName : R9_LocalLanguageName; }
		}

		[ReadOnlyMember(Schema.R9_IsSystem)]
		public override ZString R9_InternationalName
		{
			get { return base.R9_InternationalName; }
			set { base.R9_InternationalName = value.TrimStart(); }
		}

		[List("Lookups.Countries")]
		[ReadOnlyMember(Schema.R9_IsSystem)]
		public override ZString R9_RN_NKCountry
		{
			get { return base.R9_RN_NKCountry; }
			set
			{
				var oldStateCountry = (!string.IsNullOrEmpty(R9_RW_NKState) && State != null) ? State.RW_RN_NKCountryCode : ZString.Empty;

				base.R9_RN_NKCountry = value;

				if (base.R9_RN_NKCountry.IsValid && Country != null && oldStateCountry.IsValid && Country.RN_Code != oldStateCountry)
				{
					R9_RW_NKState = ZString.Empty;
				}
			}
		}

		[List("Lookups.States")]
		[ReadOnlyMember(Schema.R9_IsSystem)]
		public override ZString R9_RW_NKState
		{
			get { return base.R9_RW_NKState; }
			set { base.R9_RW_NKState = value; }
		}

		public override RefCountryStates State
		{
			get
			{
				if (R9_RW_NKState.IsValid && R9_RN_NKCountry.IsValid && Country != null)
				{
					var query = new ZQuery(RefCountryStatesSchema.RW_Code, R9_RW_NKState);
					query.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, Country.RN_Code);
					return Factory.LoadTop1<RefCountryStates>(query);
				}
				else
				{
					var query = new ZQuery(RefCountryStatesSchema.RW_Code, R9_RW_NKState);
					return Factory.LoadTop1<RefCountryStates>(query);
				}
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Related Objects

		[ChildEditable]
		public RefPostCodeCollection PostCodes
		{
			get
			{
				if (postCodes == null)
				{
					postCodes = new RefPostCodeCollection(this);
					RegisterEditableChildObject(postCodes);
				}
				return postCodes;
			}
		}

		RefPostCodeCollection postCodes;

		#endregion
	}
}
