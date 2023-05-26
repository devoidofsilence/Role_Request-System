$(document).ready(function () {
    $('.btnRevokeAppAccess').on('click', function (e) {
        var clickedButton = $(this);
        if (confirm('Do you want to proceed?')) {
            $("#overlay").show();
            $.ajax({
                url: '/IDManagement/RevokeRequestAppAccess?revokeId=' + $(this).data('revoke-id') + '&appId=' + $(this).data('app-id'),
                dataType: "json",
                type: "POST",
                success: function (data) {
                    if (data > 0) {
                        $(clickedButton).closest('.btnAppAccessBlock').html('<label>Revoked</label>');
                        $('#TakenBy').val($(clickedButton).data('taking-by'));
                        $(clickedButton).remove();
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

    $('.btnRevokePrimRole').on('click', function (e) {
        var clickedButton = $(this);
        if (confirm('Do you want to proceed?')) {
            $("#overlay").show();
            $.ajax({
                url: '/IDManagement/RevokeRequestPrimRole?revokeId=' + $(this).data('revoke-id') + '&primRoleId=' + $(this).data('prim-role-id'),
                dataType: "json",
                type: "POST",
                success: function (data) {
                    if (data > 0) {
                        $(clickedButton).closest('.btnPrimRoleBlock').html('<label>Revoked</label>');
                        $('#TakenBy').val($(clickedButton).data('taking-by'));
                        $(clickedButton).remove();
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

    $('#btnRevokeAll').on('click', function (e) {
        var clickedButton = $(this);
        if (confirm('Do you want to proceed?')) {
            $("#overlay").show();
            $.ajax({
                url: '/IDManagement/RevokeRequestAll?revokeId=' + $(this).data('revoke-id'),
                dataType: "json",
                type: "POST",
                success: function (data) {
                    if (data > 0) {
                        $('.btnPrimRoleBlock').each(function () {
                            $(this).html('<label>Revoked</label>');
                        });
                        $('.btnAppAccessBlock').each(function () {
                            $(this).html('<label>Revoked</label>');
                        });
                        $('#TakenBy').val($(clickedButton).data('taking-by'));
                        $(clickedButton).remove();
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
});