// Set global Toastr options
toastr.options = {
    progressBar: true,
    preventDuplicates: true,
    newestOnTop: true,
    positionClass: "toast-top-right",
    timeOut: "3000" // 3 seconds
};

// Central AJAX handler
function handleAjaxForm(form) {
    $.ajax({
        type: "POST",
        url: $(form).attr("action"),
        data: $(form).serialize(),
        success: function (response) {
            if (!response.success) {
                Swal.fire("Error", response.message || "Request failed", "error");
                return;
            }

            // Determine page type
            const page = $(form).data("page");
            switch (page) {
                case "login":
                    showToast("Login Successfully", "success");
                    setTimeout(() => redirectByDepartment(response.departmentId), 3000);
                    break;

                case "verifyCode":
                    showToast("Code Verified Successfully", "success");
                    setTimeout(() => window.location.href = "/Account/ChangePassword", 3000);
                    break;

                case "sendCode":
                    showToast("Code Sent Successfully", "success");
                    setTimeout(() => window.location.href = "/Account/VerifyCode", 3000);
                    break;

                case "changePassword":
                    showToast("Password Changed Successfully", "success");
                    setTimeout(() => window.location.href = "/Account/Logout", 3000);
                    break;
            }
        },
        error: function () {
            Swal.fire("Error", "Something went wrong!", "error");
        }
    });
}

// Updated showToast to use Toastr
function showToast(message, type = "success") {
    switch (type) {
        case "success":
            toastr.success(message);
            break;
        case "error":
            toastr.error(message);
            break;
        case "warning":
            toastr.warning(message);
            break;
        case "info":
            toastr.info(message);
            break;
    }
}

// Redirect based on department
function redirectByDepartment(department) {
    switch (department) {
        case 1: window.location.href = `/Admin/Index`; break;
        case 2: window.location.href = `/Purchase/UserDashboardPRK`; break;
        case 3: window.location.href = `/GRN/GRNDashboardRHK`; break;
        case 4: window.location.href = `/Quality/QCheckDashboardNAM`; break;
        case 5: window.location.href = `/Accountant/Index`; break;
        case 6: window.location.href = `/Inventory/Index`; break;
        case 7: window.location.href = `/Production/Index`; break;
        case 8: window.location.href = `/AdmSalesAndDistributionin/Index`; break;
        case 9: window.location.href = `/HumanResources/Index`; break;
        case 10: window.location.href = `/Maintenance/Index`; break;
        default: window.location.href = `/Account/Logout`; break;
    }
}
