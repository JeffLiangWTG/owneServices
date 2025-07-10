//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusOutturnLookups
//
//    This class should be used for overriding collections in AutoCusOutturnLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Business.Interfaces;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusOutturnLookups : AutoCusOutturnLookups
	{
		public CusOutturnLookups(AutoCusOutturn parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList OutturnResultTypeList
		{
			get
			{
				if (outturnResultTypeList == null)
				{
					outturnResultTypeList = new CodeDescriptionPairList();
				}
				return outturnResultTypeList;
			}
		}
		CodeDescriptionPairList outturnResultTypeList;

		public CodeDescriptionPairList OutturnLineList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				CusUnderbond underbond = Outturn.Underbond;
				if (underbond != null)
				{
					ICusUnderbondDependentCollectionParent linkedObject = underbond.LinkedObject;
					if (linkedObject != null)
					{
						foreach (IOutturnableLine line in linkedObject.OutturnableLines)
						{
							result.AddPair(line.UnderbondHumanReadableName, line.UnderbondHumanReadableName);
						}
					}
				}
				return result;
			}
		}

		public virtual ReadOnlyCodeDescriptionPairList CommercialStatusList => new ReadOnlyCodeDescriptionPairList();

		#region CargoTypes

		public CodeDescriptionPairList CargoTypes
		{
			get
			{
				if (fCargoTypes == null)
				{
					fCargoTypes = GetNewCargoTypes();
				}
				return fCargoTypes;
			}
		}
		CodeDescriptionPairList fCargoTypes;

		protected virtual CodeDescriptionPairList GetNewCargoTypes()
		{
			return new CodeDescriptionPairList();
		}

		#endregion

		#region PackageTypes

		public CodeDescriptionPairList PackageTypes
		{
			get
			{
				if (fPackageTypes == null)
				{
					fPackageTypes = GetNewPackageTypes();
				}
				return fPackageTypes;
			}
		}
		internal CodeDescriptionPairList fPackageTypes;

		protected virtual CodeDescriptionPairList GetNewPackageTypes()
		{
			return new CodeDescriptionPairList();
		}

		public void PurgePackageTypes()
		{
			fPackageTypes = null;
		}

		#endregion

		CusOutturn Outturn
		{
			get { return (CusOutturn)Parent; }
		}
	}
}
