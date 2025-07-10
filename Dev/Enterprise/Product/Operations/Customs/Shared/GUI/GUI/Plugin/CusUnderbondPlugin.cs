using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GUI
{
	public class CusUnderbondPlugin : ZPlugIn
	{
		#region Constructors

		public CusUnderbondPlugin(ICusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend, string pluginName)
			: base(hostEntity)
		{
			this.bindPrepend = bindPrepend;
			businessObject = hostEntity;
			this.pluginName = pluginName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "plugin name")]
		public CusUnderbondPlugin(ICusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend)
			: this(hostEntity, bindPrepend, "Customs Underbond Movement")
		{
		}

		public CusUnderbondPlugin(ICusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend, string pluginName, Type typeOfCurrent)
			: this(hostEntity, bindPrepend, pluginName)
		{
			if (typeOfCurrent == null)
			{
				Dispose();
				throw new ArgumentNullException(nameof(typeOfCurrent), "TypeOfCurrent is not allowed to be null.");
			}
			fTypeOfCurrent = typeOfCurrent;
		}

		#endregion

		#region Plugin Overrides

		public override string Name
		{
			get { return pluginName; }
		}

		public new CusUnderbondUserControl UserControl
		{
			get { return (CusUnderbondUserControl)base.UserControl; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			CusUnderbondUserControl control = CreateNewUserControl();
			control.SetUnderbondParent(businessObject as ICusUnderbondNilUnderbondPerformer);
			ICusUnderbondUnionCollectionParent decider = businessObject as ICusUnderbondUnionCollectionParent;
			if (decider != null)
			{
				if (decider.IsForAirCargo)
				{
					RemoveColumnsForAir(control);
				}
				else
				{
					RemoveColumnsForSea(control);
				}
			}
#if DEBUG
			else
			{
				throw new ApplicationException("Business object for plugin (" + businessObject + ") does not implement ICusUnderbondUnionCollectionParent.");
			}
#endif
			control.SetBindPrepend(bindPrepend);
			return control;
		}

		protected virtual CusUnderbondUserControl CreateNewUserControl()
		{
			return new CusUnderbondUserControl();
		}

		protected virtual void RemoveColumnsForAir(CusUnderbondUserControl control)
		{
			control.RemoveColumnsForAir();
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Collection;
		}

		protected internal bool RegisterPlugInBusinessEntityAsEditableInternal => RegisterPlugInBusinessEntityAsEditable;
		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return false; }
		}

		protected internal void OnCurrentChangedInternal() => OnCurrentChanged();
		protected override void OnCurrentChanged()
		{
			base.OnCurrentChanged();
			UserControl.Enabled = !IsCurrentDependent || Current != null;
			RefreshCollection();
		}

		#endregion

		#region RemoveColumnsForSea

		protected virtual void RemoveColumnsForSea(CusUnderbondUserControl control)
		{
			control.RemoveColumnsForSea();
		}

		#endregion

		#region Implementation

		#region Collection

		CusUnderbondUnionCollectionParentCollection fCollection;
		internal CusUnderbondUnionCollectionParentCollection Collection
		{
			get
			{
				if (fCollection == null)
				{
					if (IsCurrentDependent)
					{
						fCollection = GetNewCurrentDependentCusUnderbondUnionCollectionParentCollection(HostBusinessEntity.Factory, TypeOfCurrent);
					}
					else
					{
						fCollection = GetNewCusUnderbondUnionCollectionParentCollection(HostBusinessEntity.Factory);
					}

					RefreshCollection();
				}
				return fCollection;
			}
		}

		protected virtual CusUnderbondUnionCollectionParentCollection GetNewCurrentDependentCusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory, Type typeOfCurrent)
		{
			return new CusUnderbondUnionCollectionParentCollection(factory, typeOfCurrent);
		}

		protected virtual CusUnderbondUnionCollectionParentCollection GetNewCusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory)
		{
			return new CusUnderbondUnionCollectionParentCollection(factory);
		}

		void RefreshCollection()
		{
			if (fCollection != null)
			{
				Collection.RemoveAll();
				BusinessObject businessObjectToAdd = IsCurrentDependent ? Current : (BusinessObject)businessObject;
				if (businessObjectToAdd != null)
				{
					Collection.Add(businessObjectToAdd);
				}
			}
			UserControl.DetailsUserControl.ChangeVisibility(UserControl.CurrentUnderbond);
		}

		#endregion

		#region TypeOfCurrent

		internal Type TypeOfCurrent
		{
			get
			{
				if (fTypeOfCurrent == null && IsCurrentDependent)
				{
					if (Current == null)
					{
						throw new ArgumentNullException("When this plugin is used as a ZCurrentDependent plugin, you must pass a TypeOfCurrent into the constructor of this plugin. This is the type of the business object on whose collection this plugin depends on.");
					}
					else
					{
						return Current.GetType();
					}
				}
				else
				{
					return fTypeOfCurrent;
				}
			}
		}
		internal Type fTypeOfCurrent;

		#endregion

		readonly ZString bindPrepend;
		internal IBusiness businessObject;
		readonly string pluginName;

		#endregion

		public new virtual bool IsCurrentDependent
		{
			get { return base.IsCurrentDependent; }
		}

		public new virtual BusinessObject Current
		{
			get { return base.Current; }
		}
	}
}
