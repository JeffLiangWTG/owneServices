$.extend($.jgrid.search, {
    multipleSearch: true,
    multipleGroup: true,
    recreateFilter: true,
    closeOnEscape: true,
    //closeAfterSearch: true,
    overlay: 0,
    afterRedraw: function () {
        $('input.add-rule', this).val('Add new rule');
        $('input.add-group', this).val('Add new group');
        $('input.delete-rule', this).val('Delete rule');
        $('input.delete-group', this).val('Delete group');
        $(this).find("table.group:not(:first)").css({
            borderWidth: "1px",
            borderStyle: "dashed"
        });
    }
});