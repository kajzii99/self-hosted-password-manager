$(function () {
    var placeholderElement = $('#modal-placeholder');
    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
            var modalElement = placeholderElement.find('.modal');
            var modalUsernameElement = modalElement.find('#modalUsername');
            modalUsernameElement.val("wartosc losowa uzupelnienia");
            modalElement.modal('show');
        });
    });

    placeholderElement.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();
        // Łapiemy referencję do naszych komunikatów i je chowamy na wysłaniu zapytania
        const successData = $('#valid-info');
        successData.css("display", "none");
        const errorData = $('#not-valid-info');
        errorData.css("display", "none")
        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');
        var dataToSend = form.serialize();
        if (dataToSend.Password === 'aaaaa') return;

        $.post(actionUrl, dataToSend)
            .done(function (data, statusText, xhr) {
                // Włączamy odpowiedni komunikat - sukces lub błąd - wraz ze zmianą jego tekstu na odpowiednią - do rozwinięcia

                var status = xhr.status;

                switch (status) {
                    case 200:
                        successData.css("display", "block");
                        break;
                    case 400:
                        errorData.innerHtml = "Something went wrong. Check your password. It should be composed of a minimum of 12 characters, should contain uppercase and lowercase letters, special characters and numbers."
                        errorData.css("display", "block");
                        break;
                    case 401:
                        errorData.innerHtml = "Not user's credential.";
                        errorData.css("display", "block");
                        break;
                    case 403:
                        errorData.innerHtml = "Credential not found";
                        errorData.css("display", "block");
                        break;
                    case 404:
                        errorData.innerHtml = "Credential not found";
                        errorData.css("display", "block");
                        break;
                    default:
                        errorData.innerHtml = "Something went wrong. Check your password. It should be composed of a minimum of 12 characters, should contain uppercase and lowercase letters, special characters and numbers."
                        errorData.css("display", "block");
                        break;
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                errorData.css("display", "block")
            });

    });

    placeholderElement.on('click', '[data-dismiss="modal"]', function (event) {
        // Reload by ściągnął nowe dane na wyjściu z modala po edycji
        window.location.reload();
    });
});


function handleSaveButtonClick() {
    // pobieranie danych z pól formularza
    const website = document.getElementById("website-input").value;
    const username = document.getElementById("username-input").value;
    const password = document.getElementById("password-input").value;
    const notes = document.getElementById("notes-input").value;

    // Wywołanie funckji która zwróci json z odpowiedzią
    fetch("/CredentialController/Edit", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ website, username, password, notes })
    })
        .then(response => response.json())
        .then(data => {
            if (data.errors) {
                // Wyświetlanie błędów użytkownikowi 
                alert("Wystąpił błąd: " + data.errors.join(", "));
            } else {
                // Widok update + zamknięcie modala
                updateView(data);
                /*closeModal();*/
            }
        })
        .catch(error => {
            // Wyśietlanie błędu 
            alert("Wystąpił błąd: " + error.message);
        });

}
