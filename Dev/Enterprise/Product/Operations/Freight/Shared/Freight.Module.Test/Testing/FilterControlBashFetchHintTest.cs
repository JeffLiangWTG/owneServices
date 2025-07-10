using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	public abstract class FilterControlBashFetchHintTest<T> : TestCaseWithFactory
			where T : BusinessObject
	{
		[RequiresSTA]
		public void TestMissingPropertiesForTestFetchHint()
		{
			using (var control = GetNewFilterStripControl())
			{
				var grid = control.FilteredGrid;

				var excludedColumnNames = GetExcludedColumnNamesForTestFetchHint();

				var columnNames = grid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Select(c => c.ColumnName)
					.Where(c => excludedColumnNames.All(d => d != c))
					.ToArray();

				var methodNames = GetType().GetMethods()
					.Select(c => c.Name)
					.Where(c => c.StartsWith(PrefixForMethodName, true, CultureInfo.InvariantCulture))
					.ToArray();

				var list = new List<string>();

				foreach (var columnName in columnNames)
				{
					var methodName = string.Concat(PrefixForMethodName, columnName.Replace('.', '_').Replace('+', '_'));

					if (methodNames.All(c => c != methodName))
					{
						var propertyAndMethod = string.Concat(columnName, " - ", methodName);
						list.Add(propertyAndMethod);
					}
				}

				var message = string.Concat("Following properties miss matching test cases. For performance reason please check their max db hits and add corresponding test cases.",
					System.Environment.NewLine,
					string.Join(System.Environment.NewLine, list));

				Assert(message, !list.Any());
			}
		}

		protected void BashFetchForView(string bindToString, int maxDbHits)
		{
			var factory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};

			Assert("Should setup business objects to run test.", keysForTest.Any());

			var query = new ZQuery(PkColumn, keysForTest);

			var objs = factory.Load<T>(query);

			factory.ResetDatabaseLoadCount();

			var columns = new[] { new TableColumn(string.Empty, bindToString) };

			var collection = GetNewCollection();
			if (collection != null)
			{
				collection.FetchStrategy.FetchForView(objs, columns);
			}

			foreach (var obj in objs)
			{
				obj.FetchStrategy.FetchForView(columns);
			}

			foreach (var obj in objs)
			{
				HitProperty(obj, bindToString);
			}

			var message = string.Format("The {0}'s max db hit should be {1}", bindToString, maxDbHits);
			AssertMaxDbHits(message, maxDbHits, factory);
		}

		void HitProperty(object root, string pathStr)
		{
			var path = pathStr.Split(new[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);

			var o = root;

			for (var i = 0; i < path.Length; i++)
			{
				var message = string.Join("+", path, 0, i) + " returned null, you should populate your data better";
				AssertNotNull(message, o);

				if (o != null)
				{
					o = o.GetType().InvokeMember(path[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
				}
			}
		}

		ZGuid[] keysForTest;

		protected override void SetUp()
		{
			base.SetUp();

			keysForTest = keysForTest ?? (keysForTest = CreateKeysForTest());
		}

		const string PrefixForMethodName = "TestBashFetchForView_";

		protected abstract SchemaPKColumn PkColumn { get; }

		protected abstract ZGuid[] CreateKeysForTest();

		protected abstract ZFilterStripControl GetNewFilterStripControl();

		protected abstract IBusinessObjectCollection GetNewCollection();

		protected virtual string[] GetExcludedColumnNamesForTestFetchHint()
		{
			return Array.Empty<string>();
		}
	}
}
