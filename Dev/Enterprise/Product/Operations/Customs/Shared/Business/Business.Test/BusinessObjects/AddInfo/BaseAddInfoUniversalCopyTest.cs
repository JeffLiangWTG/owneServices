using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseAddInfoUniversalCopyTest : TestCaseWithFactory
	{
		#region TestUniversalCopyShouldContainsChildCusAddInfos

		public void TestUniversalCopyShouldContainsChildCusAddInfos()
		{
			var infoManager = GetManager();
			var type = infoManager.GetType();

			var cachedResult = new Dictionary<Type, Type>();
			var propertiesAndTypes = new List<(PropertyInfo info, Type elementType)>();

			GetAllChildCusAddInfoPropertyAndTypeList(type, propertiesAndTypes, cachedResult);

			var invalidTypes = propertiesAndTypes
				.Select(c => c.elementType)
				.Distinct()
				.Where(c => !HasEmptyGlowInterfaceReferenceAttribute(c) && !IsValidClassType(c));

			var invalidPropertyInfos = propertiesAndTypes
				.Where(c => !HasUniversalCopyCollectionEntityAttribute(c.info))
				.Select(c => c.info);

			CombineAssertions(() =>
			{
				var message = $@"These below types should implement ""[GlowInterfaceReference("")]"" for universal copy:

{string.Join(System.Environment.NewLine, invalidTypes.Select(c => c.FullName).OrderBy(c => c))}";

				Assert(message, !invalidTypes.Any());

				message = $@"These below property infos should follow this below rule for universal copy.
If the type of property is BusinessObject: ""[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = ""CY_ParentID,CY_ParentTableCode"", DisableCopyMethodLink = true)]""
If the type of property is BusinessObjectCollection: ""[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]""

{string.Join(System.Environment.NewLine, invalidPropertyInfos.Select(c => c.DeclaringType.FullName + @"." + c.Name).OrderBy(c => c))}";

				Assert(message, !invalidPropertyInfos.Any());
			});
		}

		bool HasEmptyGlowInterfaceReferenceAttribute(Type type)
		{
			var attribute = type.GetCustomAttribute<GlowInterfaceReferenceAttribute>();
			return attribute != null && string.IsNullOrWhiteSpace(attribute.InterfaceName);
		}

		bool IsValidClassType(Type type)
		{
			return type.BaseType != cusAddInfoType || type.BaseType?.BaseType != cusAddInfoType;
		}

		bool HasUniversalCopyCollectionEntityAttribute(PropertyInfo info)
		{
			var result = false;
			var type = info.PropertyType;

			if (type.IsSubclassOf(cusAddInfoType))
			{
				var attribute = info.GetCustomAttribute<UniversalCopyRelatedEntityAttribute>();

				result = attribute != null
					&& attribute.CommaSeparatedSkipPropertiesNames == "B7_ParentID,B7_ParentTableCode"
					&& attribute.DisableCopyMethodLink;
			}
			else if (type.IsSubclassOf(businessObjectCollectionType))
			{
				var attribute = info.GetCustomAttribute<UniversalCopyCollectionEntityAttribute>();

				result = attribute != null
					&& attribute.ItemsTableName == AutoCusAddInfo.Schema.TableName
					&& attribute.ItemPropertyName == AutoCusAddInfo.Schema.B7_ParentID;
			}

			return result;
		}

		void GetAllChildCusAddInfoPropertyAndTypeList(Type type, List<(PropertyInfo info, Type elementType)> list, Dictionary<Type, Type> cachedResult)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(c => c.PropertyType.IsSubclassOf(cusAddInfoType) || c.PropertyType.IsSubclassOf(businessObjectCollectionType));

			var infoAndTypes = properties
				.Select(c => (c, GetActualEntityTypeIfIsSubCalss(c.PropertyType, cachedResult)))
				.Where(c => c.Item2 != null)
				.ToList();

			list.AddRange(infoAndTypes);

			foreach (var infoAndType in infoAndTypes)
			{
				var entityType = infoAndType.Item2;

				if (entityType != type && list.All(c => c.elementType != entityType))
				{
					GetAllChildCusAddInfoPropertyAndTypeList(entityType, list, cachedResult);
				}
			}
		}

		readonly Type cusAddInfoType = typeof(CusAddInfo);
		readonly Type businessObjectCollectionType = typeof(BusinessObjectCollection);

		Type GetActualEntityTypeIfIsSubCalss(Type type, Dictionary<Type, Type> cachedResult)
		{
			Type result = null;

			if (cachedResult.TryGetValue(type, out result))
			{
				return result;
			}
			else
			{
				if (type.IsSubclassOf(cusAddInfoType))
				{
					result = type;
				}
				else if (type.IsSubclassOf(businessObjectCollectionType))
				{
					var childElementType = BusinessObjectCollection.GetElementTypeFromCollectionType(type);
					result = GetActualEntityTypeIfIsSubCalss(childElementType, cachedResult);
				}

				cachedResult.Add(type, result);
			}

			return result;
		}

		#endregion

		#region TestUniversalCopyShouldContainsAddInfoProperties

		public void TestUniversalCopyShouldContainsAddInfoProperties()
		{
			var infoManager = GetManager();

			var type = infoManager.GetType();
			var assembly = type.Assembly;

			var assemblyAttribute = assembly.GetCustomAttribute<UniversalCopyAddInfoPropertyDefinitionAttribute>();
			AssertNotNull($"The assembly - {assembly.FullName} should implement UniversalCopyAddInfoPropertyDefinitionAttribute for Universal Copy.", assemblyAttribute);

			var classAttribute = type.GetCustomAttribute<UniversalCopyAddInfoAttribute>();
			AssertNotNull($"The class - {type.FullName} should implement UniversalCopyAddInfoAttribute for Universal Copy.", classAttribute);

			var copyTemplateTree = new CopyTemplateTree(type, type, copyTreeConfiguration: BusinessObjectCopyManager.CopyTreeConfiguration);
			var includedPropertyInfos = GetIncludedPropertyInfos(infoManager);

			var nodes = ((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.OfType<PropertyCopyTemplateNode>();
			var missProperties = includedPropertyInfos.Where(c => nodes.All(d => d.Name != c.Name));

			if (missProperties.Any())
			{
				var message = new ZStringBuilder();
				message.Append("These below AddInfo properties are miss in the Universal Copy Template");
				message.Append(string.Join(", ", missProperties));
				message.AppendLine();

				message.Append("All Nodes:");
				message.Append(string.Join(", ", nodes.Select(c => $"{c.Name} ({c.PropertyType})")));

				Assert(message.ToStringWithNewLineBetweenAppends(), false);
			}

			var allNodeNames = ((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Select(c => c.Name).ToArray();
			AssertHasOtherNodes(allNodeNames);
		}

		protected virtual void AssertHasOtherNodes(string[] allNodeNames)
		{
		}

		protected virtual ZPropertyInfo[] GetIncludedPropertyInfos(IAddInfoManager infoManager)
		{
			var businessObject = (BusinessObject)infoManager;
			var addInfo = infoManager.AddInfo switch
			{
				BusinessObject bo => bo,
				Integration.Customs.IAddInfoBase baseAddInfo => baseAddInfo.Parent,
				_ => null
			};

			if (addInfo == null)
			{
				return [];
			}

			var addInfoPropertyInfos = addInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>();

			return businessObject
				.ZPropertyInfoHash
				.OfType<ZWrappedPropertyInfo>()
				.Where(c => !((c.PropertyDescriptor.Attributes[typeof(ReadOnlyAttribute)] as ReadOnlyAttribute)?.IsReadOnly ?? false) && addInfoPropertyInfos.Any(d => d == c.InnerInfo))
				.ToArray();
		}

		#endregion

		protected abstract IAddInfoManager GetManager();
	}
}
