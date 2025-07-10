using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business
{
	public abstract class CodeDataPairCollection : NonPersistentBusinessObjectCollection<CodeDataPair>
	{
		public CodeDataPairCollection(BusinessObjectFactory factory, ZPropertyInfo addInfoStringInfo)
			: base(factory)
		{
			this.addInfoStringInfo = addInfoStringInfo;
		}
		readonly ZPropertyInfo addInfoStringInfo;

		public CodeDataPair AddNew(string code, string data)
		{
			CodeDataPair newItem = AddNew();
			newItem.ZO_Code = code;
			newItem.ZO_Data = data;
			return newItem;
		}

		public void AddOrUpdateExisting(ZString code, ZString data)
		{
			if (!code.IsEmpty)
			{
				CodeDataPair existingInfo = GetElementWithThisCode(code);
				if (existingInfo == null)
				{
					AddNew(code, data);
				}
				else
				{
					existingInfo.ZO_Data = data;
				}
			}
		}

		public void LoadFromString(ZString addInfoString)
		{
			LoadFromString(addInfoString, clearExisting: true);
		}

		public void LoadFromString(ZString addInfoString, bool clearExisting)
		{
			LoadFromString(addInfoString, clearExisting, allowDuplicates: true);
		}

		public void LoadFromStringWithNoDuplicates(ZString addInfoString)
		{
			LoadFromString(addInfoString, clearExisting: false, allowDuplicates: false);
		}

		void LoadFromString(ZString addInfoString, bool clearExisting, bool allowDuplicates)
		{
			using (SuspendCodesChangedRelatedActions())
			{
				isInitialised = false;
				if (clearExisting)
				{
					RemoveAll();
				}

				if (!addInfoString.IsEmpty)
				{
					ZString[] pairs = addInfoString.Split(Customs.Business.BaseAddInfo.CodeInfoSeparator);
					foreach (ZString pair in pairs)
					{
						CodeDataPair codeDataPair = AddNew();
						codeDataPair.LoadFromString(pair);

						if (string.IsNullOrEmpty(codeDataPair.ZO_Code) || (!allowDuplicates && CodeDataPairIsADuplicate(codeDataPair)))
						{
							Remove(codeDataPair);
						}
					}
				}
				isInitialised = true;
			}
		}
		protected bool isInitialised;

		bool CodeDataPairIsADuplicate(CodeDataPair codeDataPair)
		{
			foreach (CodeDataPair p in this)
			{
				if ((p != codeDataPair) && (p.ZO_Code == codeDataPair.ZO_Code) && (p.ZO_Data == codeDataPair.ZO_Data))
				{
					return true;
				}
			}

			return false;
		}

		public void ValidateDuplicateCode(CodeDataPair infoWithCodeJustEntered)
		{
			foreach (CodeDataPair codeInfo in this)
			{
				if (codeInfo != infoWithCodeJustEntered && !codeInfo.ZO_CodeInfo.HasNotifications() && !infoWithCodeJustEntered.ZO_Code.IsEmpty && codeInfo.ZO_Code == infoWithCodeJustEntered.ZO_Code)
				{
					infoWithCodeJustEntered.ZO_CodeInfo.AddError(infoWithCodeJustEntered.ZO_Code + " already exists.");
				}
			}
		}

		public ZString AggregatedCodes
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (CodeDataPair codeInfo in this)
				{
					result.Append(codeInfo.ZO_Code + CodeSeparatorInAggregated);
				}
				return result.ToString().Trim(CodeSeparatorInAggregated);
			}
		}

		public ZString AggregatedDatas
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (CodeDataPair codeInfo in this)
				{
					result.Append(codeInfo.ZO_Data + CodeSeparatorInAggregated);
				}
				return result.ToString().Trim(CodeSeparatorInAggregated);
			}
		}

		protected override bool AllowSort
		{
			get { return true; }
		}

		const char CodeSeparatorInAggregated = '/';

		public CodeDataPair GetElementWithThisCode(string code)
		{
			foreach (CodeDataPair codeInfo in this)
			{
				if (codeInfo.ZO_Code == code)
				{
					return codeInfo;
				}
			}
			return null;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			foreach (CodeDataPair info in this)
			{
				result.Append(info.ToString() + Customs.Business.BaseAddInfo.CodeInfoSeparator);
			}
			return result.ToString().Trim(Customs.Business.BaseAddInfo.CodeInfoSeparator);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (!IsValidationSuspended)
			{
				RunPreSaveValidation();//Duplicate Code validation
			}
			CodeDataPair dataPair = bizO as CodeDataPair;
			if (dataPair != null)
			{
				dataPair.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(NewElement_HasChangesChanged);
				if (!dataPair.ZO_Code.IsEmpty)
				{
					FireCodesInListHaveChanged();
				}
			}
			UpdateAddInfoStringInfo();
		}

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			CodeDataPair newElement = CreateNewCodeInfo();
			newElement.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(NewElement_HasChangesChanged);
			return newElement;
		}

		#region Parent Accessors
		public BusinessObject Parent
		{
			get { return (addInfoStringInfo?.BizObj); }
		}

		public JobComInvoiceLine ParentInvoiceLine => Parent as JobComInvoiceLine;

		public JobDeclaration ParentDeclaration => (Parent as NZAddInfo)?.ParentObject as JobDeclaration;

		public JobDeclaration Declaration => ParentDeclaration ?? ParentInvoiceLine?.Declaration;
		#endregion

		protected abstract CodeDataPair CreateNewCodeInfo();

		void NewElement_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged)
			{
				UpdateAddInfoStringInfo();
			}
		}

		protected void UpdateAddInfoStringInfo()
		{
			if (addInfoStringInfo != null && isInitialised)
			{
				addInfoStringInfo.Value = (ZString)ToString();
			}
		}

		#region CodesInListHaveChanged Event
		public delegate void CodesInListHaveChangedEventHandler();

		public event CodesInListHaveChangedEventHandler CodesInListHaveChanged;

		public void FireCodesInListHaveChanged()
		{
			if (CodesInListHaveChanged != null && !IsCodesChangedRelatedActionsSuspended)
			{
				CodesInListHaveChanged();
			}
		}
		#endregion

		public void CopyContentsOverwriting(CodeDataPairCollection destination)
		{
			destination.RemoveAll();
			foreach (CodeDataPair pair in this)
			{
				destination.AddClone(pair);
			}
		}

		public BusinessObject AddClone(CodeDataPair businessObjectToClone)
		{
			return AddNew(businessObjectToClone.ZO_Code, businessObjectToClone.ZO_Data);
		}

		#region CodesChangedRelatedActionsSuspender
		public IDisposable SuspendCodesChangedRelatedActions()
		{
			return new CodesChangedRelatedActionsSuspender(this);
		}

		public bool IsCodesChangedRelatedActionsSuspended
		{
			get { return codesChangedRelatedActionsSuspenderIndex > 0; }
		}

		int codesChangedRelatedActionsSuspenderIndex;
		class CodesChangedRelatedActionsSuspender : IDisposable
		{
			public CodesChangedRelatedActionsSuspender(CodeDataPairCollection collection)
			{
				this.collection = collection;
				collection.codesChangedRelatedActionsSuspenderIndex++;
			}

			readonly CodeDataPairCollection collection;

			public void Dispose()
			{
				collection.codesChangedRelatedActionsSuspenderIndex--;
			}
		}
		#endregion
	}
}
