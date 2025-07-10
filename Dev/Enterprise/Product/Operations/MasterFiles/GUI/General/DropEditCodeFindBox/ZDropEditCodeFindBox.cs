using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZDropEditCodeFindBox : ZUserControl, IBindingMemberForCompileTimeCheckProvider, IResourceStringBindingMember
	{
		public ZDropEditCodeFindBox()
		{
			InitializeComponent();
		}

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZDropEditCodeFindBox>()
				.Property("Text", "")
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region Properties

		string CodeType
		{
			get { return CodeTypeDropEdit.Text; }
			set { CodeTypeDropEdit.Text = value; }
		}

		#endregion

		#region Code ModuleID

		[SmartTagVisible]
		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public ModuleIdentifier CodeModuleID
		{
			get { return CodeFindBox.ModuleID; }
			set
			{
				if (value != ModuleIDs.NotAssigned)
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(CodeFindBox);
				}
				CodeFindBox.ModuleID = value;
			}
		}

		#endregion

		#region ZEmbeddedCodeFindBox class

		class ZEmbeddedCodeFindBox : ZCodeFindBox
		{
			public ZEmbeddedCodeFindBox(ZDropEditCodeFindBox owner)
			{
				this.Owner = owner;
			}
			readonly ZDropEditCodeFindBox Owner;

			protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
			{
				if (CodeTypeSchema != null)
				{
					var codeTypeList = Owner.CodeTypeDropEdit.List;
					if (codeTypeList != null && codeTypeList.Count > 0)
					{
						module.FilterBusinessObject.ModuleFilters.AddTextFilter("D6F082DB-DC4A-40D6-A169-D222BF564EFD", new GetTextQuery(GetCode), codeTypeList).Visibility = FilterVisibility.AlwaysAppliedAndHidden;
					}

					if (!string.IsNullOrEmpty(Owner.CodeType))
					{
						module.FilterBusinessObject.SetInitialCodeForSearch(Owner.CodeType, CodeTypeSchema.Name);
					}
				}

				var result = base.CreateEmbeddedPopup(module);
				result.Selected += new EmbeddedModulePopup.SelectedEventHandler(OnSelectedBusinessObjectFromPopup);
				return result;
			}

			protected override ZFilterModule NewModuleFromModuleID()
			{
				var result = base.NewModuleFromModuleID();
				var codeTypeFilterSupportedModule = result.FilterBusinessObject as IDropEditCodeFindBoxSupportFilterStripBusinessObject;
				if (codeTypeFilterSupportedModule != null)
				{
					CodeTypeSchema = codeTypeFilterSupportedModule.CodeTypeSchema;
				}
				return result;
			}

			SchemaColumn CodeTypeSchema;

			ZQuery GetCode(ZString target)
			{
				var codeList = new List<ZString>();
				foreach (ICodeDescription pair in Owner.CodeTypeDropEdit.List)
				{
					codeList.Add(pair.Code);
				}
				return new ZQuery(CodeTypeSchema, codeList);
			}

			void OnSelectedBusinessObjectFromPopup(object sender, EmbeddedModulePopup.SelectedEventArgs e)
			{
				if (e.SelectedBusinessObjects.Length == 1)
				{
					var bizo = e.SelectedBusinessObjects[0];
					var codeDescriptionWithCodeType = (ICodeTypeCodeDescription)bizo;
					this.Owner.CodeType = codeDescriptionWithCodeType.CodeType;
					this.Owner.CodeTypeDropEdit.CommitBoundValue();
					Owner.CodeFindBox.Code = codeDescriptionWithCodeType.Code;
				}
			}
		}

		#endregion

		#region BindToCode / BindToCodeType

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToCode
		{
			get { return BindingSource.GetBindingMember(CodeFindBox); }
			set
			{
				BindingSource.SetBindingMember(CodeFindBox, value);
				if (string.IsNullOrEmpty(BindingMemberHelper.BindingMember))
				{
					BindingMemberHelper.BindingMember = ".";
				}
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToCodeType
		{
			get { return BindingSource.GetBindingMember(CodeTypeDropEdit); }
			set
			{
				BindingSource.SetBindingMember(CodeTypeDropEdit, value);

				if (string.IsNullOrEmpty(BindingMemberHelper.BindingMember))
				{
					BindingMemberHelper.BindingMember = ".";
				}
			}
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		public override Type DataSourceType
		{
			get { return typeof(object); }
		}

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region BindToList

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToCodeTypeList
		{
			get { return CodeTypeDropEdit.BindToList; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(this);
				}
				CodeTypeDropEdit.BindToList = value;
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToCodeList
		{
			get { return CodeFindBox.BindToList; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(this);
				}
				CodeFindBox.BindToList = value;
			}
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			if (!string.IsNullOrEmpty(BindToCode))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(ZString), BindToCode));
			}
			if (!string.IsNullOrEmpty(BindToCodeType))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(ZString), BindToCodeType));
			}
			return result;
		}

		#endregion

		#region IResourceStringBindingMember Members

		string IResourceStringBindingMember.ResourceStringBindingMember
		{
			get { return BindToCodeType; }
		}

		#endregion
	}
}
