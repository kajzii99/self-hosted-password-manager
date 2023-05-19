    var passwordField = document.getElementById("password");
    var showPasswordButton = document.getElementById("show-password");

    showPasswordButton.addEventListener("click", function() {
  if (passwordField.type === "password") {
        passwordField.type = "text";
    showPasswordButton.value = "Hide password";
  }
    else {
        passwordField.type = "password";
    showPasswordButton.value = "Show password";
  }
    });
    