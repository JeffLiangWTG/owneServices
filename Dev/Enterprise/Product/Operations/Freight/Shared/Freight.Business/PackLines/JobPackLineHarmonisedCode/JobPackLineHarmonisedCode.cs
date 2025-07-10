using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class JobPackLineHarmonisedCode : AutoJobPackLineHarmonisedCode, IHarmonisedCode
	{
		public JobPackLineHarmonisedCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[RelatedBusinessObject("PackLine")]
		public override ZGuid JLH_JL
		{
			get => base.JLH_JL;
			set => base.JLH_JL = value;
		}

		public PackLine PackLine
		{
			get { return Factory.Load<PackLine>(JLH_JL); }
		}

		#endregion

		#region Properties

		[RelatedBusinessObject(nameof(Country))]
		public override ZString JLH_RN_NKCountry
		{
			get
			{
				return base.JLH_RN_NKCountry;
			}
			set
			{
				if (base.JLH_RN_NKCountry != value)
				{
					base.JLH_RN_NKCountry = value;

					IsAutoAddedItem = false;
					OnCountryOrCodeUpdated();

					if (!IsValidationSuspended && !IsDeleted)
					{
						Validation.ValidateJLH_Code();
						PackLine?.Validation.ValidateJL_HarmonisedCode();
					}
				}
			}
		}

		public override ZString JLH_Code
		{
			get
			{
				return base.JLH_Code;
			}
			set
			{
				if (base.JLH_Code != value)
				{
					base.JLH_Code = value;

					IsAutoAddedItem = false;
					OnCountryOrCodeUpdated();

					if (!IsValidationSuspended && !IsDeleted)
					{
						Validation.ValidateJLH_RN_NKCountry();
						PackLine?.Validation.ValidateJL_HarmonisedCode();
					}
				}
			}
		}

		#endregion

		#region IsAutoAddedItem

		internal bool IsAutoAddedItem
		{
			get { return isAutoAddedItem; }
			set
			{
				if (isAutoAddedItem != value)
				{
					OnAutoAddedItemNowValid();
					isAutoAddedItem = value;
				}
			}
		}

		bool isAutoAddedItem;

		internal event EventHandler AutoAddedItemNowValid;

		void OnAutoAddedItemNowValid()
		{
			if (AutoAddedItemNowValid != null)
			{
				AutoAddedItemNowValid(this, EventArgs.Empty);
			}
		}

		#endregion

		#region IsEmptyItem

		internal bool IsEmptyItem => JLH_RN_NKCountry.IsEmpty && JLH_Code.IsEmpty;

		#endregion

		#region CountryOrCodeUpdated

		internal event EventHandler CountryOrCodeUpdated;

		void OnCountryOrCodeUpdated()
		{
			if (CountryOrCodeUpdated != null && !isUpdatingCountryOrCode)
			{
				try
				{
					isUpdatingCountryOrCode = true;
					CountryOrCodeUpdated(this, EventArgs.Empty);
				}
				finally
				{
					isUpdatingCountryOrCode = false;
				}
			}
		}
		bool isUpdatingCountryOrCode;

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !IsAutoAddedItem); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IHarmonisedCode

		ZString IHarmonisedCode.Country
		{
			get => JLH_RN_NKCountry;
			set => JLH_RN_NKCountry = value;
		}

		ZString IHarmonisedCode.Code
		{
			get => JLH_Code;
			set => JLH_Code = value;
		}

		#endregion
	}
}
