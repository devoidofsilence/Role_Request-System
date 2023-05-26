$(document).ready(function () {
    $('.dateFieldAllDateShow').datepicker();

    //$('.btnRevokeRequest').on('click', function (e) {
    //    var clickedButton = $(this);
    //    if (confirm('Do you want to proceed?')) {
    //        $("#overlay").show();
    //        $.ajax({
    //            url: '/HR/RevokeRequest?employeeUsername=' + $(this).data('value'),
    //            dataType: "json",
    //            type: "POST",
    //            success: function (data) {
    //                if (data > 0) {
    //                    $(clickedButton).remove();
    //                }
    //                $("#overlay").hide();
    //            },
    //            error: function (xhr) {
    //                alert('error');
    //                $("#overlay").hide();
    //            }
    //        });
    //    }
    //    else {
    //        return false;
    //    }
    //});
});