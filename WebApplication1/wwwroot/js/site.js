function loadMessages(url) {
    $.getJSON(url, function (data) {
        if (data && data.length > 0) {
            $("#messagesBody").empty(); // очищает содержимое таблицы
            $.each(data, function (index, item) {
                var timestamp = new Date(item.timestamp);
                var formattedTime = timestamp.toLocaleString();

                var newRow = $('<tr>');
                newRow.append($('<td>').text(item.message));
                newRow.append($('<td>').text(formattedTime));
                $("#messagesBody").append(newRow);
            });
            $("#messagesContainer").show(); // показываем контейнер с таблицей
        } else {
            $("#messagesContainer").html('<p>No messages available.</p>');
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.log("AJAX request failed: " + errorThrown);
    });
}

$("#showAllMessages").click(function () {
    loadMessages('/Message/GetAllMessages');
});

$(document).ready(function () {
    $("#showMessages").click(function () {
        loadMessages('/Message/GetUserMessages');
    });
});

$(document).ready(function () {
    $("#submitMessage").click(function () {
        var message = $("#message").val();

        $.ajax({
            url: "/Message/Submit",
            type: "POST",
            data: { message: message },
            success: function () {
                $("#successMessage").show(); // Показать сообщение об успешной отправке
                $("#message").val("");
                setTimeout(function () {
                    $("#successMessage").hide(); // Скрыть сообщение через некоторое время
                }, 3000); // Время в миллисекундах
            },
            error: function () {
                swal.fire("Ошибка", "Произошла ошибка при отправке сообщения", "error");
            }
        });
    });
});