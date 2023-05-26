$(document).ready(function () {
    $("#hrUserForm").submit(function (event) {
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
                onAjaxRequestSuccessHRForm(result);
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

    $("#hrUserRejectForm").submit(function (event) {
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
                onAjaxRequestSuccessHRRejectForm(result);
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

    var onAjaxRequestSuccessHRForm = function (result) {
        if (result.EnableError) {
            // Setting.  
            if (result.Errors != undefined) {
                $('.validated-control').removeClass('input-validation-error');
                $('.text-danger').text("");
                _.forEach(result.Errors, function (value) {
                    $('input[name="' + value.key + '"]').addClass('input-validation-error');
                    _.forEach(value.errors, function (err) {
                        $('span[handle="' + value.key + '"]').text(err);
                    });
                });
            }
            else {
                alert(result.ErrorMsg);
            }
        } else if (result.EnableSuccess) {
            // Setting
            // var currrentUserSamAccName = @Html.Raw(currrentUserSamAccName);
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

            //Start//Refresh Enroll List//
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
            //End//Refresh Enroll List

            //Start//Refresh IDM List//
            var user = $("#btnIDMUsersForm").data('value');
            $.ajax({
                async: false,
                url: '/IDManagement/IDMUsersListAJAX?idmAdminADName=' + user,
                type: 'GET',
                success: function (objOperations) {
                    $("#prtlIDMUsers").html(objOperations);
                    $("#idmUsersCnt").text($('#prtlIDMUsers #idmUsersListTBody tr.countableRow').length);

                    var tbl_IDMUsers_New = $('#tbl_IDMUsers').DataTable({
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
                        "order": [],
                        "language": {
                            "zeroRecords": "No data!"
                        }
                    });

                    tbl_IDMUsers_New.on('order.dt search.dt', function () {
                        tbl_IDMUsers_New.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
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
                    tbl_IDMUsers_New.columns().every(function () {
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
            //End//Refresh IDM List//

            $('#hrFormModal').modal('hide')
            alert(result.SuccessMsg);
            // Resetting form.  
            $('#hrUserForm').get(0).reset();
        }
        $("#overlay").hide();
    }

    $('#hrFormModal').on('hidden.bs.modal', function (event) {
        $('.validated-control').removeClass('input-validation-error');
        $('.text-danger').text("");
    });

    var onAjaxRequestSuccessHRRejectForm = function (result) {
        if (result.EnableError) {
            // Setting.  
            if (result.Errors != undefined) {
                $('.validated-control').removeClass('input-validation-error');
                $('.text-danger').text("");
                _.forEach(result.Errors, function (value) {
                    $('input[name="' + value.key + '"]').addClass('input-validation-error');
                    _.forEach(value.errors, function (err) {
                        $('span[handle="' + value.key + '"]').text(err);
                    });
                });
            }
            else {
                alert(result.ErrorMsg);
            }
        } else if (result.EnableSuccess) {
            // Setting
            // var currrentUserSamAccName = @Html.Raw(currrentUserSamAccName);
            var user = $("#btnHRUsersReject").data('value');
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

            //Start//Refresh Enroll List//
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
            //End//Refresh Enroll List

            $('#hrFormRejectModal').modal('hide')
            alert(result.SuccessMsg);
            // Resetting form.  
            $('#hrUserRejectForm').get(0).reset();
        }
        $("#overlay").hide();
    }

    $('#hrFormRejectModal').on('hidden.bs.modal', function (event) {
        $('.validated-control').removeClass('input-validation-error');
        $('.text-danger').text("");
    });
});