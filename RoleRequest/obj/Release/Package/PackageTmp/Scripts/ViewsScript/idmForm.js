$(document).ready(function () {
    $("#idmUserForm").submit(function (event) {
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
                onAjaxRequestSuccessIDMForm(result);
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

    var onAjaxRequestSuccessIDMForm = function (result) {
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

            $('#idmUserForm').get(0).reset();
            $('#idmFormModal').modal('hide')
            alert(result.SuccessMsg);
            // Resetting form.  
        }
        $("#overlay").hide();
    }

    $('#idmFormModal').on('hidden.bs.modal', function (event) {
        $('.validated-control').removeClass('input-validation-error');
        $('.text-danger').text("");
    });
});