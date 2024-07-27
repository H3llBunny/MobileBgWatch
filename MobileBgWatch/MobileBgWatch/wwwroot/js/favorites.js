function toggleFavorite(element) {
    event.preventDefault();
    event.stopPropagation();

    const vehicleId = element.getAttribute('data-vehicle-id');
    const isFavorite = element.querySelector('i').classList.contains('fa-solid');

    fetch(`/Vehicle/ToggleFavorite`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': '@Html.AntiForgeryToken()'
        },
        body: JSON.stringify({ vehicleId: vehicleId, favorite: !isFavorite })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const icon = element.querySelector('i');
                const textSpan = document.getElementById('favIconText');
                if (data.favorite) {
                    icon.classList.remove('fa-regular');
                    icon.classList.add('fa-solid');
                    icon.title = "Remove from Favorite";
                    if (textSpan) {
                        textSpan.textContent = "Remove from Favorite"
                    }
                } else {
                    icon.classList.remove('fa-solid');
                    icon.classList.add('fa-regular');
                    icon.title = "Add to Favorite";
                    if (textSpan) {
                        textSpan.textContent = "Add to Favorite"
                    }
                }
            } else {
                alert('An error occurred while toggling the favorite status.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while toggling the favorite status.');
        });
}