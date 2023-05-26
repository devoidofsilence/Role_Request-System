$(document).ready(function () {
    $(".dateField").datepicker({
        endDate: "today",
        maxDate: "today"
    });

    $('.btnViewRemarksInline').on('click', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var rrfId = button.data('value');
        $.ajax({
            url: '/RoleRequest/GetAllRemarksInThisRequest?rrfId=' + rrfId,
            dataType: "json",
            type: "GET",
            success: function (data) {
                if (data != null || data != undefined) {
                    var html = "";
                    _.forEach(data, function (value) {
                        html = html + "<tr class='table-default'>";
                        html = html + "<td style='white-space: nowrap'>" + value.RequestId.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.RemarksBy.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.AssignDate.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.CompleteDate.toString() + "</td>";
                        html = html + "<td>" + value.AdditionalRequest.toString() + "</td>";
                        html = html + "<td>" + value.Remarks.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.Action.toString() + "</td></tr>";
                    });
                    $("#tbdRemarks").html(html);
                }
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    $("#enrollUserCnt").text($('#enrollUserListTBody tr.countableRow').length);
    $("#hrUsersCnt").text($('#hrUsersListTBody tr.countableRow').length);
    $("#idmUsersCnt").text($('#idmUsersListTBody tr.countableRow').length);
    $("#hrUsersListForRevocationCnt").text($('#hrUsersListTBodyForRevocation tr.countableRow').length);
    $("#idmUsersListForRevocationCnt").text($('#idmUsersListTBodyForRevocation tr.countableRow').length);

    $("body").prepend('<div id="overlay" class="ui-widget-overlay" style="z-index: 1055; display: none;"></div>');

    $('#go-to-top').each(function () {
        $(this).click(function () {
            $('html,body').animate({ scrollTop: 0 }, 'slow');
            return false;
        });
    });

    // the selector will match all input controls of type :checkbox
    // and attach a click event handler 
    $("input:checkbox").on('click', function () {
        // in the handler, 'this' refers to the box clicked on
        var $box = $(this);
        if ($box.is(":checked")) {
            // the name of the box is retrieved using the .attr() method
            // as it is assumed and expected to be immutable
            var group = "input:checkbox[definedName='" + $box.attr("definedName") + "']";
            // the checked state of the group/box on the other hand will change
            // and the current value is retrieved using .prop() method
            $(group).prop("checked", false);
            $box.prop("checked", true);
        } else {
            $box.prop("checked", false);
        }
    });


    //    //Datatables Initialization
    var tbl_EnrollList = $('#tbl_EnrollList').DataTable({
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 8) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 9],
            "orderable": false,
            "searchable": false
        },
        {
            "targets": [7],
            "type": "date"
        }],
        "order": [[8, "desc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_EnrollList.on('order.dt search.dt', function () {
        tbl_EnrollList.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countEnroll = 0;
    $('#tbl_EnrollList tfoot th').each(function () {
        if (countEnroll !== 0 && countEnroll != 8 && countEnroll !== 9) {
            var title = $('#tbl_EnrollList thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countEnroll++;
    });

    // Apply the filter
    tbl_EnrollList.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_Requester = $('#tbl_Requester').DataTable({
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 3) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 4],
            "orderable": false,
            "searchable": false
        }],
        "order": [[1, "desc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_Requester.on('order.dt search.dt', function () {
        tbl_Requester.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countRequester = 0;
    $('#tbl_Requester tfoot th').each(function () {
        if (countRequester !== 0 && countRequester !== 3 && countRequester !== 4) {
            var title = $('#tbl_Requester thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countRequester++;
    });

    // Apply the filter
    tbl_Requester.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_Recommender = $('#tbl_Recommender').DataTable({
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 3) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 4],
            "orderable": false,
            "searchable": false
        }],
        "order": [[3, "desc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_Recommender.on('order.dt search.dt', function () {
        tbl_Recommender.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countRecommender = 0;
    $('#tbl_Recommender tfoot th').each(function () {
        if (countRecommender !== 0 && countRecommender !== 3 && countRecommender !== 4) {
            var title = $('#tbl_Recommender thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countRecommender++;
    });

    // Apply the filter
    tbl_Recommender.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_Approver = $('#tbl_Approver').DataTable({
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 3) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 4],
            "orderable": false,
            "searchable": false
        }],
        "order": [[3, "desc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_Approver.on('order.dt search.dt', function () {
        tbl_Approver.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countApprover = 0;
    $('#tbl_Approver tfoot th').each(function () {
        if (countApprover !== 0 && countApprover !== 3 && countApprover !== 4) {
            var title = $('#tbl_Approver thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countApprover++;
    });

    // Apply the filter
    tbl_Approver.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_Admin = $('#tbl_Admin').DataTable({
        //"searching": false,
        //"colReorder": true,
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 2) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            }
        ],
        "columnDefs": [{
            "targets": [7],
            "orderable": false,
            "searchable": false
        }],
        "order": [[2, "desc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    var count = 0;
    $('#tbl_Admin tfoot th').each(function () {
        if (count != 2 && count !== 7) {
            var title = $('#tbl_Admin thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        count++;
    });

    // Apply the filter
    tbl_Admin.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_HRUsers = $('#tbl_HRUsers').DataTable({
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 8],
            "orderable": false,
            "searchable": false
        },
        {
            "targets": [7],
            "type": "date"
        }],
        "order": [[1, "asc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_HRUsers.on('order.dt search.dt', function () {
        tbl_HRUsers.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countHRUsers = 0;
    $('#tbl_HRUsers tfoot th').each(function () {
        if (countHRUsers !== 0 && countHRUsers !== 8) {
            var title = $('#tbl_HRUsers thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countHRUsers++;
    });

    // Apply the filter
    tbl_HRUsers.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    //    var tbl_IDMUsers = $('#tbl_IDMUsers').DataTable({
    //        "searching": false,
    //        "ordering": false,
    //        "info": false,
    //        "dom": '<"bottom"l>rt<"bottom"p><"clear">'
    //    });

    var tbl_IDMUsers = $('#tbl_IDMUsers').DataTable({
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 9],
            "orderable": false,
            "searchable": false
        },
        {
            "targets": [8],
            "type": "date"
        }],
        "order": [[1, "asc"]],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_IDMUsers.on('order.dt search.dt', function () {
        tbl_IDMUsers.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countIDMUsers = 0;
    $('#tbl_IDMUsers tfoot th').each(function () {
        if (countIDMUsers !== 0 && countIDMUsers !== 9) {
            var title = $('#tbl_IDMUsers thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countIDMUsers++;
    });

    // Apply the filter
    tbl_IDMUsers.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_HRUsersForRevocation = $('#tbl_HRUsersForRevocation').DataTable({
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 6) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 7],
            "orderable": false,
            "searchable": false
        }],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_HRUsersForRevocation.on('order.dt search.dt', function () {
        tbl_HRUsersForRevocation.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countHRUsers = 0;
    $('#tbl_HRUsersForRevocation tfoot th').each(function () {
        if (countHRUsers !== 0 && countHRUsers !== 6 && countHRUsers !== 7) {
            var title = $('#tbl_HRUsersForRevocation thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countHRUsers++;
    });

    // Apply the filter
    tbl_HRUsersForRevocation.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });

    var tbl_IDMUsersForRevocation = $('#tbl_IDMUsersForRevocation').DataTable({
        initComplete: function () {
            var countForCol = 0;
            this.api().columns().every(function () {
                if (countForCol == 4) {
                    var column = this;
                    var select = $('<select><option value="">[none]</option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                }
                countForCol++;
            });
        },
        "info": false,
        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
        "buttons": [
            {
                extend: 'csvHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            },
            {
                extend: 'excelHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            },
            {
                extend: 'pdfHtml5',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6]
                }
            }
        ],
        "columnDefs": [{
            "targets": [0, 7],
            "orderable": false,
            "searchable": false
        }],
        "language": {
            "zeroRecords": "No data!"
        }
    });

    tbl_IDMUsersForRevocation.on('order.dt search.dt', function () {
        tbl_IDMUsersForRevocation.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var countIDMUsers = 0;
    $('#tbl_IDMUsersForRevocation tfoot th').each(function () {
        if (countIDMUsers !== 0 && countIDMUsers !== 4 && countIDMUsers !== 7) {
            var title = $('#tbl_IDMUsersForRevocation thead th').eq($(this).index()).text();
            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
        }
        countIDMUsers++;
    });

    // Apply the filter
    tbl_IDMUsersForRevocation.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change clear', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });
    //    //Datatables Initialization


    $("#enrollUserForm").submit(function (event) {
        if (confirm('Do you want to proceed?')) {
            $("#overlay").show();
            event.preventDefault();
            var form = $(this);
            $.ajax({
                url: form.attr("action"),
                method: form.attr("method"),  // post
                data: form.serialize()
            })
                .done(function (result) {
                    // Result.  
                    onAjaxRequestSuccessEnrollForm(result);
                })
                .fail(function (jqXHR, textStatus) {
                    //do your own thing
                    $("#overlay").hide();
                });
            return true;
        }
        else {
            return false;
        }
    });

    var onAjaxRequestSuccessEnrollForm = function (result) {
        if (result.EnableError) {
            // Setting.
            $('.validated-control').removeClass('input-validation-error');
            $('.text-danger').text("");
            _.forEach(result.Errors, function (value) {
                $('input[name="' + value.key + '"]').addClass('input-validation-error');
                _.forEach(value.errors, function (err) {
                    $('span[handle="' + value.key + '"]').text(err);
                });
            });
        } else if (result.EnableSuccess) {
            // Setting
            // var currrentUserSamAccName = @Html.Raw(currrentUserSamAccName);
            var user = $("#btnEnrollUser").data('val');
            $.ajax({
                async: false,
                url: '/EnrollUser/EnrollUsersListBySupervisorADNameAJAX?supervisorADName=' + user,
                type: 'GET',
                success: function (objOperations) {
                    $("#prtlEnrollUser").html(objOperations);
                    $("#enrollUserCnt").text($('#prtlEnrollUser #enrollUserListTBody tr.countableRow').length);
                    var tbl_EnrollList_New = $('#tbl_EnrollList').DataTable({
                        initComplete: function () {
                            var countForCol = 0;
                            this.api().columns().every(function () {
                                if (countForCol == 8) {
                                    var column = this;
                                    var select = $('<select><option value="">[none]</option></select>')
                                        .appendTo($(column.footer()).empty())
                                        .on('change', function () {
                                            var val = $.fn.dataTable.util.escapeRegex(
                                                $(this).val()
                                            );

                                            column
                                                .search(val ? '^' + val + '$' : '', true, false)
                                                .draw();
                                        });

                                    column.data().unique().sort().each(function (d, j) {
                                        select.append('<option value="' + d + '">' + d + '</option>')
                                    });
                                }
                                countForCol++;
                            });
                        },
                        "info": false,
                        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
                        "buttons": [
                            {
                                extend: 'csvHtml5',
                                exportOptions: {
                                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                                }
                            },
                            {
                                extend: 'excelHtml5',
                                exportOptions: {
                                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                                }
                            },
                            {
                                extend: 'pdfHtml5',
                                exportOptions: {
                                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8]
                                }
                            }
                        ],
                        "columnDefs": [{
                            "targets": [0, 9],
                            "orderable": false,
                            "searchable": false
                        },
                        {
                            "targets": [7],
                            "type": "date"
                        }],
                        "order": [[8, "desc"]]
                    });

                    tbl_EnrollList_New.on('order.dt search.dt', function () {
                        tbl_EnrollList_New.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                            cell.innerHTML = i + 1;
                        });
                    }).draw();

                    var countEnrollRefresh = 0;
                    $('#tbl_EnrollList tfoot th').each(function () {
                        if (countEnrollRefresh !== 0 && countEnrollRefresh != 8 && countEnrollRefresh !== 9) {
                            var title = $('#tbl_EnrollList thead th').eq($(this).index()).text();
                            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
                        }
                        countEnrollRefresh++;
                    });

                    // Apply the filter
                    tbl_EnrollList_New.columns().every(function () {
                        var that = this;

                        $('input', this.footer()).on('keyup change clear', function () {
                            if (that.search() !== this.value) {
                                that
                                    .search(this.value)
                                    .draw();
                            }
                        });
                    });
                }
            });

            //Start//Refresh HR List//
            var user = $("#btnHRUsersForm").data('value');
            $.ajax({
                async: false,
                url: '/HR/HRUsersListAJAX?hrAdminADName=' + user,
                type: 'GET',
                success: function (objOperations) {
                    $("#prtlHRUsers").html(objOperations);
                    $("#hrUsersCnt").text($('#prtlHRUsers #hrUsersListTBody tr.countableRow').length);

                    var tbl_HRUsers_New = $('#tbl_HRUsers').DataTable({
                        "info": false,
                        "dom": '<"top"i>rt<"bottom"lp>B<"clear">',
                        "buttons": [
                            {
                                extend: 'csvHtml5',
                                exportOptions: {
                                    columns: [0, 1, 2, 3, 4, 5, 6, 7]
                                }
                            },
                            {
                                extend: 'excelHtml5',
                                exportOptions: {
                                    columns: [0, 1, 2, 3, 4, 5, 6, 7]
                                }
                            },
                            {
                                extend: 'pdfHtml5',
                                exportOptions: {
                                    columns: [0, 1, 2, 3, 4, 5, 6, 7]
                                }
                            }
                        ],
                        "columnDefs": [{
                            "targets": [0, 8],
                            "orderable": false,
                            "searchable": false
                        },
                        {
                            "targets": [7],
                            "type": "date"
                        }],
                        "order": [[1, "asc"]]
                    });

                    tbl_HRUsers_New.on('order.dt search.dt', function () {
                        tbl_HRUsers_New.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                            cell.innerHTML = i + 1;
                        });
                    }).draw();

                    var countHRUsers = 0;
                    $('#tbl_HRUsers tfoot th').each(function () {
                        if (countHRUsers !== 0 && countHRUsers !== 8) {
                            var title = $('#tbl_HRUsers thead th').eq($(this).index()).text();
                            $(this).html('<input type="text" class="form-control" placeholder="' + $.trim(title) + '" />');
                        }
                        countHRUsers++;
                    });

                    // Apply the filter
                    tbl_HRUsers_New.columns().every(-function () {
                        var that = this;

                        $('input', this.footer()).on('keyup change clear', function () {
                            if (that.search() !== this.value) {
                                that
                                    .search(this.value)
                                    .draw();
                            }
                        });
                    });
                }
            });
            //End//Refresh HR List//

            $('#enrollUserModal').modal('hide')
            alert(result.SuccessMsg);
            // Resetting form.  
            $('#enrollUserForm').get(0).reset();
        }
        $("#overlay").hide();
    }

    $('#enrollUserModal').on('hidden.bs.modal', function (event) {
        $('.validated-control').removeClass('input-validation-error');
        $('.text-danger').text("");
    });

    $('#btnTakeRequest').on('click', function () {
        if (confirm('Do you want to proceed?')) {
            $("#overlay").show();
            $.ajax({
                url: '/Provision/TakeRequest?rrfId=' + $('#RoleRequestId').val() + '&provisionId=' + $('#ProvisionId').val(),
                dataType: "json",
                type: "POST",
                success: function (data) {
                    if (data === 1) {
                        $('#btnBlock').replaceWith(function () {
                            return $("<button type='button' class='btn btn-sbl-yellow float-right btn-pre-submit' data-name='Command' data-value='Return' data-toggle='modal' data-target='#remarksModal'>Return</button><button id='btnSave' type='button' class='btn btn-sbl-yellow float-right btn-pre-submit' data-name='Command' data-value='Save' data-toggle='modal' data-target='#remarksModal'>Save</button>");
                        });
                    }
                    $("#overlay").hide();
                },
                error: function (xhr) {
                    alert('error');
                    $("#overlay").hide();
                }
            });
        }
        else {
            return false;
        }
    });

    $('#takeBackRemarksModal').on('show.bs.modal', function (event) {
        //event.preventDefault();
        var button = $(event.relatedTarget) // Button that triggered the modal
        var name = button.data('name') // Extract info from data-* attributes
        var value = button.data('value') // Extract info from data-* attributes
        // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
        // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
        var modal = $(this)
        modal.find('.btn-submission-take-back').attr('name', name)
        modal.find('.btn-submission-take-back').attr('value', value)
        modal.find('#TakeBackRemarks').val('');
    });

    $('.btn-submission-take-back').on('click', function () {
        if ($('#TakeBackRemarks').val().trim() === "") {
            alert("Please provide remarks");
        }
        else if (confirm('Do you want to take back request?')) {
            $("#overlay").show();
            $.ajax({
                url: '/RoleRequest/TakeBackRequest?rrfId=' + $(this).val() + '&remarks=' + $('#TakeBackRemarks').val(),
                dataType: "json",
                type: "POST",
                success: function (data) {
                    if (data === 1) {
                        if (!alert('Operation successful!!!!')) {
                            window.location.reload();
                        }
                    }
                    $("#overlay").hide();
                },
                error: function (xhr) {
                    alert('error');
                    $("#overlay").hide();
                }
            });
        }
        else {
            return false;
        }
    });

    $('.btnViewRemarksInline').on('click', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var rrfId = button.data('value');
        $.ajax({
            url: '/RoleRequest/GetAllRemarksInThisRequest?rrfId=' + rrfId,
            dataType: "json",
            type: "GET",
            success: function (data) {
                if (data != null || data != undefined) {
                    var html = "";
                    _.forEach(data, function (value) {
                        html = html + "<tr class='table-default'>";
                        html = html + "<td style='white-space: nowrap'>" + value.RequestId.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.RemarksBy.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.AssignDate.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.CompleteDate.toString() + "</td>";
                        html = html + "<td>" + value.AdditionalRequest.toString() + "</td>";
                        html = html + "<td>" + value.Remarks.toString() + "</td>";
                        html = html + "<td style='white-space: nowrap'>" + value.Action.toString() + "</td></tr>";
                    });
                    $("#tbdRemarks").html(html);
                }
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    $(document).on('click', '.btnHRUsersForm', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var enrollUserID = button.data('value');
        $.ajax({
            async: false,
            url: '/HR/UserHRDetailForm?EnrollUserID=' + enrollUserID,
            type: "GET",
            success: function (objOperations) {
                $("#prtlHRUserForm").html(objOperations);
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    $(document).on('click', '.btnHRUsersReject', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var enrollUserID = button.data('value');
        $.ajax({
            async: false,
            url: '/HR/UserHRRejectForm?EnrollUserID=' + enrollUserID,
            type: "GET",
            success: function (objOperations) {
                $("#prtlHRUserRejectForm").html(objOperations);
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    $(document).on('click', '.btnIDMUsersForm', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var enrollUserID = button.data('value');
        $.ajax({
            async: false,
            url: '/IDManagement/UserIDMDetailForm?EnrollUserID=' + enrollUserID,
            type: "GET",
            success: function (objOperations) {
                $("#prtlIDMUserForm").html(objOperations);
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    $(document).on('click', '.btnEnrollUserDetailView', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var enrollUserID = button.data('value');
        $.ajax({
            async: false,
            url: '/EnrollUser/EnrollUserDetail?EnrollUserID=' + enrollUserID,
            type: "GET",
            success: function (objOperations) {
                $("#prtlEnrollUserDetail").html(objOperations);
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    //Start //Open Revoke Form Modal with getting data for the user to be revoked roles for
    $(document).on('click', '.btnRevokeRequestModalOpener', function () {
        var button = $(this); // Button that triggered the modal
        $("#overlay").show();
        var empUsername = button.data('value');
        $.ajax({
            async: false,
            url: '/HR/OpenModalFormForRevoke?EmpUsernameForRevoke=' + empUsername,
            type: "GET",
            success: function (objOperations) {
                $("#prtlRevokeRequest").html(objOperations);
                $("#overlay").hide();
            },
            error: function (xhr) {
                alert('error');
                $("#overlay").hide();
            }
        });
    });

    $('#provisionForm').submit(function () {
        if (confirm('Do you want to proceed?')) {
            //        $("#overlay, #PleaseWait").show();
            $("#overlay").show();

            return true;
        }
        else {
            return false;
        }
    });

    $('#rrForm').submit(function (e) {
        var atLeastOneHeadIsCheckedForFC = false;
        var atLeastOneChildIsCheckedForFC = false;
        var showAlertForFCHead = false;
        var showAlertForFCChild = false;
        _.forEach($('.flexcubeAccessLvlChk'), function (value) {
            if ($(value).prop("checked") === true) {
                atLeastOneHeadIsCheckedForFC = true;
            }
        });

        _.forEach($('.flexcubeRoleChk'), function (value) {
            if ($(value).prop("checked") === true) {
                atLeastOneChildIsCheckedForFC = true;
            }
        });

        var atLeastOneHeadIsCheckedForEDMS = false;
        var atLeastOneChildIsCheckedForEDMS = false;
        var showAlertForEDMSHead = false;
        var showAlertForEDMSChild = false;
        _.forEach($('.edmsAccessLvlChk'), function (value) {
            if ($(value).prop("checked") === true) {
                atLeastOneHeadIsCheckedForEDMS = true;
            }
        });

        _.forEach($('.edmsRoleChk'), function (value) {
            if ($(value).prop("checked") === true) {
                atLeastOneChildIsCheckedForEDMS = true;
            }
        });

        if (atLeastOneHeadIsCheckedForFC === true && atLeastOneChildIsCheckedForFC === false) {
            showAlertForFCChild = true;
        }

        if (atLeastOneHeadIsCheckedForEDMS === true && atLeastOneChildIsCheckedForEDMS === false) {
            showAlertForEDMSChild = true;
        }

        if (atLeastOneHeadIsCheckedForFC === false && atLeastOneChildIsCheckedForFC === true) {
            showAlertForFCHead = true;
        }

        if (atLeastOneHeadIsCheckedForEDMS === false && atLeastOneChildIsCheckedForEDMS === true) {
            showAlertForEDMSHead = true;
        }

        if (showAlertForFCHead === true || showAlertForEDMSHead === true || showAlertForFCChild === true || showAlertForEDMSChild === true) {
            if (showAlertForFCHead === true) {
                alert("Please select one of the access levels for Flexcube UBS");
            }
            if (showAlertForEDMSHead === true) {
                alert("Please select one of the access levels for EDMS");
            }
            if (showAlertForEDMSChild === true) {
                alert("Please select one of the roles for EDMS");
            }
            if (showAlertForFCChild === true) {
                alert("Please select one of the roles for Flexcube UBS");
            }
            return false;
        } else {
            if (confirm('Do you want to proceed?')) {
                //        $("#overlay, #PleaseWait").show();
                $("#overlay").show();

                return true;
            }
            else {
                return false;
            }
        }
    });

    $('.rrf-view-btn').on('click', function () {
        //    $("#overlay, #PleaseWait").show();
        $("#overlay").show();

        return true;
    });

    //$('.dashboard-btn').on('click', function () {
    //    //    $("#overlay, #PleaseWait").show();
    //    $("#overlay").show();

    //    return true;
    //});

    $('#remarksModal').on('show.bs.modal', function (event) {
        //event.preventDefault();
        var button = $(event.relatedTarget) // Button that triggered the modal
        var name = button.data('name') // Extract info from data-* attributes
        var value = button.data('value') // Extract info from data-* attributes
        // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
        // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
        var modal = $(this)
        modal.find('.btn-submission').attr('name', name)
        modal.find('.btn-submission').attr('value', value)
        modal.find('.remarks-text').val('');
        modal.find('#modalLabel').text(value);
    });
    //Load Active Directory Names except requester
    $(function () {
        var empNamesWithValue = [];
        var empNames = [];
        $.ajax({
            url: '/Common/GetEmployeesJSON',
            dataType: "json",
            type: "GET",
            success: function (data) {
                empNames = data;
                _.forEach(data, function (value) {
                    if (value.SamAccountName.trim() !== $('#loggedInUser').val()) {
                        empNamesWithValue.push({ label: value.EmployeeFullName + "(" + value.SamAccountName + ")", value: value.SamAccountName });
                    }
                });

                $(".adloader").on("keypress", function (e, u) {
                    clearFields(e);
                });

                $(".adloader").on("keydown", function (e) {
                    if (e.keyCode === 8) {
                        clearFields(e);
                    }
                });

                $(".adloader").autocomplete({
                    source: empNamesWithValue,
                    select: function (event, ui) {
                        event.preventDefault();
                    },
                    focus: function (event, ui) {
                        event.preventDefault();
                    },
                    close: function (event, ui) {
                        event.preventDefault();
                    }
                });

                $(".adloader").on("autocompleteselect", function (event, ui) {
                    autoCompleteCall(event, ui);
                });
                $(".adloader").on("autocompleteclose", function (event, ui) {

                });

                function autoCompleteCall(event, ui) {
                    $('#' + event.target.id).val(ui.item.label);
                    var selectedEmployee = _.filter(empNames, { 'SamAccountName': ui.item.value });
                    $('#' + $('#' + event.target.id).parent('div').find('.by-id').attr('id')).val(selectedEmployee[0].SamAccountName);
                }

                function clearFields(e) {
                    $('#' + $('#' + e.target.id).parent('div').find('.by-id').attr('id')).val("");
                }
            },
            error: function (xhr) {
                alert('error');
            }
        });
    });
});