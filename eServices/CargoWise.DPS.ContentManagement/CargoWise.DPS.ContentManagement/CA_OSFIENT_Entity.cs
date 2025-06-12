using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;


namespace CargoWise.DPS.ContentManagement
{

    [IgnoreFirst(5)]
    [IgnoreEmptyLines()]
    [ConditionalRecord(RecordCondition.ExcludeIfMatchRegex, "^(\"|\t|3 T| |[a-zA-Z])(.+)$")]

    //[ConditionalRecord(RecordCondition.ExcludeIfMatchRegex, "^[\"\t]")]

    [DelimitedRecord("	")]
    public sealed class CA_OSFIENT_Entity
    {

        public String id;
        [FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
        public String name;
        [FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
        public String address;
        [FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
        public String basis;

    }
}
