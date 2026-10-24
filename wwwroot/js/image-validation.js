// browser check for image uploads
// .jpg .jpeg .png .webp only, 2 MB max (server checks again)

document.querySelectorAll("input[type=file]").forEach(function (input) {
    input.addEventListener("change", function () {
        // red error text under the input
        var message = document.querySelector("span[data-valmsg-for='" + input.name + "']");
        message.textContent = "";

        if (input.files.length === 0) {
            return;
        }

        var file = input.files[0];
        var name = file.name.toLowerCase();

        var goodType = name.endsWith(".jpg") || name.endsWith(".jpeg")
            || name.endsWith(".png") || name.endsWith(".webp");

        if (!goodType) {
            message.textContent = "Only .jpg, .jpeg, .png or .webp images are allowed.";
            input.value = ""; // clear the bad file so it isn't uploaded
        } else if (file.size > 2 * 1024 * 1024) {
            message.textContent = "The image must be 2 MB or smaller.";
            input.value = "";
        }
    });
});
