using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ReportTesting
{
	public abstract class ReportFunctionalFilterTestCase<T> : ReportFunctionalTestCase
		where T : BusinessObject
	{
		protected override bool ShouldTestColumns
		{
			get { return false; }
		}

		/// <summary>
		/// Override this to tell the framework which parameter on the function should have its value tweaked as part of the test.
		/// If your fucntion is dbo.GetFoo(a, b, c) and you are testing parameter b, return 1. Zero-based.
		/// </summary>
		protected abstract int ParameterIndexToReplace { get; }

		protected override void PrepareTestData()
		{
			var bizoToInclude = MakeNewBusinessObject();
			var bizOToExclude = MakeNewBusinessObject();
			TweakOneField(bizoToInclude, bizOToExclude);
		}

		protected virtual T MakeNewBusinessObject()
		{
			return Factory.New<T>();
		}

		void TweakOneField(T bizOToInclude, T bizOToExclude)
		{
			if (NameOfBusinessObjectFieldToSetViaIndexer != null)
			{
				bizOToInclude[NameOfBusinessObjectFieldToSetViaIndexer] = ValueToSetOnInclude;
				bizOToExclude[NameOfBusinessObjectFieldToSetViaIndexer] = ValueToSetOnExclude;
			}
			else if (SetValueManually != null)
			{
				SetValueManually(bizOToInclude, ValueToSetOnInclude, false);
				SetValueManually(bizOToExclude, ValueToSetOnExclude, true);
			}
			else
			{
				throw new NotSupportedException("Override either MethodToSetValue or NameOfBusinessObjectFieldToSetViaIndexer");
			}
		}

		protected override void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames)
		{
			AssertEquals("Should be only 1 row returned by the report. Could be: a) the two test rows do not differ on the field you think they should differ on (if overridden, check that SetValueManually is assigning correctly); b) you have the wrong parameter index; c) the function is not filtering how you think it should.... ",
							1, resultsOrderedByExpectedColumnNames.Rows.Count);
			var row1 = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[0], resultsOrderedByExpectedColumnNames) + ';';
			AssertContainsMoreHelpfully("[" + AliasedFieldReturnedByReport + "]='" + ValueToExpectBackForFoundRow + "';", row1);  // e.g. "[JE_MessageType]='EXP'" or "[Message Type]='EXP'"
			AssertNotContains("[" + AliasedFieldReturnedByReport + "]='" + ValueBackThatShouldNotBePresentInFoundRow + "';", row1);
		}

		/// <summary>
		/// Only override this if the value your supply as an argument to the function is not to be expected back as a column. For example,you might supply an orgPK as a paramter, but when checking results you might be looking for a column called "Supplier Code" etc
		/// </summary>
		protected virtual IZType ValueToExpectBackForFoundRow
		{
			get { return ValueToSetOnInclude; }
		}

		protected virtual IZType ValueBackThatShouldNotBePresentInFoundRow
		{
			get { return ValueToSetOnExclude; }
		}

		/// <summary>
		/// This should give us a standard list of parameters, all with blank/default/standard/null values. We'll vary the value of each in turn.
		/// </summary>
		protected abstract List<string> ParametersValuesListUnadulterated { get; }

		protected override List<string> ParametersValuesList
		{
			get
			{
				var core = ParametersValuesListUnadulterated;
				core[ParameterIndexToReplace] = QuoteParameter(ValueToSetOnInclude);
				return core;
			}
		}

		public delegate void MethodToSetValue(T bizO, IZType value, bool isExclude);

		/// <summary>
		/// Override this is the set-up of your two test rows cannot be achieved by a simple business object indexer.
		/// For example if you want to set JobDeclaration.SomeJobDocAddress.E2_OA_Address, we cannot to that via JobDeclaration[fieldName],
		/// so delegate some got to perform the assignment.
		/// Do not override NameOfBusinessObjectFieldToSetViaIndexer, but you must also override ValueToSetOnInclude and ValueToSetOnExclude because those cannot be created automatically.
		/// </summary>
		protected virtual MethodToSetValue SetValueManually
		{
			get { return null; }
		}

		/// <summary>
		/// Override this to tell the framework which field it should set on your bizO during set-up.  If you retuen JE_Whatever, then during set up it will assign your test value (e.g. XXXXX) to JobDeclaration.JE_Whatever using JobDeclaration[JE_Whatever] = "XXXXX"
		/// </summary>
		protected abstract SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer { get; }

		/// <summary>
		/// Override this if the column returned has a different name to the business object member.
		/// So if your report does "select JE_Foo as Bar from dbo.JobDeclaration", this member should return the alias "Bar"
		/// </summary>
		protected virtual string AliasedFieldReturnedByReport  // e.g. JE_RL_NKOrigin might be aliased as BOX15PORTOFORIGIN
		{
			get { return NameOfBusinessObjectFieldToSetViaIndexer.Name; }
		}

		/// <summary>
		/// Override this if assigning "XXXXX" as a test value on the row you expect to find is not sufficient
		/// You MUST override this if you do not override NameOfBusinessObjectFieldToSetViaIndexer.
		/// </summary>
		protected virtual IZType ValueToSetOnInclude
		{
			get { return (ZString)(new String('X', NameOfBusinessObjectFieldToSetViaIndexer.MaxLength)); }  // XXX etc
		}

		/// <summary>
		/// Override this if assigning "YYYYY" as a test value on the row you expect to exclude is not sufficient.
		/// You MUST override this if you do not override NameOfBusinessObjectFieldToSetViaIndexer.
		/// </summary>
		protected virtual IZType ValueToSetOnExclude
		{
			get { return (ZString)(new String('Y', NameOfBusinessObjectFieldToSetViaIndexer.MaxLength)); } // YYY etc
		}

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get { return null; }
		}
	}
}
