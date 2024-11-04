/*document.getElementById('searchButton').addEventListener('click', function () {
    const btuValue = document.getElementById('btuInput').value;
    if (btuValue) {
        fetch('@Url.Action("SearchByBTU", "FindConditionersBTU")', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ btu: btuValue })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                // Обновляем список кондиционеров
                updateConditionerList(data);
            })
            .catch(error => {
                document.getElementById('errorMessage').innerText = 'Ошибка: ' + error.message;
                document.getElementById('errorMessage').style.display = 'block';
            });
    }
});

function updateConditionerList(conditioners) {
    const conditionersList = document.getElementById('conditionersList');
    conditionersList.innerHTML = ''; // Очищаем текущий список

    if (conditioners.length === 0) {
        conditionersList.innerHTML = '<p style="margin: 15px 0px; color: red;">No conditioners found. Please try another search.</p>';
        return;
    }

    const table = document.createElement('table');
    table.className = 'full-width-table';

    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    ['Name', 'Price', 'BTU', 'Service Area'].forEach(text => {
        const th = document.createElement('th');
        th.innerText = text;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    const tbody = document.createElement('tbody');
    conditioners.forEach(conditioner => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td><a href="${conditioner.Url}" target="_blank">${conditioner.Name}</a></td>
            <td>${conditioner.Price}</td>
            <td>${conditioner.BTU}</td>
            <td>${conditioner.ServiceArea}</td>
        `;
        tbody.appendChild(row);
    });
    table.appendChild(tbody);
    conditionersList.appendChild(table);
}
*/