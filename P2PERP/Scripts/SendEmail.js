function sendEmailAjax(formData, swalOptions) {
    // Step 1: Show loading Swal using the options provided
    Swal.fire({
        title: swalOptions.title || "Sending...",
        html: swalOptions.html || "Please wait...",
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    // Step 2: Send email via AJAX
    $.ajax({
        url: "/Account/SendEmail",
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            Swal.close();
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    html: `<p>${response.message}</p><p>${swalOptions.html || "Please wait..."}</p>`,
                    showConfirmButton: false,
                    timer: 3000,
                    willClose: swalOptions.willClose
                });
            } else {
                Swal.fire("Error", response.message, "error");
            }
        },
        error: function () {
            debugger;
            Swal.close();
            Swal.fire("Error", "Something went wrong while sending the email.", "error");
        }
    });
}
